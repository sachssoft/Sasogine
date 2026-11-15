# Sasogine – `IHttpService`

## Ziel

`IHttpService` ist die gemeinsame HTTP-Abstraktion für Sasogine. Variety Golf kann dieselbe Schnittstelle für REST-APIs und Course-Downloads verwenden – unabhängig davon, ob Daten von einem Server oder aus lokalen Testdateien stammen.

**Architektur:** In Sasogine selbst wird **nur das Interface** hinzugefügt. `HttpService` und `FakeHttpService` sind austauschbare Beispielimplementierungen und können in Infrastruktur- bzw. Testprojekten liegen. Ein separates `IApiService` oder `IDownloadService` ist für diesen Anwendungsfall nicht nötig.

```text
Sasogine/
└── Services/Networking/
    └── IHttpService.cs

Sasogine.Testing/ (Beispiel)
└── Networking/
    └── FakeHttpService.cs

VarietyGolf/ (Beispiel)
├── Networking/
│   └── HttpService.cs
└── Courses/
    └── CourseDownloader.cs
```

## 1. Interface – Sasogine

**Datei:** `Services/Networking/IHttpService.cs`

```csharp
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Sachssoft.Engine.Services.Networking;

/// <summary>
/// Provides asynchronous HTTP communication for APIs and streamed downloads.
/// </summary>
public interface IHttpService
{
    /// <summary>
    /// Sends an HTTP request and returns the response without requiring the entire body to be buffered.
    /// </summary>
    /// <param name="request">The HTTP request to send.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The HTTP response. The caller must dispose of it.</returns>
    Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default);
}
```

**Vertrag:** Die Antwort soll nach Empfang der Header zur Verfügung stehen, sodass große Dateien direkt gestreamt werden können. Der Aufrufer entsorgt `HttpResponseMessage` und `HttpRequestMessage`.

## 2. Echter HTTP-Service – Beispiel

Die konkrete Implementierung kann z. B. in Variety Golf oder einem separaten Infrastrukturprojekt liegen.

```csharp
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Sachssoft.Engine.Services.Networking;

namespace VarietyGolf.Networking;

/// <summary>
/// Sends real HTTP requests using an externally managed <see cref="HttpClient"/>.
/// </summary>
/// <param name="client">The HTTP client used to send requests.</param>
public sealed class HttpService(HttpClient client) : IHttpService
{
    /// <inheritdoc />
    public Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }
}
```

`HttpCompletionOption.ResponseHeadersRead` ist für die gewünschte Download-ProgressBar wichtig: Die Methode wartet nicht erst auf den vollständig empfangenen Body. Die Lebensdauer des `HttpClient` verwaltet der aufrufende Code.

## 3. Lokaler FakeHttpService – GET, POST und PUT

Der Fake verwendet **keine Netzwerkverbindung**. Für jede Kombination aus HTTP-Methode und URL-Pfad kann eine lokale Antwortdatei sowie ein Statuscode hinterlegt werden.

```csharp
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Sachssoft.Engine.Services.Networking;

namespace Sachssoft.Engine.Testing.Networking;

/// <summary>
/// Simulates HTTP responses from local files without making network requests.
/// </summary>
public sealed class FakeHttpService : IHttpService
{
    private readonly ConcurrentDictionary<(HttpMethod Method, string Path),
        (HttpStatusCode Status, string? FilePath)> _routes = new();

    /// <summary>
    /// Gets captured HTTP methods, paths, and request bodies for test assertions.
    /// </summary>
    public ConcurrentQueue<(HttpMethod Method, string Path, string? Body)> Requests { get; } = new();

    /// <summary>
    /// Registers a response for an HTTP method and absolute URL path.
    /// </summary>
    /// <param name="method">The HTTP method, such as GET, POST or PUT.</param>
    /// <param name="path">The URL path, for example <c>/api/profile</c>.</param>
    /// <param name="filePath">The local response file, or null for an empty body.</param>
    /// <param name="status">The status returned by the fake.</param>
    public void Register(HttpMethod method, string path, string? filePath = null,
        HttpStatusCode status = HttpStatusCode.OK)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _routes[(method, path)] = (status, filePath);
    }

    /// <inheritdoc />
    public async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var path = request.RequestUri?.IsAbsoluteUri == true
            ? request.RequestUri.AbsolutePath
            : throw new ArgumentException("An absolute request URI is required.", nameof(request));

        var body = request.Content is null
            ? null
            : await request.Content.ReadAsStringAsync(cancellationToken);

        Requests.Enqueue((request.Method, path, body));

        if (!_routes.TryGetValue((request.Method, path), out var route))
            return new HttpResponseMessage(HttpStatusCode.NotFound) { RequestMessage = request };

        var response = new HttpResponseMessage(route.Status) { RequestMessage = request };
        if (route.FilePath is null)
            return response;

        // Eine fehlende Fixture ist ein Testkonfigurationsfehler und wird nicht still ignoriert.
        var stream = new FileStream(route.FilePath, FileMode.Open, FileAccess.Read,
            FileShare.Read, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
        response.Content = new StreamContent(stream);
        response.Content.Headers.ContentLength = stream.Length;
        response.Content.Headers.ContentType = new MediaTypeHeaderValue(
            Path.GetExtension(route.FilePath).Equals(".json", StringComparison.OrdinalIgnoreCase)
                ? "application/json"
                : "application/octet-stream");
        return response;
    }
}
```

**Verhalten:**

| Anfrage | Fake-Verhalten |
|---|---|
| GET | Lokale JSON- oder Binärdatei zurückgeben |
| POST | Request-Body erfassen und konfigurierte Antwortdatei zurückgeben |
| PUT | Request-Body erfassen und konfigurierte Antwortdatei zurückgeben |
| Nicht registrierte Route | `404 Not Found` |
| Registrierter Fehlerstatus | Z. B. `401`, `404`, `500` |
| Abbruch | `CancellationToken` beim Start und beim Lesen des Request-Bodys berücksichtigen |

**Hinweis:** POST und PUT **simulieren** die Serverantwort, ändern aber keine lokalen Dateien. Das ist für reproduzierbare API-Tests oft erwünscht. Die Routenauswahl erfolgt nur anhand von Methode und URL-Pfad; Query-Parameter und Request-Body beeinflussen die Auswahl nicht. Relative Fixture-Pfade werden relativ zum aktuellen Arbeitsverzeichnis aufgelöst. Für Streaming-Tests wird der Response-Body aus einem lokalen `FileStream` gelesen.

## 4. Testdaten und Verwendung

```text
TestData/
├── api/
│   ├── profile.json
│   ├── login.json
│   └── profile-updated.json
└── courses/
    └── course_123.vg
```

### Routen registrieren

```csharp
using System.Net;
using System.Net.Http;
using Sachssoft.Engine.Testing.Networking;

var fake = new FakeHttpService();
fake.Register(HttpMethod.Get, "/api/profile", "TestData/api/profile.json");
fake.Register(HttpMethod.Post, "/api/login", "TestData/api/login.json");
fake.Register(HttpMethod.Put, "/api/profile", "TestData/api/profile-updated.json");
fake.Register(HttpMethod.Get, "/courses/123", "TestData/courses/course_123.vg");
fake.Register(HttpMethod.Get, "/api/unauthorized", status: HttpStatusCode.Unauthorized);
fake.Register(HttpMethod.Get, "/api/server-error", status: HttpStatusCode.InternalServerError);

IHttpService http = fake;
```

Falls das Beispiel als eigene Datei kompiliert wird, `using Sachssoft.Engine.Services.Networking;` ergänzen.

### GET lesen

```csharp
using var request = new HttpRequestMessage(HttpMethod.Get, "https://fake.local/api/profile");
using var response = await http.SendAsync(request);
response.EnsureSuccessStatusCode();
var json = await response.Content.ReadAsStringAsync();
```

### POST senden

```csharp
using var request = new HttpRequestMessage(HttpMethod.Post, "https://fake.local/api/login")
{
    Content = new StringContent("{\"username\":\"tester\"}", System.Text.Encoding.UTF8, "application/json")
};
using var response = await http.SendAsync(request);
response.EnsureSuccessStatusCode();
var json = await response.Content.ReadAsStringAsync();
```

### PUT senden

```csharp
using var request = new HttpRequestMessage(HttpMethod.Put, "https://fake.local/api/profile")
{
    Content = new StringContent("{\"nickname\":\"Player123\"}", System.Text.Encoding.UTF8, "application/json")
};
using var response = await http.SendAsync(request);
response.EnsureSuccessStatusCode();
```

### Gesendete Daten prüfen

```csharp
foreach (var sent in fake.Requests)
    Console.WriteLine($"{sent.Method} {sent.Path}: {sent.Body}");
```

## 5. Course-Download mit optionalem Fortschritt

`IHttpService` selbst benötigt **keinen** Progress-Parameter: Der aufrufende Course-Downloader liest den Response-Stream und meldet bei bekannter Größe die Prozentwerte. Ohne `Content-Length` meldet er `null` für einen unbestimmten Ladeindikator. Das Beispiel speichert erst in eine `.part`-Datei und übernimmt diese nach einem vollständigen Download in den Cache.

```csharp
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Sachssoft.Engine.Services.Networking;

namespace VarietyGolf.Courses;

/// <summary>
/// Downloads a course through <see cref="IHttpService"/> into a local cache file.
/// </summary>
public static class CourseDownloader
{
    /// <summary>
    /// Downloads a course and reports progress when requested.
    /// </summary>
    /// <param name="http">The HTTP service to use.</param>
    /// <param name="source">The course download URI.</param>
    /// <param name="destinationPath">The final local cache file path.</param>
    /// <param name="progress">Optional progress from 0 to 100, or null for unknown total size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes after the cache file has been written.</returns>
    public static async Task DownloadAsync(IHttpService http, Uri source, string destinationPath,
        IProgress<double?>? progress = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(http);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);

        using var request = new HttpRequestMessage(HttpMethod.Get, source);
        using var response = await http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var total = response.Content.Headers.ContentLength;
        progress?.Report(total.HasValue ? 0d : null);

        var partPath = destinationPath + ".part";
        try
        {
            await using (var input = await response.Content.ReadAsStreamAsync(cancellationToken))
            await using (var output = new FileStream(partPath, FileMode.Create, FileAccess.Write,
                FileShare.None, 81920, FileOptions.Asynchronous))
            {
                var buffer = new byte[81920];
                long received = 0;
                int count;
                while ((count = await input.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    await output.WriteAsync(buffer.AsMemory(0, count), cancellationToken);
                    received += count;
                    if (total is > 0)
                        progress?.Report(Math.Min(99.9d, received * 100d / total.Value));
                }
                await output.FlushAsync(cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
            File.Move(partPath, destinationPath, overwrite: true);
            progress?.Report(100d);
        }
        catch
        {
            if (File.Exists(partPath)) File.Delete(partPath);
            throw;
        }
    }
}
```

**Voraussetzung:** Das Zielverzeichnis existiert und Downloads auf denselben Zielpfad werden nicht parallel gestartet. Validierung (z. B. Dateihash/Format) kann vor `File.Move` ergänzt werden. `100 %` ist optional und wird erst nach vollständigem Schreiben und Übernehmen der Datei gemeldet. Die Weiterverarbeitung richtet sich nach dem erfolgreichen Abschluss des `Task`, nicht nach dem Progress-Event.

### Danach Course laden

```csharp
await CourseDownloader.DownloadAsync(http, courseUri, coursePath, progress, cancellationToken);
await LoadCourseAsync(coursePath, cancellationToken);
```

Wenn `progress` nicht übergeben wird, läuft der Download genauso und der Course wird nach Abschluss automatisch geladen. Bei Fehler oder Abbruch wird `LoadCourseAsync` nicht ausgeführt.

## 6. Empfehlung für Sasogine

- **Jetzt nur:** `Sachssoft.Engine.Services.Networking.IHttpService`.
- **Für Produktion:** Ein `HttpService` mit `HttpClient` und `ResponseHeadersRead`.
- **Für Tests:** Ein `FakeHttpService` mit lokalen Dateien, GET/POST/PUT und konfigurierbaren Statuscodes.
- **Für Variety Golf:** Ein Course-Downloader auf Basis von `IHttpService`; Download, Validierung, Cache und Gameplay-Laden bleiben getrennte Verantwortlichkeiten.
- **Native AOT:** Die gezeigten HTTP- und Dateistream-Operationen benötigen keine Reflection. Für typisierte JSON-Modelle ggf. `System.Text.Json` mit Source Generation verwenden.

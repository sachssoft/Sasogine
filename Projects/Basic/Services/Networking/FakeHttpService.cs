using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Sachssoft.Engine.Services.Networking;

namespace Sachssoft.Engine.Testing.Networking;

/// <summary>
/// Simulates HTTP requests using local response files without network access.
/// </summary>
public sealed class FakeHttpService : IHttpService
{
    private readonly Dictionary<(HttpMethod Method, string Path), (HttpStatusCode Status, string? File)> _routes = new();

    /// <summary>
    /// Gets the requests received by this service, including their body content.
    /// </summary>
    public ConcurrentQueue<(HttpMethod Method, string Path, string? Body)> Requests { get; } = new();

    /// <summary>
    /// Registers a simulated HTTP response for the specified method and path.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="path">The request path.</param>
    /// <param name="filePath">The local response file, or null for an empty response.</param>
    /// <param name="status">The HTTP response status code.</param>
    public void Register(HttpMethod method, string path, string? filePath = null,
        HttpStatusCode status = HttpStatusCode.OK)
    {
        _routes[(method, path)] = (status, filePath);
    }

    /// <inheritdoc />
    public async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var path = request.RequestUri?.AbsolutePath
            ?? throw new ArgumentException("An absolute request URI is required.", nameof(request));

        var body = request.Content is null
            ? null
            : await request.Content.ReadAsStringAsync(cancellationToken);

        Requests.Enqueue((request.Method, path, body));

        if (!_routes.TryGetValue((request.Method, path), out var route))
            return new HttpResponseMessage(HttpStatusCode.NotFound) { RequestMessage = request };

        var response = new HttpResponseMessage(route.Status) { RequestMessage = request };

        if (route.File is not null)
        {
            var stream = new FileStream(route.File, FileMode.Open, FileAccess.Read,
                FileShare.Read, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);

            response.Content = new StreamContent(stream);
            response.Content.Headers.ContentLength = stream.Length;
        }

        return response;
    }
}
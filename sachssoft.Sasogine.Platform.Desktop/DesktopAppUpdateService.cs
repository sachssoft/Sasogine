using sachssoft.Sasogine.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace sachssoft.Sasogine.Platform.Desktop;

/// <summary>
/// Desktop implementation of <see cref="IAppUpdateService"/> that checks a webpage for updates
/// and opens the update page with a provided web client service.
/// </summary>
public class DesktopAppUpdateService : IAppUpdateService
{
    private readonly HttpClient _httpClient;
    private const string UpdateUrl = "https://example.com/update";
    private const string UpdateMarker = "<!-- UPDATE_AVAILABLE -->";

    public bool HasPendingUpdate { get; private set; }

    private readonly Func<Task<bool>> _checkForUpdate;

    public DesktopAppUpdateService(HttpClient httpClient, Func<Task<bool>> checkForUpdate)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _checkForUpdate = checkForUpdate ?? throw new ArgumentNullException(nameof(checkForUpdate));
    }

    public async Task<bool> CheckForUpdatesAsync()
    {
        try
        {
            HasPendingUpdate = await _checkForUpdate();
            return HasPendingUpdate;
        }
        catch
        {
            HasPendingUpdate = false;
            return false;
        }
    }

    public async Task ApplyUpdatesAsync()
    {
        // Hier könntest du Logik ergänzen, die bei Update z.B. Download startet.
        await Task.CompletedTask;
    }

    public void OpenWebClient(IWebClientService webClient)
    {
        if (!HasPendingUpdate)
            return;

        if (webClient == null)
            throw new ArgumentNullException(nameof(webClient));

        webClient.Open(new Uri(UpdateUrl));
        HasPendingUpdate = false;
    }
}

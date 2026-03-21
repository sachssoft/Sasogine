using Sachssoft.Sasogine.Platform;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Services;

/// <summary>
/// Provides functionality to check for and apply application updates.
/// </summary>
public interface IAppUpdateService
{
    /// <summary>
    /// Checks whether an update is available, e.g., by retrieving a website.
    /// </summary>
    /// <returns>True if an update is available; otherwise, false.</returns>
    Task<bool> CheckForUpdatesAsync();

    /// <summary>
    /// Applies the update if one is available (e.g., by opening a website or downloading resources).
    /// </summary>
    Task ApplyUpdatesAsync();

    /// <summary>
    /// Gets a value indicating whether an update is currently available.
    /// </summary>
    bool HasPendingUpdate { get; }

    /// <summary>
    /// Opens the update page using the specified web client service.
    /// </summary>
    /// <param name="webClient">The web client service used to open the update page.</param>
    void OpenWebClient(IWebClient webClient);
}
using Sachssoft.Engine.Services.Platform;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sachssoft.Engine.Platform.Windows;

/// <summary>
/// Windows implementation of <see cref="IAppUpdateService"/> using an application-provided update check.
/// </summary>
public sealed class DesktopAppUpdate : IAppUpdateService
{
    private readonly Func<Task<bool>> _checkForUpdate;

    /// <inheritdoc />
    public bool HasPendingUpdate { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DesktopAppUpdate"/> class.
    /// </summary>
    /// <param name="httpClient">
    /// HTTP client reserved for the platform update workflow.
    /// </param>
    /// <param name="checkForUpdate">
    /// Application-provided callback that checks whether an update is available.
    /// </param>
    public DesktopAppUpdate(HttpClient httpClient, Func<Task<bool>> checkForUpdate)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _checkForUpdate = checkForUpdate ?? throw new ArgumentNullException(nameof(checkForUpdate));
    }

    /// <inheritdoc />
    public async Task<bool> CheckForUpdatesAsync()
    {
        try
        {
            HasPendingUpdate = await _checkForUpdate().ConfigureAwait(false);
            return HasPendingUpdate;
        }
        catch
        {
            HasPendingUpdate = false;
            return false;
        }
    }

    /// <inheritdoc />
    public Task ApplyUpdatesAsync()
    {
        // The actual update mechanism is application-specific.
        return Task.CompletedTask;
    }
}

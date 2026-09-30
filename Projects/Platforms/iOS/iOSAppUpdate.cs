using Sachssoft.Engine.Services.Platform;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sachssoft.Engine.Platform.iOS;

/// <summary>
/// iOS implementation of <see cref="IAppUpdateService"/> using an application-provided update check.
/// </summary>
public sealed class iOSAppUpdate : IAppUpdateService
{
    private readonly Func<Task<bool>> _checkForUpdate;

    /// <inheritdoc />
    public bool HasPendingUpdate { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="iOSAppUpdate"/> class.
    /// </summary>
    public iOSAppUpdate(HttpClient httpClient, Func<Task<bool>> checkForUpdate)
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
    public Task ApplyUpdatesAsync() => Task.CompletedTask;
}

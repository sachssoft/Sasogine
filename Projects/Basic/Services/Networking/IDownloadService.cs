using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sachssoft.Engine.Services.Networking;

/// <summary>
/// Provides asynchronous file downloads with optional progress reporting and cancellation.
/// </summary>
public interface IDownloadService
{
    /// <summary>
    /// Downloads a resource to the specified local file path.
    /// </summary>
    /// <param name="source">The URI of the resource to download.</param>
    /// <param name="destinationPath">The local destination file path.</param>
    /// <param name="progress">
    /// Reports download progress from 0 to 100, or null when the total size is unknown.
    /// </param>
    /// <param name="cancellationToken">Cancels the download operation.</param>
    /// <returns>A task representing the download operation.</returns>
    Task DownloadAsync(
        Uri source,
        string destinationPath,
        IProgress<double?>? progress = null,
        CancellationToken cancellationToken = default);
}
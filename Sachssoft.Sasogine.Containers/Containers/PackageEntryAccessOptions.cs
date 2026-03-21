using System;
using System.Threading;

namespace Sachssoft.Sasogine.Containers;

/// <summary>
/// Options for writing a package entry into a ZIP archive or other package stream.
/// Provides control over buffer size, write mode, progress reporting, and cancellation.
/// </summary>
public class PackageEntryAccessOptions
{
    /// <summary>
    /// Gets or sets the buffer size in bytes used during writing.
    /// Larger buffers may improve performance but use more RAM.
    /// Default is 81920 bytes (80 KB), which is usually a good trade-off.
    /// </summary>
    public int BufferSize { get; set; } = 81920;

    /// <summary>
    /// Gets or sets the write mode for the entry.
    /// Determines how the data is temporarily stored before writing to the package:
    /// <list type="bullet">
    /// <item><description><see cref="PackageEntryAccessMode.Direct"/>: Writes directly to the package stream. Minimal RAM and disk usage.</description></item>
    /// <item><description><see cref="PackageEntryAccessMode.TemporaryFile"/>: Writes to a temporary file first to save RAM, then copies into the package. Uses disk I/O.</description></item>
    /// <item><description><see cref="PackageEntryAccessMode.MemoryOnly"/>: Stores everything in memory before writing. Fast but RAM-intensive.</description></item>
    /// </list>
    /// </summary>
    public PackageEntryAccessMode Mode { get; set; } = PackageEntryAccessMode.MemoryOnly;

    /// <summary>
    /// Optional progress reporter that receives updates as a double from 0.0 (0%) to 1.0 (100%).
    /// Useful for displaying progress to the user during large writes.
    /// </summary>
    public IProgress<double>? Progress { get; set; } = null;

    /// <summary>
    /// Optional cancellation token that can be used to abort the write operation.
    /// The token is checked periodically during writing.
    /// </summary>
    public CancellationToken CancellationToken { get; set; } = default;
}

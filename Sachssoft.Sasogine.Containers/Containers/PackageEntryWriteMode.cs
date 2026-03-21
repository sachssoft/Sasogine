
namespace Sachssoft.Sasogine.Containers
{
    /// <summary>
    /// Defines how data is written into a package entry.
    /// </summary>
    public enum PackageEntryAccessMode
    {
        /// <summary>
        /// Writes data directly from the source stream into the package entry.
        /// Minimal RAM and disk usage.
        /// Recommended for most scenarios and medium-to-large files.
        /// </summary>
        Direct,

        /// <summary>
        /// Writes data first to a temporary file on disk, then copies it into the package entry.
        /// Minimizes RAM usage, but involves extra disk I/O.
        /// Useful for very large files or memory-constrained environments.
        /// </summary>
        TemporaryFile,

        /// <summary>
        /// Stores all data in memory (MemoryStream) before writing it to the package entry.
        /// Fast for small files but highly RAM-intensive.
        /// </summary>
        MemoryOnly
    }
}

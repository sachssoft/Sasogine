using System.Collections.Generic;
using System.IO;

namespace Sachssoft.Engine.Resources;

/// <summary>
/// Defines a resource source that supports file synchronization.
/// </summary>
public interface IFileSynchronizationSource
{
    /// <summary>
    /// Gets whether the source is read-only.
    /// </summary>
    bool IsReadOnly { get; }

    /// <summary>
    /// Determines whether the specified file exists.
    /// </summary>
    bool FileExists(string filePath);

    /// <summary>
    /// Determines whether the specified directory exists.
    /// </summary>
    bool DirectoryExists(string path);

    /// <summary>
    /// Gets the files contained in the specified path.
    /// </summary>
    /// <param name="path">
    /// The relative resource path.
    /// </param>
    /// <param name="searchPattern">
    /// The search pattern used to filter files.
    /// </param>
    /// <param name="recursive">
    /// Whether files in subdirectories are included.
    /// </param>
    /// <returns>
    /// The relative resource file paths.
    /// </returns>
    IEnumerable<string> GetFiles(
        string path,
        string searchPattern = "*",
        bool recursive = true);

    /// <summary>
    /// Opens a stream for the specified file.
    /// </summary>
    Stream OpenStream(string filePath);

    /// <summary>
    /// Synchronizes the specified resource path.
    /// </summary>
    void Synchronize(string path);
}
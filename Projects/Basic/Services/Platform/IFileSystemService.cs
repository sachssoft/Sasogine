using System.IO;

namespace Sachssoft.Sasogine.Services.Platform;

/// <summary>
/// Platform-independent access to file system functions.
/// </summary>
public interface IFileSystemService
{
    /// <summary>
    /// Returns a special system directory, such as cache, app data, or user folder.
    /// </summary>
    /// <param name="directory">The type of special directory to retrieve.</param>
    /// <returns>The path to the requested directory.</returns>
    string GetSpecialDirectory(SpecialDirectories directory);

    /// <summary>
    /// Combines multiple path components into a valid path.
    /// </summary>
    /// <param name="path">The first path component.</param>
    /// <param name="paths">Additional path components.</param>
    /// <returns>A combined valid path string.</returns>
    string CombinePath(string path, params string[] paths);

    /// <summary>
    /// Checks whether a directory exists.
    /// </summary>
    /// <param name="path">The path of the directory to check.</param>
    /// <returns><c>true</c> if the directory exists; otherwise, <c>false</c>.</returns>
    bool IsDirectoryExists(string path);

    /// <summary>
    /// Checks whether a file exists.
    /// </summary>
    /// <param name="filePath">The full path to the file.</param>
    /// <returns><c>true</c> if the file exists; otherwise, <c>false</c>.</returns>
    bool IsFileExists(string filePath);

    /// <summary>
    /// Creates a new directory if it does not already exist.
    /// </summary>
    /// <param name="path">The path of the directory to create.</param>
    void CreateDirectory(string path);

    /// <summary>
    /// Deletes a directory if it exists.
    /// </summary>
    /// <param name="path">The path of the directory to delete.</param>
    void DeleteDirectory(string path);

    /// <summary>
    /// Deletes a file if it exists.
    /// </summary>
    /// <param name="filePath">The path of the file to delete.</param>
    void DeleteFile(string filePath);

    /// <summary>
    /// Creates or overwrites a file and returns a writable stream.
    /// </summary>
    /// <param name="filePath">The full path of the file to create.</param>
    /// <returns>A writable stream to the created file.</returns>
    Stream CreateFile(string filePath);

    /// <summary>
    /// Opens an existing file for reading and returns a read-only stream.
    /// </summary>
    /// <param name="filePath">The path of the file to open.</param>
    /// <returns>A readable Stream for the file.</returns>
    Stream OpenFile(string filePath);
}

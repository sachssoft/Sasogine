using System;
using System.Collections.Generic;
using System.IO;

namespace Sachssoft.Engine.Resources;

/// <summary>
/// Provides file synchronization access to a local directory.
/// </summary>
public sealed class LocalFileSynchronizationSource :
    IFileSynchronizationSource
{
    private readonly string _rootPath;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="LocalFileSynchronizationSource"/> class.
    /// </summary>
    /// <param name="rootPath">
    /// The root directory of the synchronization source.
    /// </param>
    public LocalFileSynchronizationSource(string rootPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);

        _rootPath = Path.GetFullPath(rootPath);
    }

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <summary>
    /// Gets the root directory of the synchronization source.
    /// </summary>
    public string RootPath => _rootPath;

    /// <inheritdoc />
    public bool FileExists(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        return File.Exists(GetFullPath(filePath));
    }

    /// <inheritdoc />
    public bool DirectoryExists(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return Directory.Exists(GetFullPath(path));
    }

    /// <inheritdoc />
    public IEnumerable<string> GetFiles(
        string path,
        string searchPattern = "*",
        bool recursive = true)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);

        var directoryPath = GetFullPath(path);

        if (!Directory.Exists(directoryPath))
            yield break;

        var searchOption = recursive
            ? SearchOption.AllDirectories
            : SearchOption.TopDirectoryOnly;

        foreach (var filePath in Directory.EnumerateFiles(
            directoryPath,
            searchPattern,
            searchOption))
        {
            yield return NormalizePath(
                Path.GetRelativePath(_rootPath, filePath));
        }
    }

    /// <inheritdoc />
    public Stream OpenStream(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        return File.OpenRead(
            GetFullPath(filePath));
    }

    /// <inheritdoc />
    public void Synchronize(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var directoryPath = GetFullPath(path);

        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);
    }

    private string GetFullPath(string path)
    {
        var normalizedPath = path
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);

        var fullPath = Path.GetFullPath(
            Path.Combine(_rootPath, normalizedPath));

        var relativePath = Path.GetRelativePath(
            _rootPath,
            fullPath);

        if (relativePath == ".." ||
            relativePath.StartsWith(
                $"..{Path.DirectorySeparatorChar}",
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "The path must be located within the synchronization root.",
                nameof(path));
        }

        return fullPath;
    }

    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/');
    }
}
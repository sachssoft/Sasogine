using Sachssoft.Engine.Services.Platform;
using System;
using System.IO;

namespace Sachssoft.Engine.Platform.Windows;

/// <summary>
/// Windows implementation of <see cref="IFileSystemService"/>.
/// </summary>
public sealed class WindowsFileSystem : IFileSystemService
{
    /// <inheritdoc />
    public string CombinePath(string path, params string[] paths)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(paths);

        var result = path;
        foreach (var part in paths)
            result = Path.Combine(result, part);

        return result;
    }

    /// <inheritdoc />
    public string GetSpecialDirectory(SpecialDirectories directory)
    {
        return directory switch
        {
            SpecialDirectories.Cache => Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Cache"),

            SpecialDirectories.Temporary => Path.GetTempPath(),

            SpecialDirectories.Application =>
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),

            SpecialDirectories.User =>
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),

            _ => throw new ArgumentOutOfRangeException(nameof(directory), directory, null)
        };
    }

    /// <inheritdoc />
    public bool IsDirectoryExists(string path) => Directory.Exists(path);

    /// <inheritdoc />
    public bool IsFileExists(string filePath) => File.Exists(filePath);

    /// <inheritdoc />
    public void CreateDirectory(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        Directory.CreateDirectory(path);
    }

    /// <inheritdoc />
    public void DeleteDirectory(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }

    /// <inheritdoc />
    public void DeleteFile(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    /// <inheritdoc />
    public Stream CreateFile(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        return new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
    }

    /// <inheritdoc />
    public Stream OpenFile(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}", filePath);

        return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }
}

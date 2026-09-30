using Foundation;
using Sachssoft.Engine.Services.Platform;
using System;
using System.IO;

namespace Sachssoft.Engine.Platform.iOS;

/// <summary>iOS implementation of <see cref="IFileSystemService"/>.</summary>
public sealed class iOSFileSystem : IFileSystemService
{
    public string CombinePath(string path, params string[] paths)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(paths);
        var result = path;
        foreach (var part in paths)
            result = Path.Combine(result, part);
        return result;
    }

    public string GetSpecialDirectory(SpecialDirectories directory)
    {
        var home = NSHomeDirectory();
        return directory switch
        {
            SpecialDirectories.Cache => Path.Combine(home, "Library", "Caches"),
            SpecialDirectories.Temporary => Path.GetTempPath(),
            SpecialDirectories.Application => Path.Combine(home, "Library", "Application Support"),
            SpecialDirectories.User => Path.Combine(home, "Documents"),
            _ => throw new ArgumentOutOfRangeException(nameof(directory), directory, null)
        };
    }

    public bool IsDirectoryExists(string path) => Directory.Exists(path);
    public bool IsFileExists(string filePath) => File.Exists(filePath);
    public void CreateDirectory(string path) { ArgumentException.ThrowIfNullOrWhiteSpace(path); Directory.CreateDirectory(path); }
    public void DeleteDirectory(string path) { ArgumentException.ThrowIfNullOrWhiteSpace(path); if (Directory.Exists(path)) Directory.Delete(path, true); }
    public void DeleteFile(string filePath) { ArgumentException.ThrowIfNullOrWhiteSpace(filePath); if (File.Exists(filePath)) File.Delete(filePath); }
    public Stream CreateFile(string filePath) { ArgumentException.ThrowIfNullOrWhiteSpace(filePath); return new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None); }
    public Stream OpenFile(string filePath) { ArgumentException.ThrowIfNullOrWhiteSpace(filePath); if (!File.Exists(filePath)) throw new FileNotFoundException($"File not found: {filePath}", filePath); return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read); }
}

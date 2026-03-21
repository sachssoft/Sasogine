using System;
using System.IO;
using Sachssoft.Sasogine.Services;

namespace Sachssoft.Sasogine.Platform.Desktop;

public class DesktopFileSystemService : IFileSystemService
{
    /// <summary>
    /// Kombiniert mehrere Pfadbestandteile zu einem gültigen Pfad.
    /// </summary>
    public string CombinePath(string path, params string[] paths)
    {
        return Path.Combine(path, Path.Combine(paths));
    }

    /// <summary>
    /// Gibt ein spezielles Systemverzeichnis zurück, z. B. Cache, App-Daten, Benutzerordner.
    /// </summary>
    public string GetSpecialDirectory(SpecialDirectories directory)
    {
        switch (directory)
        {
            case SpecialDirectories.Cache:
                // Lokaler AppData-Cache (Windows/macOS/Linux)
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Cache");

            case SpecialDirectories.Temporary:
                // Temporäres Systemverzeichnis (z. B. %TEMP%)
                return Path.GetTempPath();

            case SpecialDirectories.Application:
                // App-spezifisches Verzeichnis für Settings etc.
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            case SpecialDirectories.User:
                // Benutzerordner
                return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            default:
                throw new ArgumentOutOfRangeException(nameof(directory), directory, null);
        }
    }

    /// <summary>
    /// Prüft, ob ein Verzeichnis existiert.
    /// </summary>
    public bool IsDirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    /// <summary>
    /// Prüft, ob eine Datei existiert.
    /// </summary>
    public bool IsFileExists(string filePath)
    {
        return File.Exists(filePath);
    }

    /// <summary>
    /// Erstellt ein Verzeichnis falls es nicht existiert.
    /// </summary>
    public void CreateDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    /// <summary>
    /// Löscht ein Verzeichnis rekursiv, falls es existiert.
    /// </summary>
    public void DeleteDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }

    /// <summary>
    /// Löscht eine Datei, falls sie existiert.
    /// </summary>
    public void DeleteFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    public Stream CreateFile(string filePath)
    {
        // Erstelle oder überschreibe die Datei und öffne einen Schreibstream
        return new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
    }

    public Stream OpenFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }
}

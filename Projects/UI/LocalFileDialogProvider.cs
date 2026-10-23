using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sachssoft.Engine.Controls;
/// <summary>
/// Reads the local filesystem in explicit, caller-requested operations. Never writes files.
/// </summary>
public sealed class LocalFileDialogProvider : IFileDialogProvider
{
    /// <summary>
    /// Performs read directory using the current control state.
    /// </summary>
    public FileDialogDirectory ReadDirectory(string path)
    {
        string full = Path.GetFullPath(path);
        var entries = new List<FileDialogEntry>();
        foreach (string item in Directory.EnumerateFileSystemEntries(full))
        {
            try
            {
                bool directory = (File.GetAttributes(item) & FileAttributes.Directory) != 0;
                entries.Add(new FileDialogEntry(Path.GetFileName(item), item, directory));
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        FileDialogEntry[] sorted = entries.OrderByDescending(e => e.IsDirectory).ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase).ThenBy(e => e.Name, StringComparer.Ordinal).ToArray();
        return new FileDialogDirectory(full, Directory.GetParent(full)?.FullName, sorted);
    }

    /// <summary>
    /// Performs resolve using the current control state.
    /// </summary>
    public string Resolve(string directory, string name) => Path.GetFullPath(Path.IsPathRooted(name) ? name : Path.Combine(directory, name));
    /// <summary>
    /// Performs exists using the current control state.
    /// </summary>
    public bool Exists(string path, bool directory) => directory ? Directory.Exists(path) : File.Exists(path);
}

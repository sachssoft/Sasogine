using Sachssoft.Engine.Services.Platform;
using System;
using System.Diagnostics;
using System.IO;

namespace Sachssoft.Engine.Platform.Windows;

/// <summary>
/// Windows implementation of <see cref="ILauncherService"/>.
/// </summary>
public sealed class WindowsLauncher : ILauncherService
{
    /// <inheritdoc />
    public bool TryReveal(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path) ||
                (!File.Exists(path) && !Directory.Exists(path)))
            {
                return false;
            }

            var argument = File.Exists(path)
                ? $"/select,\"{Path.GetFullPath(path)}\""
                : $"\"{Path.GetFullPath(path)}\"";

            Process.Start(new ProcessStartInfo("explorer.exe", argument)
            {
                UseShellExecute = true
            });

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public bool TryOpenUri(Uri uri)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(uri);

            if (!uri.IsAbsoluteUri)
                return false;

            Process.Start(new ProcessStartInfo(uri.AbsoluteUri)
            {
                UseShellExecute = true
            });

            return true;
        }
        catch
        {
            return false;
        }
    }
}

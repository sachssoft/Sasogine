using System;
using System.Diagnostics;
using System.IO;
using Sachssoft.Sasogine.Platform;

namespace Sachssoft.Sasogine.Platform.Windows
{
    /// <summary>
    /// Windows implementation of ILauncherService.
    /// Opens files/folders in Explorer and URLs in default browser.
    /// </summary>
    public class WindowsLauncher : ILauncherService
    {
        public bool TryReveal(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path) || !File.Exists(path) && !Directory.Exists(path))
                    return false;

                // Explorer argument: /select,"fullpath"
                var argument = File.Exists(path)
                    ? $"/select,\"{path}\""
                    : $"\"{path}\""; // folder path just open

                Process.Start(new ProcessStartInfo("explorer.exe", argument) { UseShellExecute = true });
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TryOpenUri(Uri uri)
        {
            try
            {
                if (uri == null || !uri.IsAbsoluteUri)
                    return false;

                Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
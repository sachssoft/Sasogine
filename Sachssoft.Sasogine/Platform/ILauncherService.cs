using System;

namespace Sachssoft.Sasogine.Platform
{
    /// <summary>
    /// Platform service to open URLs in the browser or reveal files in the OS explorer.
    /// Try-methods return false if the operation fails (e.g., path not found or unsupported on platform).
    /// </summary>
    public interface ILauncherService
    {
        /// <summary>
        /// Reveals a file or folder in the OS file explorer (selects/highlights it if supported).
        /// </summary>
        /// <param name="path">Full path to the file or folder.</param>
        /// <returns>True if operation succeeded; false if not supported or path not found.</returns>
        bool TryReveal(string path);

        /// <summary>
        /// Opens a URL in the default web browser.
        /// </summary>
        /// <param name="uri">The URL to open.</param>
        /// <returns>True if operation succeeded; false if not supported or invalid URL.</returns>
        bool TryOpenUri(Uri uri);
    }
}
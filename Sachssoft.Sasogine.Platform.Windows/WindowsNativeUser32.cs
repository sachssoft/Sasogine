using System;
using System.Runtime.InteropServices;

namespace Sachssoft.Sasogine.Platform.Windows
{
    /// <summary>
    /// Native Windows API (User32 / Shcore) wrapper.
    /// Encapsulates functions for DPI detection and monitor information.
    /// </summary>
    internal static class WindowsNative
    {
        /// <summary>
        /// DPI type for GetDpiForMonitor.
        /// MDT_EFFECTIVE_DPI: Recommended DPI for layouts/UI.
        /// </summary>
        // DPI-Typ für GetDpiForMonitor
        // MDT_EFFECTIVE_DPI: Empfohlene DPI für Layouts/UI
        internal const int MDT_EFFECTIVE_DPI = 0;

        /// <summary>
        /// Monitor selection if window is not on a monitor:
        /// MONITOR_DEFAULTTONEAREST: Chooses the nearest monitor.
        /// </summary>
        // Monitor-Auswahl, falls Fenster auf keinem Monitor liegt
        // MONITOR_DEFAULTTONEAREST: Nähster Monitor wird gewählt
        internal const uint MONITOR_DEFAULTTONEAREST = 2;

        /// <summary>
        /// Gets the horizontal and vertical DPI of a monitor (Windows API via Shcore.dll).
        /// </summary>
        /// <param name="hMonitor">Monitor handle.</param>
        /// <param name="dpiType">DPI type (e.g., MDT_EFFECTIVE_DPI).</param>
        /// <param name="dpiX">Horizontal DPI (output).</param>
        /// <param name="dpiY">Vertical DPI (output).</param>
        /// <returns>Windows result code (HRESULT).</returns>
        // Ermittelt die horizontale und vertikale DPI eines Monitors
        // Shcore.dll API
        [DllImport("Shcore.dll")]
        internal static extern int GetDpiForMonitor(IntPtr hMonitor, int dpiType, out uint dpiX, out uint dpiY);

        /// <summary>
        /// Returns the handle of the monitor that contains the given window (User32.dll).
        /// </summary>
        /// <param name="hwnd">Window handle.</param>
        /// <param name="dwFlags">
        /// MONITOR_DEFAULTTONEAREST, MONITOR_DEFAULTTOPRIMARY, MONITOR_DEFAULTTONULL
        /// Fallback if window is not on a monitor.
        /// </param>
        /// <returns>Monitor handle.</returns>
        // Gibt das Handle des Monitors zurück, auf dem das Fenster liegt
        // User32.dll API
        [DllImport("user32.dll")]
        internal static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);
    }
}
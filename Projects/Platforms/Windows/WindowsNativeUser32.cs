using System;
using System.Runtime.InteropServices;

namespace Sachssoft.Engine.Platform.Windows;

/// <summary>
/// Native Windows API wrapper used by Windows platform services.
/// </summary>
internal static class WindowsNative
{
    /// <summary>
    /// Effective DPI used for layout and UI scaling.
    /// </summary>
    internal const int MdtEffectiveDpi = 0;

    /// <summary>
    /// Selects the nearest monitor when a window is not currently located on one.
    /// </summary>
    internal const uint MonitorDefaultToNearest = 2;

    [DllImport("Shcore.dll")]
    internal static extern int GetDpiForMonitor(
        IntPtr hMonitor,
        int dpiType,
        out uint dpiX,
        out uint dpiY);

    [DllImport("user32.dll")]
    internal static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint flags);
}

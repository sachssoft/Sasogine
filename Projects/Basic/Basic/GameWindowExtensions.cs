using Microsoft.Xna.Framework;
using System;
using System.Runtime.InteropServices;

namespace Sachssoft.Sasogine;

/// <summary>
/// Provides extensions for <see cref="GameWindow"/>.
/// </summary>
public static class GameWindowExtensions
{
    /// <summary>
    /// Maximizes the game window.
    /// </summary>
    /// <param name="window">
    /// The game window to maximize.
    /// </param>
    /// <exception cref="PlatformNotSupportedException">
    /// Thrown when the current platform does not support the operation.
    /// </exception>
    public static void Maximize(this GameWindow window)
    {
        ArgumentNullException.ThrowIfNull(window);

        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException(
                "Maximizing a GameWindow is currently only supported on Windows.");

        if (window.Handle == IntPtr.Zero)
            throw new InvalidOperationException(
                "The game window handle is not available.");

        ShowWindow(window.Handle, SW_MAXIMIZE);
    }

    private const int SW_MAXIMIZE = 3;

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(
        IntPtr hWnd,
        int nCmdShow);
}
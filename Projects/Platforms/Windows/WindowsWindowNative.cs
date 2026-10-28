using Microsoft.Xna.Framework;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Sachssoft.Engine.Platform.Windows;

/// <summary>Provides Win32 window operations and resolves SDL2 window handles without reflection.</summary>
internal static class WindowsWindowNative
{
    /// <summary>Identifies the standard window style value.</summary>
    internal const int GwlStyle = -16;

    /// <summary>Requests maximization.</summary>
    internal const int SwMaximize = 3;

    /// <summary>Requests minimization.</summary>
    internal const int SwMinimize = 6;

    /// <summary>Requests restoration of normal window bounds.</summary>
    internal const int SwRestore = 9;

    /// <summary>Identifies the single window-policy owner attached to an HWND.</summary>
    internal const string OwnerProperty = "Sasogine.Platform.WindowOwner";

    /// <summary>Indicates that a system menu item is enabled.</summary>
    internal const uint MfEnabled = 0;

    /// <summary>Indicates that a system menu item is disabled and grayed.</summary>
    internal const uint MfGrayed = 1;

    /// <summary>Identifies the system close command.</summary>
    internal const uint ScClose = 0xF060;

    /// <summary>Identifies the system maximize command.</summary>
    internal const uint ScMaximize = 0xF030;

    /// <summary>Identifies the system resize command.</summary>
    internal const uint ScSize = 0xF000;

    /// <summary>Refreshes the nonclient frame without moving, resizing, reordering, or activating the window.</summary>
    internal const uint SwpFrameChanged = 0x0037;

    /// <summary>Identifies a native close request.</summary>
    internal const uint WmClose = 0x0010;

    /// <summary>Identifies final destruction of the native window.</summary>
    internal const uint WmNcDestroy = 0x0082;

    /// <summary>Identifies a system menu command.</summary>
    internal const uint WmSysCommand = 0x0112;

    /// <summary>Enables the maximize title-bar button.</summary>
    internal const uint WsMaximizeBox = 0x00010000;

    /// <summary>Enables the window's sizing frame.</summary>
    internal const uint WsThickFrame = 0x00040000;

    /// <summary>Resolves either a direct HWND or an initialized DesktopGL SDL2 window.</summary>
    /// <param name="game">The game whose window must be resolved.</param>
    /// <returns>The actual Win32 HWND.</returns>
    /// <exception cref="ArgumentNullException">The game is null.</exception>
    /// <exception cref="InvalidOperationException">The game window is not initialized.</exception>
    /// <exception cref="PlatformNotSupportedException">The backend requires an explicit HWND.</exception>
    internal static IntPtr ResolveHandle(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);
        IntPtr handle = game.Window.Handle;
        if (handle == IntPtr.Zero)
            throw new InvalidOperationException("Initialize the game window before creating its platform service.");

        if (IsWindow(handle))
            return handle;

        // GameWindow.Handle is an SDL_Window pointer for DesktopGL, never an HWND.
        if (GetModuleHandle("SDL2.dll") != IntPtr.Zero)
        {
            SdlGetVersion(out WindowsSdlVersion version);
            if (version.Major == 2)
            {
                var information = new WindowsSdlWindowInfo { Version = version };
                if (SdlGetWindowInfo(handle, ref information) != 0 && information.Subsystem == 1 && IsWindow(information.Window))
                    return information.Window;
            }
        }

        throw new PlatformNotSupportedException("This backend requires the WindowsWindow constructor accepting an explicit native HWND.");
    }

    /// <summary>Enables or grays the close menu entry while retaining the system title bar.</summary>
    /// <param name="window">The native HWND.</param>
    /// <param name="state">The enabled, disabled, or grayed menu state.</param>
    internal static void SetCloseState(IntPtr window, uint state)
    {
        IntPtr menu = GetSystemMenu(window, false);
        if (menu == IntPtr.Zero)
            return;

        if (EnableMenuItem(menu, ScClose, state) == uint.MaxValue)
            throw new InvalidOperationException("The window has no close menu entry.");

        DrawMenuBar(window);
    }

    /// <summary>Applies sizing and maximize-button style bits without replacing unrelated styles.</summary>
    /// <param name="window">The native HWND.</param>
    /// <param name="bits">The desired sizing and maximize-button bits.</param>
    internal static void SetResizeStyle(IntPtr window, uint bits)
    {
        uint mask = WsThickFrame | WsMaximizeBox;
        uint style = unchecked((uint)GetWindowLong(window, GwlStyle));
        uint updated = (style & ~mask) | (bits & mask);
        if (updated == style)
            return;

        Marshal.SetLastPInvokeError(0);
        if (SetWindowLong(window, GwlStyle, unchecked((int)updated)) == 0 && Marshal.GetLastPInvokeError() != 0)
            throw new Win32Exception(Marshal.GetLastPInvokeError());

        if (!SetWindowPos(window, IntPtr.Zero, 0, 0, 0, 0, SwpFrameChanged))
            throw new Win32Exception(Marshal.GetLastPInvokeError());
    }

    /// <summary>Calls the native IsWindow window API.</summary>
    /// <param name="window">The native HWND.</param>
    /// <returns>Whether the handle identifies an existing window.</returns>
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool IsWindow(IntPtr window);

    /// <summary>Calls the native IsIconic window API.</summary>
    /// <param name="window">The native HWND.</param>
    /// <returns>Whether the window is minimized.</returns>
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool IsIconic(IntPtr window);

    /// <summary>Calls the native IsZoomed window API.</summary>
    /// <param name="window">The native HWND.</param>
    /// <returns>Whether the window is maximized.</returns>
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool IsZoomed(IntPtr window);

    /// <summary>Calls the native ShowWindow window API.</summary>
    /// <param name="window">The native HWND.</param>
    /// <param name="command">The requested show command.</param>
    /// <returns>Whether the window was previously visible.</returns>
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool ShowWindow(IntPtr window, int command);

    /// <summary>Calls the native GetWindowThreadProcessId window API.</summary>
    /// <param name="window">The native HWND.</param>
    /// <param name="processId">Receives the owning process identifier.</param>
    /// <returns>The owning native thread identifier.</returns>
    [DllImport("user32.dll", SetLastError = true)]
    internal static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);

    /// <summary>Calls the native GetCurrentThreadId window API.</summary>
    /// <returns>The calling native thread identifier.</returns>
    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern uint GetCurrentThreadId();

    /// <summary>Calls the native GetSystemMenu window API.</summary>
    /// <param name="window">The native HWND.</param>
    /// <param name="revert">Whether to restore the default menu.</param>
    /// <returns>The system menu handle.</returns>
    [DllImport("user32.dll", SetLastError = true)]
    internal static extern IntPtr GetSystemMenu(IntPtr window, [MarshalAs(UnmanagedType.Bool)] bool revert);

    /// <summary>Calls the native GetMenuState window API.</summary>
    /// <param name="menu">The menu handle.</param>
    /// <param name="item">The menu item identifier.</param>
    /// <param name="flags">The menu lookup flags.</param>
    /// <returns>The item state, or the unsigned minus-one value on failure.</returns>
    [DllImport("user32.dll", SetLastError = true)]
    internal static extern uint GetMenuState(IntPtr menu, uint item, uint flags);

    /// <summary>Calls the native EnableMenuItem window API.</summary>
    /// <param name="menu">The menu handle.</param>
    /// <param name="item">The menu item identifier.</param>
    /// <param name="flags">The requested menu state.</param>
    /// <returns>The previous state, or the unsigned minus-one value on failure.</returns>
    [DllImport("user32.dll", SetLastError = true)]
    internal static extern uint EnableMenuItem(IntPtr menu, uint item, uint flags);

    /// <summary>Calls the native DrawMenuBar window API.</summary>
    /// <param name="window">The native HWND.</param>
    /// <returns>Whether the menu was redrawn.</returns>
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool DrawMenuBar(IntPtr window);

    /// <summary>Calls the native GetWindowLongW window API.</summary>
    /// <param name="window">The native HWND.</param>
    /// <param name="index">The window value index.</param>
    /// <returns>The requested 32-bit window value.</returns>
    [DllImport("user32.dll", SetLastError = true, EntryPoint = "GetWindowLongW", CharSet = CharSet.Unicode, ExactSpelling = true)]
    internal static extern int GetWindowLong(IntPtr window, int index);

    /// <summary>Calls the native SetWindowLongW window API.</summary>
    /// <param name="window">The native HWND.</param>
    /// <param name="index">The window value index.</param>
    /// <param name="value">The new 32-bit window value.</param>
    /// <returns>The previous value, or zero on failure.</returns>
    [DllImport("user32.dll", SetLastError = true, EntryPoint = "SetWindowLongW", CharSet = CharSet.Unicode, ExactSpelling = true)]
    internal static extern int SetWindowLong(IntPtr window, int index, int value);

    /// <summary>Calls the native SetWindowPos window API.</summary>
    /// <param name="window">The native HWND.</param>
    /// <param name="insertAfter">The window ordering handle.</param>
    /// <param name="x">The requested horizontal coordinate.</param>
    /// <param name="y">The requested vertical coordinate.</param>
    /// <param name="width">The requested width.</param>
    /// <param name="height">The requested height.</param>
    /// <param name="flags">The position update flags.</param>
    /// <returns>Whether the update succeeded.</returns>
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool SetWindowPos(IntPtr window, IntPtr insertAfter, int x, int y, int width, int height, uint flags);

    /// <summary>Calls the native GetModuleHandleW window API.</summary>
    /// <param name="moduleName">The name of an already loaded library.</param>
    /// <returns>The loaded module handle, or zero.</returns>
    [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "GetModuleHandleW", CharSet = CharSet.Unicode, ExactSpelling = true)]
    internal static extern IntPtr GetModuleHandle(string moduleName);

    /// <summary>Calls the native GetPropW window API.</summary>
    /// <param name="window">The native HWND.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The property value, or zero.</returns>
    [DllImport("user32.dll", SetLastError = true, EntryPoint = "GetPropW", CharSet = CharSet.Unicode, ExactSpelling = true)]
    internal static extern IntPtr GetProp(IntPtr window, string name);

    /// <summary>Calls the native SetPropW window API.</summary>
    /// <param name="window">The native HWND.</param>
    /// <param name="name">The property name.</param>
    /// <param name="value">The property value.</param>
    /// <returns>Whether the property was set.</returns>
    [DllImport("user32.dll", SetLastError = true, EntryPoint = "SetPropW", CharSet = CharSet.Unicode, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool SetProp(IntPtr window, string name, IntPtr value);

    /// <summary>Calls the native RemovePropW window API.</summary>
    /// <param name="window">The native HWND.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The removed property value, or zero.</returns>
    [DllImport("user32.dll", SetLastError = true, EntryPoint = "RemovePropW", CharSet = CharSet.Unicode, ExactSpelling = true)]
    internal static extern IntPtr RemoveProp(IntPtr window, string name);

    /// <summary>Calls the native SetWindowSubclass window API.</summary>
    /// <param name="window">The native HWND.</param>
    /// <param name="procedure">The managed subclass callback.</param>
    /// <param name="id">The unique subclass identifier.</param>
    /// <param name="data">The native reference data.</param>
    /// <returns>Whether the callback was installed.</returns>
    [DllImport("comctl32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool SetWindowSubclass(IntPtr window, WindowsWindowProcedure procedure, UIntPtr id, UIntPtr data);

    /// <summary>Calls the native RemoveWindowSubclass window API.</summary>
    /// <param name="window">The native HWND.</param>
    /// <param name="procedure">The registered subclass callback.</param>
    /// <param name="id">The registered subclass identifier.</param>
    /// <returns>Whether the callback was removed.</returns>
    [DllImport("comctl32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool RemoveWindowSubclass(IntPtr window, WindowsWindowProcedure procedure, UIntPtr id);

    /// <summary>Calls the native DefSubclassProc window API.</summary>
    /// <param name="window">The native HWND.</param>
    /// <param name="message">The message identifier.</param>
    /// <param name="wParam">The first message argument.</param>
    /// <param name="lParam">The second message argument.</param>
    /// <returns>The next window procedure result.</returns>
    [DllImport("comctl32.dll", SetLastError = true)]
    internal static extern IntPtr DefSubclassProc(IntPtr window, uint message, UIntPtr wParam, IntPtr lParam);

    /// <summary>Reads the version of the already loaded SDL2 backend.</summary>
    /// <param name="version">Receives the runtime SDL2 version.</param>
    [DllImport("SDL2.dll", EntryPoint = "SDL_GetVersion", CallingConvention = CallingConvention.Cdecl)]
    private static extern void SdlGetVersion(out WindowsSdlVersion version);

    /// <summary>Resolves native window information from a DesktopGL SDL2 window pointer.</summary>
    /// <param name="window">The SDL_Window pointer.</param>
    /// <param name="information">The initialized system window information.</param>
    /// <returns>One on success, or zero on failure.</returns>
    [DllImport("SDL2.dll", EntryPoint = "SDL_GetWindowWMInfo", CallingConvention = CallingConvention.Cdecl)]
    private static extern int SdlGetWindowInfo(IntPtr window, ref WindowsSdlWindowInfo information);
}

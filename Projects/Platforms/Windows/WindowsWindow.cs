using Microsoft.Xna.Framework;
using Sachssoft.Engine.Services.Platform;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using static Sachssoft.Engine.Platform.Windows.WindowsWindowNative;

namespace Sachssoft.Engine.Platform.Windows;

/// <summary>Controls a native Windows game window without depending on Controls or Windows Forms.</summary>
/// <remarks>
/// Create one instance per initialized game window on its owning thread. Both a direct HWND and the
/// DesktopGL SDL2 handle are supported. Other backends can supply an explicit HWND. Dispose before
/// the game is destroyed; native destruction also removes the callback and its lifetime reservation.
/// AllowClose governs native user requests and does not intercept Game.Exit or operating system termination.
/// </remarks>
public sealed class WindowsWindow : IPlatformWindow, IDisposable
{
    /// <summary>Stores the associated game and its backend window settings.</summary>
    private readonly Game _game;

    /// <summary>Stores the actual Win32 window handle.</summary>
    private readonly IntPtr _window;

    /// <summary>Keeps the native callback delegate alive until it is detached.</summary>
    private readonly WindowsWindowProcedure _procedure;

    /// <summary>Stores the initial backend Alt+F4 policy for restoration.</summary>
    private readonly bool _initialAltF4;

    /// <summary>Stores the initial backend resizing policy for restoration.</summary>
    private readonly bool _initialResize;

    /// <summary>Stores the initial close menu enabled or grayed state.</summary>
    private readonly uint _initialCloseState;

    /// <summary>Stores the initial sizing-frame and maximize-button style bits.</summary>
    private readonly uint _initialResizeStyle;

    /// <summary>Stores the owning native thread identifier.</summary>
    private readonly uint _threadId;

    /// <summary>Roots this instance for the lifetime of its unmanaged callback.</summary>
    private GCHandle _lifetime;

    /// <summary>Stores the requested close permission.</summary>
    private bool _allowClose;

    /// <summary>Stores the requested manual resize permission.</summary>
    private bool _allowResize;

    /// <summary>Tracks whether the native subclass is attached.</summary>
    private bool _attached;

    /// <summary>Tracks whether close policy was changed by this service.</summary>
    private bool _closeChanged;

    /// <summary>Tracks disposal of this service.</summary>
    private bool _disposed;

    /// <summary>Tracks whether resize policy was changed by this service.</summary>
    private bool _resizeChanged;

    /// <summary>Tracks final destruction of the original native window.</summary>
    private bool _windowDestroyed;

    /// <summary>Creates the window service after resolving the active backend's native HWND.</summary>
    /// <param name="game">The initialized game, accessed on its window thread.</param>
    /// <exception cref="ArgumentNullException">The game is null.</exception>
    /// <exception cref="InvalidOperationException">The window is unavailable or already managed.</exception>
    /// <exception cref="PlatformNotSupportedException">The backend needs an explicit HWND.</exception>
    public WindowsWindow(Game game)
        : this(game, ResolveHandle(game))
    {
    }

    /// <summary>Creates the window service for an explicitly supplied Win32 HWND.</summary>
    /// <param name="game">The associated game.</param>
    /// <param name="nativeWindowHandle">The game's actual HWND, not an SDL window pointer.</param>
    /// <exception cref="ArgumentNullException">The game is null.</exception>
    /// <exception cref="ArgumentException">The handle is invalid or belongs to another thread or process.</exception>
    /// <exception cref="InvalidOperationException">Another window service already owns this HWND.</exception>
    /// <exception cref="Win32Exception">The window callback could not be installed.</exception>
    public WindowsWindow(Game game, IntPtr nativeWindowHandle)
    {
        ArgumentNullException.ThrowIfNull(game);
        if (!IsWindow(nativeWindowHandle))
            throw new ArgumentException("An initialized Win32 HWND is required.", nameof(nativeWindowHandle));

        _threadId = GetWindowThreadProcessId(nativeWindowHandle, out uint process);
        if (_threadId != GetCurrentThreadId() || process != (uint)Environment.ProcessId)
            throw new ArgumentException("The window must belong to this process and the calling thread.", nameof(nativeWindowHandle));

        if (GetProp(nativeWindowHandle, OwnerProperty) != IntPtr.Zero)
            throw new InvalidOperationException("A platform window service already manages this HWND.");

        _game = game;
        _window = nativeWindowHandle;
        _procedure = ProcessMessage;
        _initialAltF4 = game.Window.AllowAltF4;
        _initialResize = game.Window.AllowUserResizing;
        _initialResizeStyle = unchecked((uint)GetWindowLong(_window, GwlStyle)) & (WsThickFrame | WsMaximizeBox);
        IntPtr menu = GetSystemMenu(_window, false);
        uint closeState = menu == IntPtr.Zero ? 0 : GetMenuState(menu, ScClose, 0);
        _initialCloseState = closeState == uint.MaxValue ? 0 : closeState & 3;
        _allowClose = _initialAltF4 && _initialCloseState == 0;
        _allowResize = _initialResize;
        _lifetime = GCHandle.Alloc(this);
        try
        {
            if (!SetProp(_window, OwnerProperty, GCHandle.ToIntPtr(_lifetime)))
                throw new Win32Exception(Marshal.GetLastPInvokeError());

            if (!SetWindowSubclass(_window, _procedure, (UIntPtr)1, UIntPtr.Zero))
                throw new Win32Exception(Marshal.GetLastPInvokeError());

            _attached = true;
            if (!_allowClose)
                AllowClose = false;
        }
        catch
        {
            Detach();
            throw;
        }
    }

    /// <inheritdoc/>
    public bool SupportsWindowMode => true;

    /// <inheritdoc/>
    public bool SupportsTitle => true;

    /// <inheritdoc/>
    public bool SupportsCloseControl => true;

    /// <inheritdoc/>
    public bool SupportsResize => true;

    /// <inheritdoc/>
    public bool SupportsMaximize => true;

    /// <inheritdoc/>
    public bool SupportsMinimize => true;

    /// <inheritdoc/>
    public bool IsMaximized
    {
        get
        {
            VerifyAccess();
            return IsZoomed(_window);
        }
    }

    /// <inheritdoc/>
    public bool IsMinimized
    {
        get
        {
            VerifyAccess();
            return IsIconic(_window);
        }
    }

    /// <summary>Gets the resolved Win32 HWND for native integration.</summary>
    public IntPtr NativeHandle
    {
        get
        {
            VerifyAccess();
            return _window;
        }
    }

    /// <inheritdoc/>
    public string Title
    {
        get
        {
            VerifyAccess();
            return _game.Window.Title;
        }
        set
        {
            VerifyAccess();
            ArgumentNullException.ThrowIfNull(value);
            _game.Window.Title = value;
        }
    }

    /// <inheritdoc/>
    public bool AllowClose
    {
        get
        {
            VerifyAccess();
            return _allowClose;
        }
        set
        {
            VerifyAccess();
            SetCloseState(_window, value ? MfEnabled : MfGrayed);
            _allowClose = value;
            _game.Window.AllowAltF4 = value;
            _closeChanged = true;
        }
    }

    /// <inheritdoc/>
    public bool AllowResize
    {
        get
        {
            VerifyAccess();
            return _allowResize;
        }
        set
        {
            VerifyAccess();
            _game.Window.AllowUserResizing = value;
            SetResizeStyle(_window, value ? WsThickFrame | WsMaximizeBox : 0);
            _allowResize = value;
            _resizeChanged = true;
        }
    }

    /// <inheritdoc/>
    public void Maximize()
    {
        VerifyAccess();
        if (!_allowResize)
            throw new InvalidOperationException("Enable manual resizing before maximizing the window.");

        ShowWindow(_window, SwMaximize);
    }

    /// <inheritdoc/>
    public void Minimize()
    {
        VerifyAccess();
        ShowWindow(_window, SwMinimize);
    }

    /// <inheritdoc/>
    public void Restore()
    {
        VerifyAccess();
        ShowWindow(_window, SwRestore);
    }

    /// <summary>Restores the original close and resize policy and detaches the native callback.</summary>
    /// <remarks>Disposing after native window destruction remains safe. Repeated disposal has no effect.</remarks>
    public void Dispose()
    {
        if (_disposed)
            return;

        if (GetCurrentThreadId() != _threadId)
            throw new InvalidOperationException("Window services must be disposed on their owning thread.");

        if (!_windowDestroyed && IsWindow(_window))
        {
            if (_closeChanged)
            {
                SetCloseState(_window, _initialCloseState);
                _game.Window.AllowAltF4 = _initialAltF4;
            }

            if (_resizeChanged)
            {
                _game.Window.AllowUserResizing = _initialResize;
                SetResizeStyle(_window, _initialResizeStyle);
            }
        }

        Detach();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>Filters close and sizing commands and releases the callback during window destruction.</summary>
    /// <param name="window">The native HWND.</param>
    /// <param name="message">The native message identifier.</param>
    /// <param name="wParam">The first message argument.</param>
    /// <param name="lParam">The second message argument.</param>
    /// <param name="subclassId">The installed subclass identifier.</param>
    /// <param name="referenceData">The associated native reference data.</param>
    /// <returns>Zero for a blocked request, otherwise the next native procedure's result.</returns>
    private IntPtr ProcessMessage(IntPtr window, uint message, UIntPtr wParam, IntPtr lParam,
        UIntPtr subclassId, UIntPtr referenceData)
    {
        if (message == WmClose && !_allowClose)
            return IntPtr.Zero;

        if (message == WmSysCommand)
        {
            uint command = (uint)(wParam.ToUInt64() & 0xFFF0);
            if ((command == ScClose && !_allowClose) ||
                ((command == ScSize || command == ScMaximize) && !_allowResize))
                return IntPtr.Zero;
        }

        if (message == WmNcDestroy)
        {
            Detach(true);
            _windowDestroyed = true;
        }

        return DefSubclassProc(window, message, wParam, lParam);
    }

    /// <summary>Removes the native callback, owner marker, and managed lifetime reservation.</summary>
    /// <param name="destroying">Whether final destruction guarantees removal of remaining native callbacks.</param>
    private void Detach(bool destroying = false)
    {
        if (_attached)
        {
            if (!RemoveWindowSubclass(_window, _procedure, (UIntPtr)1) && !destroying)
                throw new Win32Exception(Marshal.GetLastPInvokeError());

            _attached = false;
        }

        if (_lifetime.IsAllocated)
        {
            if (GetProp(_window, OwnerProperty) == GCHandle.ToIntPtr(_lifetime))
                RemoveProp(_window, OwnerProperty);

            _lifetime.Free();
        }
    }

    /// <summary>Validates the service lifetime, native window lifetime, and calling thread.</summary>
    /// <exception cref="ObjectDisposedException">The service or original native window is no longer alive.</exception>
    /// <exception cref="InvalidOperationException">The caller is not the owning native window thread.</exception>
    private void VerifyAccess()
    {
        ObjectDisposedException.ThrowIf(_disposed || _windowDestroyed, this);
        if (GetCurrentThreadId() != _threadId)
            throw new InvalidOperationException("Window services must be used on their owning thread.");

        ObjectDisposedException.ThrowIf(!IsWindow(_window), this);
    }
}

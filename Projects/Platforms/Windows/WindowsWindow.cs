
using Microsoft.Xna.Framework;
using Sachssoft.Engine.Services.Platform;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using static Sachssoft.Engine.Platform.Windows.WindowsWindowNative;

namespace Sachssoft.Engine.Platform.Windows;

/// <summary>Controls a native Windows game window without depending on Controls or Windows Forms.</summary>
/// <remarks>
/// Register the service before game initialization and call Initialize after the native window is available.
/// Window permissions are configured during construction and applied during initialization.
/// All native operations must execute on the owning window thread. Dispose before the game is destroyed.
/// Native destruction also removes the callback and its lifetime reservation.
/// AllowClose governs native user requests and does not intercept Game.Exit or OS termination.
/// </remarks>
public sealed class WindowsWindow : IPlatformWindow, IPlatformInitializable, IDisposable
{
    private Game? _game;
    private IntPtr _window;
    private readonly WindowsWindowProcedure _procedure;

    private bool _initialAltF4;
    private bool _initialResize;
    private uint _initialCloseState;
    private uint _initialResizeStyle;
    private uint _threadId;

    private GCHandle _lifetime;
    private bool _attached;
    private bool _closeChanged;
    private bool _disposed;
    private bool _initialized;
    private bool _resizeChanged;
    private bool _windowDestroyed;

    /// <summary>Creates an uninitialized Windows window service.</summary>
    public WindowsWindow()
    {
        _procedure = ProcessMessage;
    }

    /// <inheritdoc/>
    public nint Handle
    {
        get
        {
            VerifyAccess();
            return _window;
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

    /// <summary>Gets whether the service has been initialized and is available.</summary>
    public bool IsInitialized => _initialized && !_disposed && !_windowDestroyed;

    /// <inheritdoc/>
    public string Title
    {
        get
        {
            VerifyAccess();
            return _game!.Window.Title;
        }
        set
        {
            VerifyAccess();
            ArgumentNullException.ThrowIfNull(value);
            _game!.Window.Title = value;
        }
    }

    /// <inheritdoc/>
    public bool AllowClose { get; init; } = true;

    /// <inheritdoc/>
    public bool AllowResize { get; init; }

    /// <inheritdoc/>
    public void Initialize(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_initialized)
            throw new InvalidOperationException("The window service is already initialized.");

        if (_attached || _lifetime.IsAllocated)
            throw new InvalidOperationException("The window service still owns native resources.");

        IntPtr window = ResolveHandle(game);

        if (!IsWindow(window))
            throw new InvalidOperationException("An initialized Win32 HWND is required.");

        uint threadId = GetWindowThreadProcessId(window, out uint process);
        if (threadId != GetCurrentThreadId() || process != (uint)Environment.ProcessId)
            throw new InvalidOperationException("The window must belong to this process and the calling thread.");

        if (GetProp(window, OwnerProperty) != IntPtr.Zero)
            throw new InvalidOperationException("A platform window service already manages this HWND.");

        _game = game;
        _window = window;
        _threadId = threadId;
        _windowDestroyed = false;

        _initialAltF4 = game.Window.AllowAltF4;
        _initialResize = game.Window.AllowUserResizing;
        _initialResizeStyle = unchecked((uint)GetWindowLong(_window, GwlStyle)) & (WsThickFrame | WsMaximizeBox);

        IntPtr menu = GetSystemMenu(_window, false);
        uint closeState = menu == IntPtr.Zero ? 0 : GetMenuState(menu, ScClose, 0);
        _initialCloseState = closeState == uint.MaxValue ? 0 : closeState & 3;

        _closeChanged = false;
        _resizeChanged = false;

        try
        {
            _lifetime = GCHandle.Alloc(this);

            if (!SetProp(_window, OwnerProperty, GCHandle.ToIntPtr(_lifetime)))
                throw new Win32Exception(Marshal.GetLastPInvokeError());

            if (!SetWindowSubclass(_window, _procedure, (UIntPtr)1, UIntPtr.Zero))
                throw new Win32Exception(Marshal.GetLastPInvokeError());

            _attached = true;

            _closeChanged = true;
            SetCloseState(_window, AllowClose ? MfEnabled : MfGrayed);
            game.Window.AllowAltF4 = AllowClose;

            _resizeChanged = true;
            game.Window.AllowUserResizing = AllowResize;
            SetResizeStyle(_window, AllowResize ? WsThickFrame | WsMaximizeBox : 0);

            _initialized = true;
        }
        catch
        {
            try
            {
                if (!_windowDestroyed && IsWindow(_window))
                {
                    if (_closeChanged)
                    {
                        SetCloseState(_window, _initialCloseState);
                        game.Window.AllowAltF4 = _initialAltF4;
                    }

                    if (_resizeChanged)
                    {
                        game.Window.AllowUserResizing = _initialResize;
                        SetResizeStyle(_window, _initialResizeStyle);
                    }
                }
            }
            finally
            {
                try
                {
                    Detach();
                }
                finally
                {
                    if (!_attached && !_lifetime.IsAllocated)
                        ResetInitialization();
                }
            }

            throw;
        }
    }

    /// <inheritdoc/>
    public void Maximize()
    {
        VerifyAccess();

        if (!AllowResize)
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
    /// <remarks>
    /// Disposal before initialization is supported. After initialization, disposal must occur
    /// on the owning window thread. Repeated disposal has no effect.
    /// </remarks>
    public void Dispose()
    {
        if (_disposed)
            return;

        if (!_initialized && !_attached && !_lifetime.IsAllocated)
        {
            _disposed = true;
            GC.SuppressFinalize(this);
            return;
        }

        if (GetCurrentThreadId() != _threadId)
            throw new InvalidOperationException("Window services must be disposed on their owning thread.");

        if (!_windowDestroyed && IsWindow(_window))
        {
            if (_closeChanged)
            {
                SetCloseState(_window, _initialCloseState);
                _game!.Window.AllowAltF4 = _initialAltF4;
            }

            if (_resizeChanged)
            {
                _game!.Window.AllowUserResizing = _initialResize;
                SetResizeStyle(_window, _initialResizeStyle);
            }
        }

        Detach();
        _disposed = true;
        _initialized = false;
        _game = null;
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
        if (message == WmClose && !AllowClose)
            return IntPtr.Zero;

        if (message == WmSysCommand)
        {
            uint command = (uint)(wParam.ToUInt64() & 0xFFF0);

            if ((command == ScClose && !AllowClose) ||
                ((command == ScSize || command == ScMaximize) && !AllowResize))
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
    /// <param name="destroying">Whether native window destruction is in progress.</param>
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
            if (IsWindow(_window) &&
                GetProp(_window, OwnerProperty) == GCHandle.ToIntPtr(_lifetime))
            {
                RemoveProp(_window, OwnerProperty);
            }

            _lifetime.Free();
        }
    }

    /// <summary>Resets managed initialization state after unsuccessful initialization.</summary>
    private void ResetInitialization()
    {
        _game = null;
        _window = IntPtr.Zero;
        _threadId = 0;
        _initialized = false;
        _windowDestroyed = false;
        _closeChanged = false;
        _resizeChanged = false;
    }

    /// <summary>Validates initialization, service lifetime, native window lifetime, and calling thread.</summary>
    /// <exception cref="InvalidOperationException">The service has not been initialized or the caller is on another thread.</exception>
    /// <exception cref="ObjectDisposedException">The service or native window is no longer available.</exception>
    private void VerifyAccess()
    {
        ObjectDisposedException.ThrowIf(_disposed || _windowDestroyed, this);

        if (!_initialized)
            throw new InvalidOperationException("The window service has not been initialized.");

        if (GetCurrentThreadId() != _threadId)
            throw new InvalidOperationException("Window services must be used on their owning thread.");

        ObjectDisposedException.ThrowIf(!IsWindow(_window), this);
    }
}

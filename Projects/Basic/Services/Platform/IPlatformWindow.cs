namespace Sachssoft.Engine.Services.Platform;

/// <summary>
/// Describes platform window capabilities and controls the native game window.
/// </summary>
/// <remarks>
/// Capabilities describe platform support, independently of the current window state or permissions.
/// Desktop implementations must be used on the owning window thread after initialization.
/// Mobile implementations report SupportsWindowMode as false and reject desktop-only operations.
/// </remarks>
public interface IPlatformWindow
{
    /// <summary>Gets the backend-specific native window handle.</summary>
    /// <remarks>The handle type depends on the active platform and backend.</remarks>
    nint Handle { get; }

    /// <summary>Reports whether the platform supports a desktop window mode.</summary>
    bool SupportsWindowMode
    {
        get;
    }

    /// <summary>Reports whether a native window title can be changed.</summary>
    bool SupportsTitle
    {
        get;
    }

    /// <summary>Reports whether user close requests can be enabled or blocked.</summary>
    bool SupportsCloseControl
    {
        get;
    }

    /// <summary>Reports whether manual resizing is supported.</summary>
    bool SupportsResize
    {
        get;
    }

    /// <summary>Reports whether maximizing a desktop window is supported.</summary>
    bool SupportsMaximize
    {
        get;
    }

    /// <summary>Reports whether minimizing a desktop window is supported.</summary>
    bool SupportsMinimize
    {
        get;
    }

    /// <summary>Gets whether the native window is currently maximized.</summary>
    bool IsMaximized
    {
        get;
    }

    /// <summary>Gets whether the native window is currently minimized. Mobile app suspension is not a minimized window.</summary>
    bool IsMinimized
    {
        get;
    }

    /// <summary>Gets or sets the native window title when title changes are supported.</summary>
    string Title
    {
        get;
        set;
    }

    /// <summary>Gets whether users are permitted to close the native window.</summary>
    /// <remarks>
    /// Configured during service creation and applied during initialization.
    /// Does not prevent application-directed exit or operating system termination.
    /// </remarks>
    bool AllowClose { get; }

    /// <summary>Gets whether users are permitted to resize the native window.</summary>
    /// <remarks>
    /// Configured during service creation and applied during initialization.
    /// Application-driven resizing is separate.
    /// </remarks>
    bool AllowResize { get; }

    /// <summary>Maximizes the window without switching to fullscreen mode.</summary>
    /// <exception cref="System.PlatformNotSupportedException">Maximizing is not supported.</exception>
    /// <exception cref="System.InvalidOperationException">Resizing is currently disabled.</exception>
    void Maximize();

    /// <summary>Minimizes the native window.</summary>
    /// <exception cref="System.PlatformNotSupportedException">Minimizing is not supported.</exception>
    void Minimize();

    /// <summary>Restores a minimized or maximized window to its previous normal bounds.</summary>
    /// <exception cref="System.PlatformNotSupportedException">Desktop window states are not supported.</exception>
    void Restore();
}

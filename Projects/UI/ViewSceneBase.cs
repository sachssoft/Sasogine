using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Graphics.Cameras;
using Sachssoft.Engine.Graphics.Materials;
using Sachssoft.Engine.Scenes;
using System;
using System.Runtime.CompilerServices;

namespace Sachssoft.Engine.Controls;

/// <summary>Hosts a workspace as a scene without a world camera or material.</summary>
/// <remarks>
/// The scene owns its workspace, which manages the drawing resources. Constructor-supplied fonts and skins
/// remain caller-owned. The standard font is borrowed from the shared text backend.
/// CreateWorkspace runs after Enter and before the first update.
/// Unload releases the workspace; loading again creates a fresh tree. Use OnUpdate to poll controls
/// after input has been processed. All lifecycle calls must run on the application thread.
/// </remarks>
public abstract class ViewSceneBase : IScene, IDisposable, IApplicationExitAware
{
    /// <summary>Retains an optional caller-owned font supplied at construction.</summary>
    private readonly IFont? _defaultFont;

    /// <summary>Determines whether a font created at load time belongs to this scene.</summary>
    private readonly bool _ownsDefaultFont;

    /// <summary>Identifies the application supplied during activation.</summary>
    private GameApplicationBase? _application;

    /// <summary>Retains the default font for the current load cycle.</summary>
    private IFont? _loadedFont;
    private IFont? _sharedDefaultFont;
    private IFontBackend? _fontBackend;

    /// <summary>Contains the loaded control tree and its bindings.</summary>
    private Workspace? _workspace;

    /// <summary>Prevents updates and text input while the scene is inactive.</summary>
    private bool _active;

    /// <summary>Defers input cancellation until an in-progress workspace traversal finishes.</summary>
    private bool _deactivatePending;

    /// <summary>Prevents reuse after final disposal.</summary>
    private bool _disposed;

    /// <summary>Tracks whether an update prepared a drawable frame.</summary>
    private bool _frameReady;

    /// <summary>Defers workspace disposal requested by a control callback.</summary>
    private bool _unloadPending;

    /// <summary>Guards the update traversal and its scene callbacks.</summary>
    private bool _updating;

    /// <summary>Creates a scene that obtains its default font during Load.</summary>
    /// <remarks>The built-in default is Noto Sans Regular. Overrides may supply a scene-owned font.</remarks>
    protected ViewSceneBase()
    {
        _ownsDefaultFont = true;
    }

    /// <summary>Creates a camera-free scene using a caller-owned default font.</summary>
    /// <param name="defaultFont">The font used for both measurement and rendering.</param>
    /// <exception cref="ArgumentNullException">The font is null.</exception>
    protected ViewSceneBase(IFont defaultFont)
    {
        _defaultFont = defaultFont ?? throw new ArgumentNullException(nameof(defaultFont));
    }

    /// <summary>Gets the application supplied by Enter.</summary>
    /// <exception cref="InvalidOperationException">The scene has not been activated.</exception>
    protected GameApplicationBase Application => _application ??
        throw new InvalidOperationException("The scene has not been activated.");

    /// <summary>Gets or sets the clear color used before drawing the workspace.</summary>
    public Color BackgroundColor
    {
        get;
        set;
    } = Color.Transparent;

    /// <inheritdoc/>
    public bool IsLoaded => _workspace is not null;

    /// <inheritdoc/>
    public virtual bool IsPersistent => false;

    /// <inheritdoc/>
    public bool RequiresCamera => false;

    /// <inheritdoc/>
    public int ViewCount => 1;

    /// <summary>Gets the workspace created during Load, including its DPI and style registries.</summary>
    /// <exception cref="InvalidOperationException">The scene is not loaded.</exception>
    public Workspace Workspace => _workspace ??
        throw new InvalidOperationException("The scene is not loaded.");

    /// <inheritdoc/>
    public void Enter(SceneEnterEventArgs eventArgs)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(eventArgs);
        if (_updating)
            throw new InvalidOperationException("A scene cannot be reactivated during its own update.");
        if (_application is not null && !ReferenceEquals(_application, eventArgs.Application))
            throw new InvalidOperationException("A view scene cannot move between applications.");
        if (_active)
            return;

        _application = eventArgs.Application;
        _workspace?.ResetInput();
        _frameReady = false;
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            SetDesktopTextInputSubscription(true);
        _active = true;
    }

    /// <inheritdoc/>
    public void Exit()
    {
        if (!_active)
            return;

        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            SetDesktopTextInputSubscription(false);
        _active = false;
        _frameReady = false;
        if (_updating)
            _deactivatePending = true;
        else
            DeactivateWorkspace();
        _workspace?.ResetInput();
    }

    /// <inheritdoc/>
    public void Load()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (IsLoaded)
            return;

        GameApplicationBase application = Application;
        Workspace? workspace = null;
        try
        {
            workspace = CreateWorkspace() ??
                throw new InvalidOperationException("CreateWorkspace must return a workspace.");
            IFontBackend? applicationBackend = application.GetService<IFontBackend>();
            _fontBackend = workspace.Registry.FontBackend ?? applicationBackend;
            _loadedFont = CreateDefaultFont() ??
                throw new InvalidOperationException("CreateDefaultFont must return a font.");
            workspace.Initialize(application, _loadedFont, applicationBackend);
            _workspace = workspace;
        }
        catch
        {
            try
            {
                workspace?.Dispose();
            }
            finally
            {
                ReleaseDefaultFont();
                Exit();
            }
            throw;
        }
    }

    /// <inheritdoc/>
    public void Unload()
    {
        Exit();
        if (_updating)
        {
            _unloadPending = true;
            return;
        }

        _deactivatePending = false;
        _frameReady = false;
        try
        {
            _workspace?.Dispose();
        }
        finally
        {
            _workspace = null;
            ReleaseDefaultFont();
        }
    }

    /// <inheritdoc/>
    public void Update(SceneUpdateContext context)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(context);
        if (_updating)
            throw new InvalidOperationException("Scene updates cannot be reentered.");
        _frameReady = false;
        if (!_active || !IsLoaded)
            return;

        Rectangle viewport = context.GraphicsDevice.Viewport.Bounds;
        if (viewport.Width <= 0 || viewport.Height <= 0)
            return;

        _updating = true;
        try
        {
            Workspace.InitializeGraphics(context.GraphicsDevice);
            UpdateWorkspace(context, viewport);
            if (_active && IsLoaded)
                OnUpdate(context);
            _frameReady = _active && IsLoaded;
        }
        finally
        {
            _updating = false;
            if (_unloadPending)
            {
                _unloadPending = false;
                Unload();
            }
            else if (_deactivatePending)
            {
                _deactivatePending = false;
                DeactivateWorkspace();
            }
        }
    }

    /// <inheritdoc/>
    public void Draw(SceneDrawContext context)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(context);
        if (!_frameReady || _workspace is null)
            return;

        context.GraphicsDevice.Clear(BackgroundColor);
        _workspace.Draw(context);
    }

    /// <summary>Unloads the scene and prevents subsequent activation.</summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            Unload();
        }
        finally
        {
            _application = null;
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    /// <summary>Releases scene resources when the application shuts down.</summary>
    void IApplicationExitAware.OnApplicationExited() => Unload();

    /// <summary>Obtains the default font after activation and asset initialization.</summary>
    /// <returns>The font used for this load cycle.</returns>
    /// <remarks>
    /// The default implementation borrows the font from the shared text backend.
    /// An override that creates a disposable font transfers ownership to the scene when using the parameterless constructor.
    /// </remarks>
    protected virtual IFont CreateDefaultFont() => _defaultFont ??
        (_sharedDefaultFont = BuiltInFonts.GetFont(BuiltInFont.Sans, BuiltInFonts.DefaultSize,
            backend: _fontBackend));

    /// <summary>Creates and configures the workspace, including styles and optional skin.</summary>
    /// <returns>A new workspace owned by this scene until Unload.</returns>
    protected abstract Workspace CreateWorkspace();

    /// <summary>Processes the workspace using one snapshot of each input device.</summary>
    /// <param name="context">The current scene update and timing.</param>
    /// <param name="viewport">The physical viewport used for input and layout.</param>
    /// <remarks>Override to supply host snapshots or replay input instead of polling devices.</remarks>
    protected virtual void UpdateWorkspace(SceneUpdateContext context, Rectangle viewport) =>
        Workspace.Update(context, viewport);

    /// <summary>Runs after workspace input, bindings and layout; frame flags can be polled here.</summary>
    /// <param name="context">The current scene update and timing.</param>
    protected virtual void OnUpdate(SceneUpdateContext context)
    {
    }

    /// <summary>Changes the desktop-only text event binding outside mobile lifecycle methods.</summary>
    /// <param name="subscribe">Whether the active scene should receive desktop text input.</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void SetDesktopTextInputSubscription(bool subscribe)
    {
        if (subscribe)
            Application.Window.TextInput += OnTextInput;
        else
            Application.Window.TextInput -= OnTextInput;
    }

    /// <summary>Queues native text only while this scene and its application are active.</summary>
    /// <param name="sender">The application window.</param>
    /// <param name="args">The native text character.</param>
    private void OnTextInput(object? sender, TextInputEventArgs args)
    {
        if (_active && Application.IsActive)
            _workspace?.AddTextInput(args.Character);
    }

    /// <summary>Cancels held input without reentering an active workspace traversal.</summary>
    private void DeactivateWorkspace()
    {
        if (_workspace is not null)
        {
            _workspace.Update(new ControlInputFrame(_workspace.Size, TimeSpan.Zero,
                new[] { new ControlInputEvent(ControlInputEventType.Deactivate) }));
        }
    }

    /// <summary>Releases the scene-owned default font after the workspace has been released.</summary>
    private void ReleaseDefaultFont()
    {
        IFont? font = _loadedFont;
        bool ownsFont = _ownsDefaultFont && !ReferenceEquals(font, _sharedDefaultFont);
        _loadedFont = null;
        _sharedDefaultFont = null;
        _fontBackend = null;
        if (ownsFont && font is IDisposable disposable)
            disposable.Dispose();
    }

    /// <summary>Rejects camera creation; the scene manager skips this factory.</summary>
    /// <param name="graphicsDevice">The unused graphics device.</param>
    /// <param name="index">The unused view index.</param>
    /// <returns>No camera is created.</returns>
    /// <exception cref="NotSupportedException">Camera creation is not supported.</exception>
    ICamera IScene.CreateCamera(GraphicsDevice graphicsDevice, int index) =>
        throw new NotSupportedException("View scenes do not use world cameras.");

    /// <summary>Rejects world material creation; workspace drawing owns its renderer.</summary>
    /// <param name="graphicsDevice">The unused graphics device.</param>
    /// <returns>No material is created.</returns>
    /// <exception cref="NotSupportedException">World material creation is not supported.</exception>
    IMaterial IScene.CreateDefaultMaterial(GraphicsDevice graphicsDevice) =>
        throw new NotSupportedException("View scenes do not use world materials.");
}

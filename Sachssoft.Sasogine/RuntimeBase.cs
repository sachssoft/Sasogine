using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Diagnostics;
using Sachssoft.Sasogine.Graphics;
using System;

namespace Sachssoft.Sasogine;

/*
 -----------------------------------------------------------------------------------------------------
 Die RuntimeBase ist das zentrale Fundament für jede Spiel- oder Editorlaufzeit im Framework.
 Sie übernimmt:
 - Kameraaktualisierung (CameraBase)
 - Rendering auf ein internes Offscreen-RenderTarget (Scene)
 - Rendering dieses RenderTargets auf den Bildschirm (Fullscreen)
 - Optionales Hinzufügen von Post-Processing-Effekten
 - Verwaltung einer Komponentenliste, die Update und Draw unterstützt
 - Diagnosedaten, Input-Zustände und Screenshot-Erstellung

 Diese Klasse selbst implementiert keine Spiellogik. 
 Stattdessen wird sie von konkreten Runtimes (z.B. GameRuntime oder EditorRuntime) abgeleitet,
 die eigene Inhalte im OnSceneRender()-Hook zeichnen.

 Vorteile dieses Aufbaus:
 - Einheitliche Rendering-Pipeline für alle Runtimes
 - Post-Processing einfach erweiterbar (EffectOverride)
 - Klares Trennen von Spiellogik und Rendering
 - Effizient durch Zwischenspeicherung in RenderTarget2D
 - Stabilität durch sicheres Ressourcen-Management und kontrolliertes Rendern
 -----------------------------------------------------------------------------------------------------
*/

/// <summary>
/// The RuntimeBase class serves as the central runtime for all engine backends.
/// It handles:
/// - Camera updates via CameraBase
/// - Rendering the scene to an offscreen RenderTarget2D (scene render target)
/// - Drawing the scene render target to the screen (fullscreen)
/// - Optional post-processing effects via EffectOverride
/// - Management of a component collection that supports update and draw
/// - Diagnostic data and UI input state reporting
/// - Screenshot capture of the current scene
/// 
/// This class does not implement game logic directly. It is intended to be subclassed
/// for specific runtime implementations, such as GameRuntime or EditorRuntime.
/// Subclasses typically override OnSceneRender() to draw game objects or editor content.
/// 
/// Advantages:
/// - Unified rendering pipeline for all runtimes
/// - Easy to extend with post-processing effects
/// - Clear separation of rendering and game logic
/// - Efficient rendering via offscreen RenderTarget2D
/// - Safe resource management and controlled rendering lifecycle
/// </summary>
public abstract class RuntimeBase : IDisposable
{
    private RenderTarget2D? _sceneRenderTarget;
    private SpriteBatch? _spriteBatch;
    private readonly CameraBase _camera;
    private readonly IEffectAdapter _effect;
    private readonly RuntimeComponentCollection _components = new();
    private readonly DiagnosticsContext _diagnostics = new();
    private GameBaseContext? _viewContext;
    private Color[]? _screenshotBuffer;
    private bool _disposed;
    private bool _isLoaded;

    /// <summary>
    /// Number of samples for MSAA in the scene RenderTarget.
    /// Only considered during RenderTarget creation.
    /// </summary>
    protected int SceneSampleCount { get; set; } = 2;

    /// <summary>
    /// Creates a new RuntimeBase instance with a camera and optional effect.
    /// </summary>
    /// <param name="camera">The camera used for rendering the scene.</param>
    /// <param name="effect">Optional rendering effect. If null, a basic effect is used.</param>
    protected RuntimeBase(CameraBase camera, IEffectAdapter? effect)
    {
        _camera = camera ?? throw new ArgumentNullException(nameof(camera));
        _effect = effect ?? new BasicEffectAdapter(IGameApplication.Current.GraphicsDevice);
        BackgroundColor = Color.CornflowerBlue;
        RenderVisibility = true;
    }

    /// <summary>
    /// Gets a value indicating whether the runtime has been loaded.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="Load(GameBaseContext)"/> has been called successfully;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// This property can be used to check the current load state before calling
    /// <see cref="Load(GameBaseContext)"/> or <see cref="Unload"/> to avoid exceptions.
    /// </remarks>
    public bool IsLoaded => _isLoaded;

    /// <summary>
    /// Gets the game or editor context. Throws if Load() has not been called.
    /// </summary>
    protected GameBaseContext ViewContext => _viewContext ?? throw new InvalidOperationException("No Scene Context");

    /// <summary>
    /// Indicates whether the mouse is currently hovering over a UI surface.
    /// </summary>
    public bool IsAnySurfaceHovered { get; private set; }

    /// <summary>
    /// Indicates whether a UI surface currently has focus.
    /// </summary>
    public bool IsAnySurfaceFocused { get; private set; }

    /// <summary>
    /// Indicates whether the runtime should render.
    /// </summary>
    public bool RenderVisibility { get; set; }

    /// <summary>
    /// Background color used when clearing the scene render target.
    /// </summary>
    public Color BackgroundColor { get; set; }

    /// <summary>
    /// Gets the camera used for rendering.
    /// </summary>
    public CameraBase Camera => _camera;

    /// <summary>
    /// Gets the effect used for rendering.
    /// </summary>
    public IEffectAdapter Effect => _effect;

    /// <summary>
    /// Provides diagnostics data for the runtime.
    /// </summary>
    public DiagnosticsContext Diagnostics => _diagnostics;

    /// <summary>
    /// Gets the collection of runtime components that are updated and drawn automatically.
    /// </summary>
    public RuntimeComponentCollection Components => _components;

    /// <summary>
    /// Loads the runtime and initializes necessary resources.
    /// Must be called before calling Draw().
    /// </summary>
    /// <param name="context">The graphics context containing the GraphicsDevice.</param>
    public virtual void Load(GameBaseContext context)
    {
        if (_isLoaded)
            throw new InvalidOperationException("The runtime has already been loaded.");

        _viewContext = context ?? throw new ArgumentNullException(nameof(context));
        _spriteBatch = new SpriteBatch(context.GraphicsDevice);
        EnsureSceneRenderTargetSize(context.GraphicsDevice);
        _isLoaded = true;
    }

    /// <summary>
    /// Unloads all resources associated with this runtime.
    /// </summary>
    public virtual void Unload()
    {
        if (!_isLoaded)
            throw new InvalidOperationException("The runtime has not been loaded yet.");

        DisposeSceneResources();
        _isLoaded = false;
    }

    /// <summary>
    /// Disposes all resources used by the runtime.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_isLoaded)
            Unload();
    }

    private void DisposeSceneResources()
    {
        _sceneRenderTarget?.Dispose();
        _sceneRenderTarget = null;

        _spriteBatch?.Dispose();
        _spriteBatch = null;

        _screenshotBuffer = null;
    }

    /// <summary>
    /// Updates the camera and all components.
    /// </summary>
    /// <param name="context">The frame context.</param>
    public virtual void Update(GameFrameContext context)
    {
        _camera?.Update(context);
        _components.ForEachRuntime(context);
    }

    /// <summary>
    /// Draws the scene to the offscreen render target and then to the screen.
    /// Applies optional effects via EffectOverride.
    /// </summary>
    /// <param name="context">The frame context.</param>
    public virtual void Draw(GameFrameContext context)
    {
        if (!RenderVisibility)
            return;

        var graphicsDevice = context.GraphicsDevice;
        if (graphicsDevice.IsDisposed || graphicsDevice.GraphicsDeviceStatus != GraphicsDeviceStatus.Normal || !_isLoaded)
            return;

        OnSceneRenderBegin(context);

        EnsureSceneRenderTargetSize(graphicsDevice);

        graphicsDevice.SetRenderTarget(_sceneRenderTarget);
        graphicsDevice.Clear(BackgroundColor);
        OnSceneRender(context);
        graphicsDevice.SetRenderTarget(null);

        graphicsDevice.Clear(Color.Black);

        var effect = EffectOverride(context);
        var rasterizer = new RasterizerState { MultiSampleAntiAlias = true };

        _spriteBatch!.Begin(
            SpriteSortMode.Immediate,
            samplerState: SamplerState.PointClamp,
            rasterizerState: rasterizer,
            effect: effect?.InnerEffect
        );

        _spriteBatch.Draw(
            _sceneRenderTarget!,
            new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height),
            Color.White
        );

        _spriteBatch.End();
    }

    /// <summary>
    /// Called before rendering the scene. Can be used to set up additional render targets.
    /// </summary>
    /// <param name="context">The frame context.</param>
    protected virtual void OnSceneRenderBegin(GameFrameContext context) { }

    /// <summary>
    /// Renders the actual scene. Typically overridden by subclasses to draw game or editor objects.
    /// </summary>
    /// <param name="context">The frame context.</param>
    protected virtual void OnSceneRender(GameFrameContext context)
    {
        _components.ForEachDrawable(context);
    }

    /// <summary>
    /// Provides a way to override the rendering effect (e.g., for post-processing).
    /// </summary>
    /// <param name="context">The frame context.</param>
    /// <returns>An optional effect adapter.</returns>
    protected virtual IEffectAdapter? EffectOverride(GameFrameContext context) => null;

    /// <summary>
    /// Ensures the scene render target has the correct size matching the back buffer.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    protected void EnsureSceneRenderTargetSize(GraphicsDevice graphicsDevice)
    {
        int width = graphicsDevice.PresentationParameters.BackBufferWidth;
        int height = graphicsDevice.PresentationParameters.BackBufferHeight;

        if (_sceneRenderTarget == null ||
            _sceneRenderTarget.Width != width ||
            _sceneRenderTarget.Height != height)
        {
            _sceneRenderTarget?.Dispose();
            _sceneRenderTarget = new RenderTarget2D(
                graphicsDevice,
                width,
                height,
                false,
                graphicsDevice.PresentationParameters.BackBufferFormat,
                graphicsDevice.PresentationParameters.DepthStencilFormat,
                SceneSampleCount,
                RenderTargetUsage.PlatformContents
            );
        }
    }

    /// <summary>
    /// Reports whether the mouse is hovering over a UI surface.
    /// </summary>
    /// <param name="hoverState">True if hovering, false otherwise.</param>
    public void ReportSurfaceHover(bool hoverState)
    {
        IsAnySurfaceHovered = hoverState;
    }

    /// <summary>
    /// Reports whether a UI surface currently has focus.
    /// </summary>
    /// <param name="focusState">True if focused, false otherwise.</param>
    public void ReportSurfaceFocus(bool focusState)
    {
        IsAnySurfaceFocused = focusState;
    }

    /// <summary>
    /// Captures a screenshot of the current scene render target as a Texture2D.
    /// </summary>
    /// <returns>A Texture2D containing the screenshot.</returns>
    public Texture2D CaptureScreenshot()
    {
        if (_sceneRenderTarget == null)
            throw new InvalidOperationException("No scene render target available for screenshot.");

        var device = _sceneRenderTarget.GraphicsDevice;
        int pixelCount = _sceneRenderTarget.Width * _sceneRenderTarget.Height;

        _screenshotBuffer ??= new Color[pixelCount];
        if (_screenshotBuffer.Length != pixelCount)
            _screenshotBuffer = new Color[pixelCount];

        _sceneRenderTarget.GetData(_screenshotBuffer);

        var screenshot = new Texture2D(device, _sceneRenderTarget.Width, _sceneRenderTarget.Height);
        screenshot.SetData(_screenshotBuffer);
        return screenshot;
    }
}

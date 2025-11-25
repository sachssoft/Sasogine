using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Diagnostics;
using Sachssoft.Sasogine.Graphics;
using Sachssoft.Sasogine.Presentation;
using System;

namespace Sachssoft.Sasogine.Engine;

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
    private readonly RuntimeComponentCollection _components = new();
    private readonly DiagnosticsContext _diagnostics = new();
    private readonly int _viewportCount;
    private readonly RuntimeViewportContext[] _runtimeContextes;
    private GameApplication? _application;
    private GraphicsDevice? _graphicsDevice;
    private RenderTarget2D[] _sceneRenderTargets;
    private SpriteBatch? _spriteBatch;
    private bool _disposed;
    private bool _wasInitialized;
    private bool _isLoaded;

    /// <summary>
    /// Konstruktor: Kameras müssen bereitgestellt werden.
    /// </summary>
    protected RuntimeBase(int viewportCount = 1)
    {
        if (viewportCount < 1 || viewportCount > MaxViewports)
            throw new ArgumentException($"ViewportCount must be 1..{MaxViewports}.");

        _viewportCount = viewportCount;
        _runtimeContextes = new RuntimeViewportContext[_viewportCount];
        _sceneRenderTargets = new RenderTarget2D[_viewportCount];
    }

    internal void EnsureInitialized(GameApplication application)
    {
        if (_wasInitialized)
            return; // bereits eingerichtet

        _application = application ?? throw new ArgumentNullException(nameof(application));
        _graphicsDevice = _application.GraphicsDevice;

        for (int i = 0; i < _viewportCount; i++)
        {
            var context = new RuntimeViewportContext(application, this, i)
            {
                Camera = CreateCamera(_graphicsDevice) ?? throw new InvalidOperationException($"Camera at index {i} is null."),
                EffectAdapter = CreateDefaultEffectAdapter(_graphicsDevice),
            };

            _runtimeContextes[i] = context;
        }

        _wasInitialized = true;
    }

    /// <summary>Maximal 4 Viewports für Split-Screen.</summary>
    public const int MaxViewports = 4;

    /// <summary>
    /// Die „Standardkamera“ der Runtime, genutzt als Ausgangspunkt für Viewports.
    /// </summary>
    public CameraBase PrimaryCamera => _runtimeContextes[0].Camera;

    /// <summary>
    /// Der Effektadapter, der standardmäßig auf den Primär-Viewport angewendet wird.
    /// </summary>
    public IEffectAdapter PrimaryEffectAdapter => _runtimeContextes[0].EffectAdapter;

    /// <summary>Hintergrundfarbe für alle Viewports.</summary>
    public Color BackgroundColor { get; set; } = Color.CornflowerBlue;

    /// <summary>Sichtbarkeit der Runtime.</summary>
    public bool RenderVisibility { get; set; } = true;

    /// <summary>Zahl der Samples für MSAA.</summary>
    protected int RenderSampleCount { get; set; } = 2;

    /// <summary>Runtime-Komponenten, die Update/Draw unterstützen.</summary>
    public RuntimeComponentCollection Components => _components;

    /// <summary>Diagnostics für Profiler/Debugging.</summary>
    public DiagnosticsContext Diagnostics => _diagnostics;

    /// <summary>Gibt an, ob Runtime geladen ist.</summary>
    public bool IsLoaded => _isLoaded;

    public int ViewportCount => _viewportCount;

    /// <summary>
    /// Gibt an, ob die Maus oder ein Zeigegerät gerade über einem Presentation-Element ist.
    /// </summary>
    public bool IsAnyPresentationHovered { get; private set; }

    /// <summary>
    /// Gibt an, ob ein Presentation-Element aktuell den Fokus hat.
    /// </summary>
    public bool IsAnyPresentationFocused { get; private set; }

    /// <summary>
    /// Hilfsproperty: true, wenn die Runtime keinen Input verarbeiten soll.
    /// </summary>
    public bool IsInputBlocked => IsAnyPresentationHovered || IsAnyPresentationFocused;

    /// <summary>
    /// Wird von Presentation/SceneBase aufgerufen, wenn Maus über Presentation-Element ist.
    /// </summary>
    public void ReportPresentationHover(bool hoverState) => IsAnyPresentationHovered = hoverState;

    /// <summary>
    /// Wird von Presentation/SceneBase aufgerufen, wenn Presentation-Element Fokus erhält/verliert.
    /// </summary>
    public void ReportPresentationFocus(bool focusState) => IsAnyPresentationFocused = focusState;

    public abstract CameraBase CreateCamera(GraphicsDevice graphicsDevice);

    public virtual IEffectAdapter CreateDefaultEffectAdapter(GraphicsDevice graphicsDevice)
    {
        return new BasicEffectAdapter(graphicsDevice);
    }

    public Rectangle GetSplitViewport(int index)
    {
        if (_graphicsDevice == null || !_wasInitialized)
            throw new InvalidOperationException(" not setup.");

        var full = _graphicsDevice.Viewport.Bounds;
        var viewports = GetSplitViewports(full, ViewportCount);
        return viewports[index];
    }

    /// <summary>Load Runtime: RenderTargets & SpriteBatch.</summary>
    public virtual void Load()
    {
        if (!_wasInitialized) throw new InvalidOperationException("Runtime not setup.");
        if (_isLoaded) throw new InvalidOperationException("Runtime already loaded.");

        _spriteBatch = new SpriteBatch(_graphicsDevice);

        _sceneRenderTargets = new RenderTarget2D[ViewportCount];
        EnsureSceneRenderTargets();

        _isLoaded = true;
    }

    /// <summary>Unload Runtime: Ressourcen freigeben.</summary>
    public virtual void Unload()
    {
        if (!_isLoaded) throw new InvalidOperationException("Runtime not loaded.");

        DisposeSceneResources();
        _isLoaded = false;
    }

    private void DisposeSceneResources()
    {
        if (_sceneRenderTargets != null)
        {
            foreach (var rt in _sceneRenderTargets)
                rt.Dispose();
            _sceneRenderTargets = new RenderTarget2D[] { };
        }

        _spriteBatch?.Dispose();
        _spriteBatch = null;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_isLoaded) Unload();
    }

    /// <summary>Update Runtime + Kameras + Komponenten.</summary>
    public virtual void Update(RuntimeContext context)
    {
        if (!_wasInitialized) throw new InvalidOperationException("Runtime not setup.");

        for (int i = 0; i < ViewportCount; i++)
        {
            _runtimeContextes[i].Update(context.GameTime);
            _runtimeContextes[i].Camera.Update(context);
        }

        _components.ForEachRuntime(context);
    }

    public virtual void Draw(RuntimeContext context)
    {
        if (!_wasInitialized) throw new InvalidOperationException("Runtime not setup.");
        if (!RenderVisibility || _spriteBatch == null || _sceneRenderTargets == null) return;

        var graphicsDevice = context.GraphicsDevice;
        if (graphicsDevice.IsDisposed || graphicsDevice.GraphicsDeviceStatus != GraphicsDeviceStatus.Normal)
            return;

        OnRenderPrevious(context);

        var full = graphicsDevice.Viewport.Bounds;
        var viewports = GetSplitViewports(full, ViewportCount);

        // Jede Kamera auf eigenes RenderTarget zeichnen
        for (int i = 0; i < ViewportCount; i++)
        {
            graphicsDevice.SetRenderTarget(_sceneRenderTargets[i]);
            graphicsDevice.Clear(BackgroundColor);

            _runtimeContextes[i].Update(context.GameTime);
            OnViewportRender(_runtimeContextes[i]);
        }

        graphicsDevice.SetRenderTarget(null);
        graphicsDevice.Clear(Color.Black);

        // Sprites auf Bildschirm
        var effect = FullScreenEffectOverride(context);
        _spriteBatch.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp, rasterizerState: new RasterizerState { MultiSampleAntiAlias = true }, effect: effect?.InnerEffect);

        for (int i = 0; i < ViewportCount; i++)
        {
            var dst = viewports[i];
            _spriteBatch.Draw(_sceneRenderTargets[i], dst, Color.White);
        }

        _spriteBatch.End();

        OnRenderPost(context);
    }

    protected virtual void OnRenderPrevious(GameContext context) { }

    protected virtual void OnRenderPost(GameContext context) { }

    protected virtual void OnViewportRender(RuntimeViewportContext context)
    {
        _components.ForEachDrawable(context);
    }

    protected virtual IEffectAdapter? FullScreenEffectOverride(GameContext context) => null;

    private void EnsureSceneRenderTargets()
    {
        if (_graphicsDevice == null || !_wasInitialized)
            throw new InvalidOperationException(" not setup.");

        int width = _graphicsDevice.PresentationParameters.BackBufferWidth;
        int height = _graphicsDevice.PresentationParameters.BackBufferHeight;

        for (int i = 0; i < ViewportCount; i++)
        {
            if (_sceneRenderTargets[i] != null &&
                _sceneRenderTargets[i].Width == width &&
                _sceneRenderTargets[i].Height == height)
                continue;

            _sceneRenderTargets[i]?.Dispose();
            _sceneRenderTargets[i] = new RenderTarget2D(
                _graphicsDevice,
                width,
                height,
                false,
                _graphicsDevice.PresentationParameters.BackBufferFormat,
                _graphicsDevice.PresentationParameters.DepthStencilFormat,
                RenderSampleCount,
                RenderTargetUsage.PlatformContents
            );
        }
    }

    private static Rectangle[] GetSplitViewports(Rectangle bounds, int count)
    {
        int w = bounds.Width;
        int h = bounds.Height;

        return count switch
        {
            1 => new[] { new Rectangle(0, 0, w, h) },
            2 => new[] { new Rectangle(0, 0, w, h / 2), new Rectangle(0, h / 2, w, h / 2) },
            3 => new[] { new Rectangle(0, 0, w / 2, h / 2), new Rectangle(w / 2, 0, w / 2, h / 2), new Rectangle(0, h / 2, w, h / 2) },
            4 => new[] { new Rectangle(0, 0, w / 2, h / 2), new Rectangle(w / 2, 0, w / 2, h / 2), new Rectangle(0, h / 2, w / 2, h / 2), new Rectangle(w / 2, h / 2, w / 2, h / 2) },
            _ => throw new NotSupportedException("ViewportCount > 4 not supported.")
        };
    }

    /// <summary>
    /// Screenshot des kompletten Bildschirms (alle Viewports zusammengesetzt).
    /// </summary>
    public Texture2D CaptureFullScreen()
    {
        if (_graphicsDevice == null || !_wasInitialized) throw new InvalidOperationException("Runtime not setup.");
        if (!IsLoaded) throw new InvalidOperationException("Runtime not loaded.");

        int width = _graphicsDevice.PresentationParameters.BackBufferWidth;
        int height = _graphicsDevice.PresentationParameters.BackBufferHeight;

        Color[] buffer = new Color[width * height];
        _graphicsDevice.GetBackBufferData(buffer); // holt den finalen Bildschirm

        var tex = new Texture2D(_graphicsDevice, width, height);
        tex.SetData(buffer);
        return tex;
    }

    /// <summary>
    /// Screenshot eines einzelnen Viewports (Offscreen-RenderTarget).
    /// </summary>
    public Texture2D CaptureViewportScreenshot(int index)
    {
        if (_sceneRenderTargets == null || index < 0 || index >= _sceneRenderTargets.Length)
            throw new ArgumentOutOfRangeException(nameof(index), "Invalid viewport index.");

        if (_graphicsDevice == null || !_wasInitialized) throw new InvalidOperationException("Runtime not setup.");
        if (!IsLoaded) throw new InvalidOperationException("Runtime not loaded.");

        var rt = _sceneRenderTargets[index];
        int pixelCount = rt.Width * rt.Height;
        Color[] buffer = new Color[pixelCount];

        rt.GetData(buffer);

        var tex = new Texture2D(_graphicsDevice, rt.Width, rt.Height);
        tex.SetData(buffer);
        return tex;
    }

}

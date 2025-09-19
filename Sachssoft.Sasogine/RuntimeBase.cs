using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Diagnostics;
using Sachssoft.Sasogine.Graphics;
using Sachssoft.Sasogine.Surface;
using System;

namespace Sachssoft.Sasogine;

// Die RuntimeBase bildet die zentrale Zeichen- und Steuerlogik für alle Backends des Frameworks ab. 
// Sie verwaltet Kamera-Updates, das Rendern auf einen internen Zwischenspeicher (RenderTarget) sowie das finale 
// Bildschirm-Rendering inklusive optionaler Effekte. Diese abstrakte Basisklasse ist für alle Runtimes wie EditorRuntime 
// oder GameRuntime zuständig und wird im Backend verwendet. Sie enthält keine Benutzereingabe oder UI-Logik.
public abstract class RuntimeBase
{
    private RenderTarget2D? _screenTarget;
    private SpriteBatch? _spriteBatch;
    private readonly CameraBase _camera;
    private readonly IEffectAdapter _effect;
    private readonly RuntimeComponentCollection _components = new();
    private readonly DiagnosticsContext _diagnostics = new();
    private GameBaseContext? _viewContext;

    public RuntimeBase(CameraBase camera, IEffectAdapter? effect)
    {
        _camera = camera ?? throw new ArgumentNullException(nameof(camera));
        _effect = effect ?? new BasicEffectAdapter(IMyGameApp.Current.GraphicsDevice);
        BackgroundColor = Color.CornflowerBlue;
        RenderVisibility = true;
    }

    protected GameBaseContext ViewContext => _viewContext ?? throw new InvalidOperationException("No View Context");

    // Gibt an, ob gerade die Maus über einer UI-Surface schwebt.
    public bool IsAnySurfaceHovered { get; private set; }

    // Gibt an, ob die Runtime aktuell gerendert werden soll.
    public bool RenderVisibility { get; set; }

    // Definiert die Hintergrundfarbe der Runtime.
    public Color BackgroundColor { get; set; }

    // Referenz auf die Kamera, die für die Darstellung verwendet wird.
    public CameraBase Camera => _camera;

    // Optionaler Effekt (Shader), der beim Rendern angewendet wird.
    public IEffectAdapter Effect => _effect;

    public DiagnosticsContext Diagnostics => _diagnostics;

    public RuntimeComponentCollection Components => _components;

    public virtual void Load(GameBaseContext context)
    {
        //var graphicsDevice = GetGraphicsDeviceSafely();
        _spriteBatch = new SpriteBatch(context.GraphicsDevice);
        ResizeRenderTarget(context.GraphicsDevice);
        _viewContext = context;
    }

    public virtual void Unload()
    {
        _screenTarget?.Dispose();
        _screenTarget = null;

        _spriteBatch?.Dispose();
        _spriteBatch = null;
    }

    public virtual void Update(GameFrameContext context)
    {
        Camera?.Update(context);
        _components.ForEachRuntime(context);
    }

    public virtual void Draw(GameFrameContext context)
    {
        if (!RenderVisibility)
            return;

        var graphics_device = context.GraphicsDevice; // GetGraphicsDeviceSafely();
        if (graphics_device.IsDisposed || graphics_device.GraphicsDeviceStatus != GraphicsDeviceStatus.Normal)
            return;

        OnScreenDrawBefore(context);

        ResizeRenderTarget(graphics_device);

        var previous_multi_sample = graphics_device.PresentationParameters.MultiSampleCount;
        graphics_device.PresentationParameters.MultiSampleCount = 2;

        try
        {
            graphics_device.SetRenderTarget(_screenTarget);
            graphics_device.Clear(BackgroundColor);

            OnScreenDraw(context);
        }
        finally
        {
            graphics_device.SetRenderTarget(null);
            graphics_device.PresentationParameters.MultiSampleCount = previous_multi_sample;
        }

        graphics_device.Clear(Color.Black);

        if (_spriteBatch == null)
            _spriteBatch = new SpriteBatch(graphics_device);

        var effect = EffectOverrite(context);
        var rasterizer = new RasterizerState { MultiSampleAntiAlias = true };

        if (effect != null)
        {
            _spriteBatch.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp, rasterizerState: rasterizer, effect: effect.InnerEffect);
        }
        else
        {
            _spriteBatch.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp, rasterizerState: rasterizer);
        }

        _spriteBatch.Draw(_screenTarget!, new Rectangle(0, 0, graphics_device.Viewport.Width, graphics_device.Viewport.Height), Color.White);
        _spriteBatch.End();
    }

    protected virtual void OnScreenDrawBefore(GameFrameContext context)
    {
        // Sinnvoll für eigene RenderTargets
    }

    protected virtual void OnScreenDraw(GameFrameContext context)
    {
        _components.ForEachDrawable(context);

        // Zum Überschreiben vorgesehen
    }

    protected virtual IEffectAdapter? EffectOverrite(GameFrameContext context)
    {
        return null;
    }

    protected void ResizeRenderTarget(GraphicsDevice graphics_device)
    {
        int width = graphics_device.PresentationParameters.BackBufferWidth;
        int height = graphics_device.PresentationParameters.BackBufferHeight;

        if (_screenTarget == null ||
            _screenTarget.Width != width ||
            _screenTarget.Height != height)
        {
            _screenTarget?.Dispose();
            _screenTarget = new RenderTarget2D(
                graphics_device,
                width,
                height,
                false,
                graphics_device.PresentationParameters.BackBufferFormat,
                graphics_device.PresentationParameters.DepthStencilFormat,
                graphics_device.PresentationParameters.MultiSampleCount,
                RenderTargetUsage.PlatformContents);
        }
    }

    //protected static GraphicsDevice GetGraphicsDeviceSafely()
    //{
    //    // Im echten System: hole es aus Singleton oder Context
    //    // Hier z. B. als Platzhalter:
    //    return IMyGameApp.Current.GraphicsDevice
    //        ?? throw new InvalidOperationException("GraphicsDevice is not available.");
    //}

    // Meldet der Runtime, ob die Maus gerade über einer UI-Surface ist.
    // Diese Information ist wichtig, damit die Runtime z.B. Eingaben korrekt verarbeitet:
    // Wenn die Maus über einer UI-Surface schwebt, soll die Runtime Spiel- oder Simulationsinteraktionen pausieren oder ignorieren.
    // Erfolgt keine Meldung, weiß die Runtime nicht, dass die UI den Eingabefokus hat, 
    // was dazu führen kann, dass z.B. Spielaktionen fälschlich ausgelöst werden, obwohl der Nutzer mit der UI interagiert.
    public void ReportInputOverSurface(bool state)
    {
        IsAnySurfaceHovered = state;
    }

    // Screenhot
    public Texture2D GetScreenshotTexture()
    {
        if (_screenTarget == null)
            throw new InvalidOperationException("No render target available for screenshot.");

        var graphics_device = _screenTarget.GraphicsDevice;

        // Hole Pixeldaten vom RenderTarget
        Color[] pixel_data = new Color[_screenTarget.Width * _screenTarget.Height];
        _screenTarget.GetData(pixel_data);

        // Erzeuge neues Texture2D-Objekt
        Texture2D screenshot = new Texture2D(graphics_device, _screenTarget.Width, _screenTarget.Height);
        screenshot.SetData(pixel_data);

        return screenshot;
    }
}

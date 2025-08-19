using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics;
using System;

namespace Sachssoft.Sasogine;

// Die RuntimeBase bildet die zentrale Zeichen- und Steuerlogik für alle Backends des Frameworks ab. 
// Sie verwaltet Kamera-Updates, das Rendern auf einen internen Zwischenspeicher (RenderTarget) sowie das finale 
// Bildschirm-Rendering inklusive optionaler Effekte. Diese abstrakte Basisklasse ist für alle Runtimes wie EditorRuntime 
// oder GameRuntime zuständig und wird im Backend verwendet. Sie enthält keine Benutzereingabe oder UI-Logik.
public abstract class RuntimeBase
{
    private RenderTarget2D? _screen_target;
    private SpriteBatch? _sprite_batch;

    public RuntimeBase(CameraBase camera, IEffect? effect)
    {
        Camera = camera ?? throw new ArgumentNullException(nameof(camera));
        Effect = effect;
        BackgroundColor = Color.CornflowerBlue;
        RenderVisibility = true;
    }

    // Gibt an, ob gerade die Maus über einer UI-Surface schwebt.
    public bool IsAnySurfaceHovered { get; private set; }

    // Gibt an, ob die Runtime aktuell gerendert werden soll.
    public bool RenderVisibility { get; set; }

    // Definiert die Hintergrundfarbe der Runtime.
    public Color BackgroundColor { get; set; }

    // Referenz auf die Kamera, die für die Darstellung verwendet wird.
    public CameraBase Camera { get; }

    // Optionaler Effekt (Shader), der beim Rendern angewendet wird.
    public IEffect? Effect { get; }

    public virtual void Load()
    {
        var graphics_device = GetGraphicsDeviceSafely();
        _sprite_batch = new SpriteBatch(graphics_device);
        ResizeRenderTarget(graphics_device);
    }

    public virtual void Unload()
    {
        _screen_target?.Dispose();
        _screen_target = null;

        _sprite_batch?.Dispose();
        _sprite_batch = null;
    }

    public virtual void Update(GameContext context)
    {
        Camera?.Update(context);
    }

    public virtual void Draw(GameContext context)
    {
        if (!RenderVisibility)
            return;

        var graphics_device = GetGraphicsDeviceSafely();
        if (graphics_device.IsDisposed || graphics_device.GraphicsDeviceStatus != GraphicsDeviceStatus.Normal)
            return;

        ResizeRenderTarget(graphics_device);

        var previous_multi_sample = graphics_device.PresentationParameters.MultiSampleCount;
        graphics_device.PresentationParameters.MultiSampleCount = 2;

        try
        {
            graphics_device.SetRenderTarget(_screen_target);
            graphics_device.Clear(BackgroundColor);

            OnScreenDraw(context);
        }
        finally
        {
            graphics_device.SetRenderTarget(null);
            graphics_device.PresentationParameters.MultiSampleCount = previous_multi_sample;
        }

        graphics_device.Clear(Color.Black);

        if (_sprite_batch == null)
            _sprite_batch = new SpriteBatch(graphics_device);

        var effect = EffectOverrite(context);
        var rasterizer = new RasterizerState { MultiSampleAntiAlias = true };

        if (effect != null)
        {
            _sprite_batch.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp, rasterizerState: rasterizer, effect: effect.Result);
        }
        else
        {
            _sprite_batch.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp, rasterizerState: rasterizer);
        }

        _sprite_batch.Draw(_screen_target!, new Rectangle(0, 0, graphics_device.Viewport.Width, graphics_device.Viewport.Height), Color.White);
        _sprite_batch.End();
    }

    protected virtual void OnScreenDraw(GameContext context)
    {
        // Zum Überschreiben vorgesehen
    }

    protected virtual IEffect? EffectOverrite(GameContext context)
    {
        return null;
    }

    protected void ResizeRenderTarget(GraphicsDevice graphics_device)
    {
        int width = graphics_device.PresentationParameters.BackBufferWidth;
        int height = graphics_device.PresentationParameters.BackBufferHeight;

        if (_screen_target == null ||
            _screen_target.Width != width ||
            _screen_target.Height != height)
        {
            _screen_target?.Dispose();
            _screen_target = new RenderTarget2D(
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

    protected static GraphicsDevice GetGraphicsDeviceSafely()
    {
        // Im echten System: hole es aus Singleton oder Context
        // Hier z. B. als Platzhalter:
        return IMyGameApp.Current.GraphicsDevice
            ?? throw new InvalidOperationException("GraphicsDevice is not available.");
    }

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
        if (_screen_target == null)
            throw new InvalidOperationException("No render target available for screenshot.");

        var graphics_device = _screen_target.GraphicsDevice;

        // Hole Pixeldaten vom RenderTarget
        Color[] pixel_data = new Color[_screen_target.Width * _screen_target.Height];
        _screen_target.GetData(pixel_data);

        // Erzeuge neues Texture2D-Objekt
        Texture2D screenshot = new Texture2D(graphics_device, _screen_target.Width, _screen_target.Height);
        screenshot.SetData(pixel_data);

        return screenshot;
    }
}

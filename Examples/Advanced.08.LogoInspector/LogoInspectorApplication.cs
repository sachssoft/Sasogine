using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Sachssoft.Engine.Graphics.Text;
using Sachssoft.Engine.Scenes;

namespace Sachssoft.Engine.Controls.Examples;
internal sealed class LogoInspectorApplication : GameApplicationBase
{
    private readonly ExampleOptions _options;
    private InspectorResizeChecks? _resizeChecks;
    private readonly ControlInputSampler _input = new();
    private FontFaceControlFont? _font;
    private ControlSkin? _skin;
    private FontFaceControlFont? _titleFont;
    private ControlLayoutContext? _layout;
    private ControlDrawContext? _draw;
    private LogoInspectorView? _view;
    private RenderTarget2D? _renderTarget;
    private Workspace? _workspace;
    private SpriteBatch? _present;
    private RenderTargetColorSource? _pickSource;
    private bool _frameReady;
    private int _snapshotFrames;
    internal LogoInspectorApplication(ExampleOptions options) => _options = options;
    /// <inheritdoc/>
    protected override GraphicsDeviceManager ConfigureGraphicsDevice() => new(this)
    {
        PreferredBackBufferWidth = 1360,
        PreferredBackBufferHeight = 900,
        IsFullScreen = false,
        GraphicsProfile = GraphicsProfile.Reach,
        PreferredDepthStencilFormat = DepthFormat.None,
        SynchronizeWithVerticalRetrace = true
    };
    /// <inheritdoc/>
    protected override ISceneManager CreateScenes(GameConfiguration configuration) => new SceneManager(this, new LogoInspectorScene());
    /// <inheritdoc/>
    protected override void Initialize()
    {
        Window.Title = "Sasogine | Logo Entity Inspector";
        Window.AllowUserResizing = true;
        _view = new LogoInspectorView();
        _workspace = _view.Workspace;
        _workspace.Dpi = _options.Dpi;
        if (_options.VerifyResize) _resizeChecks = new InspectorResizeChecks(_options.Dpi);
        base.Initialize();
        Window.TextInput += OnTextInput;
    }

    /// <inheritdoc/>
    protected override void LoadContent()
    {
        base.LoadContent();
        var face = new FontFace(File.ReadAllBytes(_options.FontPath), Path.GetFileNameWithoutExtension(_options.FontPath));
        _font = new FontFaceControlFont(face, 20);
        _layout = new ControlLayoutContext(_font);
        _draw = new ControlDrawContext(this, _font, GraphicsDevice.Viewport.Bounds);
        _skin = ControlSkin.Load(GraphicsDevice, Path.Combine(AppContext.BaseDirectory, "Skins", "Sachssoft" + _options.SkinId));
        _workspace!.SetSkin(_skin);
        _titleFont = new FontFaceControlFont(new FontFace(File.ReadAllBytes(_options.FontPath), "Logo title"), 48);
        _view!.Canvas.TitleFont = _titleFont;
        _view.AssetGallery.SetGraphicsDevice(GraphicsDevice);
        _present = new SpriteBatch(GraphicsDevice);
        EnsureRenderTarget();
        if (_options.Verify)
        {
            InspectorChecks.Verify(_view!, _layout);
            AssetGraphicsChecks.Verify(GraphicsDevice);
        }
        if (_options.ValueType is string valueType)
        {
            _view!.ValueGallery.Select(valueType);
            _view.InspectorTabs.SelectedIndex = 1;
        }

        if (_options.Assets) _view!.InspectorTabs.SelectedIndex = 2;
    }

    /// <inheritdoc/>
    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        _resizeChecks?.BeforeUpdate(this, _workspace!);
        Rectangle viewport = GraphicsDevice.Viewport.Bounds;
        _frameReady = viewport.Width > 0 && viewport.Height > 0;
        if (!_frameReady || _draw is null || _layout is null || _view is null)
            return;
        // Sample devices once; Workspace owns the DPI transform and physical viewport size.
        EnsureRenderTarget();
        float scale = _workspace!.DpiScale;
        bool snapshot = _options.SnapshotPath is not null;
        MouseState mouse = snapshot ? default : Mouse.GetState();
        KeyboardState keyboard = snapshot ? default : Keyboard.GetState();
        TouchCollection touches = snapshot ? default : TouchPanel.GetState();
        _workspace!.Update(_input, mouse, keyboard, touches, viewport, gameTime.ElapsedGameTime, _layout, snapshot || IsActive);
        _view.Update();
        if (_pickSource is not null)
        {
            Bounds2 b = _view.Canvas.Bounds;
            _pickSource.Viewport = b;
            _pickSource.Source = new Rectangle((int)(b.X * scale), (int)(b.Y * scale), Math.Max(1, (int)(b.Width * scale)), Math.Max(1, (int)(b.Height * scale)));
        }
        _snapshotFrames++;
    }

    private void OnTextInput(object? sender, TextInputEventArgs args)
    {
        if (IsActive && _options.SnapshotPath is null)
            _input.AddText(args.Character);
    }

    /// <inheritdoc/>
    protected override void Draw(GameTime gameTime)
    {
        if (!_frameReady || _draw is null || _view is null)
            return;
        EnsureRenderTarget();
        GraphicsDevice.SetRenderTarget(_renderTarget);
        base.Draw(gameTime);
        _draw.SetFrameTime(gameTime);
        // The sampled target excludes the pipette cursor and magnifier to avoid sampling its own overlay.
        _workspace!.Draw(_draw, includePickingOverlay: false);
        GraphicsDevice.SetRenderTarget(null);
        _present!.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp);
        _present.Draw(_renderTarget, GraphicsDevice.Viewport.Bounds, Color.White);
        _present.End();
        _workspace.DrawColorPickingOverlay(_draw);
        _resizeChecks?.AfterDraw(_view, _draw);
        if (_renderTarget is null || _snapshotFrames < 3 || _resizeChecks is { IsComplete: false } || _options.SnapshotPath is not string path)
            return;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using (var stream = File.Create(path))
            _renderTarget.SaveAsPng(stream, _renderTarget.Width, _renderTarget.Height);
        Console.WriteLine($"PASS: Logo inspector rendered. Snapshot: {path}");
        Exit();
    }

    private void EnsureRenderTarget()
    {
        Rectangle viewport = GraphicsDevice.Viewport.Bounds;
        if (_renderTarget is not null && _renderTarget.Width == viewport.Width && _renderTarget.Height == viewport.Height) return;
        _workspace!.ColorPicking.Cancel();
        _renderTarget?.Dispose();
        _renderTarget = new RenderTarget2D(GraphicsDevice, viewport.Width, viewport.Height, false, SurfaceFormat.Color,
            DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        _pickSource = new RenderTargetColorSource(_renderTarget, _view!.Canvas.Bounds);
        _workspace.ColorPickSource = _pickSource;
    }

    /// <inheritdoc/>
    protected override void UnloadContent()
    {
        Window.TextInput -= OnTextInput;
        _view?.Dispose();
        _titleFont?.Dispose();
        _draw?.Dispose();
        _font?.Dispose();
        _skin?.Dispose();
        _renderTarget?.Dispose();
        _present?.Dispose();
        base.UnloadContent();
    }
}

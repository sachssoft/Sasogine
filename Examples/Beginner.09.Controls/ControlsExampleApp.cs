using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Sachssoft.Engine.Graphics.Text;
using Sachssoft.Engine.Scenes;

namespace Sachssoft.Engine.Controls.Examples;
internal sealed class ControlsExampleApp : GameApplicationBase
{
    private readonly ExampleOptions _options;
    private readonly ControlInputSampler _input = new();
    private FontFaceControlFont? _font;
    private ControlSkin? _skin;
    private ControlLayoutContext? _layout;
    private ControlDrawContext? _draw;
    private ControlsExampleView? _view;
    private RenderTarget2D? _snapshot;
    private Workspace? _workspace;
    private bool _frameReady;
    internal ControlsExampleApp(ExampleOptions options)
    {
        _options = options;
        // Variable updates keep the telemetry interval tied to the actual game loop.
        IsFixedTimeStep = false;
    }

    /// <inheritdoc/>
    protected override GraphicsDeviceManager ConfigureGraphicsDevice() => new(this)
    {
        PreferredBackBufferWidth = 1160,
        PreferredBackBufferHeight = 760,
        IsFullScreen = false,
        GraphicsProfile = GraphicsProfile.Reach,
        PreferredDepthStencilFormat = DepthFormat.None,
        SynchronizeWithVerticalRetrace = true
    };
    /// <inheritdoc/>
    protected override ISceneManager CreateScenes(GameConfiguration configuration) => new SceneManager(this, new ControlsExampleScene());
    /// <inheritdoc/>
    protected override void Initialize()
    {
        Window.Title = "Sasogine Controls | DesktopGL";
        Window.AllowUserResizing = true;
        _view = new ControlsExampleView(ChangeSkin);
        _workspace = _view.Workspace;
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
        ChangeSkin(_options.SkinId);
        _workspace!.Registry.Clipboard = new ExampleClipboardService();
        ((TabControl)_workspace.Content).SelectedIndex = _options.PageIndex;
        if (_options.SnapshotPath is not null)
            _snapshot = new RenderTarget2D(GraphicsDevice, 1160, 760);
    }

    /// <inheritdoc/>
    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        Rectangle viewport = GraphicsDevice.Viewport.Bounds;
        _frameReady = viewport.Width > 0 && viewport.Height > 0;
        if (!_frameReady || _draw is null || _layout is null || _view is null)
            return;
        // Poll devices once. Both layout/input and draw use the same logical scale.
        float scale = MathF.Min(viewport.Width / 1160f, viewport.Height / 760f);
        _draw.SetViewport(viewport, scale);
        bool snapshot = _options.SnapshotPath is not null;
        MouseState mouse = snapshot ? default : Mouse.GetState();
        KeyboardState keyboard = snapshot ? default : Keyboard.GetState();
        TouchCollection touches = snapshot ? default : TouchPanel.GetState();
        _workspace!.Update(_input.SampleReusable(mouse, keyboard, touches, viewport, scale, gameTime.ElapsedGameTime, snapshot || IsActive), _layout);
        _view.Update();
    }

    private void ChangeSkin(string id)
    {
        string folder = id switch
        {
            "dark" => "SachssoftDark",
            "light" => "SachssoftLight",
            "modern" => "SachssoftModern",
            "futuristic" => "SachssoftFuturistic",
            "medieval" => "SachssoftMedieval",
            _ => throw new ArgumentException("Unknown skin ID.", nameof(id))};
        ControlSkin next = ControlSkin.Load(GraphicsDevice, Path.Combine(AppContext.BaseDirectory, "Skins", folder));
        ControlSkin? previous = _skin;
        _workspace!.SetSkin(next);
        _view!.ApplyPalette(next);
        _skin = next;
        Window.Title = "Sasogine Controls | " + next.Name;
        previous?.Dispose();
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
        if (_snapshot is not null)
            GraphicsDevice.SetRenderTarget(_snapshot);
        base.Draw(gameTime);
        _draw.SetFrameTime(gameTime);
        _workspace!.Draw(_draw);
        if (_snapshot is null || _options.SnapshotPath is not string path)
            return;
        GraphicsDevice.SetRenderTarget(null);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using (var stream = File.Create(path))
            _snapshot.SaveAsPng(stream, _snapshot.Width, _snapshot.Height);
        Console.WriteLine($"PASS: Controls example rendered. Snapshot: {path}");
        Exit();
    }

    /// <inheritdoc/>
    protected override void UnloadContent()
    {
        Window.TextInput -= OnTextInput;
        _workspace?.Dispose();
        _draw?.Dispose();
        _font?.Dispose();
        _skin?.Dispose();
        _snapshot?.Dispose();
        base.UnloadContent();
    }
}

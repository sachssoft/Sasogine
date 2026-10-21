using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Sachssoft.Engine.Graphics.Text;
using Sachssoft.Engine.Scenes;

namespace Sachssoft.Engine.Controls.Examples;
internal sealed class JoystickExampleApp : GameApplicationBase
{
    private readonly ExampleOptions _options;
    private readonly ControlInputSampler _input = new();
    private FontFaceControlFont? _font;
    private ControlSkin? _skin;
    private ControlLayoutContext? _layout;
    private ControlDrawContext? _draw;
    private Workspace? _workspace;
    private RenderTarget2D? _snapshot;
    private bool _frameReady;
    internal JoystickExampleApp(ExampleOptions options) => _options = options;
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
    protected override ISceneManager CreateScenes(GameConfiguration configuration) => new SceneManager(this, new JoystickExampleScene());
    /// <inheritdoc/>
    protected override void Initialize()
    {
        Window.Title = "Sasogine Virtual Joysticks | DesktopGL";
        Window.AllowUserResizing = true;
        _workspace = new Workspace(new ControlsJoystickPage());
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
        _skin = ControlSkin.Load(GraphicsDevice, Path.Combine(AppContext.BaseDirectory, "Skins", "SachssoftDark"));
        _workspace!.SetSkin(_skin);
        if (_options.SnapshotPath is not null)
            _snapshot = new RenderTarget2D(GraphicsDevice, 1160, 760);
    }

    /// <inheritdoc/>
    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        Rectangle viewport = GraphicsDevice.Viewport.Bounds;
        _frameReady = viewport.Width > 0 && viewport.Height > 0;
        if (!_frameReady || _draw is null || _layout is null || _workspace is null)
            return;
        // Sample devices once; Workspace owns the DPI transform and physical viewport size.
        bool snapshot = _options.SnapshotPath is not null;
        MouseState mouse = snapshot ? default : Mouse.GetState();
        KeyboardState keyboard = snapshot ? default : Keyboard.GetState();
        TouchCollection touches = snapshot ? default : TouchPanel.GetState();
        _workspace!.Update(_input, mouse, keyboard, touches, viewport, gameTime.ElapsedGameTime, _layout, snapshot || IsActive);
    }

    private void OnTextInput(object? sender, TextInputEventArgs args)
    {
        if (IsActive && _options.SnapshotPath is null)
            _input.AddText(args.Character);
    }

    /// <inheritdoc/>
    protected override void Draw(GameTime gameTime)
    {
        if (!_frameReady || _draw is null || _workspace is null)
            return;
        if (_snapshot is not null)
            GraphicsDevice.SetRenderTarget(_snapshot);
        base.Draw(gameTime);
        _draw.SetFrameTime(gameTime);
        _workspace.Draw(_draw);
        if (_snapshot is null || _options.SnapshotPath is not string path)
            return;
        GraphicsDevice.SetRenderTarget(null);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using (var stream = File.Create(path))
            _snapshot.SaveAsPng(stream, _snapshot.Width, _snapshot.Height);
        Console.WriteLine($"PASS: Joystick example rendered. Snapshot: {path}");
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

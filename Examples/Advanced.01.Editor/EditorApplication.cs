using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Sachssoft.Engine.Graphics.Text;
using Sachssoft.Engine.Scenes;

namespace Sachssoft.Engine.Controls.Examples;
internal sealed class EditorApplication : GameApplicationBase
{
    private readonly ExampleOptions _options;
    private readonly ControlInputSampler _input = new();
    private FontFaceControlFont? _font;
    private ControlSkin? _skin;
    private ControlLayoutContext? _layout;
    private ControlDrawContext? _draw;
    private EditorView? _view;
    private RenderTarget2D? _snapshot;
    private Workspace? _workspace;
    private bool _frameReady;
    internal EditorApplication(ExampleOptions options) => _options = options;
    /// <inheritdoc/>
    protected override GraphicsDeviceManager ConfigureGraphicsDevice() => new(this)
    {
        PreferredBackBufferWidth = 1280,
        PreferredBackBufferHeight = 800,
        IsFullScreen = false,
        GraphicsProfile = GraphicsProfile.Reach,
        PreferredDepthStencilFormat = DepthFormat.None,
        SynchronizeWithVerticalRetrace = true
    };
    /// <inheritdoc/>
    protected override ISceneManager CreateScenes(GameConfiguration configuration) => new SceneManager(this, new EditorScene());
    /// <inheritdoc/>
    protected override void Initialize()
    {
        Window.Title = "Sasogine | Simple 2D Editor";
        Window.AllowUserResizing = true;
        _view = new EditorView(_options.DocumentPath);
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
        _skin = ControlSkin.Load(GraphicsDevice, Path.Combine(AppContext.BaseDirectory, "Skins", "SachssoftDark"));
        _workspace!.SetSkin(_skin);
        _workspace.Registry.Clipboard = new ExampleClipboardService();
        if (_options.Verify)
            _view!.Verify(_layout);
        if (_options.SnapshotPath is not null)
            _snapshot = new RenderTarget2D(GraphicsDevice, 1280, 800);
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
        float scale = MathF.Min(viewport.Width / 1280f, viewport.Height / 800f);
        _draw.SetViewport(viewport, scale);
        bool snapshot = _options.SnapshotPath is not null;
        MouseState mouse = snapshot ? default : Mouse.GetState();
        KeyboardState keyboard = snapshot ? default : Keyboard.GetState();
        TouchCollection touches = snapshot ? default : TouchPanel.GetState();
        _workspace!.Update(_input.SampleReusable(mouse, keyboard, touches, viewport, scale, gameTime.ElapsedGameTime, snapshot || IsActive), _layout);
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
        Console.WriteLine($"PASS: Editor example rendered. Snapshot: {path}");
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

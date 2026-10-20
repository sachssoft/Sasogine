using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Drawing;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Scenes;
namespace LayoutCacheProbe;
/// <summary>Checks that drawing never measures text or rebuilds its prepared placement.</summary>
internal sealed class CacheApplication : GameApplicationBase
{
    private readonly FontSystem _fonts = new();
    private readonly TextBlock _text = new() { Text = "Cached text", Width = 500, Height = 150, Padding = new Insets2(16), Background = new Color(35,49,66), TextColor = Color.White };
    private Workspace? _workspace;
    private ProbeFont? _font;
    private ControlDrawContext? _draw;
    private ControlLayoutContext? _layout;
    private RenderTarget2D? _target;
    protected override GraphicsDeviceManager ConfigureGraphicsDevice() => new(this) { PreferredBackBufferWidth = 640, PreferredBackBufferHeight = 240, SynchronizeWithVerticalRetrace = false };
    protected override ISceneManager CreateScenes(GameConfiguration configuration) => new SceneManager(this, new BenchmarkScene());
    protected override void LoadContent()
    {
        base.LoadContent();
        _fonts.AddFont(File.ReadAllBytes(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "segoeui.ttf")));
        _font = new ProbeFont(_fonts.GetFont(24));
        _layout = new ControlLayoutContext(_font);
        _draw = new ControlDrawContext(this, _font, new Rectangle(0,0,640,240));
        _target = new RenderTarget2D(GraphicsDevice,640,240);
        _workspace = new Workspace(_text);
    }
    private void Prepare() => _workspace!.Update(new ControlInputFrame(new Size2(640,240),TimeSpan.Zero), _layout!);
    private void DrawChecked()
    {
        int before = _font!.Measurements;
        _workspace!.Draw(_draw!);
        if (_font.Measurements != before) throw new InvalidOperationException("Draw measured text.");
    }
    protected override void Draw(GameTime gameTime)
    {
        if (_workspace is null) return;
        GraphicsDevice.SetRenderTarget(_target);
        GraphicsDevice.Clear(new Color(16,23,32));
        Prepare(); DrawChecked(); // Warm glyph generation before the repeated rendering test.
        int baseline = _font!.Measurements;
        Vector2 initial = _font.LastPosition;
        for (int i=0;i<100;i++) { Prepare(); DrawChecked(); }
        if (_font.Measurements != baseline) throw new InvalidOperationException("Unchanged frames remeasured text.");
        _text.TextHorizontalAlignment = ControlAlignment.End;
        Prepare(); DrawChecked();
        if (_font.Measurements != baseline || _font.LastPosition.X <= initial.X) throw new InvalidOperationException("Cached alignment did not update.");
        _text.TextColor = Color.Yellow;
        _text.Opacity = .75f;
        Prepare(); DrawChecked();
        if (_font.Measurements != baseline) throw new InvalidOperationException("Visual change measured text.");
        _text.Text = "Updated cache";
        Prepare(); DrawChecked();
        if (_font.Measurements != baseline + 1 || _font.LastText != "Updated cache") throw new InvalidOperationException("Changed text did not update cache.");
        GraphicsDevice.Clear(new Color(16,23,32)); DrawChecked();
        using (var output = File.Create(Path.Combine(AppContext.BaseDirectory, "cache.png"))) _target!.SaveAsPng(output,640,240);
        GraphicsDevice.SetRenderTarget(null);
        Console.WriteLine($"PASS: {_font.Draws} real draws; no measurement during Draw; stable frames reuse text metrics; alignment, color and text changes verified.");
        Exit();
    }
    protected override void UnloadContent()
    {
        _workspace?.Dispose(); _draw?.Dispose(); _target?.Dispose(); _fonts.Dispose(); base.UnloadContent();
    }
}

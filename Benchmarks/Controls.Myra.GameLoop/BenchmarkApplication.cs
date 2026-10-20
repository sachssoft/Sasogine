using System.Diagnostics;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI.Styles;
using Sachssoft.Engine.Controls.Drawing;
using Sachssoft.Engine.Scenes;

namespace Sachssoft.Engine.Controls.Benchmarks;
/// <summary>Runs one UI update and one draw per real game frame, with normal Present.</summary>
internal sealed class BenchmarkApplication : GameApplicationBase
{
    private readonly BenchmarkOptions _options;
    private readonly FontSystem _fonts = new();
    private readonly List<Measurement> _results = [];
    private SharedFont? _font;
    private SpriteFontBase? _heading;
    private SpriteBatch? _dashboard;
    private Stylesheet? _styles;
    private ControlDrawContext? _draw;
    private IBenchmarkTree? _sasogine, _myra, _active;
    private int _countIndex, _block, _frame;
    private long _blockStart, _uiTicks, _updateTicks;
    private double _calls, _textures, _shaders;
    private bool _next = true, _finished, _saved;
    private bool _measuring;
    /// <summary>Configures an uncapped game loop, including when the window is not focused.</summary>
    public BenchmarkApplication(BenchmarkOptions options)
    {
        _options = options;
        IsFixedTimeStep = false;
        InactiveSleepTime = TimeSpan.Zero;
    }
    /// <inheritdoc/>
    protected override GraphicsDeviceManager ConfigureGraphicsDevice() => new(this)
    {
        PreferredBackBufferWidth = 1200, PreferredBackBufferHeight = 800,
        SynchronizeWithVerticalRetrace = false, GraphicsProfile = GraphicsProfile.Reach,
        PreferredDepthStencilFormat = DepthFormat.None
    };
    /// <inheritdoc/>
    protected override ISceneManager CreateScenes(GameConfiguration configuration) => new SceneManager(this, new BenchmarkScene());
    /// <inheritdoc/>
    protected override void LoadContent()
    {
        base.LoadContent();
        IsMouseVisible = true;
        Window.AllowUserResizing = false;
        MyraEnvironment.Game = this;
        MyraEnvironment.SetMouseCursorFromWidget = false;
        MyraEnvironment.MouseInfoGetter = static () => new Myra.Graphics2D.UI.MouseInfo { Position = new Point(-10,-10) };
        MyraEnvironment.DownKeysGetter = static keys => Array.Clear(keys);
        MyraEnvironment.ImageTextureFiltering = TextureFiltering.Linear;
        MyraEnvironment.TextTextureFiltering = TextureFiltering.Linear;
        _fonts.AddFont(File.ReadAllBytes(_options.FontPath));
        _font = new SharedFont(_fonts.GetFont(10));
        _heading = _fonts.GetFont(22);
        _dashboard = new SpriteBatch(GraphicsDevice);
        var brush = new SolidBrush(new Color(35,49,66));
        _styles = new Stylesheet
        {
            ButtonStyle = new ButtonStyle { Background = brush, OverBackground = brush, PressedBackground = brush },
            LabelStyle = new LabelStyle { Font = _font.Font, TextColor = Color.White }
        };
        Stylesheet.Current = _styles;
        _draw = new ControlDrawContext(this,_font,new Rectangle(0,0,1200,800));
        Directory.CreateDirectory(_options.OutputPath);
    }
    private void NextBlock()
    {
        if (_block == _options.Rounds * 2)
        {
            _sasogine!.Verify(); _myra!.Verify();
            _sasogine.Dispose(); _myra.Dispose();
            _sasogine = _myra = _active = null;
            _block = 0; _countIndex++;
        }
        if (_countIndex == _options.Counts.Length) { _finished = true; return; }
        int count = _options.Counts[_countIndex];
        if (_sasogine is null)
        {
            var spec = new TreeSpec(count);
            _sasogine = new SasogineTree(spec,_font!,_draw!);
            _myra = new MyraTree(spec,_font!,_styles!);
            _sasogine.Prepare(); _myra.Prepare();
            _sasogine.Verify(); _myra.Verify();
        }
        int round = _block / 2;
        _active = ((_block % 2) ^ (round % 2)) == 0 ? _sasogine : _myra;
        _active!.Prepare();
        _frame = 0; _uiTicks = 0; _calls = _textures = _shaders = 0;
        Window.Title = $"GAME LOOP | {_active!.Library} | {count} buttons | Round {round+1}/{_options.Rounds}";
        _next = false;
    }
    /// <inheritdoc/>
    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape)) { Exit(); return; }
        if (_next && !_finished) NextBlock();
        _measuring = !_finished && _frame >= _options.Warmup;
        if (_measuring && _frame == _options.Warmup) _blockStart = Stopwatch.GetTimestamp();
        base.Update(gameTime);
        if (_finished) return;
        long start = Stopwatch.GetTimestamp();
        _active!.Update();
        _updateTicks = Stopwatch.GetTimestamp() - start;
    }
    /// <inheritdoc/>
    protected override void Draw(GameTime gameTime)
    {
        if (_draw is null) return;
        if (_finished)
        {
            if (!_saved)
            {
                BenchmarkReport.Write(_options,_results,GraphicsDevice.Adapter.Description);
                using var target = new RenderTarget2D(GraphicsDevice,1200,800);
                GraphicsDevice.SetRenderTarget(target);
                DrawDashboard();
                using (var file = File.Create(Path.Combine(_options.OutputPath,"Ergebnis.png"))) target.SaveAsPng(file,1200,800);
                GraphicsDevice.SetRenderTarget(null);
                _saved = true;
            }
            DrawDashboard();
            return;
        }
        GraphicsDevice.Clear(new Color(16,23,32));
        _draw.SetFrameTime(gameTime);
        var before = GraphicsDevice.Metrics;
        long start = Stopwatch.GetTimestamp();
        _active!.Draw();
        long elapsed = Stopwatch.GetTimestamp() - start;
        var difference = GraphicsDevice.Metrics - before;
        if (_measuring)
        {
            _uiTicks += _updateTicks + elapsed;
            _calls += difference.DrawCount;
            _textures += difference.TextureCount;
            _shaders += difference.VertexShaderCount;
        }
    }
    /// <inheritdoc/>
    protected override void EndDraw()
    {
        base.EndDraw(); // Includes normal swap/present; no forced readback in measured frames.
        long end = Stopwatch.GetTimestamp();
        if (_finished) { if (_saved && _options.ExitWhenFinished) Exit(); return; }
        _frame++;
        if (_frame < _options.Warmup + _options.Frames) return;
        var sample = new Measurement(_active!.Library,_options.Counts[_countIndex],_block/2+1,_options.Frames,
            (end-_blockStart)*1000d/Stopwatch.Frequency/_options.Frames,
            _uiTicks*1000d/Stopwatch.Frequency/_options.Frames,
            _calls/_options.Frames,_textures/_options.Frames,_shaders/_options.Frames);
        _results.Add(sample);
        Console.WriteLine($"{sample.Buttons} | {sample.Library} | round {sample.Round} | frame {sample.Milliseconds:F3} ms | UI {sample.UiMilliseconds:F3} ms | calls {sample.DrawCalls:F0}");
        _block++; _next = true;
    }
    private void DrawDashboard()
    {
        GraphicsDevice.Clear(new Color(16,23,32));
        _dashboard!.Begin();
        Text("SASOGINE / MYRA - LIVE GAME LOOP",48,44,Color.Cyan);
        Text("Update + draw + normal Present | VSync off | Lower time wins",48,95,Color.LightGray);
        Text("Buttons",48,175,Color.Gray); Text("Sasogine frame",220,175,Color.Gray);
        Text("Myra frame",440,175,Color.Gray); Text("Median winner",660,175,Color.Gray); Text("Less time",985,175,Color.Gray);
        int y=235;
        foreach (int count in _options.Counts)
        {
            var c = new RenderComparison(count,_results);
            Text(count.ToString(),48,y,Color.White);
            Text($"{c.SasogineMs:F3} ms",220,y,Color.White);
            Text($"{c.MyraMs:F3} ms",440,y,Color.White);
            Text(c.Winner,660,y,Color.LightGreen); Text($"{c.Percent:F1}%",985,y,Color.LightGreen);
            double sc = BenchmarkReport.Median(_results.Where(x=>x.Buttons==count && x.Library=="Sasogine").Select(x=>x.DrawCalls));
            double mc = BenchmarkReport.Median(_results.Where(x=>x.Buttons==count && x.Library=="Myra").Select(x=>x.DrawCalls));
            Text($"Draw calls: {sc:F0} / {mc:F0}"+(c.Overlap ? " | Batch ranges overlap" : ""),220,y+36,Color.Gray);
            y+=110;
        }
        Text("Current Sasogine layout/text cache | Myra 1.6.8 | Same font and geometry",48,635,Color.LightGray);
        Text("Measured UI workload only; no game simulation or isolated GPU timings",48,680,Color.Gray);
        Text("Finished | Results saved | ESC: close",48,735,Color.Cyan);
        _dashboard.End();
    }
    private void Text(string value,int x,int y,Color color) => _heading!.DrawText(_dashboard!,value,new Vector2(x,y),color);
    /// <inheritdoc/>
    protected override void UnloadContent()
    {
        _sasogine?.Dispose(); _myra?.Dispose(); _draw?.Dispose(); _dashboard?.Dispose();
        MyraEnvironment.Reset(); _fonts.Dispose(); base.UnloadContent();
    }
}


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
    private ControlDrawContext? _draw, _legacyDraw;
    private IBenchmarkTree? _active;
    private IBenchmarkTree[]? _trees;
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
        PreferredBackBufferWidth = 1200,
        PreferredBackBufferHeight = 800,
        SynchronizeWithVerticalRetrace = false,
        GraphicsProfile = GraphicsProfile.Reach,
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
        MyraEnvironment.MouseInfoGetter = static () => new Myra.Graphics2D.UI.MouseInfo
        {
            Position = new Point(-10, -10)
        };
        MyraEnvironment.DownKeysGetter = static keys => Array.Clear(keys);
        MyraEnvironment.ImageTextureFiltering = TextureFiltering.Linear;
        MyraEnvironment.TextTextureFiltering = TextureFiltering.Linear;
        _fonts.AddFont(File.ReadAllBytes(_options.FontPath));
        _font = new SharedFont(_fonts.GetFont(10));
        _heading = _fonts.GetFont(22);
        _dashboard = new SpriteBatch(GraphicsDevice);
        var brush = new SolidBrush(new Color(35, 49, 66));
        _styles = new Stylesheet
        {
            ButtonStyle = new ButtonStyle
            {
                Background = brush,
                OverBackground = brush,
                PressedBackground = brush
            },
            LabelStyle = new LabelStyle
            {
                Font = _font.Font,
                TextColor = Color.White
            }
        };
        Stylesheet.Current = _styles;
        _draw = new ControlDrawContext(this, _font, new Rectangle(0, 0, 1200, 800));
        _legacyDraw = new ControlDrawContext(this, _font, new Rectangle(0, 0, 1200, 800), 1, ControlRenderer.SpriteBatch);
        Directory.CreateDirectory(_options.OutputPath);
    }

    private void NextBlock()
    {
        if (_block == _options.Rounds * 3)
        {
            foreach (var tree in _trees!)
            {
                tree.Verify();
                tree.Dispose();
            }

            _trees = null;
            _active = null;
            _block = 0;
            _countIndex++;
        }

        if (_countIndex == _options.Counts.Length)
        {
            _finished = true;
            return;
        }

        int count = _options.Counts[_countIndex];
        if (_trees is null)
        {
            var spec = new TreeSpec(count);
            _trees = [new SasogineTree(spec, _font!, _legacyDraw!, "Sasogine SpriteBatch"), new SasogineTree(spec, _font!, _draw!, "Sasogine FrameBatch"), new MyraTree(spec, _font!, _styles!)];
            foreach (var tree in _trees)
            {
                tree.Prepare();
                tree.Verify();
            }
        }

        int round = _block / 3, position = _block % 3;
        int library = (position + round) % 3;
        if ((round & 1) != 0)
            library = 2 - library;
        _active = _trees[library];
        _active.Prepare();
        _frame = 0;
        _uiTicks = 0;
        _calls = _textures = _shaders = 0;
        Window.Title = $"FRAME BATCH | {_active.Library} | {count} buttons | Round {round + 1}/{_options.Rounds}";
        _next = false;
    }

    /// <inheritdoc/>
    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            Exit();
            return;
        }

        if (_next && !_finished)
            NextBlock();
        _measuring = !_finished && _frame >= _options.Warmup;
        if (_measuring && _frame == _options.Warmup)
            _blockStart = Stopwatch.GetTimestamp();
        base.Update(gameTime);
        if (_finished)
            return;
        long start = Stopwatch.GetTimestamp();
        _active!.Update();
        _updateTicks = Stopwatch.GetTimestamp() - start;
    }

    /// <inheritdoc/>
    protected override void Draw(GameTime gameTime)
    {
        if (_draw is null)
            return;
        if (_finished)
        {
            if (!_saved)
            {
                BenchmarkReport.Write(_options, _results, GraphicsDevice.Adapter.Description);
                using var target = new RenderTarget2D(GraphicsDevice, 1200, 800);
                GraphicsDevice.SetRenderTarget(target);
                DrawDashboard();
                using (var file = File.Create(Path.Combine(_options.OutputPath, "Ergebnis.png")))
                    target.SaveAsPng(file, 1200, 800);
                GraphicsDevice.SetRenderTarget(null);
                _saved = true;
            }

            DrawDashboard();
            return;
        }

        GraphicsDevice.Clear(new Color(16, 23, 32));
        _draw.SetFrameTime(gameTime);
        _legacyDraw!.SetFrameTime(gameTime);
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
        if (_finished)
        {
            if (_saved && _options.ExitWhenFinished)
                Exit();
            return;
        }

        _frame++;
        if (_frame < _options.Warmup + _options.Frames)
            return;
        var sample = new Measurement(_active!.Library, _options.Counts[_countIndex], _block / 3 + 1, _options.Frames, (end - _blockStart) * 1000d / Stopwatch.Frequency / _options.Frames, _uiTicks * 1000d / Stopwatch.Frequency / _options.Frames, _calls / _options.Frames, _textures / _options.Frames, _shaders / _options.Frames);
        _results.Add(sample);
        Console.WriteLine($"{sample.Buttons} | {sample.Library} | round {sample.Round} | frame {sample.Milliseconds:F3} ms | UI {sample.UiMilliseconds:F3} ms | calls {sample.DrawCalls:F0}");
        _block++;
        _next = true;
    }

    private void DrawDashboard()
    {
        GraphicsDevice.Clear(new Color(16, 23, 32));
        _dashboard!.Begin();
        Text("SASOGINE FRAME BATCH / SPRITEBATCH / MYRA", 48, 44, Color.Cyan);
        Text("Current cache in both Sasogine paths | Real update / draw / Present", 48, 95, Color.LightGray);
        Text("Buttons", 48, 175, Color.Gray);
        Text("SpriteBatch", 200, 175, Color.Gray);
        Text("FrameBatch", 430, 175, Color.Gray);
        Text("Myra", 660, 175, Color.Gray);
        Text("Median winner", 900, 175, Color.Gray);
        int y = 235;
        foreach (int count in _options.Counts)
        {
            var rows = BenchmarkReport.Summaries(_results, count);
            Text(count.ToString(), 48, y, Color.White);
            for (int i = 0; i < 3; i++)
                Text($"{rows[i].Milliseconds:F3} ms", 200 + i * 230, y, i == 1 ? Color.LightGreen : Color.White);
            var winner = rows.MinBy(x => x.Milliseconds)!;
            Text(winner.Library.Replace("Sasogine ", ""), 900, y, Color.LightGreen);
            Text($"Draw calls: {rows[0].DrawCalls:F0} / {rows[1].DrawCalls:F0} / {rows[2].DrawCalls:F0}", 200, y + 36, Color.Gray);
            y += 110;
        }

        Text("Overlapping content retains painter order; only disjoint geometry can regroup", 48, 635, Color.LightGray);
        Text("UI workload only | No isolated GPU timestamps | See raw.csv and report", 48, 680, Color.Gray);
        Text("Finished | Results saved | ESC: close", 48, 735, Color.Cyan);
        _dashboard.End();
    }

    private void Text(string value, int x, int y, Color color) => _heading!.DrawText(_dashboard!, value, new Vector2(x, y), color);
    /// <inheritdoc/>
    protected override void UnloadContent()
    {
        if (_trees is not null)
            foreach (var tree in _trees)
                tree.Dispose();
        _draw?.Dispose();
        _legacyDraw?.Dispose();
        _dashboard?.Dispose();
        MyraEnvironment.Reset();
        _fonts.Dispose();
        base.UnloadContent();
    }
}

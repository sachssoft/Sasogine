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
/// <summary>Runs both libraries on one graphics device with one offscreen target and shared font.</summary>
internal sealed class BenchmarkApplication : GameApplicationBase
{
    private readonly BenchmarkOptions _options;
    private readonly FontSystem _fonts = new();
    private SpriteBatch? _dashboard;
    private SpriteFontBase? _dashboardFont;
    private MeasurementRunner? _runner;
    private IEnumerator<string>? _steps;
    private string _status = "Starting rendering benchmark";
    private SharedFont? _font;
    private Stylesheet? _styles;
    private ControlDrawContext? _draw;
    private RenderTarget2D? _target;
    private readonly Color[] _drain = new Color[1];
    private bool _finished;
    /// <summary>Initializes a new benchmark application instance.</summary>
    public BenchmarkApplication(BenchmarkOptions options)
    {
        _options = options;
        IsFixedTimeStep = false;
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
    protected override void Initialize()
    {
        Window.Title = "Sasogine / Myra benchmark Ã¢â‚¬â€œ results are written to disk";
        base.Initialize();
    }

    /// <inheritdoc/>
    protected override void LoadContent()
    {
        base.LoadContent();
        MyraEnvironment.Game = this;
        MyraEnvironment.SetMouseCursorFromWidget = false;
        MyraEnvironment.MouseInfoGetter = static () => new Myra.Graphics2D.UI.MouseInfo { Position = new Point(-10, -10) };
        MyraEnvironment.DownKeysGetter = static keys => Array.Clear(keys);
        MyraEnvironment.ImageTextureFiltering = TextureFiltering.Linear;
        MyraEnvironment.TextTextureFiltering = TextureFiltering.Linear;
        _fonts.AddFont(File.ReadAllBytes(_options.FontPath));
        _font = new SharedFont(_fonts.GetFont(10));
        _dashboard = new SpriteBatch(GraphicsDevice);
        _dashboardFont = _fonts.GetFont(22);
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
        _target = new RenderTarget2D(GraphicsDevice, 1200, 800, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
    }

    /// <inheritdoc/>
    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();
        base.Update(gameTime);
    }

    /// <inheritdoc/>
    protected override void Draw(GameTime gameTime)
    {
        if (_target is null || _draw is null || _font is null || _styles is null)
            return;
        bool completedNow = false;
        if (!_finished)
        {
            GraphicsDevice.SetRenderTarget(_target);
            _draw.SetFrameTime(gameTime);
            if (_runner is null)
            {
                Directory.CreateDirectory(_options.OutputPath);
                _runner = new MeasurementRunner(_options, DrainGpu);
                _steps = RunMeasurements().GetEnumerator();
            }
            if (_steps!.MoveNext())
                _status = _steps.Current;
            else
            {
                _steps.Dispose();
                _steps = null;
                _finished = true;
                completedNow = true;
                BenchmarkReport.Write(_options, _runner.Results, GraphicsDevice.Adapter.Description);
                Console.WriteLine($"Results: {_options.OutputPath}");
                _status = "Completed. Results saved: Vergleich.md / raw.csv / summary.csv";
            }
        }
        if (completedNow)
        {
            GraphicsDevice.SetRenderTarget(_target);
            DrawDashboard();
            DrainGpu();
            using var file = File.Create(Path.Combine(_options.OutputPath, "Ergebnis.png"));
            _target.SaveAsPng(file, 1200, 800);
        }
        GraphicsDevice.SetRenderTarget(null);
        DrawDashboard();
        if (completedNow && _options.ExitWhenFinished) Exit();
    }

    private IEnumerable<string> RunMeasurements()
    {
        foreach (int count in _options.Counts)
        {
            var spec = new TreeSpec(count);
            foreach (string status in _runner!.Run(spec,
                () => new SasogineTree(spec, _font!, _draw!),
                () => new MyraTree(spec, _font!, _styles!), tree => Snapshot(tree, count)))
                yield return status;
        }
    }

    private void DrawDashboard()
    {
        GraphicsDevice.Clear(new Color(16, 23, 32));
        _dashboard!.Begin();
        Text("SASOGINE / MYRA", 48, 42, new Color(77, 204, 220));
        Text("RENDERING BENCHMARK", 48, 82, Color.White);
        Text("CPU draw time per frame | Lower is better | Median of batch means", 48, 136, Color.LightGray);
        Text("Buttons", 48, 212, Color.LightGray);
        Text("Sasogine", 220, 212, Color.LightGray);
        Text("Myra", 420, 212, Color.LightGray);
        Text("Median winner", 620, 212, Color.LightGray);
        Text("Less time", 946, 212, Color.LightGray);
        int y = 274;
        foreach (int count in _options.Counts)
        {
            var c = new RenderComparison(count, _runner!.Results);
            Text(count.ToString(), 48, y, Color.White);
            if (c.Ready)
            {
                Text($"{c.SasogineMs:F3} ms", 220, y, Color.White);
                Text($"{c.MyraMs:F3} ms", 420, y, Color.White);
                Text(c.Winner, 620, y, new Color(123, 222, 161));
                Text($"{c.Percent:F1}%", 946, y, new Color(123, 222, 161));
                Text($"Difference: {c.Difference:F3} ms | Batches: {c.Sasogine.Length}/{c.Myra.Length}" + (c.Overlap ? " | Ranges overlap" : ""), 220, y + 35, Color.Gray);
            }
            else Text("Pending...", 220, y, Color.Gray);
            y += 100;
        }
        Text(_status, 48, 626, Color.White);
        int total = _options.Counts.Length * _options.Rounds * 2;
        Text($"Batches: {_runner!.Results.Count}/{total} | {_options.Frames} draws/batch | Layout/input excluded", 48, 672, Color.LightGray);
        Text(_finished ? "Finished | ESC: close | Output directory printed in console" : "Running sequentially... | ESC: stop", 48, 730, new Color(77, 204, 220));
        _dashboard.End();
    }

    private void Text(string text, int x, int y, Color color) => _dashboardFont!.DrawText(_dashboard!, text, new Vector2(x, y), color);

    private void DrainGpu() => _target!.GetData(0, new Rectangle(0, 0, 1, 1), _drain, 0, 1);
    private void Snapshot(IBenchmarkTree tree, int count)
    {
        GraphicsDevice.Clear(new Color(16, 23, 32));
        tree.Draw();
        DrainGpu();
        using var stream = File.Create(Path.Combine(_options.OutputPath, $"{tree.Library}-{count}.png"));
        _target!.SaveAsPng(stream, 1200, 800);
    }

    /// <inheritdoc/>
    protected override void UnloadContent()
    {
        _steps?.Dispose();
        _dashboard?.Dispose();
        _draw?.Dispose();
        _target?.Dispose();
        MyraEnvironment.Reset();
        _fonts.Dispose();
        base.UnloadContent();
    }
}


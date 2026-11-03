using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine;
using Sachssoft.Engine.Graphics.Brushes;
using Sachssoft.Engine.Graphics.Rendering;
using Sachssoft.Engine.Graphics.Rendering.Batches;

namespace Sachssoft.Sasogine.ShapeBatchTests;

/// <summary>Runs finite offscreen GPU readback tests and saves a gallery before exiting automatically.</summary>
internal sealed class RenderChecks : Game
{
    private readonly GraphicsDeviceManager _manager;
    private readonly TestCamera _camera = new();
    private RenderTarget2D? _target;
    private ShapeBatch? _batch;
    private TestShader? _shader;
    private bool _done;
    private int _renders;

    internal RenderChecks()
    {
        _manager = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 64, PreferredBackBufferHeight = 64,
            SynchronizeWithVerticalRetrace = false, GraphicsProfile = GraphicsProfile.HiDef
        };
        IsFixedTimeStep = false;
        Window.Title = "ShapeBatch verification";
    }

    protected override void LoadContent()
    {
        _target = new RenderTarget2D(GraphicsDevice, 512, 512, false, SurfaceFormat.Color, DepthFormat.None);
        _batch = new ShapeBatch(GraphicsDevice);
        _shader = new TestShader(GraphicsDevice);
    }

    protected override void Draw(GameTime gameTime)
    {
        if (_done) return;
        _done = true;
        Color red = new(128, 0, 0, 128);
        var pen = new Pen(new SolidColorBrush(red), 12);
        CheckAlpha("sharp-rectangle", b => b.AddStrokeRectangle(new(30, 30, 180, 100), pen));
        foreach (CornerType corner in Enum.GetValues<CornerType>())
        {
            CheckAlpha("corner-" + corner, b => b.AddStrokeRectangle(new(30, 30, 180, 100), pen, corner, 25));
            CheckAlpha("thick-" + corner, b => b.AddStrokeRectangle(new(30, 30, 50, 20), new Pen(new SolidColorBrush(red), 60), corner, 25));
            CheckAlpha("transformed-" + corner, b => b.AddStrokeRectangle(new(0, 0, 120, 80), pen, corner, 15,
                Matrix.CreateScale(1.5f, .7f, 1) * Matrix.CreateRotationZ(.4f) * Matrix.CreateTranslation(150, 150, 0)));
        }
        Point2[] line = [new(30, 30), new(180, 30), new(180, 180)];
        foreach (LineJoin join in Enum.GetValues<LineJoin>())
        foreach (LineCap cap in Enum.GetValues<LineCap>())
        {
            var stroke = new Pen(new SolidColorBrush(red), 12) { Join = join, Cap = cap };
            CheckAlpha($"line-{join}-{cap}", b => b.AddLine(line, stroke));
        }
        Point2[] cross = [new(30, 30), new(180, 180), new(30, 180), new(180, 30)];
        CheckAlpha("crossing-line", b => b.AddLine(cross, pen));
        CheckAlpha("ellipse", b => b.AddStrokeEllipse(new Bounds2(30, 30, 120, 80), pen));
        CheckAlpha("thick-ellipse", b => b.AddStrokeEllipse(new Bounds2(60, 60, 80, 20),
            new Pen(new SolidColorBrush(red), 100)));
        var cornerJoin = Render(b => b.AddStrokeRectangle(new(30, 30, 120, 80),
            12, CornerType.Bevel, 20, LineJoin.Round));
        GeometryChecks.Check(cornerJoin.Any(p => p.A > 0) && cornerJoin[70 * 512 + 90].A == 0,
            "corner and join overload renders an outline with an empty interior");
        foreach (LineCap cap in Enum.GetValues<LineCap>())
            CheckAlpha("single-line-" + cap, b => b.AddLine(new Point2(30, 30), new Point2(180, 110),
                new Pen(new SolidColorBrush(red), 12) { Cap = cap }));
        BrushAndPathChecks();
        ApiChecks();
        SaveGallery();
        Console.WriteLine($"GPU PASS: {_renders} offscreen render/readback checks. Gallery: {Path.Combine(AppContext.BaseDirectory, "artifacts", "shape-gallery.png")}");
        Exit();
    }

    private Color[] Render(Action<ShapeBatch> draw)
    {
        GraphicsDevice.SetRenderTarget(_target);
        GraphicsDevice.Clear(Color.Transparent);
        GraphicsDevice.BlendState = BlendState.AlphaBlend;
        GraphicsDevice.DepthStencilState = DepthStencilState.None;
        GraphicsDevice.RasterizerState = RasterizerState.CullNone;
        _batch!.Begin(_shader!, _camera);
        draw(_batch);
        _batch.End();
        GraphicsDevice.SetRenderTarget(null);
        var pixels = new Color[512 * 512];
        _target!.GetData(pixels);
        _renders++;
        return pixels;
    }

    private void CheckAlpha(string name, Action<ShapeBatch> draw)
    {
        Color[] pixels = Render(draw);
        Console.WriteLine($"GPU {name}: alpha {pixels.Min(p => p.A)}..{pixels.Max(p => p.A)}, background {pixels[0]}, stroke {pixels[30 * 512 + 60]}");
        if (pixels.Any(p => p.A > 129))
        {
            string directory = Path.Combine(AppContext.BaseDirectory, "artifacts");
            Directory.CreateDirectory(directory);
            using var output = File.Create(Path.Combine(directory, name + "-failed.png"));
            _target!.SaveAsPng(output, 512, 512);
        }
        int covered = 0;
        byte maximum = 0;
        foreach (Color pixel in pixels)
        {
            maximum = Math.Max(maximum, pixel.A);
            if (pixel.A > 0) covered++;
        }
        GeometryChecks.Check(maximum <= 129, name + " alpha overdraw: " + maximum);
        GeometryChecks.Check(covered > 0, name + " must produce visible pixels");
    }

    private void BrushAndPathChecks()
    {
        using var texture = new Texture2D(GraphicsDevice, 1, 1);
        texture.SetData(new[] { Color.Lime });
        Color[] mixed = Render(b =>
        {
            b.AddFillRectangle(new(20, 20, 60, 60), new SolidColorBrush(Color.Red), CornerType.Rounded, 8);
            b.AddFillRectangle(new(100, 20, 60, 60), new TextureBrush(texture), CornerType.Bevel, 8);
            b.AddFillRectangle(new(180, 20, 60, 60), new SolidColorBrush(Color.Blue));
            b.AddFillRectangle(new(20, 100, 200, 60), new LinearGradientBrush(Color.Red, Color.Blue), CornerType.Rounded, 10);
        });
        GeometryChecks.Check(mixed[50 * 512 + 50].R > 240, "solid brush before texture");
        GeometryChecks.Check(mixed[50 * 512 + 130].G > 240, "texture brush binds actual texture");
        GeometryChecks.Check(mixed[50 * 512 + 210].B > 240, "solid brush resets texture binding");
        GeometryChecks.Check(mixed[130 * 512 + 40].R > mixed[130 * 512 + 40].B, "gradient starts red");
        GeometryChecks.Check(mixed[130 * 512 + 200].B > mixed[130 * 512 + 200].R, "gradient ends blue");
        Point2[] outer = [new(20, 200), new(220, 200), new(220, 400), new(20, 400)];
        Point2[] hole = [new(70, 250), new(170, 250), new(170, 350), new(70, 350)];
        var path = new Sachssoft.Engine.Geometry.Path(new[] { outer, hole });
        var fill = Render(b => b.AddFillPolygon(path, new SolidColorBrush(Color.Red)));
        GeometryChecks.Check(fill[220 * 512 + 30].A > 240, "filled path outer region");
        GeometryChecks.Check(fill[300 * 512 + 120].A == 0, "filled path retains inner hole");
        CheckAlpha("multi-contour-path", b => b.AddStrokePolygon(path,
            new Pen(new SolidColorBrush(new Color(128, 0, 0, 128)), 70) { Join = LineJoin.Round }));
    }

    private void ApiChecks()
    {
        var pixels = Render(b =>
        {
            Expect<ArgumentOutOfRangeException>(() => b.AddStrokeRectangle(new(0, 0, 50, 50), float.NaN));
            Expect<ArgumentOutOfRangeException>(() => b.AddLine(new Point2(0, 0), new Point2(1, 1), float.PositiveInfinity));
            Expect<ArgumentOutOfRangeException>(() => b.AddFillRectangle(new(0, 0, 10, 10), (CornerType)99, 2));
            Expect<ArgumentOutOfRangeException>(() => b.AddFillRectangle(new(0, 0, 10, 10), CornerType.Rounded, -2));
            Expect<ArgumentOutOfRangeException>(() => b.AddFillEllipse(new Point2(0, 0), new Vector2(float.NaN, 2)));
            Expect<ArgumentOutOfRangeException>(() => b.AddStrokeEllipse(new Point2(0, 0), new Vector2(2, 2), float.NaN));
            Expect<ArgumentOutOfRangeException>(() => b.AddLine(Array.Empty<Point2>(), float.NaN));
            var invalid = Matrix.Identity;
            invalid.M11 = float.NaN;
            Expect<ArgumentOutOfRangeException>(() => b.AddFillRectangle(new(0, 0, 10, 10), invalid));
            Expect<InvalidOperationException>(() => b.AddFillRectangle(new(300, 300, 20, 20), new ThrowingBrush()));
            b.AddFillRectangle(new(20, 20, 40, 40), new SolidColorBrush(Color.Red), CornerType.Rounded, 8);
        });
        GeometryChecks.Check(pixels[30 * 512 + 30].R > 200, "batch remains usable after rejected input and brush failure");
        GeometryChecks.Check(pixels[305 * 512 + 305].A == 0, "failed brush leaves no geometry behind");
        Expect<InvalidOperationException>(() => _batch!.End());
        Expect<InvalidOperationException>(() => _batch!.AddFillRectangle(new(0, 0, 1, 1)));
    }

    private void SaveGallery()
    {
        Render(b =>
        {
            int index = 0;
            foreach (CornerType corner in Enum.GetValues<CornerType>())
            {
                float x = 20 + index++ * 165;
                b.AddFillRectangle(new(x, 20, 135, 90), new SolidColorBrush(new Color(65, 185, 235)), corner, 24);
                b.AddStrokeRectangle(new(x, 145, 135, 90), new Pen(new SolidColorBrush(new Color(0, 180, 80, 180)), 12), corner, 24);
                b.AddStrokeRectangle(new(x, 285, 110, 30), new Pen(new SolidColorBrush(new Color(220, 95, 40)), 40), corner, 24);
            }
            b.AddLine(new Point2[] { new(25, 420), new(120, 370), new(185, 440), new(270, 390), new(460, 440) },
                new Pen(new SolidColorBrush(new Color(180, 0, 100, 180)), 22) { Join = LineJoin.Round, Cap = LineCap.Round });
        });
        string directory = Path.Combine(AppContext.BaseDirectory, "artifacts");
        Directory.CreateDirectory(directory);
        using var stream = File.Create(Path.Combine(directory, "shape-gallery.png"));
        _target!.SaveAsPng(stream, 512, 512);
    }

    private static void Expect<T>(Action action) where T : Exception
    {
        try { action(); } catch (T) { return; }
        throw new InvalidOperationException("Expected " + typeof(T).Name);
    }

    protected override void UnloadContent()
    {
        _batch?.Dispose();
        _shader?.Dispose();
        _target?.Dispose();
    }
}

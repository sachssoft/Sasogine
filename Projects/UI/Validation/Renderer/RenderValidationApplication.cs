using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Controls.Drawing;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Graphics.Text;
using Sachssoft.Engine.Scenes;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Compares native rendering against the retained SpriteBatch path on real GPU targets.</summary>
internal sealed class RenderValidationApplication : GameApplicationBase
{
    private FontFaceControlFont? _font;
    private ControlSkin? _skin;
    private RenderTarget2D? _target;
    private Texture2D? _image;
    private SpriteFontControlFont? _spriteFont;
    private bool _done;
    internal static bool HalfPixelMode { get; set; }

    protected override GraphicsDeviceManager ConfigureGraphicsDevice() => new(this)
    {
        PreferredBackBufferWidth = 1160,
        PreferredBackBufferHeight = 760,
        SynchronizeWithVerticalRetrace = false,
        GraphicsProfile = GraphicsProfile.Reach,
        PreferHalfPixelOffset = HalfPixelMode
    };
    protected override ISceneManager CreateScenes(GameConfiguration c) => new SceneManager(this, new BenchmarkScene());
    protected override void LoadContent()
    {
        base.LoadContent();
        if (GraphicsDevice.UseHalfPixelOffset != HalfPixelMode)
            throw new InvalidOperationException("Pixel addressing option was not applied.");
        Console.WriteLine($"HALF PIXEL: {GraphicsDevice.UseHalfPixelOffset}");
        _font = new FontFaceControlFont(new FontFace(File.ReadAllBytes(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "segoeui.ttf")), "Segoe"), 20);
        _skin = ControlSkin.Load(GraphicsDevice, Path.Combine(AppContext.BaseDirectory, "Skins", "SachssoftDark"));
        _target = new RenderTarget2D(GraphicsDevice, 1160, 760, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        _image = new Texture2D(GraphicsDevice, 8, 8);
        var data = new Color[64];
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
                data[y * 8 + x] = new Color(x * 30, y * 30, 180, 255);
        _image.SetData(data);
        var sprite = new SpriteFont(_image, new() { new(0, 0, 3, 7), new(3, 0, 3, 7), new(0, 0, 3, 7), new(0, 0, 1, 1) }, new() { new(0, 1, 3, 7), new(0, 0, 3, 7), new(0, 0, 3, 7), new(0, 0, 1, 1) }, new() { ' ', '?', 'A', 'B' }, 11, 2, new() { new(-1, 4, 1), new(1, 3, -1), new(0, 4, 1), new(0, 3, 0) }, '?');
        _spriteFont = new SpriteFontControlFont(sprite);
        Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "Captures"));
    }

    private Color[] Render(Workspace root, ControlDrawContext context, string? save)
    {
        GraphicsDevice.SetRenderTarget(_target);
        GraphicsDevice.Clear(new Color(9, 14, 23));
        root.Draw(context);
        var pixels = new Color[1160 * 760];
        _target!.GetData(pixels);
        if (save is not null)
        {
            using var f = File.Create(Path.Combine(AppContext.BaseDirectory, "Captures", save + ".png"));
            _target.SaveAsPng(f, 1160, 760);
        }

        GraphicsDevice.SetRenderTarget(null);
        return pixels;
    }

    private void Compare(Workspace root, string name, float scale, Rectangle viewport)
    {
        var layout = new ControlLayoutContext(_font!);
        root.Update(new ControlInputFrame(new Size2(viewport.Width / scale, viewport.Height / scale), TimeSpan.Zero), layout);
        using var old = new ControlDrawContext(this, _font!, viewport, scale, ControlRenderer.SpriteBatch);
        using var native = new ControlDrawContext(this, _font!, viewport, scale, ControlRenderer.FrameBatch);
        Render(root, old, null);
        Render(root, native, null); // Warm glyph creation before comparison.
        var a = Render(root, old, name + "-sprite");
        var b = Render(root, native, name + "-frame");
        int differences = 0, large = 0, edges = 0, max = 0;
        for (int i = 0; i < a.Length; i++)
        {
            int error = Math.Max(Math.Max(Math.Abs(a[i].R - b[i].R), Math.Abs(a[i].G - b[i].G)), Math.Max(Math.Abs(a[i].B - b[i].B), Math.Abs(a[i].A - b[i].A)));
            if (error > 0)
                differences++;
            if (error > 2)
            {
                int x = i % 1160, y = i / 1160;
                if (Near(a, b[i], x, y) && Near(b, a[i], x, y))
                    edges++;
                else
                    large++;
            }

            max = Math.Max(max, error);
        }

        Console.WriteLine($"COMPARE {name}: changed {differences}, >2 interior {large}, edge coverage {edges}, max {max}, native calls {native.FrameDrawCalls}");
        if (large > 0 || edges > 16)
            throw new InvalidOperationException($"Rendering differs: {name}, {large} pixels.");
    }

    private static bool Near(Color[] pixels, Color expected, int x, int y)
    {
        for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
            {
                int xx = x + dx, yy = y + dy;
                if (xx < 0 || xx >= 1160 || yy < 0 || yy >= 760)
                    continue;
                Color c = pixels[yy * 1160 + xx];
                if (Math.Abs(c.R - expected.R) <= 8 && Math.Abs(c.G - expected.G) <= 8 && Math.Abs(c.B - expected.B) <= 8 && Math.Abs(c.A - expected.A) <= 2)
                    return true;
            }

        return false;
    }

    private void AllocationProbe()
    {
        using var root = new Workspace(new TextBlock { Text = "Native cached text", Width = 200, Height = 60, Background = Color.Black });
        root.Update(new ControlInputFrame(new Size2(1160, 760), TimeSpan.Zero), new ControlLayoutContext(_font!));
        using var native = new ControlDrawContext(this, _font!, new Rectangle(0, 0, 1160, 760));
        GraphicsDevice.SetRenderTarget(_target);
        for (int i = 0; i < 100; i++)
            root.Draw(native);
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++)
            root.Draw(native);
        long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
        GraphicsDevice.SetRenderTarget(null);
        Console.WriteLine($"ALLOC: 1000 native geometry/text draws: {bytes} bytes");
        // DesktopGL's two SetData calls allocate 32 bytes each under the tested NativeAOT runtime.
        long budget = System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported ? 0 : 64000;
        if (bytes > budget)
            throw new InvalidOperationException("Native drawing exceeded its measured backend allocation budget.");
    }

    protected override void Draw(GameTime t)
    {
        if (_done || _font is null)
            return;
        _done = true;
        var view = new ControlsExampleView();
        using (var root = view.Workspace)
        {
            root.SetSkin(_skin!);
            var tabs = (TabControl)root.Content;
            for (int page = 0; page < 5; page++)
            {
                tabs.SelectedIndex = page;
                if (page == 4)
                    for (int frame = 0; frame < 180; frame++)
                        root.Update(new ControlInputFrame(new Size2(1160, 760), TimeSpan.FromMilliseconds(16 + Math.Sin(frame * .15) * 4)), new ControlLayoutContext(_font));
                Compare(root, "gallery-" + page, 1, new Rectangle(0, 0, 1160, 760));
            }

            tabs.SelectedIndex = 1;
            Compare(root, "gallery-dpi", 1.25f, new Rectangle(13, 17, 1120, 720));
            var dialog = new MessageBox
            {
                Title = "Native modal",
                Message = "Overlay order, transparency and clipping",
                Buttons = MessageBoxButtons.OKCancel,
                Width = 420,
                Height = 220
            };
            _ = root.ShowDialog(dialog);
            Compare(root, "modal", 1, new Rectangle(0, 0, 1160, 760));
            dialog.Complete(DialogResult.Cancel);
            var files = new OpenFileDialog
            {
                InitialDirectory = AppContext.BaseDirectory
            };
            _ = root.ShowDialog(files);
            Compare(root, "file-dialog-sidebar", 1, new Rectangle(0, 0, 1160, 760));
            files.Complete(DialogResult.Cancel);
        }

        using (var root = new Workspace(new RendererGeometryControl(_image!, _spriteFont!, new LegacyTestFont(_font))))
        {
            Compare(root, "geometry", 1, new Rectangle(0, 0, 1160, 760));
            Compare(root, "geometry-dpi", 1.3f, new Rectangle(11, 9, 1120, 720));
        }

        using (var root = new Workspace(new RendererStressControl(_image!)))
            Compare(root, "buffer-stress", 1, new Rectangle(0, 0, 1160, 760));
        using (var root = new Workspace(new ScriptTextBox { Highlighter = new GallerySyntaxHighlighter(), Text = "-- Custom game script\nentity ball = \"player\"\nmove ball 10\n\nwait 0.5\nsay \"Ready\"", Padding = new Insets2(20) }))
        {
            root.SetSkin(_skin!);
            Compare(root, "custom-script", 1, new Rectangle(0, 0, 1160, 760));
            Compare(root, "custom-script-dpi", 1.25f, new Rectangle(13, 17, 1120, 720));
        }

        foreach (string theme in new[]
        {
            "Light",
            "Modern",
            "Futuristic",
            "Medieval"
        }

        )
        {
            using var themedSkin = ControlSkin.Load(GraphicsDevice, Path.Combine(AppContext.BaseDirectory, "Skins", "Sachssoft" + theme));
            var themedView = new ControlsExampleView();
            using var themedRoot = themedView.Workspace;
            themedRoot.SetSkin(themedSkin);
            themedView.ApplyPalette(themedSkin);
            var themedTabs = (TabControl)themedRoot.Content;
            themedTabs.SelectedIndex = 0;
            Compare(themedRoot, "skin-" + theme.ToLowerInvariant(), 1, new Rectangle(0, 0, 1160, 760));
            themedTabs.SelectedIndex = 4;
            Compare(themedRoot, "skin-" + theme.ToLowerInvariant() + "-tools", 1, new Rectangle(0, 0, 1160, 760));
            themedTabs.SelectedIndex = 2;
            Compare(themedRoot, "skin-" + theme.ToLowerInvariant() + "-advanced", 1, new Rectangle(0, 0, 1160, 760));
        }

        using (var root = new Workspace(new Grid { Background = new Color(30, 60, 80) }))
        {
            Compare(root, "opaque-full-target", 1, new Rectangle(0, 0, 1160, 760));
            Compare(root, "opaque-partial-viewport", 1, new Rectangle(13, 17, 1120, 720));
        }

        var slotTextures = new Texture2D[12];
        try
        {
            for (int i = 0; i < slotTextures.Length; i++)
            {
                var texture = slotTextures[i] = new Texture2D(GraphicsDevice, 32, 32, true, SurfaceFormat.Color);
                for (int level = 0; level < texture.LevelCount; level++)
                {
                    int edge = Math.Max(1, 32 >> level);
                    var data = new Color[edge * edge];
                    for (int n = 0; n < data.Length; n++)
                        data[n] = new Color(20 + i * 17, 30 + level * 24, 210 - i * 11, 180 + (n % 3) * 20);
                    texture.SetData(level, null, data, 0, data.Length);
                }
            }

            using var root = new Workspace(new RendererTextureSlotControl(slotTextures));
            Compare(root, "texture-slot-rollover", 1, new Rectangle(0, 0, 1160, 760));
            Compare(root, "texture-slot-rollover-dpi", 1.25f, new Rectangle(13, 17, 1120, 720));
            root.Update(new ControlInputFrame(new Size2(1160, 760), TimeSpan.Zero), new ControlLayoutContext(_font!));
            var expectedSamplers = new SamplerState[7];
            for (int slot = 1; slot < 8; slot++)
            {
                GraphicsDevice.Textures[slot] = slotTextures[slot];
                GraphicsDevice.SamplerStates[slot] = SamplerState.PointWrap;
                expectedSamplers[slot - 1] = GraphicsDevice.SamplerStates[slot];
            }

            using (var native = new ControlDrawContext(this, _font!, new Rectangle(0, 0, 1160, 760)))
                Render(root, native, null);
            for (int slot = 1; slot < 8; slot++)
                if (!ReferenceEquals(GraphicsDevice.Textures[slot], slotTextures[slot]) || !ReferenceEquals(GraphicsDevice.SamplerStates[slot], expectedSamplers[slot - 1]))
                    throw new InvalidOperationException("Texture-slot rendering changed caller sampler or texture state.");
            Console.WriteLine("PASS: twelve textures, mipmaps, slot rollover, transparency and caller texture/sampler restoration.");
        }
        finally
        {
            for (int slot = 1; slot < 8; slot++)
                GraphicsDevice.Textures[slot] = null;
            foreach (var texture in slotTextures)
                texture?.Dispose();
        }

        var changingText = new TextBlock
        {
            Text = "FPS: 40.0",
            Width = 360,
            Height = 60,
            TextHorizontalAlignment = ControlAlignment.End,
            TextVerticalAlignment = ControlAlignment.Center
        };
        using (var changingRoot = new Workspace(changingText))
        using (var expectedRoot = new Workspace(new TextBlock { Text = "FPS: 60.0", Width = 360, Height = 60, TextHorizontalAlignment = ControlAlignment.End, TextVerticalAlignment = ControlAlignment.Center }))
        using (var context = new ControlDrawContext(this, _font!, new Rectangle(0, 0, 1160, 760)))
        {
            var frame = new ControlInputFrame(new Size2(1160, 760), TimeSpan.Zero);
            var layout = new ControlLayoutContext(_font!);
            changingRoot.Update(frame, layout);
            Color[] before = Render(changingRoot, context, null);
            changingText.Text = "FPS: 60.0";
            changingRoot.Update(frame, layout);
            Color[] after = Render(changingRoot, context, null);
            expectedRoot.Update(frame, layout);
            Color[] expected = Render(expectedRoot, context, null);
            if (before.AsSpan().SequenceEqual(after) || !after.AsSpan().SequenceEqual(expected))
                throw new InvalidOperationException("Live equal-size text must render the updated glyphs at the same position.");
            Console.WriteLine("PASS: live text refresh matches a freshly laid-out control pixel for pixel.");
        }

        SceneViewportRenderTests.Run(this, _font!);
        AllocationProbe();
        var numeric = new ValueTextBox
        {
            Minimum = 0,
            Maximum = 100,
            Step = 1,
            Value = 50,
            Width = 360,
            Height = 40,
            Margin = new Insets2(20),
            HorizontalAlignment = ControlAlignment.Start,
            VerticalAlignment = ControlAlignment.Start
        };
        using (var root = new Workspace(numeric))
        {
            root.SetSkin(_skin!);
            Compare(root, "value-bar", 1, new Rectangle(0, 0, 1160, 760));
            Bounds2 thumb = numeric.RangeBar.ThumbBounds;
            Vector2 pointer = new(thumb.X + thumb.Width / 2, thumb.Y + thumb.Height / 2);
            var layout = new ControlLayoutContext(_font!);
            root.Update(new ControlInputFrame(new Size2(1160, 760), TimeSpan.Zero, new[] { new ControlInputEvent(ControlInputEventType.PointerDown, pointer), new ControlInputEvent(ControlInputEventType.PointerUp, pointer) }), layout);
            if (!numeric.IsEditing)
                throw new InvalidOperationException("Click did not open numeric editing.");
            Compare(root, "value-edit", 1, new Rectangle(0, 0, 1160, 760));
            numeric.CancelEdit();
            root.Update(new ControlInputFrame(new Size2(1160, 760), TimeSpan.Zero), layout);
            root.Update(new ControlInputFrame(new Size2(1160, 760), TimeSpan.Zero, new[] { new ControlInputEvent(ControlInputEventType.PointerDown, pointer), new ControlInputEvent(ControlInputEventType.PointerMove, pointer + new Vector2(70, 0)), new ControlInputEvent(ControlInputEventType.PointerUp, pointer + new Vector2(70, 0)) }), layout);
            if (numeric.IsEditing || numeric.Value <= 50)
                throw new InvalidOperationException("Dragging must adjust the value and retain the bar.");
            Compare(root, "value-dragged", 1, new Rectangle(0, 0, 1160, 760));
        }

        Console.WriteLine("PASS: gallery, skin, clipping, DPI, transparency, sprite fonts and compatibility barriers.");
        Exit();
    }

    protected override void UnloadContent()
    {
        _font?.Dispose();
        _skin?.Dispose();
        _target?.Dispose();
        _image?.Dispose();
        base.UnloadContent();
    }
}

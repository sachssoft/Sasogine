using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls.Drawing;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Controls.Layouts;

namespace Sachssoft.Engine.Controls.Examples;

/// <summary>Measures swipe-driven carousel transitions using mixed, skinned control trees on a real graphics device.</summary>
internal static class CarouselStressBenchmark
{
    /// <summary>Reports update and render-submission times in first-use, repeated and steady slide frames.</summary>
    /// <param name="app">The application providing the active graphics device.</param>
    /// <param name="target">The off-screen target used for all draws.</param>
    /// <param name="font">The shared font used by the pages.</param>
    /// <param name="skin">The atlas skin used by the controls.</param>
    internal static void Run(GameApplicationBase app, RenderTarget2D target, IControlFont font, ControlSkin skin)
    {
        const int pages = 5;
        const int repetitions = 4;
        var cold = new List<(double Update, double Draw)>(pages * repetitions);
        var revisit = new List<(double Update, double Draw)>(pages * repetitions * 2);
        var animation = new List<(double Update, double Draw)>(pages * repetitions * 3 * 15);
        var pixel = new Color[1];
        var size = new Size2(target.Width, target.Height);
        var layout = new ControlLayoutContext(font);
        var duration = TimeSpan.FromSeconds(1.0 / 60);
        var events = new List<ControlInputEvent>(3);
        var input = new ControlInputFrame(events);
        using var draw = new ControlDrawContext(app, font, target.Bounds, 1, ControlRenderer.FrameBatch);
        int maximumDrawCalls = 0;

        for (int repetition = -1; repetition < repetitions; repetition++)
        {
            var carousel = new Carousel
            {
                NavigationMode = CarouselNavigationMode.Swipe,
                ShowIndicators = false,
                IsLooping = true,
                Transition = CarouselTransition.Slide,
                TransitionDuration = .25f
            };
            for (int page = 0; page < pages; page++)
                carousel.Items.Add(BuildPage(page));

            using var workspace = new Workspace(carousel);
            workspace.SetSkin(skin);
            Frame(workspace);
            for (int pass = 0; pass < 3; pass++)
            {
                for (int step = 0; step < pages; step++)
                {
                    int previous = carousel.SelectedIndex;
                    events.Add(Touch(ControlInputEventType.PointerDown, 850));
                    events.Add(Touch(ControlInputEventType.PointerMove, 690));
                    events.Add(Touch(ControlInputEventType.PointerUp, 550));
                    var first = Frame(workspace);
                    if (carousel.SelectedIndex != (previous + 1) % pages || !carousel.IsTransitioning)
                        throw new InvalidOperationException("The benchmark swipe did not start the expected slide.");

                    if (repetition >= 0)
                    {
                        if (pass == 0 && step < pages - 1)
                            cold.Add(first);
                        else
                            revisit.Add(first);
                    }

                    while (carousel.IsTransitioning)
                    {
                        var sample = Frame(workspace);
                        if (repetition >= 0 && carousel.IsTransitioning)
                            animation.Add(sample);
                    }

                    // Drain GPU submissions between transitions, outside the measured frames.
                    target.GetData(0, new Rectangle(0, 0, 1, 1), pixel, 0, 1);
                }
            }
        }

        Console.WriteLine($"Carousel: {pages} pages, 160 mixed row controls per page plus nested containers, header and list.");
        Report("first visit", cold);
        Report("revisit", revisit);
        Report("slide", animation);
        Console.WriteLine($"Maximum batch draw calls in a frame: {maximumDrawCalls}.");

        (double Update, double Draw) Frame(Workspace workspace)
        {
            long start = Stopwatch.GetTimestamp();
            workspace.Update(input.Reset(size, duration), layout);
            long updated = Stopwatch.GetTimestamp();
            events.Clear();
            app.GraphicsDevice.SetRenderTarget(target);
            app.GraphicsDevice.Clear(new Color(12, 20, 30));
            workspace.Draw(draw);
            app.GraphicsDevice.SetRenderTarget(null);
            long drawn = Stopwatch.GetTimestamp();
            maximumDrawCalls = Math.Max(maximumDrawCalls, draw.FrameDrawCalls);
            return (Stopwatch.GetElapsedTime(start, updated).TotalMilliseconds,
                Stopwatch.GetElapsedTime(updated, drawn).TotalMilliseconds);
        }
    }

    /// <summary>Constructs a page with text, editors, buttons, toggles, sliders and a list.</summary>
    /// <param name="page">The page index used to vary control values and order.</param>
    /// <returns>The retained page tree.</returns>
    private static Control BuildPage(int page)
    {
        var content = new StackPanel { Spacing = 4 };
        content.Add(new TextBlock { Text = $"Swipe here to change page {page + 1}", Height = 36 });
        for (int row = 0; row < 20; row++)
        {
            var strip = new StackPanel { Orientation = Orientation.Horizontal, Height = 38, Spacing = 4 };
            for (int column = 0; column < 8; column++)
            {
                Control item = ((column + page) % 8) switch
                {
                    0 => new Button { Content = $"Open {row + 1}" },
                    1 => new CheckBox { Content = "Enabled", IsChecked = row % 2 == 0 },
                    2 => new Slider { Value = row * 4 },
                    3 => new TextBox { Text = $"Value {row + 1}" },
                    4 => new Switch { IsChecked = row % 3 == 0 },
                    5 => new TextBlock { Text = "Wrapped label for layout", WrapText = true },
                    6 => new ValueTextBox { Value = row },
                    _ => new ToggleButton { Content = "Option", IsChecked = row % 3 == 1 }
                };
                item.Width = 125;
                strip.Add(item);
            }
            content.Add(strip);
        }

        var list = new ListBox { Height = 165 };
        for (int i = 0; i < 100; i++)
            list.Items.Add(new SelectorItem($"List entry {i + 1}"));
        content.Add(list);
        return new ScrollViewer { Content = content, HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden };
    }

    /// <summary>Creates an ordered swipe event in the page header.</summary>
    /// <param name="type">The pointer phase.</param>
    /// <param name="x">The horizontal position in workspace coordinates.</param>
    /// <returns>The synthetic touch event.</returns>
    private static ControlInputEvent Touch(ControlInputEventType type, float x) =>
        new(type, new Vector2(x, 16), PointerId: 1, PointerKind: ControlPointerKind.Touch);

    /// <summary>Prints average and tail timings without mixing CPU work with GPU synchronization.</summary>
    /// <param name="name">The transition phase.</param>
    /// <param name="samples">The recorded update and submission times.</param>
    private static void Report(string name, List<(double Update, double Draw)> samples)
    {
        double[] update = samples.Select(sample => sample.Update).Order().ToArray();
        double[] draw = samples.Select(sample => sample.Draw).Order().ToArray();
        double[] total = samples.Select(sample => sample.Update + sample.Draw).Order().ToArray();
        Console.WriteLine(FormattableString.Invariant(
            $"{name} ({samples.Count}): update mean {update.Average():F3}, P95 {Percentile(update, .95):F3}; submission mean {draw.Average():F3}, P95 {Percentile(draw, .95):F3}; total mean {total.Average():F3}, P95 {Percentile(total, .95):F3}, P99 {Percentile(total, .99):F3}, max {total[^1]:F3} ms"));
    }

    /// <summary>Returns the observed nearest-rank percentile.</summary>
    /// <param name="sorted">The ascending sample times.</param>
    /// <param name="fraction">The requested percentile as a fraction.</param>
    /// <returns>The corresponding sample time.</returns>
    private static double Percentile(double[] sorted, double fraction) => sorted[(int)Math.Ceiling(fraction * sorted.Length) - 1];
}

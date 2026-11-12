using System.Diagnostics;
using System.Runtime.InteropServices;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Drawing;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Controls.Layouts;

internal static class LayoutBenchmark
{
    private const int Samples = 105;
    private const int Batch = 16;
    private const int Burst = 16;
    private static readonly Size2 Size = new(1200, 900);
    private static readonly Bounds2 Slot = new(0, 0, 1200, 900);

    internal static void Run(ControlLayoutContext layout)
    {
        var wide = new Grid();
        var labels = new TextBlock[240];
        for (int column = 0; column < 12; column++)
            wide.Columns.Add(new GridColumn(GridLength.Proportion()));
        for (int row = 0; row < 20; row++)
            wide.Rows.Add(new GridRow(GridLength.Proportion()));
        for (int i = 0; i < labels.Length; i++)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal };
            row.Add(new Button { Content = "Edit" });
            row.Add(labels[i] = new TextBlock { Text = "Terrain" });
            wide.Place(row, column: i % 12, row: i / 12);
        }

        var deep = new StackPanel();
        StackPanel parent = deep;
        for (int i = 0; i < 32; i++)
        {
            var next = new StackPanel();
            parent.Add(next);
            parent = next;
        }
        var leaf = new TextBlock { Text = "Nested control" };
        parent.Add(leaf);

        using var wideWorkspace = new Workspace(wide);
        using var deepWorkspace = new Workspace(deep);
        var frame = new ControlInputFrame(Size, TimeSpan.Zero);
        void WideFrame() => wideWorkspace.Update(frame, layout);
        void DeepFrame() => deepWorkspace.Update(frame, layout);
        WideFrame();
        DeepFrame();
        int nextLabel = 0;
        Action invalidateMeasure = leaf.InvalidateMeasure;
        Action invalidateArrange = leaf.InvalidateArrange;
        Action invalidateVisual = leaf.InvalidateVisual;

        Console.WriteLine($"Runtime={RuntimeInformation.FrameworkDescription}; OS={RuntimeInformation.OSDescription}; CPU={Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER")}; logical={Environment.ProcessorCount}; groups={Samples}; batch={Batch}; burst={Burst}");
#if LAYOUT_METRICS
        Console.WriteLine("scenario,measure_calls,measure_work,arrange_calls,arrange_work,propagation,affected_union,passes,prepared");
#else
        Console.WriteLine("scenario,average_ns,median_ns,p95_batch_ns,bytes_per_op");
#endif
        Measure("static_frame", WideFrame);
        Measure("measure_cache", () => wide.Measure(layout, Size));
        Measure("arrange_cache", () => wide.Arrange(Slot));
        Measure("invalidate_measure", () => Repeat(invalidateMeasure), DeepFrame, Burst);
        Measure("invalidate_arrange", () => Repeat(invalidateArrange), DeepFrame, Burst);
        Measure("invalidate_visual", () => Repeat(invalidateVisual), DeepFrame, Burst);
        Measure("deep_frame", () => { Repeat(invalidateMeasure); DeepFrame(); });
        Measure("wide_frame", () =>
        {
            for (int i = 0; i < Burst; i++)
                labels[i].InvalidateMeasure();
            WideFrame();
        });
        Measure("property_frame", () =>
        {
            TextBlock label = labels[nextLabel++ % labels.Length];
            label.Text = label.Text == "Terrain" ? "Terrain and water" : "Terrain";
            WideFrame();
        });
        Measure("mixed_frame", () =>
        {
            TextBlock label = labels[nextLabel++ % labels.Length];
            bool toggle = label.Width != 42;
            label.Width = toggle ? 42 : 61;
            label.HorizontalAlignment = toggle ? LayoutAlignment.Start : LayoutAlignment.End;
            label.Opacity = toggle ? .6f : 1f;
            WideFrame();
        });
    }

    private static void Repeat(Action invalidate)
    {
        for (int i = 0; i < Burst; i++) invalidate();
    }

    private static void Measure(string name, Action operation, Action? reset = null, int operationsPerBatch = Batch)
    {
        for (int i = 0; i < 320; i++)
        {
            reset?.Invoke();
            operation();
        }
#if LAYOUT_METRICS
        reset?.Invoke();
        LayoutMetrics.Reset();
        int iterations = reset is null ? Samples * Batch : 1;
        for (int i = 0; i < iterations; i++) operation();
        double count = reset is null ? iterations : operationsPerBatch;
        Console.WriteLine(FormattableString.Invariant($"{name},{LayoutMetrics.MeasureCalls / count:F3},{LayoutMetrics.Measures / count:F3},{LayoutMetrics.ArrangeCalls / count:F3},{LayoutMetrics.Arranges / count:F3},{LayoutMetrics.Propagation / count:F3},{LayoutMetrics.Affected.Count},{LayoutMetrics.Passes / count:F3},{LayoutMetrics.Prepared / count:F3}"));
#else
        var times = new double[Samples];
        long bytes = 0;
        for (int sample = 0; sample < Samples; sample++)
        {
            reset?.Invoke();
            long before = GC.GetAllocatedBytesForCurrentThread();
            long start = Stopwatch.GetTimestamp();
            if (reset is not null)
                operation();
            else
                for (int i = 0; i < Batch; i++) operation();
            long ticks = Stopwatch.GetTimestamp() - start;
            bytes += GC.GetAllocatedBytesForCurrentThread() - before;
            times[sample] = ticks * (1e9 / Stopwatch.Frequency) / operationsPerBatch;
        }
        Array.Sort(times);
        Console.WriteLine(FormattableString.Invariant($"{name},{times.Average():F2},{times[Samples / 2]:F2},{times[(int)Math.Ceiling(Samples * .95) - 1]:F2},{(double)bytes / (Samples * operationsPerBatch):F2}"));
#endif
    }
}

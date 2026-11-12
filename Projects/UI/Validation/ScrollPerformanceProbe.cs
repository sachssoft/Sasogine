using System.Diagnostics;
using Microsoft.Xna.Framework;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Controls.Layouts;

/// <summary>Measures warmed touch scrolling through a large nested control tree.</summary>
internal static class ScrollPerformanceProbe
{
    /// <summary>Reports CPU update timings, allocations and root layout counts.</summary>
    /// <param name="layout">The fixed test font and layout context.</param>
    internal static void Run(ControlLayoutContext layout)
    {
        const int sampleCount = 1000;
        var root = new TextLayoutProbePanel { Padding = Insets2.Zero };
        var content = new StackPanel();
        for (int section = 0; section < 20; section++)
        {
            var group = new StackPanel();
            for (int item = 0; item < 50; item++)
                group.Add(new Button { Content = "Item", Height = 24 });
            content.Add(group);
        }

        var viewer = new ScrollViewer
        {
            Content = content,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden
        };
        root.Add(viewer);
        using var workspace = new Workspace(root);
        var size = new Size2(400, 300);
        var events = new List<ControlInputEvent>(1);
        var frame = new ControlInputFrame(events);
        workspace.Update(frame.Reset(size, TimeSpan.Zero), layout);
        Send(ControlInputEventType.PointerDown, 220);
        for (int i = 0; i < 300; i++)
            Send(ControlInputEventType.PointerMove, 140 + i % 2 * 5);

        if (!viewer.IsSwiping || viewer.VerticalScrollBar.Value <= 0)
            throw new InvalidOperationException("The benchmark did not start a scrolling gesture.");

        var times = new long[sampleCount];
        int measures = root.Measurements;
        int arrangements = root.Arrangements;
        long total = 0;
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < times.Length; i++)
        {
            long start = Stopwatch.GetTimestamp();
            Send(ControlInputEventType.PointerMove, 140 + i % 2 * 5);
            long duration = Stopwatch.GetTimestamp() - start;
            times[i] = duration;
            total += duration;
        }

        long bytes = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        Array.Sort(times);
        double milliseconds = 1000.0 / Stopwatch.Frequency;
        Console.WriteLine(FormattableString.Invariant($"Scroll update, {sampleCount} frames, 1000 buttons: mean {total * milliseconds / sampleCount:F3} ms, P95 {times[949] * milliseconds:F3} ms, P99 {times[989] * milliseconds:F3} ms, max {times[^1] * milliseconds:F3} ms, {bytes} bytes, {root.Measurements - measures} root measures, {root.Arrangements - arrangements} root arrangements."));

        void Send(ControlInputEventType type, float y)
        {
            events.Clear();
            events.Add(new ControlInputEvent(type, new Vector2(100, y), PointerId: 1, PointerKind: ControlPointerKind.Touch));
            workspace.Update(frame.Reset(size, TimeSpan.Zero), layout);
        }
    }
}

using Microsoft.Xna.Framework;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Input;
using System;

/// <summary>Checks independent visual, arrangement and measurement invalidation.</summary>
internal static class InvalidationChecks
{
    private static void CheckPendingWork(Action<bool, string> check, ControlLayoutContext layout, ControlInputFrame frame)
    {
        var root = new TextLayoutProbePanel();
        var branch = new TextLayoutProbePanel();
        var unrelated = new TextLayoutProbePanel();
        var first = new MeasureProbe();
        var second = new MeasureProbe();
        branch.Add(first);
        branch.Add(second);
        root.Add(branch);
        root.Add(unrelated);

        using var workspace = new Workspace(root);
        workspace.Update(frame, layout);
        int unrelatedPreparations = unrelated.Preparations;
        int firstMeasures = first.Measurements;
        ulong visualRevision = root.VisualRevision;
        for (int i = 0; i < 16; i++)
        {
            first.InvalidateMeasure();
            first.InvalidateArrange();
            first.InvalidateVisual();
        }

        check(root.VisualRevision == visualRevision + 1, "Repeated invalidations coalesce ancestor visual revisions");
        workspace.Update(frame, layout);
        check(first.Measurements == firstMeasures + 1 && unrelated.Preparations == unrelatedPreparations,
            "Repeated measure invalidation executes once and skips preparing an unaffected branch");

        firstMeasures = first.Measurements;
        second.OnMeasure = () =>
        {
            second.OnMeasure = null;
            first.Width = 75;
            first.InvalidateMeasure();
        };
        first.InvalidateMeasure();
        second.InvalidateMeasure();
        workspace.Update(frame, layout);
        check(first.Measurements == firstMeasures + 2 && first.DesiredSize.Width == 75,
            "Measurement re-invalidated by a later sibling is processed after the current pass");

        firstMeasures = first.Measurements;
        int firstArranges = first.Arrangements;
        second.OnArrange = () =>
        {
            second.OnArrange = null;
            first.HorizontalAlignment = LayoutAlignment.End;
            first.InvalidateArrange();
        };
        first.InvalidateArrange();
        second.InvalidateArrange();
        workspace.Update(frame, layout);
        check(first.Arrangements == firstArranges + 2 && first.Measurements == firstMeasures && first.Bounds.Right == branch.Bounds.Right,
            "Arrangement re-invalidated by a later sibling updates geometry without measuring");

        firstMeasures = first.Measurements;
        workspace.Update(new ControlInputFrame(new Size2(500, 300), TimeSpan.Zero), layout);
        check(first.Measurements > firstMeasures && first.Bounds.Right == branch.Bounds.Right,
            "A new available size invalidates cached layout constraints");

        first.IsVisible = false;
        workspace.Update(frame, layout);
        check(first.DesiredSize == Size2.Zero && first.Bounds == Bounds2.Zero,
            "Visibility changes clear cached size and bounds");
        branch.Remove(first);
        unrelated.Add(first);
        first.IsVisible = true;
        workspace.Update(frame, layout);
        check(ReferenceEquals(first.Parent, unrelated) && first.Bounds.Right == unrelated.Bounds.Right && first.DesiredSize.Width == 75,
            "Reparenting restores visible layout under the new owner");
    }

    /// <summary>Runs ancestor propagation, alignment, failure recovery and allocation checks.</summary>
    /// <param name = "check">The assertion callback.</param>
    /// <param name = "layout">The fixed test layout context.</param>
    internal static void Run(Action<bool, string> check, ControlLayoutContext layout)
    {
        var root = new TextLayoutProbePanel
        {
            Padding = Insets2.Zero
        };
        var branch = new TextLayoutProbePanel
        {
            Padding = Insets2.Zero
        };
        var child = new MeasureProbe
        {
            HorizontalAlignment = LayoutAlignment.Start,
            VerticalAlignment = LayoutAlignment.Start
        };
        var sibling = new MeasureProbe();
        branch.Add(child);
        root.Add(branch);
        root.Add(sibling);
        using var workspace = new Workspace(root);
        var frame = new ControlInputFrame(new Size2(300, 200), TimeSpan.Zero);
        workspace.Update(frame, layout);
        int rootMeasures = root.Measurements;
        int branchMeasures = branch.Measurements;
        int childMeasures = child.Measurements;
        int rootArranges = root.Arrangements;
        int childArranges = child.Arrangements;
        int siblingArranges = sibling.Arrangements;
        ulong revision = root.VisualRevision;
        ulong siblingRevision = sibling.VisualRevision;
        child.InvalidateVisual();
        check(root.VisualRevision > revision && child.VisualRevision > 0 && sibling.VisualRevision == siblingRevision, "Visual invalidation reaches ancestors without changing unrelated siblings");
        workspace.Update(frame, layout);
        check(root.Measurements == rootMeasures && branch.Measurements == branchMeasures && child.Measurements == childMeasures && root.Arrangements == rootArranges && child.Arrangements == childArranges, "Visual invalidation performs neither measurement nor arrangement");
        revision = root.VisualRevision;
        child.Background = Color.Red;
        child.BorderColor = Color.Blue;
        child.Opacity = .5f;
        workspace.Update(frame, layout);
        check(root.VisualRevision > revision && root.Measurements == rootMeasures && root.Arrangements == rootArranges, "Color and opacity changes invalidate visuals while preserving geometry");
        child.InvalidateArrange();
        workspace.Update(frame, layout);
        check(root.Arrangements == rootArranges + 1 && child.Arrangements == childArranges + 1 && sibling.Arrangements == siblingArranges, "Arrange invalidation reaches affected ancestors but preserves unrelated arrangement caches");
        check(root.Measurements == rootMeasures && branch.Measurements == branchMeasures && child.Measurements == childMeasures, "Arrange invalidation preserves every measured size in the ancestor chain");
        child.HorizontalAlignment = LayoutAlignment.End;
        child.VerticalAlignment = LayoutAlignment.End;
        workspace.Update(frame, layout);
        check(child.Bounds.Right == 300 && child.Bounds.Bottom == 200 && child.Measurements == childMeasures, "Control alignment changes update placement without measuring again");
        child.InvalidateMeasure();
        workspace.Update(frame, layout);
        check(root.Measurements == rootMeasures + 1 && branch.Measurements == branchMeasures + 1 && child.Measurements == childMeasures + 1, "Measure invalidation rebuilds the affected measurement chain");
        childMeasures = child.Measurements;
        childArranges = child.Arrangements;
        child.InvalidateLayout();
        workspace.Update(frame, layout);
        check(child.Measurements == childMeasures + 1 && child.Arrangements == childArranges + 1, "InvalidateLayout remains the complete measure-and-arrange entry point");
        childMeasures = child.Measurements;
        child.FailArrange = true;
        child.InvalidateArrange();
        bool rejected = false;
        try
        {
            workspace.Update(frame, layout);
        }
        catch (InvalidOperationException e)when (e.Message == "Arrange failure")
        {
            rejected = true;
        }

        check(rejected && child.HasValidMeasure && child.Measurements == childMeasures, "An arrange failure does not discard valid measurement results");
        child.FailArrange = false;
        childArranges = child.Arrangements;
        workspace.Update(frame, layout);
        check(child.Arrangements == childArranges + 1 && child.Measurements == childMeasures, "Failed arrangement remains dirty until the next successful retry");
        child.HorizontalAlignment = (LayoutAlignment)999;
        rejected = false;
        try
        {
            workspace.Update(frame, layout);
        }
        catch (InvalidOperationException e)when (e.Message == "Invalid control alignment.")
        {
            rejected = true;
        }

        check(rejected, "Arrange-only alignment changes retain validation");
        child.HorizontalAlignment = LayoutAlignment.Start;
        workspace.Update(frame, layout);
        for (int i = 0; i < 100; i++)
        {
            child.InvalidateArrange();
            workspace.Update(frame, layout);
        }

        childMeasures = child.Measurements;
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++)
        {
            child.InvalidateArrange();
            workspace.Update(frame, layout);
        }

        check(GC.GetAllocatedBytesForCurrentThread() == before && child.Measurements == childMeasures, "Repeated arrange-only updates allocate zero bytes and preserve measure caches");
        before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++)
            child.InvalidateVisual();
        check(GC.GetAllocatedBytesForCurrentThread() == before, "Visual invalidation allocates zero bytes");
        revision = root.VisualRevision;
        root.Paint(null!);
        root.PaintOverlay(null!);
        check(root.VisualRevision == revision, "Drawing does not consume or change visual invalidation revisions");
        child.IsEnabled = false;
        workspace.Update(frame, layout);
        check(child.Measurements == childMeasures, "Interaction availability changes preserve layout measurement");
        child.IsEnabled = true;
        rootMeasures = root.Measurements;
        branch.ZIndex = 2;
        workspace.Update(frame, layout);
        check(ReferenceEquals(root.OrderedChildren[^1], branch) && root.Measurements == rootMeasures, "Z-index updates drawing order without measuring the tree");
        CheckPendingWork(check, layout, frame);
        var font = new CountingFont();
        var textLayout = new ControlLayoutContext(font);
        var text = new InvalidationTextProbe
        {
            Text = "Text",
            Width = 100,
            Height = 60,
            Padding = Insets2.Zero
        };
        using var textWorkspace = new Workspace(text);
        textWorkspace.Update(frame, textLayout);
        int textMeasures = text.Measurements;
        int fontMeasures = font.Measurements;
        text.TextHorizontalAlignment = LayoutAlignment.End;
        text.TextVerticalAlignment = LayoutAlignment.End;
        textWorkspace.Update(frame, textLayout);
        check(text.Position == new Vector2(68, 44) && text.Measurements == textMeasures && font.Measurements == fontMeasures, "Text alignment changes move cached glyph placement without remeasurement");
        var host = new ContentControl
        {
            Content = new MeasureProbe(),
            Padding = Insets2.Zero
        };
        using var hostWorkspace = new Workspace(host);
        hostWorkspace.Update(frame, layout);
        var hosted = (MeasureProbe)host.ContentPresenter!;
        int hostedMeasures = hosted.Measurements;
        host.ContentHorizontalAlignment = LayoutAlignment.End;
        host.ContentVerticalAlignment = LayoutAlignment.End;
        hostWorkspace.Update(frame, layout);
        check(hosted.Bounds.Right == 300 && hosted.Bounds.Bottom == 200 && hosted.Measurements == hostedMeasures, "Content alignment changes only rearrange retained content");
        var textParent = new TextLayoutProbePanel();
        var liveText = new TextBlock
        {
            Text = "One"
        };
        textParent.Add(liveText);
        using var liveWorkspace = new Workspace(textParent);
        liveWorkspace.Update(frame, layout);
        int parentMeasures = textParent.Measurements;
        revision = textParent.VisualRevision;
        liveText.Text = "Two";
        liveWorkspace.Update(frame, layout);
        check(textParent.VisualRevision > revision && textParent.Measurements == parentMeasures, "Equal-size text edits still invalidate visuals while retaining parent measurement caches");
        var link = new Hyperlink
        {
            Content = "Link"
        };
        using var linkWorkspace = new Workspace(link);
        linkWorkspace.Update(frame, layout);
        revision = link.VisualRevision;
        link.IsVisited = true;
        check(link.VisualRevision > revision, "Specialized hyperlink state invalidates visuals through its optimized notification path");
        var chart = new Chart();
        var series = new ChartSeries();
        chart.Series.Add(series);
        using var chartWorkspace = new Workspace(chart);
        chartWorkspace.Update(frame, layout);
        revision = chart.VisualRevision;
        series.Append(1);
        check(chart.VisualRevision > revision && chart.HasValidMeasure, "Chart samples invalidate visuals without discarding measurement");
        var box = new TextBox
        {
            Text = "Sample"
        };
        using var boxWorkspace = new Workspace(box);
        boxWorkspace.Update(frame, layout);
        boxWorkspace.Focus(box);
        revision = box.VisualRevision;
        boxWorkspace.Update(new ControlInputFrame(frame.Size, TimeSpan.FromSeconds(.6)), layout);
        check(box.VisualRevision > revision && box.HasValidMeasure, "Caret blink transitions invalidate visuals without invalidating measurement");
        revision = box.VisualRevision;
        boxWorkspace.Update(new ControlInputFrame(frame.Size, TimeSpan.FromSeconds(.1)), layout);
        check(box.VisualRevision == revision, "Unchanged caret blink visibility retains the visual revision");
        box.Select(1, 2);
        check(box.VisualRevision > revision && box.HasValidMeasure, "Programmatic text selection invalidates visuals");
        var spinnerParent = new TextLayoutProbePanel();
        var spinner = new Spinner();
        spinnerParent.Add(spinner);
        using var spinnerWorkspace = new Workspace(spinnerParent);
        spinnerWorkspace.Update(frame, layout);
        int spinnerMeasures = spinnerParent.Measurements;
        revision = spinner.VisualRevision;
        spinnerWorkspace.Update(new ControlInputFrame(frame.Size, TimeSpan.FromSeconds(.1)), layout);
        check(spinner.VisualRevision > revision && spinnerParent.Measurements == spinnerMeasures, "Spinner animation invalidates visuals without measuring its parent");

        var ranges = new TextLayoutProbePanel();
        var slider = new Slider();
        var bar = new ScrollBar();
        ranges.Add(slider);
        ranges.Add(bar);
        using var rangeWorkspace = new Workspace(ranges);
        rangeWorkspace.Update(frame, layout);
        int rangeMeasures = ranges.Measurements;
        int rangeArrangements = ranges.Arrangements;
        revision = ranges.VisualRevision;
        slider.Value = 40;
        bar.Value = 60;
        rangeWorkspace.Update(frame, layout);
        check(ranges.VisualRevision > revision && ranges.Measurements == rangeMeasures && ranges.Arrangements == rangeArrangements,
            "Slider and independent scrollbar value changes repaint without workspace layout");
    }
}

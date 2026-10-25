using System;
using Microsoft.Xna.Framework;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Input;

/// <summary>Checks independent visual, arrangement and measurement invalidation.</summary>
internal static class InvalidationChecks
{
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
            HorizontalAlignment = ControlAlignment.Start,
            VerticalAlignment = ControlAlignment.Start
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
        child.HorizontalAlignment = ControlAlignment.End;
        child.VerticalAlignment = ControlAlignment.End;
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
        child.HorizontalAlignment = (ControlAlignment)999;
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
        child.HorizontalAlignment = ControlAlignment.Start;
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
        text.TextHorizontalAlignment = ControlAlignment.End;
        text.TextVerticalAlignment = ControlAlignment.End;
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
        host.ContentHorizontalAlignment = ControlAlignment.End;
        host.ContentVerticalAlignment = ControlAlignment.End;
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
    }
}

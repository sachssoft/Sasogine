using System;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Controls.Layouts;
using Sachssoft.Engine.Controls.Styling;

/// <summary>Checks adaptive tile sizing, retained layout, styles and scrolling.</summary>
internal static class AdaptiveTilePanelChecks
{
    /// <summary>Runs deterministic geometry and allocation checks using a fixed font.</summary>
    /// <param name = "check">The assertion callback.</param>
    /// <param name = "layout">The test measurement context.</param>
    internal static void Run(Action<bool, string> check, ControlLayoutContext layout)
    {
        var panel = new AdaptiveTilePanel
        {
            Padding = Insets2.Zero,
            PreferredTileSize = new Size2(100, 50),
            Spacing = 10
        };
        var probes = new MeasureProbe[5];
        for (int i = 0; i < probes.Length; i++)
        {
            probes[i] = new MeasureProbe();
            panel.Add(probes[i]);
        }

        using var workspace = new Workspace(panel);
        var frame = new ControlInputFrame(new Size2(350, 300), TimeSpan.Zero);
        workspace.Update(frame, layout);
        check(panel.ActualTileSize == new Size2(110, 55), "Tiles fill width while preserving their preferred aspect ratio");
        check(probes[2].Bounds.Right == 350 && probes[3].Bounds.Y == 65, "Only inner gaps are reserved and tiles wrap into the next row");
        check(probes[4].Bounds.Size == probes[0].Bounds.Size, "Incomplete rows retain the same tile size");
        check(panel.DesiredSize.Height == 120, "Measurement includes the correct line count and inner gaps");
        check(workspace.Registry.Create(nameof(AdaptiveTilePanel)) is AdaptiveTilePanel, "The built-in factory requires no customer registration");
        int notifications = 0;
        panel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(AdaptiveTilePanel.ActualTileSize))
                notifications++;
        };
        int measured = probes[0].Measurements;
        int arranged = probes[0].Arrangements;
        for (int i = 0; i < 100; i++)
            workspace.Update(frame, layout);
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++)
            workspace.Update(frame, layout);
        check(GC.GetAllocatedBytesForCurrentThread() == before, "Steady adaptive tile updates allocate zero bytes");
        check(probes[0].Measurements == measured && probes[0].Arrangements == arranged && notifications == 0, "Steady updates retain measurement, arrangement and actual-size notification caches");
        workspace.Update(new ControlInputFrame(new Size2(450, 300), TimeSpan.Zero), layout);
        check(panel.ActualTileSize == new Size2(105, 52.5f) && probes[4].Bounds.Y == 62.5f && notifications == 1, "Resizing recomputes tile count and notifies the actual size once");
        workspace.Update(new ControlInputFrame(new Size2(8, 300), TimeSpan.Zero), layout);
        check(panel.ActualTileSize == new Size2(32, 16), "Both minimum dimensions are honored even when the viewport is smaller");
        panel.MinTileSize = new Size2(80, 60);
        workspace.Update(frame, layout);
        check(panel.ActualTileSize == new Size2(170, 85), "Aspect-preserving minimum heights also constrain tile width");
        panel.PreserveAspectRatio = false;
        workspace.Update(frame, layout);
        check(panel.ActualTileSize == new Size2(110, 60), "Without aspect preservation the cross-axis keeps its preferred or minimum dimension");
        panel.MinTileSize = new Size2(16);
        panel.PreserveAspectRatio = true;
        panel.Orientation = StackOrientation.Vertical;
        workspace.Update(new ControlInputFrame(new Size2(500, 200), TimeSpan.Zero), layout);
        check(panel.ActualTileSize == new Size2(120, 60) && probes[3].Bounds.X == 130 && probes[2].Bounds.Bottom == 200, "Vertical orientation fills columns against the available height");
        panel.Orientation = StackOrientation.Horizontal;
        panel.Measure(layout, new Size2(float.PositiveInfinity, float.PositiveInfinity));
        check(panel.DesiredSize == new Size2(540, 50), "Unbounded measurement uses a natural single line instead of an empty extent");
        panel.Arrange(new Bounds2(0, 0, 350, 300));
        measured = probes[0].Measurements;
        panel.Arrange(new Bounds2(0, 0, 450, 300));
        check(probes[0].Measurements > measured && panel.ActualTileSize.Width == 105, "A changed final arrangement constraint remeasures children against the new tile size");
        probes[0].IsVisible = false;
        workspace.Update(frame, layout);
        check(probes[0].Bounds == Bounds2.Zero && probes[1].Bounds.X == 0 && probes[4].Bounds.Y == 65, "Hidden children have no slot or gap and their stale bounds are cleared");
        panel.PreferredTileSize = new Size2(100, 50);
        panel.ApplyStyle(new Style { PreferredTileSize = new Size2(80), MinTileSize = new Size2(16), PreserveAspectRatio = false });
        check(panel.PreferredTileSize == new Size2(100, 50), "Local tile sizes take precedence over central styles");
        panel.ResetStyleProperty(nameof(AdaptiveTilePanel.PreferredTileSize));
        check(panel.PreferredTileSize == new Size2(80), "Resetting a local override restores its style value");
        foreach (float invalid in new[]
        {
            0f,
            -1f,
            float.NaN,
            float.PositiveInfinity
        }

        )
        {
            bool rejected = false;
            try
            {
                panel.PreferredTileSize = new Size2(invalid, 50);
            }
            catch (ArgumentOutOfRangeException)
            {
                rejected = true;
            }

            check(rejected, "Invalid preferred dimensions are rejected before entering layout");
            rejected = false;
            try
            {
                panel.MinTileSize = new Size2(16, invalid);
            }
            catch (ArgumentOutOfRangeException)
            {
                rejected = true;
            }

            check(rejected, "Invalid minimum dimensions are rejected before entering layout");
        }

        bool invalidSpacing = false;
        try
        {
            panel.Spacing = float.NaN;
        }
        catch (ArgumentOutOfRangeException)
        {
            invalidSpacing = true;
        }

        check(invalidSpacing, "Non-finite gaps cannot enter layout");
        bool invalidOrientation = false;
        try
        {
            panel.Orientation = (StackOrientation)99;
        }
        catch (ArgumentOutOfRangeException)
        {
            invalidOrientation = true;
        }

        check(invalidOrientation, "Invalid orientations cannot enter layout");
        foreach (MeasureProbe probe in probes)
            probe.IsVisible = false;
        workspace.Update(frame, layout);
        check(panel.ActualTileSize == Size2.Zero && panel.DesiredSize == Size2.Zero, "All-hidden panels have an empty extent and actual tile size");
        var empty = new AdaptiveTilePanel
        {
            Padding = Insets2.Zero
        };
        empty.Measure(layout, new Size2(float.PositiveInfinity, float.PositiveInfinity));
        empty.Arrange(new Bounds2(0, 0, 350, 300));
        check(empty.DesiredSize == Size2.Zero && empty.ActualTileSize == Size2.Zero, "Empty panels accept unbounded measurement");
        var scrolling = new AdaptiveTilePanel
        {
            PreferredTileSize = new Size2(100, 50),
            Spacing = 10
        };
        for (int i = 0; i < 10; i++)
            scrolling.Add(new MeasureProbe());
        var viewer = new ScrollViewer
        {
            Content = scrolling,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden
        };
        using var scrollWorkspace = new Workspace(viewer);
        scrollWorkspace.Update(new ControlInputFrame(new Size2(350, 120), TimeSpan.Zero), layout);
        check(viewer.VerticalScrollBar.IsVisible && scrolling.ActualTileSize.Width <= viewer.ViewportBounds.Width, "A vertical ScrollViewer reserves its scrollbar before adaptive tiles fill the viewport width");
        check(MathF.Abs(scrolling.Children[0].Bounds.Width - scrolling.Children[9].Bounds.Width) < .001f && MathF.Abs(scrolling.Children[0].Bounds.Height - scrolling.Children[9].Bounds.Height) < .001f, "Scrolling preserves uniform sizes across all rows");
        check(scrolling.DesiredSize.Width == viewer.ViewportBounds.Width,
            "Fractional tile sizes do not produce a phantom overflow in the viewport width");
        int scrollMeasures = ((MeasureProbe)scrolling.Children[0]).Measurements;
        var scrollFrame = new ControlInputFrame(new Size2(350, 120), TimeSpan.Zero);
        for (int i = 0; i < 100; i++) scrollWorkspace.Update(scrollFrame, layout);
        before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++) scrollWorkspace.Update(scrollFrame, layout);
        check(GC.GetAllocatedBytesForCurrentThread() == before && ((MeasureProbe)scrolling.Children[0]).Measurements == scrollMeasures,
            "Adaptive tiles in a ScrollViewer retain their cache without steady frame allocations");
        workspace.Styles[nameof(AdaptiveTilePanel)].PreferredTileSize = new Size2(90, 60);
        panel.ResetStyleProperty(nameof(AdaptiveTilePanel.PreferredTileSize));
        workspace.Update(frame, layout);
        check(panel.PreferredTileSize == new Size2(90, 60), "Central style revisions apply the typed preferred size");
    }
}

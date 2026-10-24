using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Drawing;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Controls.Layouts;
using Sachssoft.Engine.Controls.Styling;

/// <summary>Checks edge-group ordering, placement, ownership, constrained slots and cached layout.</summary>
internal static class EdgePanelChecks
{
    internal static void Run(Action<bool, string> check, ControlLayoutContext layout)
    {
        var panel = new EdgePanel
        {
            Spacing = 5
        };
        var start = new MeasureProbe
        {
            Width = 40,
            Height = 20
        };
        var firstEnd = new MeasureProbe
        {
            Width = 30,
            Height = 20
        };
        var lastEnd = new MeasureProbe
        {
            Width = 20,
            Height = 20
        };
        panel.Put(start);
        panel.Put(firstEnd, PanelEdge.End);
        panel.Put(lastEnd, PanelEdge.End);
        using var workspace = new Workspace(panel);
        var idle = new ControlInputFrame(new Size2(300, 100), TimeSpan.Zero);
        void Frame() => workspace.Update(idle, layout);
        Frame();
        check(panel.Orientation == StackOrientation.Horizontal && start.Bounds.X == 0, "Default Start is left");
        check(firstEnd.Bounds.X == 245 && lastEnd.Bounds.X == 280, "End items keep insertion order against the right edge");
        int measurements = start.Measurements, arrangements = start.Arrangements;
        panel.Put(start, PanelEdge.Start);
        Frame();
        check(start.Measurements == measurements && start.Arrangements == arrangements, "Identical Put retains the layout cache");
        panel.Put(start, PanelEdge.End);
        Frame();
        check(panel.Children.Count == 3 && start.Parent == panel && start.Bounds.X == 200 && lastEnd.Bounds.Right == 300, "Put moves an existing child without duplication");
        panel.Put(firstEnd, PanelEdge.Start);
        Frame();
        check(firstEnd.Bounds.X == 0 && start.Bounds.X == 235, "Moving an end item to Start updates the two groups");
        panel.Orientation = StackOrientation.Vertical;
        Frame();
        check(firstEnd.Bounds.Y == 0 && start.Bounds.Y == 55 && lastEnd.Bounds.Y == 80, "Vertical orientation uses top and bottom");
        lastEnd.IsVisible = false;
        Frame();
        check(start.Bounds.Y == 80 && lastEnd.Bounds.Width == 0 && lastEnd.Bounds.Height == 0, "Hidden children do not reserve spacing or extent");
        panel.Remove(start);
        check(start.Parent is null && panel.GetEdge(start) == PanelEdge.Start, "Remove releases ownership and edge metadata");
        panel.Put(start);
        Frame();
        check(panel.GetEdge(start) == PanelEdge.Start && panel.Children.Count == 3, "Re-added controls default to Start");
        var another = new EdgePanel();
        bool rejected = false;
        try
        {
            another.Put(start, PanelEdge.End);
        }
        catch (InvalidOperationException)
        {
            rejected = true;
        }

        check(rejected && start.Parent == panel && another.Children.Count == 0, "Put cannot steal a control from another owner");
        rejected = false;
        try
        {
            panel.Put(null!);
        }
        catch (ArgumentNullException)
        {
            rejected = true;
        }

        check(rejected, "Null Put is rejected");
        rejected = false;
        try
        {
            panel.Put(start, (PanelEdge)99);
        }
        catch (ArgumentOutOfRangeException)
        {
            rejected = true;
        }

        check(rejected && panel.GetEdge(start) == PanelEdge.Start, "Invalid edges leave placement unchanged");
        var style = new Style
        {
            BasedOn = "EdgePanel",
            Orientation = StackOrientation.Horizontal,
            Spacing = 8
        };
        workspace.Registry.Styles.Register("Test.Edges", style);
        panel.StyleId = "Test.Edges";
        panel.ResetStyleProperty(nameof(EdgePanel.Orientation));
        panel.ResetStyleProperty(nameof(EdgePanel.Spacing));
        Frame();
        check(panel.Orientation == StackOrientation.Horizontal && panel.Spacing == 8, "Orientation and spacing use standard StackPanel styles");
        for (int i = 0; i < 100; i++)
            Frame();
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++)
            Frame();
        long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
        check(bytes == 0, "Unchanged edge panels preserve allocation-free update/layout");
        var narrow = new EdgePanel
        {
            Spacing = 5
        };
        var wide = new MeasureProbe
        {
            Width = 100
        };
        var trailing = new MeasureProbe
        {
            Width = 100
        };
        narrow.Put(wide);
        narrow.Put(trailing, PanelEdge.End);
        using var small = new Workspace(narrow);
        small.Update(new ControlInputFrame(new Size2(80, 50), TimeSpan.Zero), layout);
        check(wide.Bounds.Right <= trailing.Bounds.X && trailing.Bounds.Right <= 80, "Constrained groups clip without overlapping");
        panel.Clear();
        check(panel.Children.Count == 0 && firstEnd.Parent is null && lastEnd.Parent is null && start.Parent is null, "Clear releases all groups");
        check(workspace.Registry.Create("EdgePanel") is EdgePanel, "EdgePanel is registered without reflection");
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Drawing;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Controls.Layouts;
using Sachssoft.Engine.Controls.Styling;

/// <summary>Checks all tab-strip edges, axis-aware navigation, scrolling and style changes.</summary>
internal static class TabPlacementChecks
{
    internal static void Run(Action<bool, string> check, ControlLayoutContext layout)
    {
        var tabs = new TabControl
        {
            HeaderWidth = 110,
            HeaderHeight = 38
        };
        tabs.Items.Add(new TabItem { Header = "First", Content = new Button { Content = "First page" } });
        tabs.Items.Add(new TabItem { Header = "Second", Content = new Button { Content = "Second page" } });
        tabs.SelectedIndex = 0;
        using var workspace = new Workspace(tabs);
        var idle = new ControlInputFrame(new Size2(400, 300), TimeSpan.Zero);
        void Frame() => workspace.Update(idle, layout);
        Frame();
        var headers = (StackPanel)tabs.HeaderScrollViewer.Content!;
        Control firstHeader = headers.Children[0];
        int selections = 0;
        tabs.SelectionChanged += (_, _) => selections++;
        foreach (var placement in new[]
        {
            TabStripPlacement.Top,
            TabStripPlacement.Left,
            TabStripPlacement.Bottom,
            TabStripPlacement.Right
        }

        )
        {
            tabs.TabStripPlacement = placement;
            Frame();
            Bounds2 strip = tabs.HeaderScrollViewer.Bounds, page = tabs.SelectedItem!.Bounds;
            check(placement switch
            {
                TabStripPlacement.Top => strip.Bottom == page.Y && strip.Y == 0 && page.Bottom == 300,
                TabStripPlacement.Left => strip.Right == page.X && strip.X == 0 && page.Right == 400,
                TabStripPlacement.Bottom => page.Bottom == strip.Y && page.Y == 0 && strip.Bottom == 300,
                _ => page.Right == strip.X && page.X == 0 && strip.Right == 400
            }, placement + " places the header beside, without overlapping, the page");
            bool vertical = placement is TabStripPlacement.Left or TabStripPlacement.Right;
            check(headers.Orientation == (vertical ? StackOrientation.Vertical : StackOrientation.Horizontal), placement + " sets the header stack orientation");
            check(vertical ? strip.Width == 110 : strip.Height == 38, placement + " uses the correct strip dimension");
            check(tabs.SelectedIndex == 0 && selections == 0 && tabs.Items[0].Workspace == workspace && tabs.Items[1].Workspace is null, "Changing placement preserves selection and page ownership");
            check(ReferenceEquals(firstHeader, headers.Children[0]), "Placement changes reuse header controls");
            var action = (Button)tabs.SelectedItem.Content!;
            Vector2 point = new(action.Bounds.X + action.Bounds.Width / 2, action.Bounds.Y + action.Bounds.Height / 2);
            workspace.Update(new ControlInputFrame(idle.Size, TimeSpan.Zero, new[] { new ControlInputEvent(ControlInputEventType.PointerDown, point), new ControlInputEvent(ControlInputEventType.PointerUp, point) }), layout);
            check(action.IsPressed, placement + " routes input into the selected page");
            Frame();
            for (int i = 0; i < 100; i++)
                Frame();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 1000; i++)
                Frame();
            long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
            check(bytes == 0, placement + " retains an allocation-free cached update/layout");
        }

        tabs.TabStripPlacement = TabStripPlacement.Left;
        Frame();
        workspace.Focus(headers.Children[0]);
        workspace.Update(new ControlInputFrame(idle.Size, TimeSpan.Zero, new[] { new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Down) }), layout);
        check(tabs.SelectedIndex == 1, "Vertical tab headers navigate down");
        workspace.Update(new ControlInputFrame(idle.Size, TimeSpan.Zero, new[] { new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Up) }), layout);
        check(tabs.SelectedIndex == 0, "Vertical tab headers navigate up");
        var style = new Style
        {
            BasedOn = "TabControl",
            TabStripPlacement = TabStripPlacement.Bottom,
            HeaderWidth = 135
        };
        workspace.Registry.Styles.Register("Test.Tabs", style);
        tabs.StyleId = "Test.Tabs";
        Frame();
        check(tabs.TabStripPlacement == TabStripPlacement.Left && tabs.HeaderWidth == 110, "Local placement and width override styles");
        tabs.ResetStyleProperty(nameof(TabControl.TabStripPlacement));
        tabs.ResetStyleProperty(nameof(TabControl.HeaderWidth));
        Frame();
        check(tabs.TabStripPlacement == TabStripPlacement.Bottom && tabs.HeaderWidth == 135, "Reset restores the central placement style");
        style.TabStripPlacement = TabStripPlacement.Right;
        Frame();
        check(tabs.TabStripPlacement == TabStripPlacement.Right && tabs.HeaderScrollViewer.Bounds.Width == 135, "Live style changes update the edge layout");
        bool rejected = false;
        try
        {
            tabs.TabStripPlacement = (TabStripPlacement)99;
        }
        catch (ArgumentOutOfRangeException)
        {
            rejected = true;
        }

        check(rejected && tabs.TabStripPlacement == TabStripPlacement.Right, "Invalid edges leave placement unchanged");
        var many = new TabControl
        {
            TabStripPlacement = TabStripPlacement.Left,
            HeaderWidth = 100,
            ScrollBarThickness = 14
        };
        for (int i = 0; i < 16; i++)
            many.Items.Add(new TabItem { Header = "Tab " + i, Content = new TextBlock { Text = "Page " + i } });
        many.SelectedIndex = 15;
        using var scrolling = new Workspace(many);
        var small = new ControlInputFrame(new Size2(320, 160), TimeSpan.Zero);
        scrolling.Update(small, layout);
        scrolling.Update(small, layout);
        var viewer = many.HeaderScrollViewer;
        var manyHeaders = (StackPanel)viewer.Content!;
        check(viewer.VerticalScrollBar.IsVisible && !viewer.HorizontalScrollBar.IsVisible && viewer.VerticalScrollBar.Value > 0, "Side headers scroll vertically to the selected tab");
        check(viewer.Bounds.Width == 114 && viewer.ScrollBarThickness == 14, "Vertical overflow reserves the configured scrollbar thickness");
        Control last = manyHeaders.Children[15];
        check(last.Bounds.Y >= viewer.ViewportBounds.Y - .01f && last.Bounds.Bottom <= viewer.ViewportBounds.Bottom + .01f, "Selected vertical header is visible");
        many.Measure(layout, new Size2(float.PositiveInfinity, float.PositiveInfinity));
        many.Arrange(new Bounds2(0, 0, 320, 160), true);
        check(viewer.VerticalScrollBar.IsVisible && viewer.Bounds.Width == 114 && viewer.ViewportBounds.Width == 100, "Finite arrangement reserves vertical scrollbar space after unbounded measurement");
        many.TabStripPlacement = TabStripPlacement.Bottom;
        scrolling.Update(small, layout);
        scrolling.Update(small, layout);
        check(viewer.HorizontalScrollBar.IsVisible && !viewer.VerticalScrollBar.IsVisible && viewer.HorizontalScrollBar.Value > 0, "Changing orientation moves overflow onto the horizontal axis");
        check(viewer.Bounds.Height == many.HeaderHeight + 14 && many.SelectedIndex == 15, "Bottom overflow retains the page and scrollbar thickness");
        last = manyHeaders.Children[15];
        check(last.Bounds.X >= viewer.ViewportBounds.X - .01f && last.Bounds.Right <= viewer.ViewportBounds.Right + .01f, "Selected horizontal header is visible after changing placement");
        many.Measure(layout, new Size2(float.PositiveInfinity, float.PositiveInfinity));
        many.Arrange(new Bounds2(0, 0, 320, 160), true);
        check(viewer.HorizontalScrollBar.IsVisible && viewer.Bounds.Height == many.HeaderHeight + 14 && viewer.ViewportBounds.Height == many.HeaderHeight, "Finite arrangement reserves horizontal scrollbar space after unbounded measurement");
        scrolling.Update(new ControlInputFrame(new Size2(500, 500), TimeSpan.Zero), layout);
        check(many.SelectedItem!.Bounds.Width == 500 && viewer.Bounds.Bottom == 500, "Viewport resize keeps bottom tabs anchored");
    }
}

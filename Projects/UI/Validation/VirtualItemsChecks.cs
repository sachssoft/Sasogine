using Microsoft.Xna.Framework;
using System.Collections.ObjectModel;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Controls.Layouts;

/// <summary>Checks fixed-height virtualization without a graphics device.</summary>
internal static class VirtualItemsChecks
{
    /// <summary>Exercises bounded row creation, rebinding, touch and resizing.</summary>
    /// <param name="check">The assertion callback.</param>
    /// <param name="layout">The shared layout context.</param>
    internal static void Run(Action<bool, string> check, ControlLayoutContext layout)
    {
        const int sourceCount = 100_000;
        var indices = new Dictionary<TextBlock, int>();
        int created = 0;
        int bindings = 0;
        var list = new ItemsControl
        {
            ItemsSource = Enumerable.Range(0, sourceCount).ToArray(),
            ItemsPanel = new DelegateTemplate<ItemsControl, ContainerControl>(_ => new VirtualStackPanel { ItemHeight = 20 }),
            ItemTemplate = new RecyclingItemTemplate<TextBlock>(
                () => { created++; return new TextBlock(); },
                (row, item) => { int index = (int)item!; bindings++; indices[row] = index; row.Text = $"Rank {index + 1}"; })
        };
        ScrollViewer viewer = list.ItemsScrollViewer!;
        using var root = new Workspace(list);
        var size = new Size2(320, 200);
        var idle = new ControlInputFrame(size, TimeSpan.Zero);
        root.Update(idle, layout);
        root.Update(idle, layout);
        var bar = viewer.VerticalScrollBar;
        check(created <= 15 && bindings == created && bar.Maximum == sourceCount * 20 - viewer.ViewportBounds.Height,
            "The viewport creates at most fifteen rows for one hundred thousand entries");

        int poolSize = created;
        bar.Value = 100_000;
        root.Update(idle, layout);
        check(created == poolSize && indices.Values.Min() == 4998 && indices.Values.Max() < 5015 &&
            indices.All(entry => entry.Key.Text == $"Rank {entry.Value + 1}" && (int)entry.Key.DataContext! == entry.Value),
            "A large scroll jump reuses the pool and binds the destination ranks");

        int previousBindings = bindings;
        bar.Value += 20;
        root.Update(idle, layout);
        check(bindings == previousBindings + 1 && created == poolSize, "Crossing one row only rebinds the row entering the buffer");

        float beforeTouch = bar.Value;
        root.Update(new ControlInputFrame(size, TimeSpan.Zero, [
            Touch(ControlInputEventType.PointerDown, 150), Touch(ControlInputEventType.PointerMove, 90),
            Touch(ControlInputEventType.PointerUp, 90)]), layout);
        check(bar.Value == beforeTouch + 60 && created == poolSize, "Touch scrolling moves the virtual viewport without creating rows");

        bar.Value = bar.Maximum;
        root.Update(idle, layout);
        check(indices.Values.Max() == sourceCount - 1, "Scrolling to the end binds the final rank");

        var smaller = new ControlInputFrame(new Size2(320, 80), TimeSpan.Zero);
        root.Update(smaller, layout);
        root.Update(smaller, layout);
        check(list.ItemsPanelRoot.Children.Count <= 9, "A smaller viewport releases surplus rows");
        CheckTiles(check, layout);
    }

    /// <summary>Checks observable data changes, fixed tiles and adaptive resize anchoring.</summary>
    /// <param name="check">The assertion callback.</param>
    /// <param name="layout">The shared layout context.</param>
    private static void CheckTiles(Action<bool, string> check, ControlLayoutContext layout)
    {
        var source = new ObservableCollection<string>(Enumerable.Range(0, 1000).Select(index => $"Tile {index}"));
        var list = new ItemsControl
        {
            ItemsSource = source,
            ItemsPanel = new DelegateTemplate<ItemsControl, ContainerControl>(_ => new VirtualTilePanel
            {
                Columns = 3, TileSize = new Size2(60, 40), Spacing = 4
            }),
            ItemTemplate = new RecyclingItemTemplate<TextBlock>(() => new TextBlock(), (item, data) => item.Text = (string)data!)
        };
        using var root = new Workspace(list);
        var wide = new ControlInputFrame(new Size2(340, 200), TimeSpan.Zero);
        root.Update(wide, layout);
        root.Update(wide, layout);
        Control first = list.ContainerFromIndex(0);
        check(list.ContainerFromIndex(1).Bounds.X - first.Bounds.X == 64 &&
            list.ContainerFromIndex(3).Bounds.Y - first.Bounds.Y == 44,
            "Fixed virtual tiles preserve their configured column and row sizes");
        source[0] = "Updated tile";
        root.Update(wide, layout);
        check(ReferenceEquals(first, list.ContainerFromIndex(0)) && (string?)first.DataContext == "Updated tile" &&
            ((TextBlock)first).Text == "Updated tile", "Observable replacements rebind the existing presentation and data context");

        list.ItemsPanel = new DelegateTemplate<ItemsControl, ContainerControl>(_ => new VirtualAdaptiveTilePanel
        {
            PreferredTileSize = new Size2(100), Spacing = 4
        });
        root.Update(wide, layout);
        root.Update(wide, layout);
        ScrollViewer viewport = list.ItemsScrollViewer!;
        viewport.VerticalScrollBar.Value = 750;
        root.Update(wide, layout);
        int anchor = Enumerable.Range(0, source.Count).First(index => list.TryContainerFromIndex(index) is { } item &&
            item.Bounds.Bottom > viewport.ViewportBounds.Top);
        var narrow = new ControlInputFrame(new Size2(230, 200), TimeSpan.Zero);
        root.Update(narrow, layout);
        root.Update(narrow, layout);
        check(list.TryContainerFromIndex(anchor) is { } anchored && anchored.Bounds.Bottom > viewport.ViewportBounds.Top &&
            anchored.Bounds.Top < viewport.ViewportBounds.Bottom, "Adaptive reflow keeps the first visible item in view");
    }

    /// <summary>Creates an event for the same finger inside the scroll viewport.</summary>
    /// <param name="type">The touch event phase.</param>
    /// <param name="y">The vertical pointer coordinate.</param>
    /// <returns>The touch input event.</returns>
    private static ControlInputEvent Touch(ControlInputEventType type, float y) =>
        new(type, new Vector2(100, y), PointerId: 1, PointerKind: ControlPointerKind.Touch);
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Drawing;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Controls.Layouts;
using Sachssoft.Engine.Controls.Styling;
using System;
using System.Collections.Generic;

/// <summary>Checks finger scrolling without accidental list selection, activation or parent gesture conflicts.</summary>
internal static class ListBoxSwipeChecks
{
    /// <summary>Runs touch/tap, cancellation, style, nested scroll and steady-frame allocation checks.</summary>
    /// <param name="check">The assertion callback.</param>
    /// <param name="layout">The deterministic font and layout context.</param>
    internal static void Run(Action<bool, string> check, ControlLayoutContext layout)
    {
        var list = CreateList();
        using var root = new Workspace(list);
        var size = new Size2(300, 180);
        var idle = new ControlInputFrame(size, TimeSpan.Zero);
        void Frame(params ControlInputEvent[] events) => root.Update(events.Length == 0 ? idle : new ControlInputFrame(size, TimeSpan.Zero, events), layout);
        Frame();
        int activations = 0;
        int doubleClicks = 0;
        list.ItemActivated += (_, _) => activations++;
        list.ItemDoubleClicked += (_, _) => doubleClicks++;
        list.SelectedIndex = 3;
        Frame();
        check(list.IsSwipeEnabled && list.SwipeThreshold == 12 && list.ScrollBar.Maximum > 0, "List touch scrolling is enabled by default with a positive threshold");
        Frame(Touch(ControlInputEventType.PointerDown, 50, 120));
        check(list.SelectedIndex == 3 && activations == 0, "Touch-down defers selection until the gesture is known to be a tap");
        Frame(Touch(ControlInputEventType.PointerMove, 50, 40));
        check(list.IsSwiping && list.ScrollBar.Value == 80 && list.SelectedIndex == 3, "A vertical finger gesture scrolls without changing selection");
        Frame(Touch(ControlInputEventType.PointerUp, 50, 20));
        check(list.ScrollBar.Value == 100 && !list.IsSwiping && activations == 0 && root.GetCapturedControl(new(ControlPointerKind.Touch, 1)) is null,
            "Release applies the final displacement and clears capture without activation");
        Frame(Touch(ControlInputEventType.PointerDown, 50, 40), Touch(ControlInputEventType.PointerMove, 50, 35), Touch(ControlInputEventType.PointerUp, 50, 35));
        check(list.SelectedIndex == 4 && activations == 1 && list.ScrollBar.Value == 100, "A short finger movement remains a normal item tap");
        Frame(Touch(ControlInputEventType.PointerDown, 50, 35), Touch(ControlInputEventType.PointerUp, 50, 35));
        check(activations == 2 && doubleClicks == 1, "Touch double-taps retain the list's double-click action");
        Frame(Touch(ControlInputEventType.PointerDown, 50, 100), Touch(ControlInputEventType.PointerMove, 50, 5000), Touch(ControlInputEventType.PointerUp, 50, 5000));
        check(list.ScrollBar.Value == 0 && activations == 2, "Reverse swipes clamp at the beginning without activating items");
        Frame(Touch(ControlInputEventType.PointerDown, 50, 100), Touch(ControlInputEventType.PointerMove, 50, -5000), Touch(ControlInputEventType.PointerUp, 50, -5000));
        check(list.ScrollBar.Value == list.ScrollBar.Maximum, "Oversized finger gestures clamp at the end");
        list.ScrollBar.Value = 0;
        Frame();
        Frame(Touch(ControlInputEventType.PointerDown, 50, 100), Touch(ControlInputEventType.PointerCancel, 50, 100), Touch(ControlInputEventType.PointerMove, 50, 20), Touch(ControlInputEventType.PointerUp, 50, 20));
        check(list.ScrollBar.Value == 0 && activations == 2, "Cancelled pending touches cannot scroll or activate later");
        Frame(Touch(ControlInputEventType.PointerDown, 50, 100), Touch(ControlInputEventType.PointerMove, 50, 50), Touch(ControlInputEventType.PointerCancel, 50, 50));
        check(!list.IsSwiping && list.ScrollBar.Value == 50 && activations == 2, "Cancelling an active swipe preserves its offset and clears held state");
        list.ScrollBar.Value = 0;
        Frame();
        Frame(Touch(ControlInputEventType.PointerDown, 50, 120), Touch(ControlInputEventType.PointerMove, 50, 70), Touch(ControlInputEventType.PointerMove, 50, -100, 2), Touch(ControlInputEventType.PointerUp, 50, -100, 2));
        check(list.IsSwiping && list.ScrollBar.Value == 50, "A second finger cannot move or release the first finger's gesture");
        list.IsSwipeEnabled = false;
        check(!list.IsSwiping && root.GetCapturedControl(new(ControlPointerKind.Touch, 1)) is null, "Disabling touch scrolling releases capture immediately");
        list.IsSwipeEnabled = true;
        list.ScrollBar.Value = 0;
        Frame();
        Frame(new ControlInputEvent(ControlInputEventType.PointerDown, new Vector2(50, 15)));
        check(list.SelectedIndex == 0, "Mouse-down still selects immediately");
        Frame(new ControlInputEvent(ControlInputEventType.PointerUp, new Vector2(50, 15)));
        Frame(new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Down));
        check(list.SelectedIndex == 1, "Keyboard selection remains unchanged");
        Frame(new ControlInputEvent(ControlInputEventType.PointerWheel, new Vector2(50, 15), WheelDelta: -120));
        check(list.ScrollBar.Value > 0, "Mouse wheel scrolling remains unchanged");
        var style = new Style { BasedOn = nameof(ListBox), IsSwipeEnabled = false, SwipeThreshold = 20 };
        root.Styles.Register("Test.ListSwipe", style);
        list.StyleId = "Test.ListSwipe";
        list.ResetStyleProperty(nameof(ListBox.IsSwipeEnabled));
        list.ResetStyleProperty(nameof(ListBox.SwipeThreshold));
        Frame();
        check(!list.IsSwipeEnabled && list.SwipeThreshold == 20, "List touch settings resolve through the central style system");
        style.IsSwipeEnabled = true;
        Frame();
        check(list.IsSwipeEnabled, "Live list style changes re-enable scrolling");
        bool rejected = false;
        try { list.SwipeThreshold = 0; } catch (ArgumentOutOfRangeException) { rejected = true; }
        check(rejected && list.SwipeThreshold == 20, "Invalid thresholds preserve the current movement threshold");
        list.SwipeThreshold = 12;
        list.ScrollBar.Value = 0;
        Frame();
        Frame(Touch(ControlInputEventType.PointerDown, 50, 100));
        list.Items[0] = new SelectorItem("Replaced");
        Frame(Touch(ControlInputEventType.PointerMove, 50, 30), Touch(ControlInputEventType.PointerUp, 50, 30));
        check(!list.IsSwiping && list.ScrollBar.Value == 0, "Changing item data cancels a pending gesture");
        for (int i = 0; i < 100; i++) Frame();
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++) Frame();
        long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
        check(bytes == 0, "Idle swipe-enabled lists retain zero-allocation frames: " + bytes);
        var samples = new List<ControlInputEvent>(1);
        var moving = new ControlInputFrame(samples);
        void Move(int index)
        {
            samples.Clear();
            samples.Add(Touch(ControlInputEventType.PointerMove, 50, index % 2 == 0 ? 70 : 80));
            root.Update(moving.Reset(size, TimeSpan.Zero), layout);
        }
        Frame(Touch(ControlInputEventType.PointerDown, 50, 140));
        for (int i = 0; i < 100; i++) Move(i);
        before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++) Move(i);
        bytes = GC.GetAllocatedBytesForCurrentThread() - before;
        check(bytes == 0, "Steady finger movement reuses input and list state without allocations: " + bytes);
        Frame(Touch(ControlInputEventType.PointerUp, 50, 80));
        CheckOversizedAndEmpty(check, layout, idle);
        CheckNested(check, layout, idle);
        CheckAssets(check, layout, idle);
    }

    /// <summary>Checks empty hit regions and tall cards without selection-induced scroll jumps.</summary>
    /// <param name="check">The assertion callback.</param>
    /// <param name="layout">The deterministic font and layout context.</param>
    /// <param name="idle">The shared empty frame and viewport.</param>
    private static void CheckOversizedAndEmpty(Action<bool, string> check, ControlLayoutContext layout, ControlInputFrame idle)
    {
        var tall = new ListBox { ItemHeight = 300 };
        tall.Items.Add(new SelectorItem("Tall card")); tall.Items.Add(new SelectorItem("Next card"));
        using var root = new Workspace(tall);
        root.Update(idle, layout);
        tall.ScrollBar.Value = 40;
        root.Update(idle, layout);
        root.Update(new ControlInputFrame(idle.Size, TimeSpan.Zero, new[] { Touch(ControlInputEventType.PointerDown, 50, 100), Touch(ControlInputEventType.PointerUp, 50, 100) }), layout);
        check(tall.SelectedIndex == 0 && tall.ScrollBar.Value == 40, "Tapping an already visible tall card does not snap its scroll offset");
        var empty = new ListBox();
        using var emptyRoot = new Workspace(empty);
        emptyRoot.Update(new ControlInputFrame(idle.Size, TimeSpan.Zero, new[] { Touch(ControlInputEventType.PointerDown, 50, 100), Touch(ControlInputEventType.PointerUp, 50, 100) }), layout);
        check(empty.SelectedIndex == -1 && emptyRoot.GetCapturedControl(new(ControlPointerKind.Touch, 1)) is null, "Empty list taps release pointer capture");
    }

    /// <summary>Checks priority and boundary handoff against scroll viewers and both carousel axes.</summary>
    /// <param name="check">The assertion callback.</param>
    /// <param name="layout">The deterministic font and layout context.</param>
    /// <param name="idle">The shared empty frame and viewport.</param>
    private static void CheckNested(Action<bool, string> check, ControlLayoutContext layout, ControlInputFrame idle)
    {
        var inner = CreateList(); inner.Height = 140;
        var stack = new StackPanel(); stack.Add(inner); stack.Add(new Panel { Height = 800 });
        var outer = new ScrollViewer { Content = stack, HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden };
        using var root = new Workspace(outer);
        void Frame(params ControlInputEvent[] events) => root.Update(new ControlInputFrame(idle.Size, TimeSpan.Zero, events), layout);
        Frame();
        Frame(Touch(ControlInputEventType.PointerDown, 50, 100), Touch(ControlInputEventType.PointerMove, 50, 60));
        check(inner.ScrollBar.Value == 40 && outer.VerticalScrollBar.Value == 0 && inner.IsSwiping, "A nested list consumes vertical movement before its parent viewer");
        Frame(Touch(ControlInputEventType.PointerMove, 50, -2000));
        Frame(Touch(ControlInputEventType.PointerMove, 50, -2030), Touch(ControlInputEventType.PointerUp, 50, -2040));
        check(outer.VerticalScrollBar.Value == 40 && !inner.IsSwiping, "List boundary handoff consumes only new movement in the parent");
        foreach (ControlOrientation orientation in new[] { ControlOrientation.Horizontal, ControlOrientation.Vertical })
        {
            var page = CreateList();
            var carousel = new Carousel { Orientation = orientation, Transition = CarouselTransition.None, NavigationMode = CarouselNavigationMode.Swipe, ShowIndicators = false };
            carousel.Items.Add(page); carousel.Items.Add(new Panel());
            using var pages = new Workspace(carousel);
            pages.Update(idle, layout);
            pages.Update(new ControlInputFrame(idle.Size, TimeSpan.Zero, new[] { Touch(ControlInputEventType.PointerDown, 150, 140), Touch(ControlInputEventType.PointerMove, 150, 60), Touch(ControlInputEventType.PointerUp, 150, 40) }), layout);
            check(page.ScrollBar.Value == 100 && carousel.SelectedIndex == 0, "List scrolling wins over " + orientation + " carousel paging");
            if (orientation == ControlOrientation.Vertical) page.ScrollBar.Value = page.ScrollBar.Maximum;
            pages.Update(idle, layout);
            var end = orientation == ControlOrientation.Horizontal ? new Vector2(30, 100) : new Vector2(150, 20);
            var start = orientation == ControlOrientation.Horizontal ? new Vector2(230, 100) : new Vector2(150, 140);
            pages.Update(new ControlInputFrame(idle.Size, TimeSpan.Zero, new[] { Touch(ControlInputEventType.PointerDown, start.X, start.Y), Touch(ControlInputEventType.PointerMove, end.X, end.Y), Touch(ControlInputEventType.PointerUp, end.X, end.Y) }), layout);
            check(carousel.SelectedIndex == 1, "The parent carousel can consume its axis or an exhausted list boundary");
        }
    }

    /// <summary>Checks inherited asset tile scrolling without starting asset drag-and-drop.</summary>
    /// <param name="check">The assertion callback.</param>
    /// <param name="layout">The deterministic font and layout context.</param>
    /// <param name="idle">The shared empty frame and viewport.</param>
    private static void CheckAssets(Action<bool, string> check, ControlLayoutContext layout, ControlInputFrame idle)
    {
        var assets = new AssetView();
        for (int i = 0; i < 30; i++) assets.Items.Add(new SelectorItem("Asset " + i, new AssetItem(new Sachssoft.Engine.Assets.Graphics.Texture2DAsset("asset_" + i), "Asset " + i)));
        using var root = new Workspace(assets);
        root.Update(idle, layout);
        root.Update(new ControlInputFrame(idle.Size, TimeSpan.Zero, new[] { Touch(ControlInputEventType.PointerDown, 50, 140), Touch(ControlInputEventType.PointerMove, 50, 60), Touch(ControlInputEventType.PointerUp, 50, 40) }), layout);
        check(assets.ScrollBar.Value == 100 && assets.SelectedIndex == -1, "AssetView inherits finger scrolling without selection or activation");
    }

    /// <summary>Creates a deterministic long list with no data-source or template allocation during movement.</summary>
    /// <returns>A list of 50 fixed-height text entries.</returns>
    private static ListBox CreateList()
    {
        var list = new ListBox { ItemHeight = 30 };
        for (int i = 0; i < 50; i++) list.Items.Add(new SelectorItem("Entry " + i));
        return list;
    }

    /// <summary>Creates one logical touch event with a stable pointer identity.</summary>
    /// <param name="type">The pointer event kind.</param>
    /// <param name="x">The logical horizontal coordinate.</param>
    /// <param name="y">The logical vertical coordinate.</param>
    /// <param name="id">The independent finger identity.</param>
    /// <returns>The immutable event data.</returns>
    private static ControlInputEvent Touch(ControlInputEventType type, float x, float y, int id = 1) => new(type, new Vector2(x, y), PointerId: id, PointerKind: ControlPointerKind.Touch);
}

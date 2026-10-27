using Microsoft.Xna.Framework;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Controls.Layouts;
using Sachssoft.Engine.Controls.Styling;

/// <summary>Checks direct touch scrolling, child activation, nested handoff, carousel arbitration and stable-frame costs.</summary>
internal static class ScrollViewerSwipeChecks
{
    /// <summary>Runs deterministic swipe scrolling regression checks.</summary>
    /// <param name = "check">The assertion callback.</param>
    /// <param name = "layout">The fixed test font and layout context.</param>
    internal static void Run(Action<bool, string> check, ControlLayoutContext layout)
    {
        var button = new Button
        {
            Content = "Scrollable button",
            Height = 900
        };
        var viewer = new ScrollViewer
        {
            Content = button,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden
        };
        using var workspace = new Workspace(viewer);
        var size = new Size2(400, 300);
        var idle = new ControlInputFrame(size, TimeSpan.Zero);
        void Frame(params ControlInputEvent[] events) => workspace.Update(events.Length == 0 ? idle : new ControlInputFrame(size, TimeSpan.Zero, events), layout);
        ControlInputEvent Touch(ControlInputEventType type, float x, float y, int id = 1) => new(type, new(x, y), PointerId: id, PointerKind: ControlPointerKind.Touch);
        int clicks = 0;
        button.Clicked += (_, _) => clicks++;
        Frame();
        check(viewer.IsSwipeEnabled && !viewer.IsMouseDragEnabled && viewer.VerticalScrollBar.Maximum > 0, "Touch scrolling is enabled by default without changing desktop mouse dragging");
        Frame(Touch(ControlInputEventType.PointerDown, 100, 150), Touch(ControlInputEventType.PointerMove, 100, 145), Touch(ControlInputEventType.PointerUp, 100, 145));
        check(clicks == 1 && viewer.VerticalScrollBar.Value == 0, "A short touch remains a normal child click");
        Frame(Touch(ControlInputEventType.PointerDown, 100, 220), Touch(ControlInputEventType.PointerMove, 100, 140));
        check(viewer.IsSwiping && viewer.VerticalScrollBar.Value == 80 && !button.IsHeld && clicks == 1, "Crossing the threshold takes capture, cancels child activation and follows the finger");
        check(button.Bounds.Y == viewer.ViewportBounds.Y - 80, "Swipe offsets are reflected by arranged content before Draw");
        Frame(Touch(ControlInputEventType.PointerMove, 100, 120), Touch(ControlInputEventType.PointerUp, 100, 100));
        check(!viewer.IsSwiping && viewer.VerticalScrollBar.Value == 120 && clicks == 1 && workspace.GetCapturedControl(new(ControlPointerKind.Touch, 1))is null, "Release includes the final pointer displacement and ends capture without a click");
        Frame(Touch(ControlInputEventType.PointerDown, 100, 150), Touch(ControlInputEventType.PointerMove, 100, -2000), Touch(ControlInputEventType.PointerUp, 100, -2000));
        check(viewer.VerticalScrollBar.Value == viewer.VerticalScrollBar.Maximum, "Oversized gestures clamp at the maximum scroll offset");
        Frame(Touch(ControlInputEventType.PointerDown, 100, 100), Touch(ControlInputEventType.PointerMove, 100, 3000), Touch(ControlInputEventType.PointerUp, 100, 3000));
        check(viewer.VerticalScrollBar.Value == 0, "Reverse gestures clamp at the beginning");
        Frame(Touch(ControlInputEventType.PointerDown, 100, 200), Touch(ControlInputEventType.PointerCancel, 100, 200), Touch(ControlInputEventType.PointerMove, 100, 100), Touch(ControlInputEventType.PointerUp, 100, 100));
        check(viewer.VerticalScrollBar.Value == 0 && clicks == 1 && !viewer.IsSwiping, "Cancelling a pending gesture prevents later movement and activation");
        Frame(Touch(ControlInputEventType.PointerDown, 100, 200), Touch(ControlInputEventType.PointerMove, 100, 150), Touch(ControlInputEventType.PointerCancel, 100, 150));
        check(viewer.VerticalScrollBar.Value == 50 && !viewer.IsSwiping && !button.IsHeld, "Cancelling an active swipe preserves offset and clears held state");
        viewer.VerticalScrollBar.Value = 0;
        Frame();
        Frame(new(ControlInputEventType.PointerDown, new(100, 200)), new(ControlInputEventType.PointerMove, new(100, 100)), new(ControlInputEventType.PointerUp, new(100, 100)));
        check(viewer.VerticalScrollBar.Value == 0, "Mouse dragging keeps its original behavior until opted in");
        viewer.IsMouseDragEnabled = true;
        Frame(new(ControlInputEventType.PointerDown, new(100, 200)), new(ControlInputEventType.PointerMove, new(100, 100)), new(ControlInputEventType.PointerUp, new(100, 100)));
        check(viewer.VerticalScrollBar.Value == 100 && !viewer.IsSwiping, "Opt-in primary-button dragging scrolls the viewport");
        Frame(Touch(ControlInputEventType.PointerDown, 100, 200), Touch(ControlInputEventType.PointerMove, 100, 150));
        viewer.IsSwipeEnabled = false;
        check(!viewer.IsSwiping && workspace.GetCapturedControl(new(ControlPointerKind.Touch, 1))is null, "Disabling swipe releases an active gesture immediately");
        viewer.IsSwipeEnabled = true;
        viewer.VerticalScrollBar.Value = 0;
        Frame();
        Frame(Touch(ControlInputEventType.PointerDown, 100, 200), Touch(ControlInputEventType.PointerMove, 100, 150), Touch(ControlInputEventType.PointerMove, 100, 20, 2));
        check(viewer.VerticalScrollBar.Value == 50, "A second pointer cannot move another pointer's captured gesture");
        Frame(Touch(ControlInputEventType.PointerUp, 100, 150));
        bool rejected = false;
        try
        {
            viewer.SwipeThreshold = 0;
        }
        catch (ArgumentOutOfRangeException)
        {
            rejected = true;
        }

        check(rejected && viewer.SwipeThreshold == 12, "The movement threshold must be finite and positive");
        workspace.Registry.Styles.Register("Test.Swipe", new Style { BasedOn = "ScrollViewer", IsSwipeEnabled = false, IsMouseDragEnabled = true, SwipeThreshold = 20 });
        viewer.StyleId = "Test.Swipe";
        viewer.ResetStyleProperty(nameof(ScrollViewer.IsSwipeEnabled));
        viewer.ResetStyleProperty(nameof(ScrollViewer.IsMouseDragEnabled));
        viewer.ResetStyleProperty(nameof(ScrollViewer.SwipeThreshold));
        Frame();
        check(!viewer.IsSwipeEnabled && viewer.IsMouseDragEnabled && viewer.SwipeThreshold == 20, "Swipe configuration uses central styles and local reset");
        var plane = new Panel
        {
            Width = 1000,
            Height = 1000
        };
        var both = new ScrollViewer
        {
            Content = plane
        };
        using var bothWorkspace = new Workspace(both);
        bothWorkspace.Update(idle, layout);
        bothWorkspace.Update(new ControlInputFrame(size, TimeSpan.Zero, [Touch(ControlInputEventType.PointerDown, 200, 200), Touch(ControlInputEventType.PointerMove, 140, 120), Touch(ControlInputEventType.PointerUp, 140, 120)]), layout);
        check(both.HorizontalScrollBar.Value == 60 && both.VerticalScrollBar.Value == 80, "A captured diagonal gesture updates both enabled axes");
        var inner = new ScrollViewer
        {
            Height = 140,
            Content = new Panel
            {
                Height = 600
            },
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden
        };
        var stack = new StackPanel();
        stack.Add(inner);
        stack.Add(new Panel { Height = 800 });
        var outer = new ScrollViewer
        {
            Content = stack,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden
        };
        using var nestedWorkspace = new Workspace(outer);
        void Nested(params ControlInputEvent[] events) => nestedWorkspace.Update(events.Length == 0 ? idle : new ControlInputFrame(size, TimeSpan.Zero, events), layout);
        Nested();
        Nested(Touch(ControlInputEventType.PointerDown, 100, 110), Touch(ControlInputEventType.PointerMove, 100, 70));
        check(inner.IsSwiping && inner.VerticalScrollBar.Value == 40 && outer.VerticalScrollBar.Value == 0, "The inner scroll viewer consumes its gesture first");
        Nested(Touch(ControlInputEventType.PointerMove, 100, -500));
        check(inner.VerticalScrollBar.Value == inner.VerticalScrollBar.Maximum && outer.VerticalScrollBar.Value == 0, "The inner viewer reaches its own boundary before handoff");
        Nested(Touch(ControlInputEventType.PointerMove, 100, -530), Touch(ControlInputEventType.PointerUp, 100, -540));
        check(outer.VerticalScrollBar.Value == 40 && !inner.IsSwiping && !outer.IsSwiping, "Boundary handoff applies only new movement to the parent without a jump");
        Nested(Touch(ControlInputEventType.PointerDown, 100, 70), Touch(ControlInputEventType.PointerMove, 100, 100), Touch(ControlInputEventType.PointerUp, 100, 100));
        check(inner.VerticalScrollBar.Value == inner.VerticalScrollBar.Maximum - 30 && outer.VerticalScrollBar.Value == 40, "A new reversed gesture can scroll the inner viewer again");
        var slider = new Slider
        {
            Width = 300,
            Height = 40
        };
        var editors = new StackPanel();
        editors.Add(slider);
        editors.Add(new Panel { Height = 800 });
        var editing = new ScrollViewer
        {
            Content = editors,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden
        };
        using var editingWorkspace = new Workspace(editing);
        editingWorkspace.Update(idle, layout);
        editingWorkspace.Update(new ControlInputFrame(size, TimeSpan.Zero, [Touch(ControlInputEventType.PointerDown, 20, 20), Touch(ControlInputEventType.PointerMove, 220, 20), Touch(ControlInputEventType.PointerUp, 220, 20)]), layout);
        check(slider.Value > 50 && editing.VerticalScrollBar.Value == 0 && !editing.IsSwiping, "Sliders keep direct manipulation without starting parent scrolling");
        var scrollPage = new ScrollViewer
        {
            Content = new Panel
            {
                Height = 1000
            },
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden
        };
        var carousel = new Carousel
        {
            Transition = CarouselTransition.None,
            NavigationMode = CarouselNavigationMode.Swipe,
            ShowIndicators = false
        };
        carousel.Items.Add(scrollPage);
        carousel.Items.Add(new Panel());
        using var carouselWorkspace = new Workspace(carousel);
        carouselWorkspace.Update(idle, layout);
        carouselWorkspace.Update(new ControlInputFrame(size, TimeSpan.Zero, [Touch(ControlInputEventType.PointerDown, 200, 200), Touch(ControlInputEventType.PointerMove, 200, 100), Touch(ControlInputEventType.PointerUp, 200, 100)]), layout);
        check(scrollPage.VerticalScrollBar.Value == 100 && carousel.SelectedIndex == 0, "A scrollable carousel page consumes vertical swipes without changing pages");
        carouselWorkspace.Update(new ControlInputFrame(size, TimeSpan.Zero, [Touch(ControlInputEventType.PointerDown, 250, 100), Touch(ControlInputEventType.PointerMove, 100, 100), Touch(ControlInputEventType.PointerUp, 100, 100)]), layout);
        check(carousel.SelectedIndex == 1, "The carousel can still switch pages along an axis the viewer cannot scroll");
        var measured = new MeasureProbe
        {
            Height = 900
        };
        var cached = new ScrollViewer
        {
            Content = measured,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden
        };
        using var cachedWorkspace = new Workspace(cached);
        cachedWorkspace.Update(idle, layout);
        int measurements = measured.Measurements;
        cachedWorkspace.Update(new ControlInputFrame(size, TimeSpan.Zero, [Touch(ControlInputEventType.PointerDown, 100, 200), Touch(ControlInputEventType.PointerMove, 100, 150), Touch(ControlInputEventType.PointerUp, 100, 150)]), layout);
        check(measured.Measurements == measurements, "Changing only the scroll offset preserves cached child measurement");
        for (int i = 0; i < 100; i++)
            cachedWorkspace.Update(idle, layout);
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++)
            cachedWorkspace.Update(idle, layout);
        check(GC.GetAllocatedBytesForCurrentThread() == before, "Idle swipe-enabled viewers allocate no memory");
        var samples = new List<ControlInputEvent>(1);
        var movingFrame = new ControlInputFrame(samples);
        void Move(int index)
        {
            samples.Clear();
            samples.Add(Touch(ControlInputEventType.PointerMove, 100, index % 2 == 0 ? 150 : 160));
            cachedWorkspace.Update(movingFrame.Reset(size, TimeSpan.Zero), layout);
        }

        cachedWorkspace.Update(new ControlInputFrame(size, TimeSpan.Zero, [Touch(ControlInputEventType.PointerDown, 100, 200)]), layout);
        for (int i = 0; i < 100; i++)
            Move(i);
        measurements = measured.Measurements;
        before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++)
            Move(i);
        check(GC.GetAllocatedBytesForCurrentThread() == before && measured.Measurements == measurements, "Steady swipe movement reuses input, content measurement and traversal without allocations");
        cachedWorkspace.Update(new ControlInputFrame(size, TimeSpan.Zero, [Touch(ControlInputEventType.PointerUp, 100, 160)]), layout);
        measured.Height = 180;
        cachedWorkspace.Update(idle, layout);
        check(!cached.VerticalScrollBar.IsVisible && cached.VerticalScrollBar.Value == 0, "A smaller extent invalidates cached geometry and removes an unnecessary scrollbar");
        plane.Width = 400;
        plane.Height = 300;
        bothWorkspace.Update(idle, layout);
        check(!both.HorizontalScrollBar.IsVisible && !both.VerticalScrollBar.IsVisible, "An exact content fit resets both auto-scrollbar decisions without hysteresis");
        var wrappedText = new TextBlock { Text = new string('x', 2000), WrapText = true };
        var wrapped = new ScrollViewer { Content = wrappedText, HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden };
        using var wrappedWorkspace = new Workspace(wrapped);
        wrappedWorkspace.Update(idle, layout);
        check(wrapped.VerticalScrollBar.IsVisible, "Long wrapped text initially overflows its viewport");
        wrappedText.Text = "Fits";
        wrappedWorkspace.Update(idle, layout);
        check(!wrapped.VerticalScrollBar.IsVisible && wrapped.VerticalScrollBar.Value == 0, "Wrapped text changes invalidate cached extent and recover the full viewport width");
        measurements = measured.Measurements;
        cachedWorkspace.Update(idle, new ControlLayoutContext(new TestFont()));
        check(measured.Measurements > measurements, "Changing the default font invalidates retained content measurements");
        var dpiViewer = new ScrollViewer { Content = new Panel { Height = 900 }, HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden };
        using var dpiWorkspace = new Workspace(dpiViewer) { Dpi = 192 };
        var physical = new ControlInputFrame(new Size2(800, 600), TimeSpan.Zero,
            [Touch(ControlInputEventType.PointerDown, 200, 400), Touch(ControlInputEventType.PointerMove, 200, 300), Touch(ControlInputEventType.PointerUp, 200, 300)])
            .SetViewport(new Rectangle(0, 0, 800, 600), 1);
        dpiWorkspace.ProcessFrame(physical, layout);
        check(dpiViewer.VerticalScrollBar.Value == 50, "Physical touch movement is converted once to logical workspace coordinates at high DPI");
        ListBoxSwipeChecks.Run(check, layout);
    }
}

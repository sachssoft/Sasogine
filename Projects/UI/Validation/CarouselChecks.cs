using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Controls.Layouts;
using Sachssoft.Engine.Controls.Styling;

/// <summary>Checks carousel ownership, selection, animation, input arbitration and stable-frame costs.</summary>
internal static class CarouselChecks
{
    /// <summary>Runs deterministic carousel regression checks without a graphics device.</summary>
    /// <param name = "check">The assertion callback.</param>
    /// <param name = "layout">The fixed font and layout context.</param>
    internal static void Run(Action<bool, string> check, ControlLayoutContext layout)
    {
        var carousel = new Carousel
        {
            NavigationMode = CarouselNavigationMode.None,
            ShowIndicators = false,
            Transition = CarouselTransition.None
        };
        var first = new TreeMutationProbe();
        var second = new TreeMutationProbe();
        var third = new TreeMutationProbe();
        carousel.Items.Add(first);
        carousel.Items.Add(second);
        carousel.Items.Add(third);
        check(carousel.SelectedIndex == 0 && carousel.SelectedItem == first && first.Parent?.Parent == carousel && second.Parent?.Parent == carousel, "First page is selected automatically and all pages are owned immediately");
        using var workspace = new Workspace(carousel);
        var size = new Size2(400, 300);
        var idle = new ControlInputFrame(size, TimeSpan.Zero);
        void Frame(double seconds = 0, params ControlInputEvent[] events) => workspace.Update(seconds == 0 && events.Length == 0 ? idle : new ControlInputFrame(size, TimeSpan.FromSeconds(seconds), events), layout);
        Frame();
        check(first.Workspace == workspace && second.Workspace is null && third.Workspace is null, "Only the selected page is attached to frame traversal");
        int secondUpdates = second.Updates;
        Frame();
        check(second.Updates == secondUpdates && first.Updates > 0, "Inactive pages do not update");
        int selectedEvents = 0;
        carousel.SelectionChanged += (_, _) => selectedEvents++;
        check(carousel.Next() && carousel.SelectedItem == second, "Next changes the selected page");
        Frame();
        check(first.Workspace is null && second.Workspace == workspace && !carousel.IsTransitioning, "Immediate switching detaches the previous page");
        carousel.Next();
        Frame();
        check(!carousel.Next() && carousel.SelectedItem == third, "Non-looping navigation stops at the last page");
        carousel.IsLooping = true;
        check(carousel.Next() && carousel.SelectedItem == first, "Looping Next wraps to the first page");
        Frame();
        carousel.Previous();
        Frame();
        check(carousel.SelectedItem == third, "Looping Previous wraps to the last page");
        carousel.Items.Insert(0, new Panel());
        check(carousel.SelectedItem == third && carousel.SelectedIndex == 3, "Insertion before selection preserves page identity");
        carousel.Items.Move(3, 1);
        check(carousel.SelectedItem == third && carousel.SelectedIndex == 1, "Moving pages preserves selection identity");
        carousel.Items.Remove(third);
        check(third.Parent is null && carousel.SelectedItem == first, "Removing selection releases ownership and chooses the nearest remaining page");
        bool rejected = false;
        try
        {
            carousel.Items.Add(second);
        }
        catch (InvalidOperationException)
        {
            rejected = true;
        }

        check(rejected && carousel.Items.Count == 3, "Duplicate page ownership is rejected");
        rejected = false;
        try
        {
            carousel.Items.Add(carousel);
        }
        catch (InvalidOperationException)
        {
            rejected = true;
        }

        check(rejected, "Page ancestry cycles are rejected");
        var orphan = new Panel();
        rejected = false;
        try
        {
            carousel.Items.Insert(99, orphan);
        }
        catch (ArgumentOutOfRangeException)
        {
            rejected = true;
        }

        check(rejected && orphan.Parent is null, "Invalid insertion indices do not reserve ownership");
        Frame();
        carousel.Transition = CarouselTransition.Slide;
        carousel.TransitionDuration = 1;
        carousel.SelectedItem = first;
        Frame();
        carousel.SelectedItem = second;
        Frame();
        check(carousel.IsTransitioning && first.Workspace == workspace && second.Workspace == workspace && carousel.TransitionProgress == 0, "A transition attaches exactly its outgoing and incoming pages");
        Frame(.5);
        check(carousel.TransitionProgress == .5f && VisualBounds(first).X == -200 && VisualBounds(second).X == 200 && first.Bounds.Width == 400, "Slide geometry advances deterministically on the horizontal axis");
        check(first.Bounds.X == 0 && second.Bounds.X == 0, "Sliding leaves both pages arranged at the viewport origin");
        Frame(.5);
        check(!carousel.IsTransitioning && first.Workspace is null && second.Bounds.X == 0, "Completed slide releases the outgoing page");
        carousel.Orientation = Orientation.Vertical;
        carousel.Previous();
        Frame();
        Frame(.5);
        check(VisualBounds(first).Y == -150 && VisualBounds(second).Y == 150, "Previous slides in the opposite direction on the vertical axis");
        carousel.Transition = CarouselTransition.Fade;
        Frame();
        check(!carousel.IsTransitioning, "Changing effect terminates the previous transition safely");
        carousel.Next();
        Frame();
        Frame(.25);
        check(first.Parent!.Opacity == .75f && second.Parent!.Opacity == .25f && first.Opacity == 1 && second.Opacity == 1, "Fade opacity belongs to private hosts and preserves page properties");
        carousel.GoTo(0);
        Frame();
        check(carousel.SelectedIndex == 0 && carousel.IsTransitioning && second.Workspace == workspace && first.Workspace is null, "Rapid navigation retains only the previous destination and the new destination");
        Frame(1);
        carousel.Transition = CarouselTransition.None;
        carousel.Orientation = Orientation.Horizontal;
        carousel.NavigationMode = CarouselNavigationMode.All;
        carousel.ShowIndicators = true;
        Frame();
        check(carousel.PreviousButton.IsVisible && carousel.NextButton.IsVisible && carousel.ViewportBounds.Width < 400 && carousel.ViewportBounds.Height < 300, "Navigation buttons and indicators reserve their own page space");
        Bounds2 next = carousel.NextButton.Bounds;
        Frame(0, new(ControlInputEventType.PointerDown, new(next.X + 5, next.Y + 5)), new(ControlInputEventType.PointerUp, new(next.X + 5, next.Y + 5)));
        check(carousel.SelectedIndex == 1, "The next button navigates on activation");
        var indicators = (StackPanel)carousel.Children[2];
        Bounds2 dot = indicators.Children[0].Bounds;
        check(dot.Width == carousel.IndicatorSize && dot.Height == carousel.IndicatorSize, "Page dots honor their size without inheriting button minimum dimensions");
        Frame(0, new(ControlInputEventType.PointerDown, new(dot.X + dot.Width / 2, dot.Y + dot.Height / 2)), new(ControlInputEventType.PointerUp, new(dot.X + dot.Width / 2, dot.Y + dot.Height / 2)));
        check(carousel.SelectedIndex == 0, "Clicking a page indicator selects its page");
        workspace.Focus(carousel);
        Frame(0, new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.End));
        check(carousel.SelectedIndex == 2, "End selects the final page");
        carousel.Orientation = Orientation.Vertical;
        Frame();
        Frame(0, new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Up));
        check(carousel.SelectedIndex == 1, "Vertical carousels respond to Up and Down");
        carousel.Items.Clear();
        var button = new Button
        {
            Content = "Page button"
        };
        carousel.Items.Add(button);
        carousel.Items.Add(new Panel());
        carousel.Orientation = Orientation.Horizontal;
        carousel.NavigationMode = CarouselNavigationMode.Swipe;
        carousel.ShowIndicators = false;
        carousel.SelectedIndex = 0;
        int clicks = 0;
        button.Clicked += (_, _) => clicks++;
        Frame();
        ControlInputEvent Touch(ControlInputEventType type, float x, float y = 100, int id = 1) => new(type, new(x, y), PointerId: id, PointerKind: ControlPointerKind.Touch);
        Frame(0, Touch(ControlInputEventType.PointerDown, 200), Touch(ControlInputEventType.PointerMove, 195), Touch(ControlInputEventType.PointerUp, 195));
        check(clicks == 1 && carousel.SelectedIndex == 0, "A short touch remains a normal child button click");
        Frame(0, Touch(ControlInputEventType.PointerDown, 250), Touch(ControlInputEventType.PointerMove, 150), Touch(ControlInputEventType.PointerUp, 120));
        check(clicks == 1 && carousel.SelectedIndex == 1 && workspace.GetCapturedControl(new(ControlPointerKind.Touch, 1))is null, "A swipe cancels child activation and releases its pointer capture");
        carousel.SelectedIndex = 0;
        Frame();
        Frame(0, Touch(ControlInputEventType.PointerDown, 250), Touch(ControlInputEventType.PointerCancel, 250), Touch(ControlInputEventType.PointerMove, 100), Touch(ControlInputEventType.PointerUp, 100));
        check(clicks == 1 && carousel.SelectedIndex == 0, "A cancelled pending swipe cannot activate or navigate later");
        Frame(0, Touch(ControlInputEventType.PointerDown, 250), Touch(ControlInputEventType.PointerMove, 150), Touch(ControlInputEventType.PointerCancel, 150));
        check(carousel.SelectedIndex == 0 && !button.IsHeld, "A cancelled captured swipe leaves child controls unarmed");
        Frame(0, Touch(ControlInputEventType.PointerDown, 250), Touch(ControlInputEventType.PointerMove, 250, 200), Touch(ControlInputEventType.PointerUp, 250, 200));
        check(carousel.SelectedIndex == 0, "Perpendicular movement does not switch pages");
        carousel.NavigationMode = CarouselNavigationMode.None;
        carousel.AutoPlayInterval = 1;
        Frame(.5);
        check(carousel.SelectedIndex == 0, "Automatic advancement waits for its configured interval");
        Frame(.5);
        check(carousel.SelectedIndex == 1, "Automatic advancement uses only supplied Update time");
        carousel.AutoPlayInterval = 0;
        carousel.SelectedIndex = -1;
        Frame();
        check(carousel.SelectedItem is null && button.Workspace is null, "Minus one explicitly hides all pages");
        carousel.Items.Add(new Panel());
        check(carousel.SelectedIndex == -1, "Collection changes respect explicitly cleared selection");
        carousel.SelectedIndex = 0;
        Frame();
        var style = new Style
        {
            BasedOn = "Carousel",
            Transition = CarouselTransition.Fade,
            TransitionDuration = .4f,
            NavigationMode = CarouselNavigationMode.Arrows | CarouselNavigationMode.Swipe,
            ShowIndicators = true
        };
        style["Orientation"] = "Vertical";
        workspace.Registry.Styles.Register("Test.Carousel", style);
        carousel.StyleId = "Test.Carousel";
        carousel.ResetStyleProperty(nameof(Carousel.NavigationMode));
        carousel.ResetStyleProperty(nameof(Carousel.Transition));
        carousel.ResetStyleProperty(nameof(Carousel.TransitionDuration));
        carousel.ResetStyleProperty(nameof(Carousel.ShowIndicators));
        carousel.ResetStyleProperty(nameof(Carousel.Orientation));
        Frame();
        check(carousel.Transition == CarouselTransition.Fade && carousel.TransitionDuration == .4f && carousel.Orientation == Orientation.Vertical && carousel.ShowIndicators, "Central carousel styles restore through ResetStyleProperty");
        check(workspace.Registry.Converters.Convert<CarouselNavigationMode>("CarouselNavigationMode", "Arrows | Swipe") == (CarouselNavigationMode.Arrows | CarouselNavigationMode.Swipe), "Navigation flags use an explicit converter without reflection");
        check(workspace.Registry.Create("Carousel") is Carousel, "Carousel factory is registered automatically");
        carousel.Transition = CarouselTransition.None;
        Frame();
        for (int i = 0; i < 100; i++)
            Frame();
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++)
            Frame();
        check(GC.GetAllocatedBytesForCurrentThread() == before, "Unchanged carousel frames allocate no memory");
        var updatePage = new TreeMutationProbe();
        carousel.Items[0] = updatePage;
        carousel.SelectedIndex = 0;
        updatePage.Updating = () =>
        {
            updatePage.Updating = null;
            carousel.Items.Remove(updatePage);
            carousel.Items.Add(new Panel());
            carousel.GoTo(1);
            check(updatePage.Parent is null, "A page removed during Update releases ownership immediately");
        };
        Frame();
        check(updatePage.Workspace is null && carousel.SelectedIndex == 1 && carousel.SelectedItem!.Workspace == workspace, "Collection and selection changes during Update commit at a safe frame boundary");
        check(selectedEvents > 0, "Selection changes notify observers");
        CheckLargeSlide(check, layout, idle);
    }

    /// <summary>Gets the rendered envelope of a laid-out control.</summary>
    /// <param name="control">The page being inspected.</param>
    /// <returns>The page bounds with its host's presentation transform applied.</returns>
    private static Bounds2 VisualBounds(Control control) =>
        ControlTransforms.TransformBounds(control.Bounds, ControlTransforms.GetVisualTransform(control));

    /// <summary>Checks that steady slide frames leave both heavy page trees laid out.</summary>
    /// <param name="check">The assertion callback.</param>
    /// <param name="layout">The fixed font and layout context.</param>
    /// <param name="idle">The input frame used to attach and lay out the pages.</param>
    private static void CheckLargeSlide(Action<bool, string> check, ControlLayoutContext layout, ControlInputFrame idle)
    {
        var carousel = new Carousel
        {
            NavigationMode = CarouselNavigationMode.None,
            ShowIndicators = false,
            Transition = CarouselTransition.Slide,
            TransitionDuration = 1
        };
        var probes = new List<MeasureProbe>(1000);
        for (int page = 0; page < 2; page++)
        {
            var panel = new StackPanel();
            for (int item = 0; item < 500; item++)
            {
                var probe = new MeasureProbe();
                probes.Add(probe);
                panel.Add(probe);
            }
            carousel.Items.Add(panel);
        }

        using var workspace = new Workspace(carousel);
        workspace.Update(idle, layout);
        carousel.Next();
        workspace.Update(idle, layout);
        workspace.Update(idle, layout);
        int measurements = probes.Sum(probe => probe.Measurements);
        int arrangements = probes.Sum(probe => probe.Arrangements);
        var animation = new ControlInputFrame(idle.Size, TimeSpan.FromMilliseconds(16));
        for (int i = 0; i < 30; i++)
            workspace.Update(animation, layout);

        check(carousel.TransitionProgress > 0 && carousel.TransitionProgress < 1 &&
            probes.Sum(probe => probe.Measurements) == measurements &&
            probes.Sum(probe => probe.Arrangements) == arrangements,
            "Thirty slide frames move two pages with 1000 controls without running page layout");
    }
}

using Microsoft.Xna.Framework;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Drawing;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Controls.Layouts;
using Sachssoft.Engine.Controls.Styling;

/// <summary>Checks shared header presentation, collapse ownership and non-collapsible headers.</summary>
internal static class HeaderGroupChecks
{
    internal static void Run(Action<bool, string> check, ControlLayoutContext layout)
    {
        var header = new Header
        {
            HeaderContent = "Title",
            Content = "Body"
        };
        var expander = new Expander
        {
            HeaderContent = "Title",
            Content = "Body"
        };
        var panel = new StackPanel
        {
            Orientation = StackOrientation.Horizontal
        };
        header.Width = 200;
        expander.Width = 200;
        panel.Add(header);
        panel.Add(expander);
        using var workspace = new Workspace(panel);
        var idle = new ControlInputFrame(new Size2(400, 200), TimeSpan.Zero);
        void Frame() => workspace.Update(idle, layout);
        Frame();
        check(header.HeaderContentPresenter is TextBlock title && title.Text == "Title" && header.ContentPresenter is TextBlock body && body.Text == "Body", "Header and body support object content through shared presenters");
        check(header.HeaderContentPresenter!.Parent is not ToggleButton && expander.HeaderContentPresenter!.Parent is ToggleButton, "Only the expander has an activating header");
        check(header.HeaderContentPresenter!.Parent!.DesiredSize.Height == expander.HeaderContentPresenter!.Parent!.DesiredSize.Height, "Static and expanded headers have the same height");
        check(header.HeaderContentPresenter!.Bounds.X - header.HeaderContentPresenter!.Parent!.Bounds.X == expander.HeaderContentPresenter!.Bounds.X - expander.HeaderContentPresenter!.Parent!.Bounds.X, "Static and expanded headers use the same caption indentation");
        Vector2 point = new(header.HeaderContentPresenter!.Parent!.Bounds.X + 30, header.HeaderContentPresenter!.Parent!.Bounds.Y + 10);
        workspace.Update(new ControlInputFrame(idle.Size, TimeSpan.Zero, new[] { new ControlInputEvent(ControlInputEventType.PointerDown, point), new ControlInputEvent(ControlInputEventType.PointerUp, point) }), layout);
        check(header.ContentPresenter!.Workspace == workspace, "Clicking a static header never hides its content");
        var expanderBody = expander.ContentPresenter;
        expander.IsExpanded = false;
        Frame();
        check(expanderBody!.Parent == expander && expanderBody.Workspace is null, "Collapsed content remains logically owned");
        expander.Content = "New collapsed body";
        check(expander.ContentPresenter == expanderBody && expanderBody.Workspace is null, "Replacing collapsed text reuses its hidden presenter");
        expander.IsExpanded = true;
        Frame();
        check(expanderBody.Workspace == workspace && ((TextBlock)expanderBody).Text == "New collapsed body", "Expanding restores the same presenter");
        var value = new ContentValueProbe
        {
            Text = "Value"
        };
        header.HeaderContent = value;
        var presenter = header.HeaderContentPresenter;
        Frame();
        Frame();
        check(ReferenceEquals(header.HeaderContent, value) && value.Conversions == 1 && presenter is TextBlock, "Header data is converted on assignment, not polled");
        value.Text = "Refresh";
        header.HeaderContent = value;
        check(header.HeaderContentPresenter == presenter && ((TextBlock)presenter!).Text == "Refresh", "Same-object reassignment refreshes header text without recreating it");
        var other = new ContentControl
        {
            Content = new Image()
        };
        bool rejected = false;
        try
        {
            header.HeaderContent = other.Content;
        }
        catch (InvalidOperationException)
        {
            rejected = true;
        }

        check(rejected && ReferenceEquals(header.HeaderContent, value) && header.HeaderContentPresenter == presenter, "Failed header ownership checks preserve the previous header");
        rejected = false;
        try
        {
            header.HeaderContent = header;
        }
        catch (InvalidOperationException)
        {
            rejected = true;
        }

        check(rejected && header.Parent == panel, "Header content rejects ancestor cycles");
        var raw = new Image();
        var group = new GroupBox
        {
            HeaderContent = raw,
            Content = 42
        };
        check(raw.Parent?.Parent == group && group.HeaderContentPresenter == raw, "HeaderContent retains immediate caption ownership inside its private host");
        group.HeaderContent = null;
        group.HeaderContent = "Group";
        check(group.HeaderContentPresenter is TextBlock && group.Content is int, "GroupBox shares header and body data conversion");
        var oldCaption = group.HeaderContentPresenter;
        group.HeaderContent = new Image();
        check(oldCaption!.Parent is null, "Replacing header content releases its previous caption");
        rejected = false;
        try
        {
            group.HeaderContent = group;
        }
        catch (InvalidOperationException)
        {
            rejected = true;
        }

        check(rejected && group.HeaderContent is Image, "HeaderContent rejects owner cycles after presenter replacement");
        group.Clear();
        check(group.HeaderContentPresenter is null && group.HeaderContent is null && group.Content is null && group.Children.Count == 0, "Clearing a group releases both slots");
        var style = new Style
        {
            BasedOn = "Header",
            HeaderHeight = 50
        };
        workspace.Registry.Styles.Register("Test.Header", style);
        header.StyleId = "Test.Header";
        Frame();
        check(header.HeaderHeight == 50 && header.HeaderContentPresenter!.Parent!.Bounds.Height == 50, "Shared header height is style-controlled");
        for (int i = 0; i < 100; i++)
            Frame();
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++)
            Frame();
        long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
        check(bytes == 0, "Unchanged header groups preserve allocation-free cached layout");
        check(workspace.Registry.Create("HeaderContentControl") is HeaderContentControl, "The shared base is registered explicitly");
        var queued = new HeaderContentControl
        {
            HeaderContent = new Image(),
            Content = "Body"
        };
        var oldHeader = queued.HeaderContentPresenter;
        var trigger = new TreeMutationProbe();
        var queuedPanel = new StackPanel();
        queuedPanel.Add(trigger);
        queuedPanel.Add(queued);
        using var queuedWorkspace = new Workspace(queuedPanel);
        trigger.Updating = () =>
        {
            queued.HeaderContent = null;
            queued.HeaderContent = "Replacement";
            check(oldHeader!.Parent is null && queued.HeaderContentPresenter!.Parent?.Parent == queued, "Traversal-time header changes reserve ownership immediately");
            trigger.Updating = null;
        };
        queuedWorkspace.Update(idle, layout);
        check(queued.HeaderContentPresenter is TextBlock replacement && replacement.Text == "Replacement" && replacement.Workspace == queuedWorkspace, "Queued clear and replacement commit the final header at the safe boundary");
        check(queued.Children.Count == 2 && queued.ContentPresenter!.Workspace == queuedWorkspace, "Header replacement preserves the body without duplicating the private host");
        queued.Remove(queued.HeaderContentPresenter!);
        queuedWorkspace.Update(idle, layout);
        check(queued.HeaderContent is null && queued.HeaderContentPresenter is null && queued.Children.Count == 1, "Removing header content empties its presenter while retaining the body");
    }
}

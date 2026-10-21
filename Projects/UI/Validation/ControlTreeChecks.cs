using System;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Drawing;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Controls.Layouts;

internal static class ControlTreeChecks
{
    private static void Reject<T>(Action action, Action<bool, string> check, string message)
        where T : Exception
    {
        bool rejected = false;
        try
        {
            action();
        }
        catch (T)
        {
            rejected = true;
        }

        check(rejected, message);
    }

    internal static void Run(Action<bool, string> check, ControlLayoutContext layout)
    {
        var frame = new ControlInputFrame(new Size2(400, 300), TimeSpan.Zero);
        var first = new ContentControl();
        var image = new Image();
        first.Content = image;
        check(image.Parent == first && first.Children.Count == 1, "Content reserves ownership before the first frame");
        first.Content = image;
        check(first.Children.Count == 1, "Reassigning identical content is harmless");
        var retained = new Image();
        var second = new ContentControl
        {
            Content = retained
        };
        Reject<InvalidOperationException>(() => second.Content = image, check, "Content cannot be shared");
        check(second.Content == retained && retained.Parent == second && image.Parent == first, "Failed assignment must be atomic");
        var panel = new StackPanel();
        Reject<InvalidOperationException>(() => panel.Children.Add(image), check, "Content cannot also become a panel child");
        Reject<InvalidOperationException>(() => first.Children.Add(image), check, "Children.Add must reject an existing content child");
        Reject<ArgumentNullException>(() => panel.Children.Add(null!), check, "Null child must throw");
        Reject<ArgumentNullException>(() => first.Children.Add(null!), check, "Null content child must throw");
        Reject<InvalidOperationException>(() => new Workspace(image), check, "Owned content cannot become a workspace root");
        first.Content = null;
        second.Content = image;
        check(first.Children.Count == 0 && image.Parent == second && retained.Parent is null, "Ownership can move after release");
        Reject<InvalidOperationException>(() => first.Content = first, check, "Content cannot contain itself");
        Reject<InvalidOperationException>(() => panel.Children.Add(panel), check, "A panel cannot contain itself");
        panel.Children.Add(first);
        Reject<InvalidOperationException>(() => first.Content = panel, check, "Ancestor cycles must throw immediately");
        check(first.Content is null && panel.Parent is null, "Rejected cycle must leave the tree unchanged");
        var equalA = new EqualControlProbe();
        var equalB = new EqualControlProbe();
        var equalPanel = new StackPanel();
        equalPanel.Children.Add(equalA);
        equalPanel.Children.Add(equalB);
        equalPanel.Children.Remove(equalB);
        check(equalPanel.Children.Count == 1 && ReferenceEquals(equalPanel.Children[0], equalA) && equalB.Parent is null, "Ownership and removal use instance identity even if controls override Equals");
        var scroll = new ScrollViewer();
        var scrolled = new TextBlock();
        scroll.Content = scrolled;
        check(scrolled.Parent == scroll, "ScrollViewer reserves content immediately");
        Reject<InvalidOperationException>(() => second.Content = scrolled, check, "ScrollViewer content cannot be stolen");
        Reject<InvalidOperationException>(() => scroll.Content = scroll, check, "ScrollViewer rejects self cycles");
        Reject<InvalidOperationException>(() => scroll.Children.Add(scrolled), check, "ScrollViewer Add rejects duplicates");
        scroll.Clear();
        check(scrolled.Parent is null && scroll.Children.Count == 2, "Clearing a ScrollViewer preserves its scrollbars");
        var group = new GroupBox
        {
            Header = scrolled
        };
        Reject<InvalidOperationException>(() => group.Content = scrolled, check, "Header and content cannot own the same instance");
        group.Header = null;
        group.Content = scrolled;
        check(scrolled.Parent == group && group.Children.Count == 1, "A control can move between slots after release");
        var collapsed = new Expander
        {
            IsExpanded = false,
            Content = new Image()
        };
        var hidden = collapsed.Content!;
        using var collapsedRoot = new Workspace(collapsed);
        collapsedRoot.Update(frame, layout);
        check(hidden.Parent == collapsed && hidden.Workspace is null, "Collapsed content remains owned outside traversal");
        Reject<InvalidOperationException>(() => panel.Children.Add(hidden), check, "Collapsed content cannot be stolen");
        collapsed.IsExpanded = true;
        collapsedRoot.Update(frame, layout);
        check(hidden.Parent == collapsed && hidden.Workspace == collapsedRoot, "Expanding retains the owner and activates traversal");
        var split = new SplitButton
        {
            Content = new TextBlock()
        };
        Reject<InvalidOperationException>(() => first.Content = split.Content, check, "SplitButton content is reserved through its primary presenter");
        var tabs = new TabControl();
        var pageA = new TabItem
        {
            Content = new TextBlock(),
            HeaderContent = new Image()
        };
        var pageB = new TabItem
        {
            Content = new Image()
        };
        tabs.Items.Add(pageA);
        tabs.Items.Add(pageB);
        check(pageA.Parent == tabs && pageB.Parent == tabs, "All tab pages reserve their parent before the first frame");
        Reject<InvalidOperationException>(() => tabs.Items.Add(pageA), check, "Duplicate tab pages throw immediately");
        Reject<InvalidOperationException>(() => new TabControl().Items.Add(pageB), check, "Inactive tab pages cannot be shared");
        Reject<InvalidOperationException>(() => panel.Children.Add(pageB.Content!), check, "Inactive page content remains owned");
        Reject<InvalidOperationException>(() => first.Content = pageA.HeaderContent, check, "Custom tab headers reserve their content immediately");
        using var tabRoot = new Workspace(tabs);
        tabRoot.Update(frame, layout);
        check(pageB.Parent == tabs && pageB.Workspace is null, "Inactive pages stay owned after layout");
        tabs.SelectedIndex = 1;
        tabRoot.Update(frame, layout);
        check(pageA.Parent == tabs && pageA.Workspace is null && pageB.Workspace == tabRoot, "Tab switching changes traversal without releasing ownership");
        tabs.Items.Remove(pageA);
        check(pageA.Parent is null, "Removing a tab releases its ownership immediately");
        var otherTabs = new TabControl();
        otherTabs.Items.Add(pageA);
        otherTabs.SelectedIndex = 0;
        using var otherTabRoot = new Workspace(otherTabs);
        otherTabRoot.Update(frame, layout);
        tabRoot.Update(frame, layout);
        check(pageA.Parent == otherTabs && pageA.Workspace == otherTabRoot, "Tab pages and headers can transfer before the old tab host prepares again");
        tabs.Items.Clear();
        check(pageB.Parent is null, "Clearing tabs releases every page");
        var tooltipOwner = new Button();
        var tooltipContent = new TextBlock();
        tooltipOwner.ToolTipContent = tooltipContent;
        var tooltipParent = tooltipContent.Parent;
        check(tooltipParent is not null, "Tooltip content is reserved immediately");
        Reject<InvalidOperationException>(() => first.Content = tooltipContent, check, "Tooltip content cannot also be scene content");
        Reject<InvalidOperationException>(() => tooltipOwner.ToolTipContent = tooltipOwner, check, "Tooltip cannot contain its owner");
        check(tooltipOwner.ToolTipContent == tooltipContent && tooltipContent.Parent == tooltipParent, "Failed tooltip assignment must be atomic");
        tooltipOwner.ConfigToolTipDelay = 0;
        using var tooltipRoot = new Workspace(tooltipOwner);
        var hover = new ControlInputFrame(new Size2(400, 300), TimeSpan.FromSeconds(1), new[] { new ControlInputEvent(ControlInputEventType.PointerMove, new Microsoft.Xna.Framework.Vector2(10, 10)) });
        tooltipRoot.Update(hover, layout);
        check(tooltipRoot.VisibleToolTip?.Content == tooltipContent && tooltipContent.Parent == tooltipParent, "Showing a tooltip preserves its stable presenter");
        tooltipOwner.ToolTipContent = null;
        tooltipRoot.Update(frame, layout);
        check(tooltipContent.Parent is null && tooltipRoot.VisibleToolTip is null, "Clearing tooltip content releases its ownership");
        var container = new StackPanel();
        var mutation = new TreeMutationProbe();
        var pending = new TreeMutationProbe();
        container.Children.Add(mutation);
        using var root = new Workspace(container);
        mutation.Updating = () =>
        {
            mutation.Updating = null;
            container.Children.Add(pending);
            check(pending.Parent == container && !container.Children.Contains(pending), "Pending addition reserves Parent without mutating current traversal");
            Reject<InvalidOperationException>(() => container.Children.Add(pending), check, "Pending duplicate Add throws immediately");
            Reject<InvalidOperationException>(() => first.Content = pending, check, "Pending panel child cannot become content elsewhere");
            container.Children.Remove(pending);
            check(pending.Parent is null, "Pending removal releases the reservation immediately");
            container.Children.Add(pending);
            container.Children.Clear();
            check(pending.Parent is null && mutation.Parent is null, "Clear releases committed and pending children");
            container.Children.Add(pending);
        };
        root.Update(frame, layout);
        check(container.Children.Count == 1 && container.Children[0] == pending && pending.Workspace == root, "Superseded queued additions cannot create duplicate children");
        check(pending.Updates == 0, "A newly committed child is not updated twice in the current traversal");
        root.Update(frame, layout);
        check(pending.Updates == 1, "New child joins the following update traversal");
        var source = new ContentControl
        {
            Content = new TreeMutationProbe()
        };
        var target = new ContentControl();
        var moving = (TreeMutationProbe)source.Content;
        var transfer = new TreeMutationProbe();
        var hosts = new StackPanel();
        hosts.Children.Add(transfer);
        hosts.Children.Add(source);
        hosts.Children.Add(target);
        using var transferRoot = new Workspace(hosts);
        transfer.Updating = () =>
        {
            transfer.Updating = null;
            source.Content = null;
            target.Content = moving;
            check(moving.Parent == target && source.Children.Contains(moving), "Content transfer reserves the new parent while preserving the current traversal");
            Reject<InvalidOperationException>(() => first.Content = moving, check, "Pending content transfer rejects a third owner");
            target.Content = null;
            source.Content = moving;
            source.Content = null;
            target.Content = moving;
        };
        transferRoot.Update(frame, layout);
        check(source.Children.Count == 0 && target.Children.Count == 1 && moving.Workspace == transferRoot && moving.Updates == 1, "Content transfer commits once without stale detach or duplicate update");
        target.Content = null;
        source.Content = moving;
        check(moving.Parent == source && target.Children.Count == 0 && source.Children.Count == 1, "Content transfer also commits immediately outside traversal");
        var show = new TreeMutationProbe();
        using var overlayRoot = new Workspace(show);
        var popup = new Window();
        var dialog = new Dialog();
        System.Threading.Tasks.Task<DialogResult>? completion = null;
        show.Updating = () =>
        {
            show.Updating = null;
            overlayRoot.ShowPopup(popup, Bounds2.Zero);
            Reject<InvalidOperationException>(() => overlayRoot.ShowWindow(popup), check, "Pending overlays reserve ownership immediately");
            Reject<InvalidOperationException>(() => panel.Children.Add(popup), check, "Pending overlay cannot become a panel child");
            overlayRoot.ClosePopup(popup);
            completion = overlayRoot.ShowDialog(dialog);
            Reject<InvalidOperationException>(() => overlayRoot.ShowDialog(dialog), check, "Pending dialog cannot be opened twice");
            dialog.Close();
        };
        overlayRoot.Update(frame, layout);
        check(overlayRoot.OverlayCount == 0 && popup.RootOwner is null && dialog.RootOwner is null, "Closing queued overlays releases reservations");
        check(completion?.IsCompletedSuccessfully == true && completion.Result == DialogResult.Cancel, "Queued dialog completion is preserved");
        panel.Children.Add(popup);
        Reject<InvalidOperationException>(() => overlayRoot.ShowWindow(popup), check, "Attached controls cannot be shown as overlays");
        for (int i = 0; i < 200; i++)
            transferRoot.Update(frame, layout);
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++)
            transferRoot.Update(frame, layout);
        check(GC.GetAllocatedBytesForCurrentThread() == before, "Stable ownership and content traversal must allocate zero bytes");
    }
}

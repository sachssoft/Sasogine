using System;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Drawing;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Controls.Layouts;

/// <summary>Checks object content, presenter reuse and atomic ownership transitions.</summary>
internal static class ContentControlChecks
{
    internal static void Run(Action<bool, string> check, ControlLayoutContext layout)
    {
        var host = new ContentControl();
        check(host.Content is null && host.ContentPresenter is null && host.Children.Count == 0, "Null content has no presenter");
        int contentNotifications = 0, presenterNotifications = 0;
        host.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ContentControl.Content)) contentNotifications++;
            if (e.PropertyName == nameof(ContentControl.ContentPresenter))
            {
                presenterNotifications++;
                check(host.Content is not null || host.ContentPresenter is null, "Presenter notification observes consistent content");
            }
        };
        host.Content = "Save";
        var caption = (TextBlock)host.ContentPresenter!;
        check((string)host.Content == "Save" && caption.Text == "Save", "String content becomes a TextBlock");
        check(caption.Parent == host && host.Children.Count == 1 && host.Children[0] == caption, "Generated presenter reserves ownership immediately");
        host.Content = 123;
        check(host.Content is int number && number == 123 && caption.Text == "123", "Source value is retained without replacing it by text");
        check(ReferenceEquals(host.ContentPresenter, caption) && presenterNotifications == 1 && contentNotifications == 2, "Text changes reuse the presenter and notify Content");
        var value = new ContentValueProbe();
        host.Content = value;
        check(ReferenceEquals(host.Content, value) && caption.Text == "Value" && value.Conversions == 1, "Custom values convert once on assignment");
        using var workspace = new Workspace(host);
        var frame = new ControlInputFrame(new Size2(400, 300), TimeSpan.Zero);
        workspace.Update(frame, layout);
        check(caption.DesiredSize.Width > 0, "Generated text participates in layout");
        value.Text = "Changed";
        workspace.Update(frame, layout);
        check(caption.Text == "Value" && value.Conversions == 1, "Update does not poll object text");
        host.Content = value;
        workspace.Update(frame, layout);
        check(caption.Text == "Changed" && value.Conversions == 2, "Reassignment refreshes the same object's text");
        var failing = new ContentValueProbe { ThrowOnConvert = true };
        bool rejected = false;
        try { host.Content = failing; } catch (InvalidOperationException) { rejected = true; }
        check(rejected && ReferenceEquals(host.Content, value) && host.ContentPresenter == caption && caption.Text == "Changed", "Failed conversion preserves the previous content and presenter");
        var other = new ContentControl { Content = new Image() };
        rejected = false;
        try { host.Content = other.Content; } catch (InvalidOperationException) { rejected = true; }
        check(rejected && ReferenceEquals(host.Content, value) && caption.Parent == host, "Failed ownership checks preserve object content");
        host.Content = new ContentValueProbe { Text = null };
        check(caption.Text == string.Empty && host.ContentPresenter == caption, "Null ToString results become empty text");
        host.Content = caption;
        check(host.ContentPresenter == caption && ReferenceEquals(host.Content, caption), "Promoting the generated presenter to explicit content retains ownership");
        host.Content = "New caption";
        check(host.ContentPresenter != caption && caption.Parent is null && caption.Text == string.Empty, "Explicit controls are never overwritten by later object formatting");
        host.Children.Remove(host.ContentPresenter!);
        check(host.Content is null && host.ContentPresenter is null && host.Children.Count == 0, "Removing the presenter clears the source content");
        var image = new Image();
        host.Content = image;
        check(ReferenceEquals(host.Content, image) && host.ContentPresenter == image, "Controls are presented directly");
        host.Content = "Text after image";
        check(image.Parent is null && host.ContentPresenter is TextBlock, "Switching to text releases explicit control ownership");
        host.Clear();
        check(host.Content is null && host.ContentPresenter is null, "Clear removes both source and presenter");
        host.Content = "Reuse";
        var stable = host.ContentPresenter;
        workspace.Update(frame, layout);
        for (int i = 0; i < 100; i++) workspace.Update(frame, layout);
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++) workspace.Update(frame, layout);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        check(allocated == 0 && host.ContentPresenter == stable, "Unchanged object content keeps its presenter with zero frame allocations");
        var button = new Button { Content = "Command" };
        var toggle = new ToggleButton { Content = true };
        var checkbox = new CheckBox { Content = "Enabled" };
        var page = new TabItem { Content = "Page" };
        check(button.ContentPresenter is TextBlock && toggle.ContentPresenter is TextBlock && checkbox.ContentPresenter is TextBlock && page.ContentPresenter is TextBlock, "Button and tab subclasses inherit object content");
        var split = new SplitButton { Content = "Run" };
        check(split.ContentPresenter == split.PrimaryButton.ContentPresenter && (string)split.Content! == "Run", "SplitButton forwards source and presenter");
        int splitChanges = 0;
        split.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(SplitButton.Content)) splitChanges++; };
        split.PrimaryButton.Content = 7;
        check(splitChanges == 1 && split.Content is int && ((TextBlock)split.ContentPresenter!).Text == "7", "Primary button changes notify the split host");
        var replacement = new Image();
        var probe = new TreeMutationProbe();
        var panel = new StackPanel();
        panel.Add(probe);
        var target = new ContentControl { Content = "Old" };
        panel.Add(target);
        using var queuedWorkspace = new Workspace(panel);
        var old = target.ContentPresenter;
        probe.Updating = () =>
        {
            target.Content = replacement;
            check(target.ContentPresenter == replacement && replacement.Parent == target && old!.Parent is null, "Traversal-time assignment updates source and reserves ownership immediately");
            check(target.Children.Count == 1 && target.Children[0] == old, "Traversal changes remain queued until a safe boundary");
            probe.Updating = null;
        };
        queuedWorkspace.Update(frame, layout);
        check(target.Children.Count == 1 && target.Children[0] == replacement && replacement.Workspace == queuedWorkspace, "Queued presenter replacement commits at the frame boundary");
    }
}

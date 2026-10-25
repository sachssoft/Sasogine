using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Controls.Layouts;
using Sachssoft.Engine.Controls.Styling;
using Sachssoft.Engine.Input;
using System;
using System.Collections.Generic;

/// <summary>Checks hyperlink activation, navigation requests, state colors and retained text geometry.</summary>
internal static class HyperlinkChecks
{
    /// <summary>Runs hyperlink checks without launching a browser or other external applications.</summary>
    /// <param name = "check">The assertion callback.</param>
    internal static void Run(Action<bool, string> check)
    {
        var font = new CountingFont();
        var layout = new ControlLayoutContext(font);
        var link = new Hyperlink
        {
            Content = "Sachssoft",
            NavigateUri = new Uri("https://www.sachssoft.com/#start")
        };
        object parameter = new();
        int executions = 0;
        int requests = 0;
        bool allowed = true;
        var order = new List<string>();
        link.CommandParameter = parameter;
        link.Command = new Command(value =>
        {
            check(ReferenceEquals(value, parameter), "Hyperlinks forward CommandParameter unchanged");
            order.Add("Command");
            executions++;
        }, _ => allowed);
        link.Clicked += (_, _) => order.Add("Clicked");
        link.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(Hyperlink.IsVisited))
                order.Add("Visited");
        };
        link.NavigateRequested += (_, _) =>
        {
            check(link.NavigateUri is not null && link.IsVisited, "Navigation handlers observe the current target and visited state");
            requests++;
            order.Add("Navigate");
        };
        using var workspace = new Workspace(link);
        var idle = new ControlInputFrame(new Size2(300, 120), TimeSpan.Zero);
        void Frame(params ControlInputEvent[] events) => workspace.Update(new ControlInputFrame(idle.Size, TimeSpan.Zero, events), layout);
        workspace.Update(idle, layout);
        check(workspace.Registry.Create(nameof(Hyperlink)) is Hyperlink, "The hyperlink factory is built in");
        var presenter = (HyperlinkTextPresenter)link.ContentPresenter!;
        check(link.Bounds.Width == 72 && link.Bounds.Height == 16 && presenter.UnderlineSegments.Count == 1, "Default links size to their text with a cached underline");
        check(link.EffectiveTextColor == link.TextColor && !link.IsVisited, "An idle unvisited link uses its normal text color");
        Frame(new ControlInputEvent(ControlInputEventType.PointerMove, new Vector2(10, 8)));
        check(link.EffectiveTextColor == link.HoverTextColor && executions == 0, "Hover changes appearance without activation");
        Frame(new ControlInputEvent(ControlInputEventType.PointerDown, new Vector2(10, 8)));
        check(link.IsHeld && link.EffectiveTextColor == link.PressedTextColor && executions == 0, "Pointer down shows the held state without requesting navigation");
        Frame(new ControlInputEvent(ControlInputEventType.PointerUp, new Vector2(10, 8)));
        check(link.IsPressed && link.IsVisited && executions == 1 && requests == 1, "Pointer release activates the command and navigation request once");
        check(string.Join(",", order) == "Clicked,Command,Visited,Navigate", "Activation preserves button event ordering before navigation");
        Frame(new ControlInputEvent(ControlInputEventType.PointerMove, new Vector2(250, 100)));
        workspace.Focus(null);
        workspace.Update(idle, layout);
        check(!link.IsPressed && link.EffectiveTextColor == link.VisitedTextColor, "Polling resets next frame and visited links use their state color");
        link.NavigateUri = new Uri("https://www.sachssoft.com/#next");
        check(!link.IsVisited, "Changing only a URI fragment resets the visited state");
        workspace.Focus(link);
        Frame(new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Enter));
        check(executions == 1 && link.IsHeld, "Keyboard press waits for its matching release");
        Frame(new ControlInputEvent(ControlInputEventType.KeyUp, Key: Keys.Enter));
        check(executions == 2 && requests == 2, "Enter activates the focused hyperlink");
        Frame(new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Space), new ControlInputEvent(ControlInputEventType.KeyUp, Key: Keys.Space));
        check(executions == 3 && requests == 3, "Space uses the same activation contract");
        Frame(new ControlInputEvent(ControlInputEventType.PointerDown, new Vector2(10, 8), PointerId: 4, PointerKind: ControlPointerKind.Touch), new ControlInputEvent(ControlInputEventType.PointerCancel, new Vector2(10, 8), PointerId: 4, PointerKind: ControlPointerKind.Touch));
        check(!link.IsHeld && executions == 3, "Cancelled touch gestures do not activate links");
        allowed = false;
        workspace.Update(idle, layout);
        link.PerformClick();
        check(!link.IsEnabled && executions == 3 && requests == 3 && link.EffectiveTextColor == link.DisabledTextColor, "CanExecute blocks commands and navigation while showing the disabled color");
        allowed = true;
        link.IsEnabled = false;
        workspace.Update(idle, layout);
        link.PerformClick();
        check(executions == 3, "Explicitly disabled links cannot be activated programmatically");
        link.IsEnabled = true;
        link.NavigateUri = null;
        workspace.Update(idle, layout);
        link.PerformClick();
        check(executions == 4 && requests == 3 && link.IsVisited, "Command-only links do not raise navigation requests");
        link.NavigateUri = new Uri("/docs/sasogine", UriKind.Relative);
        link.PerformClick();
        check(requests == 4 && !link.NavigateUri.IsAbsoluteUri, "The host can handle relative targets for internal navigation");
        Control originalPresenter = link.ContentPresenter!;
        link.Content = "Documentation";
        workspace.Update(idle, layout);
        check(ReferenceEquals(originalPresenter, link.ContentPresenter) && presenter.Text == "Documentation", "Changing text reuses the retained TextBlock presenter");
        link.Content = "One\nTwo";
        workspace.Update(idle, layout);
        check(presenter.UnderlineSegments.Count == 2 && presenter.UnderlineSegments[1].Y > presenter.UnderlineSegments[0].Y, "Explicit line breaks have independently cached underlines");
        link.Content = "Alpha Beta Gamma";
        presenter.WrapText = true;
        link.Width = 48;
        workspace.Update(idle, layout);
        check(presenter.UnderlineSegments.Count >= 3, "Wrapped text uses the actual TextBlock display lines for decoration");
        foreach (Bounds2 line in presenter.UnderlineSegments)
            check(line.Width <= 48, "Wrapped underlines fit the measured text constraint");
        var style = new Style
        {
            BasedOn = nameof(Hyperlink),
            HoverTextColor = Color.Red,
            Underline = false
        };
        workspace.Styles.Register("Test.Hyperlink", style);
        link.StyleId = "Test.Hyperlink";
        workspace.Update(idle, layout);
        check(link.HoverTextColor == Color.Red && !link.Underline, "Link presentation resolves from central styles");
        link.HoverTextColor = Color.Green;
        style.HoverTextColor = Color.Blue;
        workspace.Update(idle, layout);
        check(link.HoverTextColor == Color.Green, "Local link colors override live style revisions");
        link.ResetStyleProperty(nameof(Hyperlink.HoverTextColor));
        check(link.HoverTextColor == Color.Blue, "Reset restores the current styled link color");
        int measurements = font.Measurements;
        workspace.Focus(null);
        link.IsVisited = false;
        link.Underline = true;
        link.VisitedTextColor = Color.Purple;
        workspace.Update(idle, layout);
        check(font.Measurements == measurements, "Link color and visited state changes do not remeasure text");
        font.Scale = 1.5f;
        presenter.InvalidateLayout();
        workspace.Update(idle, layout);
        check(presenter.UnderlineSegments.Count > 0 && font.Measurements > measurements, "Explicit font resource invalidation rebuilds underline geometry");
        for (int i = 0; i < 100; i++)
            workspace.Update(idle, layout);
        measurements = font.Measurements;
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++)
            workspace.Update(idle, layout);
        check(GC.GetAllocatedBytesForCurrentThread() == before && font.Measurements == measurements, "Steady hyperlink updates allocate zero bytes and reuse text measurements");
        var imageContent = new Panel
        {
            Width = 30,
            Height = 20
        };
        link.Content = imageContent;
        workspace.Update(idle, layout);
        check(ReferenceEquals(link.ContentPresenter, imageContent) && imageContent.Parent == link, "Custom control content retains normal ContentControl ownership");
        foreach (string skinName in new[] { "Dark", "Light", "Modern", "Futuristic", "Medieval" })
        {
            string folder = System.IO.Path.Combine(AppContext.BaseDirectory, "Skins", "Sachssoft" + skinName);
            string stem = "sachssoft_" + skinName.ToLowerInvariant();
            using var manifest = System.IO.File.OpenRead(System.IO.Path.Combine(folder, stem + ".skin.xml"));
            using var atlas = System.IO.File.OpenRead(System.IO.Path.Combine(folder, stem + ".atlas.xml"));
            using var skin = ControlSkin.Read(manifest, atlas);
            var themed = new Hyperlink { Content = "Documentation" };
            using var themedWorkspace = new Workspace(themed);
            themedWorkspace.SetSkin(skin);
            themedWorkspace.Update(idle, layout);
            check(themed.IsFocusable && themed.Padding.Equals(Insets2.Zero) && themed.Underline,
                "Skins retain hyperlink keyboard and text-only layout defaults: " + skinName);
            check(themed.TextColor == skin.GetColor("accent") && themed.DisabledTextColor == skin.GetColor("muted"),
                "Link colors resolve through the skin palette: " + skinName);
            Color visitedColor = skinName == "Light" ? new Color(112, 74, 166)
                : skinName == "Medieval" ? new Color(185, 149, 198) : new Color(178, 147, 232);
            check(themed.HoverTextColor == skin.GetColor("text") && themed.PressedTextColor == skin.GetColor("accent")
                && themed.VisitedTextColor == visitedColor, "Skins apply all hyperlink-specific state colors: " + skinName);
            themedWorkspace.Focus(themed);
            themedWorkspace.Update(new ControlInputFrame(idle.Size, TimeSpan.Zero, new[]
            {
                new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Enter),
                new ControlInputEvent(ControlInputEventType.KeyUp, Key: Keys.Enter)
            }), layout);
            check(themed.IsPressed && themed.IsVisited, "Themed links retain keyboard activation: " + skinName);
        }
    }
}

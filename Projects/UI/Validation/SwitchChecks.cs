using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Drawing;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Controls.Styling;
using Sachssoft.Engine.Input;

/// <summary>Checks switch activation, touch cancellation, styles, commands and cached layout.</summary>
internal static class SwitchChecks
{
    internal static void Run(Action<bool, string> check, ControlLayoutContext layout)
    {
        var control = new Switch();
        using var workspace = new Workspace(control);
        var idle = new ControlInputFrame(new Size2(200, 80), TimeSpan.Zero);
        void Frame(params ControlInputEvent[] events) => workspace.Update(new ControlInputFrame(idle.Size, TimeSpan.Zero, events), layout);
        workspace.Update(idle, layout);
        check(control.DesiredSize.Width >= 52 && control.DesiredSize.Height >= 44, "Switch reserves a mobile touch target");
        check(!control.IsChecked && control.ThumbBounds.X < control.TrackBounds.X + control.TrackBounds.Width / 2, "Unchecked thumb is on the left");
        int clicks = 0, changes = 0, executions = 0;
        object parameter = new();
        control.CommandParameter = parameter;
        control.Command = new Command(value =>
        {
            check(ReferenceEquals(value, parameter), "Switch forwards its command parameter");
            executions++;
        });
        control.Clicked += (_, _) => clicks++;
        control.CheckedChanged += (_, _) => changes++;
        Frame(new ControlInputEvent(ControlInputEventType.PointerDown, new Vector2(20, 20), PointerId: 5, PointerKind: ControlPointerKind.Touch));
        check(!control.IsChecked && control.IsHeld, "Touch press does not activate before release");
        Frame(new ControlInputEvent(ControlInputEventType.PointerUp, new Vector2(20, 20), PointerId: 5, PointerKind: ControlPointerKind.Touch));
        check(control.IsChecked && control.IsPressed && clicks == 1 && changes == 1 && executions == 1, "Touch release toggles and executes exactly once");
        check(control.ThumbBounds.X + control.ThumbBounds.Width / 2 > control.TrackBounds.X + control.TrackBounds.Width / 2, "Checked thumb is on the right");
        workspace.Update(idle, layout);
        check(!control.IsPressed, "Activation polling resets on the next frame");
        Frame(new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Space), new ControlInputEvent(ControlInputEventType.KeyUp, Key: Keys.Space));
        check(!control.IsChecked && clicks == 2 && executions == 2, "Keyboard activation toggles the switch");
        Frame(new ControlInputEvent(ControlInputEventType.PointerDown, new Vector2(20, 20), PointerId: 6, PointerKind: ControlPointerKind.Touch), new ControlInputEvent(ControlInputEventType.PointerCancel, new Vector2(20, 20), PointerId: 6, PointerKind: ControlPointerKind.Touch));
        check(!control.IsChecked && !control.IsHeld && clicks == 2, "Cancelled touch does not toggle");
        control.IsEnabled = false;
        control.PerformClick();
        check(!control.IsChecked && executions == 2, "Disabled switch rejects activation");
        control.IsEnabled = true;
        control.PerformClick();
        check(control.IsChecked && executions == 3, "Programmatic activation shares command behavior");
        var style = new Style
        {
            BasedOn = "Switch",
            ["TrackSize"] = "64,32",
            ["ThumbSize"] = "28",
            ["TextSpacing"] = "14"
        };
        workspace.Registry.Styles.Register("Test.Switch", style);
        control.StyleId = "Test.Switch";
        workspace.Update(idle, layout);
        check(control.TrackSize.Width == 64 && control.ThumbSize == 28 && control.TextSpacing == 14, "Switch-specific properties come from styles");
        control.TextSpacing = 3;
        style["TextSpacing"] = "18";
        workspace.Update(idle, layout);
        check(control.TextSpacing == 3, "Local switch values override live styles");
        control.ResetStyleProperty(nameof(Switch.TextSpacing));
        check(control.TextSpacing == 18, "Reset restores the effective switch style");
        control.Content = "Wi-Fi";
        workspace.Update(idle, layout);
        check(control.ContentPresenter is TextBlock label && label.Bounds.X >= control.TrackBounds.Right + 18, "Optional content is arranged beside the track");
        check(workspace.Registry.Create("Switch") is Switch, "Switch is registered without reflection");
        for (int i = 0; i < 100; i++)
            workspace.Update(idle, layout);
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++)
            workspace.Update(idle, layout);
        long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
        check(bytes == 0, "Unchanged switch update/layout allocates zero bytes");
        foreach (string name in new[]
        {
            "Dark",
            "Light",
            "Modern",
            "Futuristic",
            "Medieval"
        }

        )
        {
            string directory = Path.Combine(AppContext.BaseDirectory, "Skins", "Sachssoft" + name);
            string stem = "sachssoft_" + name.ToLowerInvariant();
            using var manifest = File.OpenRead(Path.Combine(directory, stem + ".skin.xml"));
            using var atlas = File.OpenRead(Path.Combine(directory, stem + ".atlas.xml"));
            using var skin = ControlSkin.Read(manifest, atlas);
            workspace.SetSkin(skin);
            workspace.Update(idle, layout);
            check(control.IsFocusable, name + " preserves keyboard focus for the switch");
            foreach (string state in new[]
            {
                "Unchecked.Normal",
                "Unchecked.Hover",
                "Unchecked.Pressed",
                "Unchecked.Disabled",
                "Checked.Normal",
                "Checked.Hover",
                "Checked.Pressed",
                "Checked.Disabled"
            }

            )
            {
                check(skin.Find(control, "Track", state, workspace.Registry.Styles, out _), name + " provides a switch track for " + state);
                check(skin.Find(control, "Thumb", state, workspace.Registry.Styles, out _), name + " provides a switch thumb for " + state);
            }
        }
    }
}

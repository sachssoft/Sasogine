using System.Globalization;
using Microsoft.Xna.Framework;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Controls.Layouts;
using Sachssoft.Engine.Controls.Styling;

/// <summary>Checks HUD placement, formatting, ownership, live styling and unchanged-frame allocations.</summary>
internal static class IconValueDisplayChecks
{
    /// <summary>Checks HUD layout, shared decoration, formatting, ownership and stable-frame costs.</summary>
    /// <param name = "check">The assertion callback.</param>
    /// <param name = "layout">The fixed test font and layout context.</param>
    internal static void Run(Action<bool, string> check, ControlLayoutContext layout)
    {
        var display = new IconValueDisplay
        {
            HorizontalAlignment = ControlAlignment.Start,
            VerticalAlignment = ControlAlignment.Start,
            Padding = Insets2.Zero
        };
        check(display.Icon is null && display.Value is null && display.IconPresenter is null && display.ValuePresenter is null, "Empty HUD has no presented content");
        var icon = new MeasureProbe
        {
            Width = 24,
            Height = 24
        };
        display.Icon = icon;
        display.Value = "100";
        var text = (TextBlock)display.ValuePresenter!;
        check(icon.Parent?.Parent == display && text.Parent?.Parent == display, "Both hosts reserve content ownership immediately");
        using var workspace = new Workspace(display);
        var idle = new ControlInputFrame(new Size2(400, 300), TimeSpan.Zero);
        void Frame() => workspace.Update(idle, layout);
        Frame();
        check(display.UseStyle && display.BorderThickness == 1 && display.Background.A > 0, "The display has one shared visible style by default");
        int initialMeasurements = icon.Measurements, initialArrangements = icon.Arrangements;
        display.IsFocusable = true;
        workspace.Focus(display);
        display.UseStyle = false;
        display.Paint(null!);
        display.PaintOverlay(null!);
        Frame();
        check(display.IconPresenter == icon && display.ValuePresenter == text && icon.Measurements == initialMeasurements && icon.Arrangements == initialArrangements, "Disabling shared decoration preserves content and cached geometry, including the focused case");
        display.UseStyle = true;
        IconPlacement[] placements = [IconPlacement.Left, IconPlacement.Right, IconPlacement.Top, IconPlacement.Bottom];
        foreach (IconPlacement placement in placements)
        {
            display.IconPlacement = placement;
            Frame();
            bool horizontal = placement is IconPlacement.Left or IconPlacement.Right;
            check(display.DesiredSize.Width == (horizontal ? 56 : 24) && display.DesiredSize.Height == (horizontal ? 24 : 48), "Placement selects the expected intrinsic axis: " + placement);
            float gap = placement switch
            {
                IconPlacement.Left => text.Bounds.X - icon.Bounds.Right,
                IconPlacement.Right => icon.Bounds.X - text.Bounds.Right,
                IconPlacement.Top => text.Bounds.Y - icon.Bounds.Bottom,
                _ => icon.Bounds.Y - text.Bounds.Bottom
            };
            check(gap == 8, "Icon and value retain their requested gap: " + placement);
            check(horizontal ? icon.Bounds.Y + 12 == text.Bounds.Y + 8 : icon.Bounds.X + 12 == text.Bounds.X + 12, "Presenters are centered across the layout axis: " + placement);
        }

        display.IconPlacement = IconPlacement.Left;
        Frame();
        int measurements = icon.Measurements, arrangements = icon.Arrangements;
        display.Value = "200";
        Frame();
        check(display.ValuePresenter == text && text.Text == "200", "Value changes reuse the generated TextBlock");
        check(icon.Measurements == measurements && icon.Arrangements == arrangements, "Equal-size text retains the icon and parent layout cache");
        display.Value = "20000";
        Frame();
        check(display.DesiredSize.Width == 72, "A wider value invalidates HUD geometry");
        display.TextColor = Color.Red;
        Frame();
        check(text.TextColor == Color.Red, "Generated text uses the HUD foreground");
        display.Value = null;
        Frame();
        check(display.DesiredSize.Width == 24 && display.DesiredSize.Height == 24 && display.ValuePresenter is null, "No gap is reserved without a value");
        display.Value = "100";
        icon.IsVisible = false;
        Frame();
        check(display.DesiredSize.Width == 24 && display.DesiredSize.Height == 16, "An invisible icon reserves neither size nor gap");
        icon.IsVisible = true;
        var probe = new HudValueProbe();
        display.Value = probe;
        display.ValueFormatProvider = CultureInfo.InvariantCulture;
        display.ValueFormat = "N1";
        Frame();
        int conversions = probe.Conversions;
        for (int i = 0; i < 10; i++)
            Frame();
        check(probe.Conversions == conversions && ((TextBlock)display.ValuePresenter!).Text == "42.5" && ReferenceEquals(display.Value, probe), "Formatting is cached and preserves the source object");
        bool rejected = false;
        try
        {
            display.ValueFormat = "fail";
        }
        catch (FormatException)
        {
            rejected = true;
        }

        check(rejected && display.ValueFormat == "N1" && ((TextBlock)display.ValuePresenter!).Text == "42.5", "Failed local formatting preserves the previous format and display");
        var style = new Style
        {
            BasedOn = "IconValueDisplay",
            IconPlacement = IconPlacement.Top,
            Spacing = 12,
            ValueFormat = "N0",
            UseStyle = false
        };
        workspace.Registry.Styles.Register("Test.Hud", style);
        display.StyleId = "Test.Hud";
        Frame();
        check(display.IconPlacement == IconPlacement.Left && display.ValueFormat == "N1" && display.Spacing == 12, "Local placement and formatting override central styles");
        check(display.UseStyle, "A local true assignment overrides a style that disables shared decoration");
        display.ResetStyleProperty(nameof(IconValueDisplay.UseStyle));
        check(!display.UseStyle, "Reset restores the central decoration switch");
        display.ResetStyleProperty(nameof(IconValueDisplay.IconPlacement));
        display.ResetStyleProperty(nameof(IconValueDisplay.ValueFormat));
        Frame();
        check(display.IconPlacement == IconPlacement.Top && display.ValueFormat == "N0" && ((TextBlock)display.ValuePresenter!).Text == "42", "Reset restores central placement and formatting");
        style.ValueFormat = "N2";
        Frame();
        check(((TextBlock)display.ValuePresenter!).Text == "42.50", "A live style change refreshes the formatted value");
        check(workspace.Registry.Create("IconValueDisplay") is IconValueDisplay, "HUD factory registration requires no reflection");
        check(workspace.Registry.Converters.Convert<IconPlacement>("IconPlacement", "Bottom") == IconPlacement.Bottom, "Placement has an explicit AOT-safe converter");
        var other = new ContentControl
        {
            Content = new Image()
        };
        object? old = display.Value;
        rejected = false;
        try
        {
            display.Value = other.Content;
        }
        catch (InvalidOperationException)
        {
            rejected = true;
        }

        check(rejected && ReferenceEquals(display.Value, old), "An owned value control cannot be stolen");
        rejected = false;
        try
        {
            display.Icon = display;
        }
        catch (InvalidOperationException)
        {
            rejected = true;
        }

        check(rejected && display.IconPresenter == icon, "A cyclic icon assignment preserves the previous icon");
        rejected = false;
        try
        {
            display.Value = icon;
        }
        catch (InvalidOperationException)
        {
            rejected = true;
        }

        check(rejected && ReferenceEquals(display.Value, old) && display.IconPresenter == icon, "The same control cannot occupy both slots");
        rejected = false;
        try
        {
            display.Spacing = float.NaN;
        }
        catch (ArgumentOutOfRangeException)
        {
            rejected = true;
        }

        check(rejected && display.Spacing == 12, "Invalid gaps leave the layout unchanged");
        rejected = false;
        try
        {
            display.IconPlacement = (IconPlacement)99;
        }
        catch (ArgumentOutOfRangeException)
        {
            rejected = true;
        }

        check(rejected && display.IconPlacement == IconPlacement.Top, "Invalid placement leaves the layout unchanged");
        display.IconPlacement = IconPlacement.Right;
        display.Width = 20;
        display.Height = 20;
        Frame();
        check(icon.Bounds.Right <= display.Bounds.Right && icon.Bounds.X >= display.Bounds.X, "Constrained layout keeps the icon within the display");
        display.Width = null;
        display.Height = null;
        Frame();
        conversions = probe.Conversions;
        for (int i = 0; i < 100; i++)
            Frame();
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++)
            Frame();
        check(GC.GetAllocatedBytesForCurrentThread() == before && probe.Conversions == conversions, "Unchanged HUD frames allocate nothing and perform no conversions");
        var customValue = new MeasureProbe
        {
            Width = 60,
            Height = 20
        };
        display.Value = customValue;
        Frame();
        check(display.ValuePresenter == customValue && ReferenceEquals(display.Value, customValue), "A custom control bypasses text formatting");
        var mutator = new TreeMutationProbe
        {
            Width = 24,
            Height = 24
        };
        display.Icon = mutator;
        mutator.Updating = () =>
        {
            mutator.Updating = null;
            display.Icon = icon;
            display.Value = "Deferred";
            check(mutator.Parent is null && icon.Parent?.Parent == display && customValue.Parent is null, "Changes during Update reserve and release slot ownership immediately");
        };
        Frame();
        check(display.IconPresenter == icon && display.ValuePresenter is TextBlock deferred && deferred.Text == "Deferred" && icon.Workspace == workspace, "Deferred presenter changes commit safely at the frame boundary");
        display.Remove(icon);
        check(display.Icon is null && icon.Parent is null && Equals(display.Value, "Deferred"), "Removing an icon releases only its slot");
        display.Clear();
        Frame();
        check(display.Value is null && display.ValuePresenter is null && display.DesiredSize.Width == 0 && display.DesiredSize.Height == 0, "Clear empties both slots while retaining their reusable hosts");
    }
}

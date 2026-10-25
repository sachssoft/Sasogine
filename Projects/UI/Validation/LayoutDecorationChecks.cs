using Microsoft.Xna.Framework;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Controls.Layouts;
using Sachssoft.Engine.Controls.Styling;

/// <summary>Checks that pure layouts keep style values without submitting decoration.</summary>
internal static class LayoutDecorationChecks
{
    internal static void Run(Action<bool, string> check, ControlLayoutContext layout)
    {
        Control[] panels = [new Panel(), new Grid(), new StackPanel(), new WrapPanel(), new TiledPanel(), new AdaptiveTilePanel(), new EdgePanel()];
        foreach (Control panel in panels)
        {
            using var workspace = new Workspace(panel);
            workspace.Registry.Styles.Register("Test.Layout", new Style
            {
                BasedOn = panel.StandardStyleName,
                Padding = new Insets2(9),
                Background = Color.Red,
                BorderColor = Color.Blue,
                BorderThickness = 3
            });
            panel.StyleId = "Test.Layout";
            panel.IsFocusable = true;
            workspace.Update(new ControlInputFrame(new Size2(300, 200), TimeSpan.Zero), layout);
            workspace.Focus(panel);
            check(panel.Padding.Equals(new Insets2(9)) && panel.Background == Color.Red && panel.BorderThickness == 3,
                "Layout properties still resolve from styles: " + panel.StandardStyleName);
            check(panel.ContentBounds.X == 9 && panel.ContentBounds.Y == 9,
                "Styled padding remains part of layout: " + panel.StandardStyleName);

            // With no graphics context, either decoration path would fail immediately.
            panel.Paint(null!);
            panel.PaintOverlay(null!);
            check(panel.IsFocused, "Focused layouts submit no background, border or focus drawing: " + panel.StandardStyleName);
        }
    }
}

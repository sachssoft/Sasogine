using Weight = Sachssoft.Engine.Graphics.Text.FontWeight;
using FaceStyle = Sachssoft.Engine.Graphics.Text.FontStyle;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Demonstrates registered font faces, inherited typography and local overrides.</summary>
public sealed class ControlsFontsPage : StackPanel
{
    /// <summary>Creates an interactive typography page using the workspace font registry.</summary>
    public ControlsFontsPage()
    {
        Padding = new Insets2(24);
        Spacing = 12;
        FontSize = 18;
        Add(new TextBlock { Text = "One family. Four faces. Central cache.", FontSize = 30, FontWeight = Weight.Bold });
        Add(new TextBlock { Text = "FontName, FontSize, FontWeight and FontStyle inherit through containers and button content." });
        var faces = new Grid
        {
            Spacing = 12
        };
        faces.Columns.Add(new GridColumn { Width = GridLength.Proportion() });
        faces.Columns.Add(new GridColumn { Width = GridLength.Proportion() });
        faces.Rows.Add(new GridRow { Height = GridLength.Auto });
        faces.Rows.Add(new GridRow { Height = GridLength.Auto });
        string[] names =
        {
            "Regular",
            "Bold",
            "Italic",
            "Bold Italic"
        };
        for (int i = 0; i < names.Length; i++)
        {
            var sample = new TextBlock
            {
                Text = names[i] + "  /  Sasogine Controls  Aa Bb 0123",
                FontSize = 24,
                FontWeight = i % 2 == 1 ? Weight.Bold : Weight.Normal,
                FontStyle = i >= 2 ? FaceStyle.Italic : FaceStyle.Normal
            };
            faces.Children.Add(sample);
            faces.Cells.Put(sample, row: i / 2, column: i % 2);
        }

        Add(faces);
        Add(new TextBlock { Text = "INHERITANCE / change the parent size and face below", FontWeight = Weight.Bold });
        var preview = new StackPanel
        {
            Spacing = 10,
            FontSize = 22
        };
        preview.Add(new TextBlock { Text = "This TextBlock inherits from its parent." });
        preview.Add(new Button { Content = new TextBlock { Text = "Button content inherits too" }, Height = 54 });
        preview.Add(new TextBox { Text = "Editable text uses the same resolved font.", Height = 54 });
        preview.Add(new TextBlock { Text = "Local override: always regular at 16 px.", FontSize = 16, FontWeight = Weight.Normal, FontStyle = FaceStyle.Normal });
        Add(preview);
        var tools = new ToolStrip();
        var size = new Slider
        {
            Minimum = 16,
            Maximum = 32,
            Value = 22,
            Width = 220,
            Height = 36
        };
        var bold = new ToggleButton
        {
            Content = new TextBlock
            {
                Text = "Bold"
            },
            Height = 36
        };
        var italic = new ToggleButton
        {
            Content = new TextBlock
            {
                Text = "Italic"
            },
            Height = 36
        };
        size.ValueChanged += (_, _) => preview.FontSize = (int)Math.Round(size.Value);
        bold.Clicked += (_, _) => preview.FontWeight = bold.IsChecked == true ? Weight.Bold : Weight.Normal;
        italic.Clicked += (_, _) => preview.FontStyle = italic.IsChecked == true ? FaceStyle.Italic : FaceStyle.Normal;
        tools.Children.Add(new TextBlock { Text = "Size", VerticalAlignment = ControlAlignment.Center });
        tools.Children.Add(size);
        tools.Children.Add(bold);
        tools.Children.Add(italic);
        Add(tools);
        Add(new TextBlock { Text = "Families are registered once. Glyph systems are shared per face; adapters are shared per size.\nMissing variants use the engine's FontFamily fallback.", FontSize = 16 });
    }
}

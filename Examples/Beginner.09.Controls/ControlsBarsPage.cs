using Sachssoft.Engine.Input;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Demonstrates grouped wrapping toolbars, button-only palettes and arbitrary status content.</summary>
public sealed class ControlsBarsPage : Grid
{
    private readonly TextBlock _status = new()
    {
        Text = "Ready. Choose a command or a tool."
    };
    private readonly TextBlock _selected = new()
    {
        Text = "Tool: Select"
    };
    private readonly ToolBox _palette = new()
    {
        Columns = 3,
        Spacing = 4
    };
    /// <summary>Creates a responsive bars gallery using the standard skin and explicit commands.</summary>
    public ControlsBarsPage()
    {
        Padding = new Insets2(20);
        Spacing = 12;
        Rows.Add(new GridRow { Height = GridLength.Auto });
        Rows.Add(new GridRow { Height = GridLength.Auto });
        Rows.Add(new GridRow { Height = GridLength.Proportion() });
        Rows.Add(new GridRow { Height = GridLength.Auto });
        var heading = new TextBlock
        {
            Text = "TOOLBAR / grouped tools, button palettes and status content",
            Height = 28
        };
        Children.Add(heading);
        Cells.Put(heading);
        var toolbar = new ToolBar();
        Children.Add(toolbar);
        Cells.Put(toolbar, row: 1);
        var documents = new ToolStrip();
        var tools = new ToolStrip();
        var settings = new ToolStrip();
        var view = new ToolStrip();
        toolbar.Children.Add(documents);
        toolbar.Children.Add(tools);
        toolbar.Children.Add(settings);
        toolbar.Children.Add(view);
        var command = new Command(parameter => _status.Text = $"Command: {parameter}");
        foreach (string name in new[]
        {
            "New",
            "Open",
            "Save"
        }

        )
            documents.Children.Add(new Button { Content = new TextBlock { Text = name }, Height = 36, Command = command, CommandParameter = name, ToolTip = "ICommand receives the button's CommandParameter." });
        foreach (string name in new[]
        {
            "Select",
            "Move",
            "Rotate"
        }

        )
            tools.Children.Add(new SelectorButton { Content = new TextBlock { Text = name }, GroupName = "bars.toolbar", IsChecked = name == "Select", Height = 36, Command = new Command(parameter => _status.Text = $"Toolbar tool: {parameter}"), CommandParameter = name });
        settings.Children.Add(new TextBlock { Text = "Zoom", VerticalAlignment = ControlAlignment.Center });
        settings.Children.Add(new Slider { Width = 135, Minimum = 25, Maximum = 200, Value = 100, ToolTip = "Arbitrary controls are allowed inside a ToolStrip." });
        settings.Children.Add(new TextBox { Width = 180, Height = 36, Placeholder = "Find a tool..." });
        view.Children.Add(new ToggleButton { Content = new TextBlock { Text = "Grid" }, Height = 36, IsChecked = true });
        view.Children.Add(new ToggleButton { Content = new TextBlock { Text = "Guides" }, Height = 36 });
        var body = new Grid
        {
            Spacing = 16
        };
        body.Columns.Add(new GridColumn { Width = 290 });
        body.Columns.Add(new GridColumn { Width = GridLength.Proportion() });
        Children.Add(body);
        Cells.Put(body, row: 2);
        var paletteCard = new Grid
        {
            Padding = new Insets2(10),
            Spacing = 8
        };
        paletteCard.Rows.Add(new GridRow { Height = GridLength.Auto });
        paletteCard.Rows.Add(new GridRow { Height = GridLength.Auto });
        paletteCard.Rows.Add(new GridRow { Height = GridLength.Proportion() });
        var paletteTitle = new TextBlock
        {
            Text = "TOOLBOX / ButtonBase only",
            Height = 26
        };
        paletteCard.Children.Add(paletteTitle);
        paletteCard.Cells.Put(paletteTitle);
        var layouts = new ToolStrip
        {
            Padding = Insets2.Zero
        };
        foreach (string mode in new[]
        {
            "Auto",
            "2 cols",
            "3 cols",
            "2 rows"
        }

        )
            layouts.Children.Add(new Button { Content = new TextBlock { Text = mode }, Height = 32, Command = new Command(parameter => SetPaletteLayout((string)parameter!)), CommandParameter = mode });
        paletteCard.Children.Add(layouts);
        paletteCard.Cells.Put(layouts, row: 1);
        foreach (string name in new[]
        {
            "Select",
            "Move",
            "Rotate",
            "Scale",
            "Box",
            "Ellipse",
            "Line",
            "Path",
            "Fill",
            "Erase",
            "Text",
            "Color"
        }

        )
            _palette.Children.Add(new SelectorButton { Content = new TextBlock { Text = name }, GroupName = "bars.palette", Width = 76, Height = 44, MinWidth = 0, Padding = new Insets2(3), IsChecked = name == "Select", ToolTip = "A selector is a ButtonBase and can be placed in a ToolBox.", Command = new Command(parameter => _selected.Text = $"Tool: {parameter}"), CommandParameter = name });
        var paletteView = new ScrollViewer
        {
            Content = _palette,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden
        };
        paletteCard.Children.Add(paletteView);
        paletteCard.Cells.Put(paletteView, row: 2);
        body.Children.Add(paletteCard);
        body.Cells.Put(paletteCard);
        var explanation = new StackPanel
        {
            Spacing = 12,
            Padding = new Insets2(16)
        };
        explanation.Children.Add(new TextBlock { Text = "RESIZE THE WINDOW", Height = 28 });
        explanation.Children.Add(new TextBlock { Text = "ToolBar wraps whole ToolStrip groups onto new lines. If one group is wider than the entire bar, its controls can wrap too.", WrapText = true });
        explanation.Children.Add(new TextBlock { Text = "A ToolStrip can contain buttons, selectors, sliders, text fields, images or other controls.", WrapText = true });
        explanation.Children.Add(new TextBlock { Text = "ToolBox uses equally sized cells. Columns fills rows; Rows fills columns. Both set to zero enables automatic wrapping. Choose a layout on the left.", WrapText = true });
        explanation.Children.Add(new TextBlock { Text = "StatusBar contains StatusStrip hosts. Each host accepts any Content, including an interactive control such as the checkbox below.", WrapText = true });
        explanation.Children.Add(new TextBlock { Text = "Commands, immediate ownership checks, central styles and cached layout are shared with the other controls.", WrapText = true });
        var explanationView = new ScrollViewer
        {
            Content = explanation,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden
        };
        body.Children.Add(explanationView);
        body.Cells.Put(explanationView, column: 1);
        var status = new StatusBar();
        status.Children.Add(new StatusStrip { Content = _status });
        status.Children.Add(new StatusStrip { Content = _selected });
        status.Children.Add(new StatusStrip { Content = new CheckBox { Content = new TextBlock { Text = "Snap" }, IsChecked = true } });
        Children.Add(status);
        Cells.Put(status, row: 3);
    }

    private void SetPaletteLayout(string mode)
    {
        _palette.Columns = mode == "2 cols" ? 2 : mode == "3 cols" ? 3 : 0;
        _palette.Rows = mode == "2 rows" ? 2 : 0;
        _status.Text = $"ToolBox layout: {mode}";
    }
}

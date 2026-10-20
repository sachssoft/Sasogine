using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>A working atlas-backed gallery. The host supplies device snapshots, font and skin.</summary>
public sealed class ControlsShowcase
{
    private readonly Label _status = new()
    {
        Text = "Ready. Tab navigates; Escape closes popups."
    };
    /// <summary>Gets the input and rendering root.</summary>
    public Workspace Workspace { get; }

    /// <summary>Creates all controls with central styles and commands for each dialog.</summary>
    public ControlsShowcase(UIRegistry? registry = null)
    {
        var tabs = new TabControl
        {
            Background = Color.Transparent
        };
        Workspace = new Workspace(tabs, registry);
        registry = Workspace.Registry;
        registry.Styles.Register("Gallery.Page", new Style { BasedOn = "StackPanel", Padding = new Insets2(24), Spacing = 14, Background = new Color(17, 24, 35) });
        registry.Styles.Register("Gallery.Column", new Style { BasedOn = "StackPanel", Width = 340, Spacing = 10, Padding = new Insets2(16), Background = new Color(25, 35, 49) });
        registry.Styles.Register("Gallery.Heading", new Style { BasedOn = "Label", TextColor = new Color(65, 199, 223), Height = 28 });
        var page = new StackPanel()
        {
            StyleId = "Gallery.Page"
        };
        tabs.Items.Add(new TabItem() { Header = "Controls", Content = page, Padding = Insets2.Zero });
        tabs.Items.Add(new TabItem() { Header = "Grid, table & commands", Content = new ControlsDataPage(Workspace), Padding = Insets2.Zero });
        tabs.Items.Add(new TabItem() { Header = "Virtual joysticks", Content = new ControlsJoystickPage(), Padding = Insets2.Zero });
        tabs.Items.Add(new TabItem() { Header = "Tree, input & dates", Content = new ControlsAdvancedPage(), Padding = Insets2.Zero });
        tabs.Items.Add(new TabItem() { Header = "Curve editor", Content = new ControlsCurvePage(), Padding = Insets2.Zero });
        var menu = new MenuBar();
        var file = new MenuItem
        {
            Text = "&File"
        };
        file.Items.Add(new MenuItem { Text = "Open file...", ShortcutText = "Ctrl+O", ShortcutKey = Keys.O, ShortcutControl = true, Action = () => OpenFile() });
        file.Items.Add(new MenuItem { Text = "Choose folder...", Action = () => OpenFolder() });
        file.Items.Add(new MenuItem { IsSeparator = true });
        file.Items.Add(new MenuItem { Text = "About", Action = () => ShowMessage() });
        menu.Items.Add(file);
        menu.Items.Add(new MenuItem { Text = "&Color", Action = () => PickColor() });
        page.Add(menu);
        page.Add(new Label() { StyleId = "Gallery.Heading", Text = "SASOGINE  /  CONTROLS" });
        var columns = new StackPanel()
        {
            Orientation = StackOrientation.Horizontal,
            Spacing = 16,
            Height = 530
        };
        page.Add(columns);
        StackPanel Column(string title)
        {
            var column = new StackPanel()
            {
                StyleId = "Gallery.Column"
            };
            column.Add(new Label() { StyleId = "Gallery.Heading", Text = title });
            columns.Add(column);
            return column;
        }

        var input = Column("Input & selection");
        input.Add(new TextBox() { Placeholder = "Type text here...", Height = 36 });
        input.Add(new TextBox() { Text = "Multiline text editor\nUnicode, selection, undo / redo", AcceptsReturn = true, Height = 82 });
        input.Add(new CheckBox() { Content = new TextBlock { Text = "Enable snapping" }, IsChecked = true, Height = 30 });
        input.Add(new CheckBox() { Content = new TextBlock { Text = "Mixed / three state" }, IsThreeState = true, IsChecked = null, Height = 30 });
        input.Add(new RadioButton() { Content = new TextBlock { Text = "Perspective" }, GroupName = "camera", IsChecked = true, Height = 30 });
        input.Add(new RadioButton() { Content = new TextBlock { Text = "Orthographic" }, GroupName = "camera", Height = 30 });
        input.Add(new ToggleButton() { Content = new TextBlock { Text = "Toggle grid" }, IsChecked = true, Height = 36 });
        var options = new WrapPanel()
        {
            Height = 38,
            Spacing = 8
        };
        options.Add(new SelectorButton() { Content = new TextBlock { Text = "Move" }, GroupName = "tool", IsChecked = true, Width = 90, Height = 34 });
        options.Add(new SelectorButton() { Content = new TextBlock { Text = "Rotate" }, GroupName = "tool", Width = 90, Height = 34 });
        input.Add(options);
        var combo = new ComboBox()
        {
            Height = 36
        };
        foreach (string item in new[]
        {
            "Sachssoft Dark",
            "Custom style",
            "Inherited style"
        }

        )
            combo.Items.Add(new SelectorItem(item));
        combo.SelectedIndex = 0;
        input.Add(combo);
        var values = Column("Ranges & collections");
        var slider = new Slider()
        {
            Value = 62,
            Height = 30
        };
        slider.ValueChanged += (_, _) => _status.Text = $"Slider: {slider.Value:0}";
        values.Add(slider);
        values.Add(new ScrollBar() { Axis = ControlOrientation.Horizontal, ViewportSize = 30, Value = 32, Height = 18 });
        var list = new ListBox()
        {
            Height = 166,
            MultiSelect = true
        };
        for (int i = 0; i < 14; i++)
            list.Items.Add(new SelectorItem($"Library {i + 1:00}"));
        list.SelectedIndex = 2;
        values.Add(list);
        var tiles = new TiledPanel()
        {
            TileSize = new Size2(80, 40),
            Columns = 3,
            Spacing = 6
        };
        for (int i = 0; i < 18; i++)
            tiles.Add(new Button() { Content = new TextBlock { Text = $"Tile {i + 1}" } });
        values.Add(new ScrollViewer() { Height = 166, HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden, Content = tiles });
        var actions = Column("Windows & dialogs");
        void Action(string caption, Action action)
        {
            var button = new Button()
            {
                Content = new TextBlock
                {
                    Text = caption
                },
                Height = 36
            };
            button.Clicked += (_, _) => action();
            actions.Add(button);
        }

        var openContent = new StackPanel()
        {
            Orientation = StackOrientation.Horizontal,
            Spacing = 8,
            Background = Color.Transparent
        };
        openContent.Add(new Image() { Region = "icon_folder_open", Width = 20, Height = 20 });
        openContent.Add(new Label() { Text = "Open file...", VerticalAlignment = ControlAlignment.Center });
        var open = new Button()
        {
            Content = openContent,
            Height = 36
        };
        open.Clicked += (_, _) => OpenFile();
        actions.Add(open);
        Action("Select folder...", () => OpenFolder());
        Action("Choose color...", () => PickColor());
        Action("Message box...", () => ShowMessage());
        Action("Modeless window", () => Workspace.ShowWindow(new Window() { Title = "Movable / resizable", Width = 420, Height = 230, Content = new Label() { Text = "Drag the title bar or the bottom-right grip.", WrapText = true } }));
        var context = new ContextMenu();
        context.Items.Add(new MenuItem { Text = "First command", Action = () => _status.Text = "Context command executed." });
        var submenu = new MenuItem
        {
            Text = "More options"
        };
        submenu.Items.Add(new MenuItem { Text = "Nested command", Action = () => _status.Text = "Submenu command executed." });
        context.Items.Add(submenu);
        Action("Context menu...", () => Workspace.ShowPopup(context, new Bounds2(760, 380, 220, 120)));
        actions.Add(new ColorPlane() { Height = 174, SelectedColor = new Color(65, 199, 223) });
        page.Add(_status);
    }

    private async void OpenFile()
    {
        var dialog = new OpenFileDialog()
        {
            InitialDirectory = Environment.CurrentDirectory
        };
        dialog.Filters.Add(new FileDialogFilter("All files", "*"));
        if (await dialog.ShowAsync(Workspace) == DialogResult.OK)
            Workspace.Post(() => _status.Text = dialog.SelectedPath ?? "");
    }

    private async void OpenFolder()
    {
        var dialog = new FolderSelectorDialog()
        {
            InitialDirectory = Environment.CurrentDirectory
        };
        if (await dialog.ShowAsync(Workspace) == DialogResult.OK)
            Workspace.Post(() => _status.Text = dialog.SelectedPath ?? "");
    }

    private async void PickColor()
    {
        var dialog = new ColorPickerDialog()
        {
            SelectedColor = new Color(65, 199, 223)
        };
        if (await dialog.ShowAsync(Workspace) == DialogResult.OK)
            Workspace.Post(() => _status.Text = $"Color: {dialog.SelectedColor}");
    }

    private void ShowMessage() => _ = MessageBox.Show(Workspace, "All controls share the same atlas, style registry and input lifecycle.", "Sasogine Controls", MessageBoxButtons.OKCancel);
}

using Microsoft.Xna.Framework;
using Sachssoft.Engine.Input;
using System;
using System.Globalization;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Interactive grid/table/command sample using ordinary typed row data.</summary>
public sealed class ControlsDataPage : Grid
{
    /// <summary>Creates a table and command sample for the supplied root.</summary>
    public ControlsDataPage(Workspace root) : base()
    {
        Padding = new Insets2(20);
        Spacing = 12;
        Background = new Color(17, 24, 35);
        Rows.Add(new RowTrack { Height = GridLength.Auto });
        Rows.Add(new RowTrack { Height = GridLength.Auto });
        Rows.Add(new RowTrack { Height = GridLength.Star() });
        Rows.Add(new RowTrack { Height = GridLength.Auto });
        Rows.Add(new RowTrack { Height = GridLength.Auto });
        Add(new Label() { Text = "GRID / DATA TABLE / ICommand", TextColor = new Color(65, 199, 223), Height = 28 });
        var form = new Grid()
        {
            Spacing = 10,
            Background = Color.Transparent
        };
        form.Tracks.Add(new ColumnTrack { Width = GridLength.Auto });
        form.Tracks.Add(new ColumnTrack { Width = GridLength.Star(2) });
        form.Tracks.Add(new ColumnTrack { Width = GridLength.Star() });
        form.Add(new Label() { Text = "Project", VerticalAlignment = ControlAlignment.Center });
        form.Add(new TextBox() { Height = 36, Text = "Sasogine libraries" }, column: 1);
        form.Add(new Label() { Text = "Auto | 2* | *", VerticalAlignment = ControlAlignment.Center }, column: 2);
        Add(form, row: 1);
        var table = new DataTable()
        {
            ItemHeight = 34,
            MultiSelect = true,
            ToolTip = "Select full rows; Ctrl toggles, Shift selects a range, Ctrl+A selects all."
        };
        table.Columns.Add(new DataTableTextColumn<ControlsDemoRow>("Library", static row => row.Name, static (row, text) => row.Name = string.IsNullOrWhiteSpace(text) ? throw new ArgumentException("A name is required.") : text) { Width = 290 });
        table.Columns.Add(new DataTableTextColumn<ControlsDemoRow>("Version", static row => row.Version, static (row, text) => row.Version = text) { Width = 160 });
        table.Columns.Add(new DataTableTextColumn<ControlsDemoRow>("Downloads", static row => row.Downloads.ToString("N0", CultureInfo.InvariantCulture), static (row, text) => row.Downloads = int.Parse(text, NumberStyles.Integer | NumberStyles.AllowThousands, CultureInfo.InvariantCulture), static (a, b) => a.Downloads.CompareTo(b.Downloads)) { Width = 170, TextAlignment = ControlAlignment.End });
        table.Columns.Add(new DataTableTextColumn<ControlsDemoRow>("License", static row => row.License) { Width = 180 });
        table.Columns.Add(new DataTableTextColumn<ControlsDemoRow>("Notes", static row => "Explicit delegates / no reflection") { Width = 340 });
        string[] names =
        {
            "Sasogine Basic",
            "Sasogine Controls",
            "Sasogine Toolkit",
            "Sasogine Documents",
            "Sasogine DesktopGL",
            "Sasogine DesktopVK"
        };
        for (int i = 0; i < 36; i++)
            table.Items.Add(new ControlsDemoRow(names[i % names.Length], "0.12.0-alpha", 1200 + i * 137, "Example data"));
        table.SelectedIndex = 1;
        Add(table, row: 2);
        var status = new Label()
        {
            Text = "Ctrl/Shift selects full rows; Ctrl+A selects all. Headers sort; F2 edits a cell.",
            WrapText = true
        };
        var actions = new WrapPanel()
        {
            Spacing = 10,
            Background = Color.Transparent
        };
        var rowCommand = new Command<ControlsDemoRow>(row => status.Text = $"Command parameter: {row!.Name} ({row.Downloads:N0} downloads)", static row => row is not null);
        table.Command = rowCommand;
        var activate = new Button()
        {
            Content = new TextBlock
            {
                Text = "Run selected row command"
            },
            Width = 250,
            Height = 36,
            Command = rowCommand
        };
        table.SelectionChanged += (_, _) => activate.CommandParameter = table.SelectedItem;
        activate.CommandParameter = table.SelectedItem;
        actions.Add(activate);
        actions.Add(new Button() { Content = new TextBlock { Text = "Add row" }, Width = 120, Height = 36, Command = new Command(_ =>
        {
            table.Items.Add(new ControlsDemoRow("New library", "1.0", 0, "Example data"));
            table.Refresh();
            status.Text = "Row added.";
        }) });
        actions.Add(new Button() { Content = new TextBlock { Text = "Color models..." }, Width = 170, Height = 36, Command = new Command(_ =>
        {
            var picker = new ColorPickerDialog()
            {
                ColorModel = ColorPickerModel.Oklab,
                SelectedColor = new Color(65, 199, 223)
            };
            _ = picker.ShowAsync(root);
        }) });
        Add(actions, row: 3);
        Add(status, row: 4);
        Table = table;
    }

    /// <summary>Gets the live table for application-side changes.</summary>
    public DataTable Table { get; }
}

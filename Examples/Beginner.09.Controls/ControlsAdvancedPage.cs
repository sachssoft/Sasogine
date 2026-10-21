using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Tree, decimal/repeat input, passwords, dates and shared hover tooltips.</summary>
public sealed class ControlsAdvancedPage : Grid
{
    /// <summary>Gets the sample tree.</summary>
    public TreeView Tree { get; }
    /// <summary>Gets the sample decimal spinner.</summary>
    public NumericSpinner Number { get; }
    /// <summary>Gets the date picker.</summary>
    public DatePicker Date { get; }
    /// <summary>Gets the visible calendar.</summary>
    public Calendar Calendar { get; }

    /// <summary>Creates the additional control samples.</summary>
    public ControlsAdvancedPage() : base()
    {
        Padding = new Insets2(20);
        Spacing = 14;
        Rows.Add(new GridRow { Height = GridLength.Auto });
        Rows.Add(new GridRow { Height = GridLength.Proportion() });
        Rows.Add(new GridRow { Height = GridLength.Auto });
        var gridChild1 = new Label()
        {
            Text = "TREE / REPEAT / NUMBERS / DATES / TOOLTIPS",
            Height = 28,
            StyleId = "Gallery.Heading"
        };
        Children.Add(gridChild1);
        Cells.Put(gridChild1);
        var columns = new Grid()
        {
            Spacing = 14
        };
        for (int i = 0; i < 3; i++)
            columns.Columns.Add(new GridColumn { Width = GridLength.Proportion() });
        Children.Add(columns);
        Cells.Put(columns, row: 1);
        var status = new Label()
        {
            Text = "Hover a control for help. Tree arrows expand; hold spinner arrows to repeat.",
            WrapText = true
        };
        Children.Add(status);
        Cells.Put(status, row: 2);
        var treeCard = new Grid()
        {
            Padding = new Insets2(14),
            Spacing = 10,
            BorderThickness = 1
        };
        treeCard.Rows.Add(new GridRow { Height = GridLength.Auto });
        treeCard.Rows.Add(new GridRow { Height = GridLength.Proportion() });
        columns.Children.Add(treeCard);
        columns.Cells.Put(treeCard);
        var gridChild2 = new Label()
        {
            Text = "TreeView",
            Height = 28,
            StyleId = "Gallery.Heading"
        };
        treeCard.Children.Add(gridChild2);
        treeCard.Cells.Put(gridChild2);
        Tree = new TreeView()
        {
            ToolTip = "Left/Right collapses or expands. Up/Down selects a visible item."
        };
        var project = new TreeViewItem
        {
            Text = "Sasogine",
            IsExpanded = true,
            Icon = "icon_folder_open"
        };
        foreach (string title in new[]
        {
            "Basic",
            "Controls",
            "Assets",
            "Scenes"
        }

        )
        {
            var folder = new TreeViewItem
            {
                Text = title,
                IsExpanded = title == "Assets",
                Icon = "icon_folder_open"
            };
            for (int i = 1; i <= 8; i++)
                folder.Items.Add(new TreeViewItem { Text = $"{title} item {i:00}" });
            project.Items.Add(folder);
        }

        Tree.Items.Add(project);
        treeCard.Children.Add(Tree);
        treeCard.Cells.Put(Tree, row: 1);
        Tree.SelectionChanged += (_, _) => status.Text = "Selected: " + Tree.SelectedItem?.Text;
        var inputCard = new StackPanel()
        {
            Padding = new Insets2(14),
            Spacing = 12,
            BorderThickness = 1
        };
        var scroll = new ScrollViewer()
        {
            Content = inputCard,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
            ToolTip = "This ScrollViewer has the same ToolTip support as every control."
        };
        columns.Children.Add(scroll);
        columns.Cells.Put(scroll, column: 1);
        inputCard.Add(new Label() { Text = "Input & repeat", Height = 28, StyleId = "Gallery.Heading" });
        inputCard.Add(new Label() { Text = "Decimal number / 0.25 steps" });
        Number = new NumericSpinner()
        {
            NumericMinimum = -100,
            NumericMaximum = 100,
            NumericValue = 12.5m,
            NumericIncrement = .25m,
            DecimalPlaces = 2,
            Height = 38,
            ToolTip = "Hold an arrow, or use Up/Down. Invalid text keeps the previous number."
        };
        inputCard.Add(Number);
        int count = 0;
        var repeatCaption = new TextBlock
        {
            Text = "Hold to repeat: 0"
        };
        var repeat = new RepeatButton()
        {
            Content = repeatCaption,
            Height = 38,
            ToolTip = "Activates immediately, then repeats using frame time."
        };
        repeat.Clicked += (_, _) => repeatCaption.Text = $"Hold to repeat: {++count}";
        inputCard.Add(repeat);
        inputCard.Add(new Label() { Text = "PasswordBox" });
        inputCard.Add(new PasswordBox() { Text = "Sasogine", Height = 38, ToolTip = "Masked input. Copy and cut do not expose the password." });
        inputCard.Add(new Label() { Text = "DatePicker / yyyy-MM-dd" });
        Date = new DatePicker()
        {
            SelectedDate = new DateOnly(2024, 2, 20),
            DisplayMonth = new DateOnly(2024, 2, 1),
            Today = new DateOnly(2024, 2, 20),
            Height = 38,
            ToolTip = "Type a date or open the calendar. F4 / Alt+Down opens the popup."
        };
        inputCard.Add(Date);
        var loading = new StackPanel()
        {
            Orientation = StackOrientation.Horizontal,
            Spacing = 12,
            Height = 44
        };
        loading.Add(new Spinner() { Width = 36, Height = 36 });
        loading.Add(new Label() { Text = "Loading spinner", VerticalAlignment = ControlAlignment.Center });
        inputCard.Add(loading);
        var help = new Button()
        {
            Content = new TextBlock
            {
                Text = "Hover: rich tooltip"
            },
            Height = 38,
            ToolTipContent = new ScrollViewer()
            {
                Width = 240,
                Height = 110,
                Content = new Label()
                {
                    Text = "A tooltip may host Controls, including a ScrollViewer. It does not take focus or capture.",
                    WrapText = true,
                    Width = 210
                },
                HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden
            }
        };
        inputCard.Add(help);
        var calendarCard = new Grid()
        {
            Padding = new Insets2(14),
            Spacing = 10,
            BorderThickness = 1
        };
        calendarCard.Rows.Add(new GridRow { Height = GridLength.Auto });
        calendarCard.Rows.Add(new GridRow { Height = GridLength.Auto });
        calendarCard.Rows.Add(new GridRow { Height = GridLength.Proportion() });
        columns.Children.Add(calendarCard);
        columns.Cells.Put(calendarCard, column: 2);
        var gridChild3 = new Label()
        {
            Text = "Calendar",
            Height = 28,
            StyleId = "Gallery.Heading"
        };
        calendarCard.Children.Add(gridChild3);
        calendarCard.Cells.Put(gridChild3);
        Calendar = new Calendar()
        {
            Height = 290,
            DisplayMonth = new DateOnly(2024, 2, 1),
            SelectedDate = new DateOnly(2024, 2, 20),
            Today = new DateOnly(2024, 2, 20),
            ToolTip = "The sample uses an explicit date, so it replays deterministically."
        };
        calendarCard.Children.Add(Calendar);
        calendarCard.Cells.Put(Calendar, row: 1);
        var gridChild4 = new Label()
        {
            Text = "Leap years, bounded date selection and culture-aware month/weekday names.\n\nToday is supplied by the host; no hidden system clock.",
            WrapText = true
        };
        calendarCard.Children.Add(gridChild4);
        calendarCard.Cells.Put(gridChild4, row: 2);
        Calendar.SelectionChanged += (_, _) => status.Text = "Calendar: " + Calendar.SelectedDate;
        Date.SelectionChanged += (_, _) => status.Text = "DatePicker: " + Date.SelectedDate;
    }
}

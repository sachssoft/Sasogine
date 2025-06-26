using System;
using sachssoft.Sasogine.Surface;
using sachssoft.Sasogine.Surface.Visuals;
using sachssoft.Sasogine.Surface.Visuals.Controls;

namespace sachssoft.Sasogine.Views.Dialogs;

public sealed class MessageDialog : Window
{
    private HorizontalStackPanel _button_panel;
    private Label _message_label;

    public enum MessageButtons
    {
        OKOnly,
        OKCancel,
        YesNo,
        YesNoCancel,
        Custom
    }

    public enum MessageResults
    {
        OK,
        Cancel,
        Yes,
        No,
        Custom
    }

    public static void Show(ViewBase view, string message, Action? a = null)
        => Show((Desktop)view.Host, message, MessageButtons.OKOnly, (rlt) => a?.Invoke());

    public static void Show(Desktop desktop, string message, Action? a = null)
        => Show(desktop, message, MessageButtons.OKOnly, (rlt) => a?.Invoke());

    public static void Show(ViewBase view, string message, MessageButtons buttons, Action<MessageResults>? result = null)
         => Show((Desktop)view.Host, message, buttons, result);
    public static void Show(Desktop desktop, string message, MessageButtons buttons, Action<MessageResults>? result = null)
    {
        var dlg = new MessageDialog(message, buttons);
        dlg.Closed += (s, e) =>
        {
            if (s is MessageDialog dlg)
            {
                result?.Invoke(dlg.Result);
            }
        };
        dlg.ShowModal(desktop);
    }

    private MessageDialog(string message, MessageButtons buttons = MessageButtons.OKCancel)
    {
        Width = 400;

        var grid = new Grid()
        {
            ColumnSpacing = 10,
            RowSpacing = 10
        };

        grid.ColumnsProportions.Add(new(ProportionType.Auto));
        grid.ColumnsProportions.Add(new(ProportionType.Fill));
        grid.RowsProportions.Add(new(ProportionType.Fill));
        grid.RowsProportions.Add(new(ProportionType.Auto));

        _message_label = new Label()
        {
            Text = message,
            Margin = new Thickness(20),
            Wrap = true
        };
        Grid.SetColumn(_message_label, 1);
        Grid.SetRow(_message_label, 0);
        grid.Widgets.Add(_message_label);

        _button_panel = new HorizontalStackPanel()
        {
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetColumn(_button_panel, 1);
        Grid.SetRow(_button_panel, 1);
        grid.Widgets.Add(_button_panel);

        switch (buttons)
        {
            case MessageButtons.OKOnly:
                AddButton("OK", MessageResults.OK);
                break;
            case MessageButtons.OKCancel:
                AddButton("OK", MessageResults.OK);
                AddButton("Cancel", MessageResults.Cancel);
                break;
            case MessageButtons.YesNo:
                AddButton("Yes", MessageResults.Yes);
                AddButton("No", MessageResults.No);
                break;
            case MessageButtons.YesNoCancel:
                AddButton("Yes", MessageResults.Yes);
                AddButton("No", MessageResults.No);
                AddButton("Cancel", MessageResults.Cancel);
                break;
        }

        Content = grid;
    }

    private void AddButton(string label, MessageResults result)
    {
        var btn = new Button()
        {
            Width = 100,
            Content = new Label()
            {
                Text = label,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        };

        btn.Click += (s, e) =>
        {
            Result = result;
            Close();
        };
        _button_panel.Widgets.Add(btn);
    }

    public new MessageResults Result
    {
        get;
        private set;
    }
}
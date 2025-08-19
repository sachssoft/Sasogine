using System;
using Sachssoft.Sasogine.Surface.Visuals.Controls;

namespace Sachssoft.Sasogine.Surface.Design;

public class BooleanEditor : PropertyEditorBase
{
    private CheckButton _check_button;

    public BooleanEditor()
    {
    }

    public override bool IsDisplayLabelVisibilty => false;

    public override void Update(object? new_value)
    {
        _check_button.IsChecked = (bool)new_value;
    }

    public override Widget CreateControl<T>(
        Action<T, string> changed,
        Func<T, object?> getter,
        Action<T, object?> setter)
    {
        var p = new HorizontalStackPanel()
        {
            Spacing = 5
        };

        _check_button = new CheckButton();
        _check_button.IsChecked = (bool)getter.Invoke((T)Source);
        _check_button.IsCheckedChanged += (s, e) =>
        {
            setter.Invoke((T)Source, _check_button.IsChecked);
        };

        _check_button.Content = new Label() { Text = DisplayLabel, Margin = new Thickness(10, 0, 0, 0) };
        p.Widgets.Add(_check_button);

        return p;
    }
}

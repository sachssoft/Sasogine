using System;
using sachssoft.Sasogine.Surface.Visuals.Controls;

namespace sachssoft.Sasogine.Surface.Design;

public class BooleanEditor : PropertyEditorBase
{
    public BooleanEditor()
    {
    }

    public override bool IsDisplayLabelVisibilty => false;

    //public override bool ForType(Type type)
    //{
    //    return type == typeof(bool);
    //}

    //public override Widget CreateControl()
    //{
    //    var p = new HorizontalStackPanel()
    //    {
    //        Spacing = 5
    //    };

    //    var on_rb = new RadioButton()
    //    {
    //        Content = new Label() { Text = "On" },
    //        IsPressed = (bool)Value!
    //    };

    //    var off_rb = new RadioButton()
    //    {
    //        Content = new Label() { Text = "Off" },
    //        IsPressed = !(bool)Value!
    //    };

    //    on_rb.PressedChanged += (s, e) =>
    //    {
    //        if (on_rb.IsPressed)
    //        {
    //            off_rb.IsPressed = false;
    //            Value = true;
    //        }
    //    };

    //    off_rb.PressedChanged += (s, e) =>
    //    {
    //        if (off_rb.IsPressed)
    //        {
    //            on_rb.IsPressed = false;
    //            Value = false;
    //        }
    //    };

    //    p.Widgets.Add(on_rb);
    //    p.Widgets.Add(off_rb);

    //    return p;
    //}

    public override Widget CreateControl<T>(
        Action<T, string> changed,
        Func<T, object?> getter,
        Action<T, object?> setter)
    {
        var p = new HorizontalStackPanel()
        {
            Spacing = 5
        };

        var chk = new CheckButton();
        chk.Content = new Label() { Text = DisplayLabel, Margin = new Visuals.Thickness(10, 0, 0, 0) };
        p.Widgets.Add(chk);

        return p;
    }
}

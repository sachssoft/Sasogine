using System;
using sachssoft.Sasogine.Surface.Visuals.Controls;

namespace sachssoft.Sasogine.Surface.Design;

public class StringEditor : PropertyEditorBase
{
    private readonly bool _is_multiline;

    public StringEditor(bool is_multiline = false)
    {
        _is_multiline = is_multiline;
    }

    public override Widget CreateControl<T>(
        Action<T, string> changed,
        Func<T, object?> getter,
        Action<T, object?> setter)
    {
        var txt = new TextBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Text = getter((T)Source)?.ToString() ?? ""
        };

        if (_is_multiline)
        {
            txt.Multiline = true;
            txt.Height = 70;
        }

        txt.TextChanged += (s, e) =>
        {
            setter((T)Source, txt.Text);
            changed?.Invoke((T)Source, nameof(txt.Text));
        };

        return txt;
    }
}

using System;
using System.Collections.Generic;
using sachssoft.Sasogine.Surface.Visuals.Controls;
using static System.Net.Mime.MediaTypeNames;

namespace sachssoft.Sasogine.Surface.Design;

public class EnumEditor : PropertyEditorBase
{
    private List<(string Label, Enum Field)> _items = new();
    private ComboBox _combo_box;

    public EnumEditor(params (string Label, Enum Field)[] items)
    {
        _items.AddRange(items);
    }

    public void AddItem<TEnum>(string label, TEnum field) where TEnum : struct, Enum
    {
        _items.Add((label, field));
    }

    public override Widget CreateControl<T>(
        Action<T, string> changed,
        Func<T, object?> getter,
        Action<T, object?> setter)
    {
        if (_items.Count == 0)
            throw new InvalidOperationException("No enum fields defined.");

        _combo_box = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        var selected = -1;
        var current = getter((T)Source);

        for (int i = 0; i < _items.Count; i++)
        {
            var item = _items[i];

            var label = new Label
            {
                Text = item.Label,
                Tag = item.Field
            };

            _combo_box.Widgets.Add(label);

            if (Equals(item.Field, current))
                selected = i;
        }

        _combo_box.SelectedIndex = selected >= 0 ? selected : 0;

        _combo_box.SelectedIndexChanged += (s, e) =>
        {
            if (_combo_box.SelectedItem is Label selectedLabel && selectedLabel.Tag is Enum enumValue)
            {
                setter((T)Source, enumValue);
            }
        };

        return _combo_box;
    }

}

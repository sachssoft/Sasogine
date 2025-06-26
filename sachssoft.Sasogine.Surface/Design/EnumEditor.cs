using System;
using System.Collections.Generic;
using sachssoft.Sasogine.Surface.Visuals.Controls;
using static System.Net.Mime.MediaTypeNames;

namespace sachssoft.Sasogine.Surface.Design;

public class EnumEditor : PropertyEditorBase
{
    private List<(string Label, Enum Field)> _items = new();

    public EnumEditor(params (string Label, Enum Field)[] items)
    {
        _items.AddRange(items);
    }

    //public override bool ForType(Type type)
    //{
    //    return type.IsEnum;
    //}

    //public override Widget CreateControl()
    //{
    //    var cbo = new ComboView()
    //    {
    //        HorizontalAlignment = HorizontalAlignment.Stretch
    //    };

    //    var type = PropertyType;
    //    var value = Value!;
    //    var enum_field = Enum.GetName(type, value);
    //    var index = 0;

    //    foreach (var name in Enum.GetNames(type))
    //    {
    //        var field = type.GetField(name);

    //        cbo.Widgets.Add(new Label()
    //        {
    //            Text = name,
    //            Tag = field!.GetValue(value)
    //        });

    //        if (enum_field == name)
    //            cbo.SelectedIndex = index;

    //        index++;
    //    }

    //    cbo.SelectedIndexChanged += (s, e) =>
    //    {
    //        var w = cbo.SelectedItem;
    //        Value = w.Tag;
    //    };

    //    return cbo;
    //}

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
            throw new InvalidOperationException("No fields");

        var cbo = new ComboView()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        var index = 0;

        foreach (var item in _items)
        {
            cbo.Widgets.Add(new Label()
            {
                Text = item.Label,
                Tag = item.Field
            });

            if (getter((T)Source) == item.Field)
                cbo.SelectedIndex = index;

            index++;
        }

        cbo.SelectedIndexChanged += (s, e) =>
        {
            var sender = ((ListView)s);
            setter((T)Source, (Enum)sender.SelectedItem.Tag);
        };

        cbo.SelectedIndex = index;

        return cbo;
    }
}

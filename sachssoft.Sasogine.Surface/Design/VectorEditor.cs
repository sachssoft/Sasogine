using Microsoft.Xna.Framework;
using System;
using System.ComponentModel;
using Sachssoft.Sasogine.Surface.Visuals.Controls;

namespace Sachssoft.Sasogine.Surface.Design;

public class VectorEditor : PropertyEditorBase
{
    private NumericEditor x_editor;
    private NumericEditor y_editor;
    private bool _b;

    public VectorEditor() { }

    //public override bool ForType(Type type)
    //{
    //    return type == typeof(Vector2);
    //}

    //public override Widget CreateControl()
    //{
    //    var type = PropertyType;
    //    var val = Value;

    //    var grid = new Grid()
    //    {
    //        ColumnSpacing = 5
    //    };

    //    //x_editor = FromField<NumericEditor>(nameof(Vector2.X), val);
    //    //y_editor = FromField<NumericEditor>(nameof(Vector2.Y), val);

    //    // https://stackoverflow.com/questions/6280506/is-there-a-way-to-set-properties-on-struct-instances-using-reflection
    //    // Boxed value !!
    //    x_editor.ValueChanged = () =>
    //    {
    //        if (_b == true)
    //        {
    //            _b = false;
    //            return;
    //        }

    //        Value = val;
    //    };
    //    y_editor.ValueChanged = () =>
    //    {
    //        if (_b == true)
    //        {
    //            _b = false;
    //            return;
    //        }

    //        Value = val;
    //    };

    //    grid.ColumnsProportions.Add(new());
    //    grid.ColumnsProportions.Add(new(ProportionType.Part, 1f));
    //    grid.ColumnsProportions.Add(new());
    //    grid.ColumnsProportions.Add(new(ProportionType.Part, 1f));

    //    grid.Widgets.Add(new Label() { Text = "X" });
    //    grid.Widgets.Add(x_editor.CreateControl());
    //    grid.Widgets.Add(new Label() { Text = "Y" });
    //    grid.Widgets.Add(y_editor.CreateControl());

    //    grid.Widgets[1].Margin = new(0, 0, 5, 0);

    //    Grid.SetColumn(grid.Widgets[1], 1);
    //    Grid.SetColumn(grid.Widgets[2], 2);
    //    Grid.SetColumn(grid.Widgets[3], 3);

    //    return grid;
    //}

    //protected override void OnSourcePropertyChanged(PropertyChangedEventArgs e)
    //{
    //    if (e.PropertyName == PropertyName)
    //    {
    //        var v = ((Vector2?)Value).GetValueOrDefault();

    //        x_editor.Value = v.X;
    //        y_editor.Value = v.Y;

    //        _b = true;
    //    }
    //}
}

using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using System;
using sachssoft.Sasogine.Surface.Visuals.Brushes;
using sachssoft.Sasogine.Surface.Visuals.Controls;

namespace sachssoft.Sasogine.Surface.Design;

public class ColorEditor : PropertyEditorBase
{
    public ColorEditor()
    {
    }

    //public override Widget CreateControl()
    //{
    //    var is_null = IsNullable();
    //    var type = PropertyType;
    //    var val = is_null ? (Color?)Value : (Color)Value!;

    //    var grid = new Grid();

    //    grid.ColumnsProportions.Add(new(ProportionType.Auto));
    //    grid.ColumnsProportions.Add(new(ProportionType.Auto));
    //    grid.ColumnsProportions.Add(new(ProportionType.Fill));
    //    grid.ColumnsProportions.Add(new(ProportionType.Auto));

    //    var chk = new CheckButton()
    //    {
    //        IsChecked = val.HasValue,
    //        Visible = IsNullable()
    //    };

    //    var view = new Label()
    //    {
    //        Background = val != null ? new SolidBrush(val.Value) : null,
    //        Width = 50,
    //        Margin = new(0, 0, 5, 0)
    //    };

    //    var txt = new TextBox()
    //    {
    //        Text = val != null ? val.Value.ToHexString() : string.Empty,
    //        Enabled = !IsNullable()
    //    };

    //    var btn = new Button()
    //    {
    //        Content = new Label()
    //        {
    //            Text = "...",
    //            Padding = new(2, 0, 2, 0)
    //        },
    //        Enabled = !IsNullable()
    //    };

    //    chk.IsCheckedChanged += (s, e) =>
    //    {
    //        if (!chk.IsChecked)
    //        {
    //            Value = null;
    //            txt.Enabled = false;
    //            btn.Enabled = false;
    //        }
    //        else
    //        {
    //            Value = Color.Black;
    //            txt.Enabled = true;
    //            btn.Enabled = true;
    //        }
    //    };

    //    btn.Click += (s, e) =>
    //    {
    //        var dlg = new ColorPickerDialogExtended();
    //        dlg.Color = val.Value;
    //        dlg.Load();

    //        dlg.Closed += (s, a) =>
    //        {
    //            if (!dlg.Result)
    //            {
    //                return;
    //            }

    //            view.Background = new SolidBrush(dlg.Color);
    //            txt.Text = dlg.Color.ToHexString();
    //            Value = dlg.Color;
    //        };

    //        dlg.ShowModal(Builder.Desktop);
    //    };

    //    grid.Widgets.Add(chk);
    //    grid.Widgets.Add(view);
    //    grid.Widgets.Add(txt);
    //    grid.Widgets.Add(btn);

    //    Grid.SetColumn(grid.Widgets[1], 1);
    //    Grid.SetColumn(grid.Widgets[2], 2);
    //    Grid.SetColumn(grid.Widgets[3], 3);

    //    return grid;
    //}

    //public override bool ForType(Type type)
    //{
    //    return type == typeof(Color);
    //}
}

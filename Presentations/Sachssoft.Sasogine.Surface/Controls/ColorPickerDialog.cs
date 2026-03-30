using FontStashSharp;
using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Controls;
using System;

namespace Sachssoft.Sasogine.Surface.Controls;

public class ColorPickerDialog : Dialog
{
    public ColorPickerPanel ColorPickerPanel { get; }

    public Color Color
    {
        get
        {
            return ColorPickerPanel.Color;
        }

        set
        {
            ColorPickerPanel.Color = value;
        }
    }

    public ColorPickerDialog()
    {
        ColorPickerPanel = new ColorPickerPanel();

        Title = "Color Picker";
        Content = ColorPickerPanel;
    }

    //public override void Close()
    //{
    //    base.Close();

    //    for (var i = 0; i < ColorPickerPanel.UserColors.Length; ++i)
    //    {
    //        var colorDisplay = ColorPickerPanel.GetUserColorImage(i);
    //        var color = colorDisplay.Color.Value;
    //        var alpha = (int)(colorDisplay.Opacity * 255);
    //        ColorPickerPanel.UserColors[i] = new Color(color.R, color.G, color.B, alpha);
    //    }
    //}

    //public void ApplyColorPickerDialogStyle(ColorPickerDialogStyle style)
    //{
    //    ApplyWindowStyle(style);

    //    ColorPickerPanel.ApplyColorPickerDialogStyle(style);
    //}

    public override void ApplyFromStyle(Style style)
    {
        base.ApplyFromStyle(style);
        ColorPickerPanel.ApplyFromStyle(style);
    }
}
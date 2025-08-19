using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;
using Sachssoft.Sasogine.Surface.Visuals.Controls;

namespace Sachssoft.Sasogine.Surface.Design;

[Obsolete("Remove!!")]
public class ColorPickerDialogExtended : ColorPickerDialog
{
    private ToggleButton[] _buttons;
    private int _selected_index = 0;

    public ColorPickerDialogExtended()
    {

    }

    internal void Load()
    {
        var palette_gen = Generate();
        var count = palette_gen.Length;

        //if (palette == null)
        //{
        //    Palette = palette_gen;
        //}

        Palette = palette_gen;

        //if (palette.Length < count)
        //{
        //    Palette = new Color[count];

        //    for (int i = 0; i < count; i++)
        //    {
        //        if (i >= palette.Length)
        //        {
        //            Palette[i] = Color.Red;
        //        }
        //        else
        //        {
        //            Palette[i] = palette[i];
        //        }
        //    }
        //}

        ButtonOk.Width = 100;
        ButtonOk.Content.HorizontalAlignment = HorizontalAlignment.Center;
        ButtonCancel.Width = 100;
        ButtonCancel.Content.HorizontalAlignment = HorizontalAlignment.Center;

        ColorPickerPanel._saveColor.IsEnabled = true;
        ColorPickerPanel._saveColor.Width = 100;
        ColorPickerPanel._saveColor.Content.HorizontalAlignment = HorizontalAlignment.Center;
        ColorPickerPanel._userColors.Widgets.Clear();
        Height = 600;

        var panel = new VerticalStackPanel()
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            Spacing = 5
        };

        ColorPickerPanel._userColors.SelectionHoverBackground = null;
        ColorPickerPanel._userColors.SelectionBackground = null;
        ColorPickerPanel._userColors.Widgets.Add(panel);

        var columns = 15;
        _buttons = new ToggleButton[count];

        for (int i = 0; i < count; i += columns)
        {
            var row = new HorizontalStackPanel()
            {
                Spacing = 5,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            for (int j = 0; j < columns; j++)
            {
                if (i + j >= count)
                    break;

                _buttons[i + j] = new ToggleButton()
                {
                    Width = 24,
                    Height = 24,
                    Content = new Label()
                    {
                        Top = 2,
                        Left = 2,
                        Width = 20,
                        Height = 20,
                        Background = new SolidBrush(Palette[i + j])
                    },
                    OverBackground = new SolidBrush(Color.LightBlue),
                    PressedBackground = new SolidBrush(Color.Blue),
                    Tag = new Tuple<int, Color>(i + j, Palette[i + j])
                };

                _buttons[i + j].Click += (s, e) =>
                {
                    var btn = (ToggleButton)s!;
                    var o = (Tuple<int, Color>)btn.Tag;

                    Color = o.Item2;
                    _selected_index = o.Item1;

                    foreach (var t in _buttons)
                    {
                        if (t == btn)
                        {
                            t.IsPressed = true; // !t.IsPressed;
                        }
                        else
                        {
                            t.IsPressed = false;
                        }
                    }
                };

                row.Widgets.Add(_buttons[i + j]);
            }

            panel.Widgets.Add(row);
        }
    }

    public Color[]? Palette { get; private set; }

    private Color[] Generate(int columns = 15, int rows = 10)
    {
        var list = new List<Color>(/*new Color[columns * rows]*/);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                if (i == 0)
                {
                    var c = j / (columns - 1f);
                    list.Add(new Color(c, c, c, 1f));
                }
                else if (i == rows - 1)
                {
                    list.Add(new Color(Color.Black, 1f));
                }
                else
                {
                    list.Add(FromHSL(j / (float)columns * 360f, 100f, i / (rows - 1f) * 100f));
                }
            }
        }

        return list.ToArray();
    }

    private Color FromHSL(float hue, float saturation, float light)
    {
        float red, green, blue;

        var h = hue / 360.0f;
        var s = saturation / 100.0f;
        var l = light / 100.0f;

        if (Math.Abs(s - 0.0f) < 0.00001f)
        {
            red = l;
            green = l;
            blue = l;
        }
        else
        {
            float var2;

            if (l < 0.5f)
            {
                var2 = l * (1.0f + s);
            }
            else
            {
                var2 = l + s - s * l;
            }

            var var1 = 2.0f * l - var2;

            red = hue2rgb(var1, var2, h + 1.0f / 3.0f);
            green = hue2rgb(var1, var2, h);
            blue = hue2rgb(var1, var2, h - 1.0f / 3.0f);
        }

        // --

        var r = Convert.ToInt32(red * 255.0);
        var g = Convert.ToInt32(green * 255.0);
        var b = Convert.ToInt32(blue * 255.0);

        return new Color(r, g, b, 255);
    }

    private float hue2rgb(float v1, float v2, float vH)
    {
        if (vH < 0.0f)
        {
            vH += 1.0f;
        }
        if (vH > 1.0f)
        {
            vH -= 1.0f;
        }
        if (6.0f * vH < 1.0f)
        {
            return v1 + (v2 - v1) * 6.0f * vH;
        }
        if (2.0f * vH < 1.0f)
        {
            return v2;
        }
        if (3.0f * vH < 2.0f)
        {
            return v1 + (v2 - v1) * (2.0f / 3.0f - vH) * 6.0f;
        }

        return v1;
    }

}

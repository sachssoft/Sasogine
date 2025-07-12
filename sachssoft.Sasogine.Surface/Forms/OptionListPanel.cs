using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using System;
using sachssoft.Sasogine.Surface.Visuals;
using sachssoft.Sasogine.Surface.Visuals.Brushes;
using sachssoft.Sasogine.Surface.Visuals.Controls;

namespace sachssoft.Sasogine.Surface.Forms;

public class OptionListPanel : VerticalStackPanel
{
    private int _label_width;

    public OptionListPanel(int label_width = 150)
    {
        Spacing = 15;
        Padding = new Thickness(10);
        _label_width = label_width;
    }

    public void AddTextbox(string label, string? text, out TextBox txt, bool vertical = false)
    {
        txt = new TextBox()
        {
            Text = text,
            Width = 200
        };

        AddSetter(label, txt, vertical);
    }

    public void AddReadOnlyTextbox(string label, string? text, bool vertical = false)
    {
        var txt = new TextBox()
        {
            Text = text,
            Width = 200,
            Readonly = true
        };

        AddSetter(label, txt, vertical);
    }

    public void AddTextarea(string label, string? text, out TextBox txt, bool vertical = false)
    {
        txt = new TextBox()
        {
            Text = text,
            Multiline = true,
            Width = 300,
            Height = 50
        };

        AddSetter(label, txt, vertical);
    }

    public void AddDropDownList(string label, string[] items, int selected_index, out ComboBox cbo, bool vertical = false, Action<int>? selected_event = null)
    {
        cbo = new ComboBox()
        {
            Width = 200
        };

        foreach (var item in items)
        {
            cbo.Widgets.Add(new Label()
            {
                Text = item
            });
        }
        cbo.SelectedIndex = selected_index;

        cbo.SelectedIndexChanged += (s, e) =>
        {
            if (s is ListView l)
            {
                selected_event?.Invoke(l.SelectedIndex.GetValueOrDefault());
            }
        };

        AddSetter(label, cbo, vertical);
    }

    public void AddOption<TEnum>(string label, int value, out ComboBox cbo, bool vertical = false, Action<int>? selected_event = null) where TEnum : struct, Enum =>
        AddOption(label, typeof(TEnum), value, out cbo, vertical, selected_event);

    public void AddOption(string label, Type enum_type, int value, out ComboBox cbo, bool vertical = false, Action<int>? selected_event = null)
    {
        cbo = new ComboBox()
        {
            Width = 200
        };

        var index = 0;

        foreach (var n in Enum.GetValues(enum_type))
        {
            var field = Enum.GetName(enum_type, n);
            cbo.Widgets.Add(new Label()
            {
                Text = field,
                Tag = field
            });

            if (Enum.GetName(enum_type, value) == field)
            {
                cbo.SelectedIndex = index;
            }

            index++;
        }

        cbo.SelectedIndexChanged += (s, e) =>
        {
            if (s is ListView l)
            {
                var item = (int)Enum.Parse(enum_type, (string)l.SelectedItem.Tag);
                selected_event?.Invoke(item);
            }
        };

        AddSetter(label, cbo, vertical);
    }

    public void AddCheckOption(string label, bool value, bool indent /* Einzug */, out CheckButton chk, Action<bool>? checked_event = null)
    {
        chk = new CheckButton()
        {
            Content = new Label()
            {
                Text = label,
                Margin = new Thickness(10, 0, 0, 0)
            },
            IsChecked = value
        };

        if (indent == true)
        {
            AddSetter("", chk, false);
        }
        else
        {
            Widgets.Add(chk);
        }

        chk.IsCheckedChanged += (s, o) =>
        {
            if (s is CheckButton c)
            {
                checked_event?.Invoke(c.IsChecked);
            }
        };
    }

    public void AddNumericSpin(string label, float min, float max, float value, out SpinButton spin, bool vertical = false, Action<float>? changed_event = null)
    {
        spin = new SpinButton()
        {
            Value = value,
            Minimum = min,
            Maximum = max,
            Width = 100
        };

        spin.ValueChanged += (s, e) =>
        {
            if (s is SpinButton sb)
            {
                changed_event?.Invoke(sb.Value != null ? sb.Value.Value : 0);
            }
        };

        AddSetter(label, spin, vertical);
    }

    public void AddFilePicker(string label, string? file_path, string? current_path, out TextBox txt, bool vertical = false, Action<string?>? changed_event = null)
    {
        var p = new HorizontalStackPanel()
        {
            Spacing = 10
        };

        txt = new TextBox()
        {
            Text = file_path,
            Width = 200
        };
        p.Widgets.Add(txt);

        var btn = new Button()
        {
            Content = new Label() { Text = "..." },
            Tag = new Tuple<TextBox>(txt)
        };
        p.Widgets.Add(btn);

        btn.Click += (s, e) =>
        {
            if (s is Button btn)
            {
                var view = btn.Content;
                var t = (Tuple<TextBox>)btn.Tag;
                var dlg = new FileDialog(FileDialogMode.OpenFile);

                dlg.Folder = current_path;
                dlg.FilePath = file_path;
                dlg.ButtonOk.Width = 100;
                dlg.ButtonOk.Content.HorizontalAlignment = HorizontalAlignment.Center;
                dlg.ButtonCancel.Width = 100;
                dlg.ButtonCancel.Content.HorizontalAlignment = HorizontalAlignment.Center;

                dlg.Closed += (s, e) =>
                {
                    if (dlg.Result == true)
                    {
                        t.Item1.Text = dlg.FilePath;
                        changed_event?.Invoke(dlg.FilePath);
                    }
                };
                dlg.ShowModal(Desktop);
            }
        };

        AddSetter(label, p, vertical);
    }

    public void AddColorPicker(string label, Color? color, out Label view, bool vertical = false, bool allow_null = false, Action<Color?>? picked_event = null)
    {
        view = new Label()
        {
            Background = color != null ? new SolidBrush(color.Value) : null,
            Width = 20,
            Height = 20,
            HorizontalAlignment = HorizontalAlignment.Center,
            Tag = color == null ? Color.White : color.Value
        };

        var txhex = new Label();

        if (color.HasValue == true)
        {
            txhex.Text = color.Value.ToHexString();
        }
        else
        {
            txhex.Text = "";
        }

        var p = new HorizontalStackPanel()
        {
            Spacing = 10
        };

        var chk = new CheckButton()
        {
            IsVisible = allow_null == true,
            VerticalAlignment = VerticalAlignment.Center,
            IsChecked = color.HasValue,
            Tag = new Tuple<Label, Label>(view, txhex)
        };

        var btn = new Button()
        {
            Content = view,
            Tag = new Tuple<Label, CheckButton>(txhex, chk),
            MouseCursor = MouseCursorType.Hand
        };

        p.Widgets.Add(chk);
        p.Widgets.Add(btn);
        p.Widgets.Add(txhex);

        chk.IsCheckedChanged += (s, e) =>
        {
            if (s is CheckButton c)
            {
                var t = (Tuple<Label, Label>)c.Tag;

                if (c.IsChecked == true)
                {
                    var clr = (Color)t.Item1.Tag;
                    t.Item1.Background = new SolidBrush(clr);
                    t.Item2.Text = clr.ToHexString();
                }
                else
                {
                    t.Item1.Background = null;
                    t.Item2.Text = "";
                }
            }
        };

        btn.Click += (s, e) =>
        {
            if (s is Button btn)
            {
                var view = btn.Content;
                var t = (Tuple<Label, CheckButton>)btn.Tag;
                var dlg = new ColorPickerDialog();

                if (t.Item2.IsChecked == false)
                    return;

                dlg.ButtonOk.Width = 100;
                dlg.ButtonOk.Content.HorizontalAlignment = HorizontalAlignment.Center;
                dlg.ButtonCancel.Width = 100;
                dlg.ButtonCancel.Content.HorizontalAlignment = HorizontalAlignment.Center;

                dlg.Closed += (s, e) =>
                {
                    if (dlg.Result == true)
                    {
                        view.Background = new SolidBrush(dlg.Color);
                        view.Tag = dlg.Color;
                        t.Item1.Text = dlg.Color.ToHexString();

                        if (t.Item2.IsChecked == true)
                        {
                            picked_event?.Invoke(dlg.Color);
                        }
                    }
                };
                dlg.ShowModal(Desktop);
            }
        };

        AddSetter(label, p, vertical);
    }

    public void AddSeparator()
    {
        AddSetter("", new Label()
        {
            Height = 20
        }, false);
    }

    public void AddLabel(string label, string text, out Label w)
    {
        w = new Label()
        {
            Text = text
        };

        AddSetter(label, w, false);
    }

    private void AddSetter(string label, Widget control, bool vertical)
    {
        var lbl = new Label()
        {
            Text = label,
            VerticalAlignment = VerticalAlignment.Top
        };

        StackPanel p;

        if (vertical == false)
        {
            lbl.Width = _label_width;

            p = new HorizontalStackPanel()
            {
                Spacing = 10
            };
        }
        else
        {
            p = new VerticalStackPanel()
            {
                Spacing = 5
            };
        }

        p.Widgets.Add(lbl);
        p.Widgets.Add(control);
        Widgets.Add(p);
    }

    public void AddHeader(string text)
    {
        var lbl = new Label()
        {
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            TextAlignment = TextHorizontalAlignment.Center,
            Background = new SolidBrush("#404040"),
            Margin = new Thickness(2)
        };

        Widgets.Add(lbl);
    }
}

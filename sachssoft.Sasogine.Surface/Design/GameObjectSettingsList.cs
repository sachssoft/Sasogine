using Microsoft.Xna.Framework;
using System;
using sachssoft.Sasogine.Surface.Visuals;
using sachssoft.Sasogine.Surface.Visuals.Brushes;
using sachssoft.Sasogine.Surface.Visuals.Controls;
using sachssoft.Sasogine.Elements;
using sachssoft.Sasogine.Graphics.Colors;
using System.Collections.Generic;
using System.Diagnostics.Metrics;

namespace sachssoft.Sasogine.Surface.Design;

public sealed class GameObjectSettingsList<T> : VerticalStackPanel where T : GameObject
{
    private readonly Dictionary<string, Entry> _editors = new();
    private T? _source;

    public GameObjectSettingsList()
    {
        Spacing = 10;
        Padding = new Thickness(10);
    }

    public T? Source
    {
        get => _source;
        set
        {
            _source = value;
            Rebuild();
        }
    }

    public void AddCaption(string label)
    {
        AddSetter(new Entry()
        {
            Label = label,
            Type = EntryType.Caption
        });
    }

    public void AddSeparator()
    {
        AddSetter(new Entry()
        {
            Type = EntryType.Separator
        });
    }

    public void Reset()
    {
        _editors.Clear();
    }

    public void UpdateProperty(string property_name, object? new_value)
    {
        if (property_name == null)
            throw new ArgumentNullException(nameof(property_name));

        if (!_editors.TryGetValue(property_name, out var entry))
            return; // Property nicht gefunden, einfach ignorieren

        if (entry.Editor != null)
        {
            entry.Editor.Update(new_value);
            return;
        }
    }

    public void Add(
            string label,
            string property_name,
            PropertyEditorBase editor,
            Func<T, object?> getter,
            Action<T, object?> setter)
    {
        if (_source == null)
            return;

        editor.DisplayLabel = label;
        editor.Source = _source;

        var ctrl = editor.CreateControl((s, p) => { }, getter, setter);
        var entry = new Entry()
        {
            Label = label,
            Editor = editor,
            Control = _source == null ? null : ctrl,
            Getter = getter,
            Setter = setter
        };
        _editors[property_name] = entry;
        AddSetter(entry);
    }

    private void Rebuild()
    {
        Widgets.Clear();
        foreach (var entry in _editors.Values)
        {
            entry.Control = _source == null ? null : entry.Editor.CreateControl((s, p) => { }, entry.Getter, entry.Setter);

            AddSetter(entry);
        }
    }

    private void AddSetter(Entry entry)
    {
        if (entry.Type == EntryType.Property && _source == null)
            return;

        var panel = new VerticalStackPanel() { Spacing = 5 };

        if (entry.Type == EntryType.Property && entry.Control == null)
            entry.Control = entry.Editor!.CreateControl((s, p) => { }, entry.Getter!, entry.Setter!);

        if (entry.Editor != null)
        {
            if (entry.Editor.IsDisplayLabelVisibilty)
                panel.Widgets.Add(new Label() { Text = entry.Label, HorizontalAlignment = HorizontalAlignment.Stretch });

            if (entry.Control != null)
            {
                entry.Control.HorizontalAlignment = HorizontalAlignment.Stretch;
                panel.Widgets.Add(entry.Control);
            }
        }

        Widgets.Add(panel);
    }

    //[Obsolete]
    //public void AddColorPicker(string label, Color? color, out Label view, bool vertical = false, bool allow_null = false, Action<Color?>? picked_event = null)
    //{
    //    view = new Label()
    //    {
    //        Background = color != null ? new SolidBrush(color.Value) : null,
    //        Width = 20,
    //        Height = 20,
    //        HorizontalAlignment = HorizontalAlignment.Center,
    //        Tag = color == null ? Color.White : color.Value
    //    };

    //    var txhex = new Label();

    //    if (color.HasValue == true)
    //    {
    //        txhex.Text = color.Value.ToHex();
    //    }
    //    else
    //    {
    //        txhex.Text = "";
    //    }

    //    var p = new HorizontalStackPanel()
    //    {
    //        Spacing = 10
    //    };

    //    var chk = new CheckButton()
    //    {
    //        Visible = (allow_null == true),
    //        VerticalAlignment = VerticalAlignment.Center,
    //        IsChecked = color.HasValue,
    //        Tag = new Tuple<Label, Label>(view, txhex)
    //    };

    //    var btn = new Button()
    //    {
    //        Content = view,
    //        Tag = new Tuple<Label, CheckButton>(txhex, chk),
    //        MouseCursor = MouseCursorType.Hand
    //    };

    //    p.Widgets.Add(chk);
    //    p.Widgets.Add(btn);
    //    p.Widgets.Add(txhex);

    //    chk.IsCheckedChanged += (s, e) =>
    //    {
    //        if (s is CheckButton c)
    //        {
    //            var t = (Tuple<Label, Label>)c.Tag;

    //            if (c.IsChecked == true)
    //            {
    //                var clr = (Color)t.Item1.Tag;
    //                t.Item1.Background = new SolidBrush(clr);
    //                t.Item2.Text = clr.ToHex();
    //            }
    //            else
    //            {
    //                t.Item1.Background = null;
    //                t.Item2.Text = "";
    //            }
    //        }
    //    };

    //    btn.Click += (s, e) =>
    //    {
    //        if (s is Button btn)
    //        {
    //            var view = btn.Content;
    //            var t = (Tuple<Label, CheckButton>)btn.Tag;
    //            var dlg = new ColorPickerDialog();

    //            if (t.Item2.IsChecked == false)
    //                return;

    //            dlg.ButtonOk.Width = 100;
    //            dlg.ButtonOk.Content.HorizontalAlignment = HorizontalAlignment.Center;
    //            dlg.ButtonCancel.Width = 100;
    //            dlg.ButtonCancel.Content.HorizontalAlignment = HorizontalAlignment.Center;

    //            dlg.Closed += (s, e) =>
    //            {
    //                if (dlg.Result == true)
    //                {
    //                    view.Background = new SolidBrush(dlg.Color);
    //                    view.Tag = dlg.Color;
    //                    t.Item1.Text = dlg.Color.ToHex();

    //                    if (t.Item2.IsChecked == true)
    //                    {
    //                        picked_event?.Invoke(dlg.Color);
    //                    }
    //                }
    //            };
    //            dlg.ShowModal(Desktop);
    //        }
    //    };

    //    AddSetter(label, p, vertical);
    //}

    private enum EntryType
    {
        Property,
        Caption,
        Separator
    }

    private class Entry
    {
        public EntryType Type;
        public string? Label;
        public PropertyEditorBase? Editor;
        public Widget? Control;
        public Func<T, object?>? Getter;
        public Action<T, object?>? Setter;
    }
}

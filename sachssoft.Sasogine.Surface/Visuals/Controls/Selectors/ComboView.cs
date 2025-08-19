using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Sachssoft.Sasogine.Surface.Attributes;
using Sachssoft.Sasogine.Surface.Events;
using Sachssoft.Sasogine.Surface.Visuals.Styles;

namespace Sachssoft.Sasogine.Surface.Visuals.Controls;

public class ComboBox : Widget
{
    private readonly ToggleButton _button;
    private readonly ListView _listView = new ListView(null);
    private IList<Widget>? _widget_source;

    [Category("Behavior")]
    [DefaultValue(300)]
    public int? DropdownMaximumHeight
    {
        get
        {
            return _listView.MaxHeight;
        }

        set
        {
            _listView.MaxHeight = value;
        }
    }

    [Browsable(false)]
    [XmlIgnore]
    public bool IsExpanded => _button.IsPressed;

    public override Desktop Desktop
    {
        get
        {
            return base.Desktop;
        }

        internal set
        {
            if (Desktop != null)
            {
                Desktop.ContextMenuClosed -= DesktopOnContextMenuClosed;
            }

            base.Desktop = value;

            if (Desktop != null)
            {
                Desktop.ContextMenuClosed += DesktopOnContextMenuClosed;
            }
        }
    }

    [Browsable(false)]
    [XmlIgnore]
    public ListView ListView => _listView;

    [Content]
    [Browsable(false)]
    public IList<Widget> Widgets => _listView.Widgets;

    [Browsable(false)]
    [XmlIgnore]
    public Widget SelectedItem
    {
        get => _listView.SelectedItem;
        set => _listView.SelectedItem = value;
    }

    [Category("Behavior")]
    [DefaultValue(SelectionMode.Single)]
    public SelectionMode SelectionMode
    {
        get => _listView.SelectionMode;
        set => _listView.SelectionMode = value;
    }

    [Browsable(false)]
    [XmlIgnore]
    public int? SelectedIndex
    {
        get => _listView.SelectedIndex;
        set => _listView.SelectedIndex = value;
    }

    public event EventHandler SelectedIndexChanged
    {
        add
        {
            _listView.SelectedIndexChanged += value;
        }

        remove
        {
            _listView.SelectedIndexChanged -= value;
        }
    }

    public ComboBox(string styleName = Stylesheet.DefaultStyleName)
    {
        _button = new ToggleButton(null)
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Content = new Label
            {
                Text = string.Empty
            }
        };

        ChildrenLayout = new SingleItemLayout<ToggleButton>(this)
        {
            Child = _button
        };

        AcceptsKeyboardFocus = true;

        _button.PressedChanged += InternalChild_PressedChanged;

        _listView._parentCombo = this;

        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Top;

        DropdownMaximumHeight = 300;

        SetStyle(styleName);
    }

    private void DesktopOnContextMenuClosed(object sender, GenericEventArgs<Widget> genericEventArgs)
    {
        // Unpress the button only if mouse is outside
        // As if it is inside, then it'll get unpressed naturally
        if (!IsMouseInside)
        {
            _button.IsPressed = false;
        }
    }

    private void InternalChild_PressedChanged(object sender, EventArgs e)
    {
        if (_listView.Widgets.Count == 0)
        {
            return;
        }

        if (_button.IsPressed)
        {
            if (_listView.SelectedIndex == null && Widgets.Count > 0)
            {
                _listView.SelectedIndex = 0;
            }

            _listView.Width = BorderBounds.Width;
            var pos = ToGlobal(new Point(0, Bounds.Height));
            Desktop.ShowContextMenu(_listView, pos);
        }
    }

    internal void UpdateSelectedItem()
    {
        _button.Content = SelectedItem.Clone();
    }

    public void ApplyComboViewStyle(ComboBoxStyle style)
    {
        if (style.ListBoxStyle != null)
        {
            var dropdownMaximumHeight = DropdownMaximumHeight;
            _listView.ApplyListBoxStyle(style.ListBoxStyle);
            DropdownMaximumHeight = dropdownMaximumHeight;
        }

        _button.ApplyButtonStyle(style);
    }

    protected override Point InternalMeasure(Point availableSize)
    {
        // Measure by the longest string
        var result = base.InternalMeasure(availableSize);

        // Temporary remove width, so it wont be used in the measure
        var oldWidth = _listView.Width;
        _listView.Width = null;

        // Make visible, otherwise Measure will return zero
        var wasVisible = _listView.IsVisible;
        _listView.IsVisible = true;

        var listResult = _listView.Measure(new Point(10000, 10000));
        if (listResult.X > result.X)
        {
            result.X = listResult.X;
        }

        // Revert ListBox settings
        _listView.Width = oldWidth;
        _listView.IsVisible = wasVisible;

        // Add some x space
        result.X += 32;

        return result;
    }

    protected override void InternalArrange()
    {
        base.InternalArrange();

        _listView.Width = BorderBounds.Width;
    }

    public override void OnKeyDown(Keys k)
    {
        base.OnKeyDown(k);

        _listView.OnKeyDown(k);
    }

    protected override void InternalSetStyle(Stylesheet stylesheet, string name)
    {
        ApplyComboViewStyle(stylesheet.ComboBoxStyles.SafelyGetStyle(name));
    }

    protected internal override void CopyFrom(Widget w)
    {
        base.CopyFrom(w);

        var comboView = (ComboBox)w;
        SelectionMode = comboView.SelectionMode;
        DropdownMaximumHeight = comboView.DropdownMaximumHeight;

        foreach (var child in comboView.Widgets)
        {
            Widgets.Add(child.Clone());
        }
    }
}

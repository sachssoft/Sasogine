using Microsoft.Xna.Framework.Input;
using System;
using System.ComponentModel;
using sachssoft.Sasogine.Surface.Attributes;
using sachssoft.Sasogine.Surface.Visuals.Styles;

namespace sachssoft.Sasogine.Surface.Visuals.Controls;

[StyleTypeName("Button")]
public class ToggleButton : ButtonBase2
{
    private readonly SingleItemLayout<Widget> _layout;

    [Category("Behavior")]
    [DefaultValue(false)]
    public bool IsToggled
    {
        get => IsPressed;
        set => IsPressed = value;
    }


    [Browsable(false)]
    [Content]
    public override Widget Content
    {
        get => _layout.Child;
        set => _layout.Child = value;
    }

    public event EventHandler IsToggledChanged
    {
        add
        {
            PressedChanged += value;
        }

        remove
        {
            PressedChanged -= value;
        }
    }

    public ToggleButton(string styleName = Stylesheet.DefaultStyleName)
    {
        _layout = new SingleItemLayout<Widget>(this);
        ChildrenLayout = _layout;
        SetStyle(styleName);
    }

    protected override void InternalOnTouchUp()
    {
    }

    protected override void InternalOnTouchDown()
    {
        SetValueByUser(!IsPressed);
    }

    public override void OnKeyDown(Keys k)
    {
        base.OnKeyDown(k);

        if (!IsEnabled)
        {
            return;
        }

        if (k == Keys.Space)
        {
            SetValueByUser(!IsPressed);
        }
    }

    protected override void InternalSetStyle(Stylesheet stylesheet, string name)
    {
        ApplyButtonStyle(stylesheet.ButtonStyles.SafelyGetStyle(name));
    }
}

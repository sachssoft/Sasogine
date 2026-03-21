using Microsoft.Xna.Framework.Input;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;
using System;

namespace Sachssoft.Sasogine.Surface.Controls;

public class Button : ButtonBase
{
    //private readonly SingleItemLayout<Widget> _layout;
    internal bool ReleaseOnTouchLeft;

    public Button()
    {
        Width = Width.Override(100);
        Height = Width.Override(25);

        //_layout = new SingleItemLayout<Widget>(this);
        //ChildrenLayout = _layout;
        ReleaseOnTouchLeft = true;
    }

    #region Direct Properties 

    public override Desktop Desktop
    {
        get => base.Desktop;
        internal set
        {
            // If we're not releasing the button on touch left,
            // we have to do it on touch up
            if (!ReleaseOnTouchLeft && Desktop != null)
            {
                Desktop.TouchUp -= DesktopTouchUp;
            }

            base.Desktop = value;

            if (!ReleaseOnTouchLeft && Desktop != null)
            {
                Desktop.TouchUp += DesktopTouchUp;
            }
        }
    }

    #endregion

    internal protected override void OnTouchLeft()
    {
        base.OnTouchLeft();

        if (ReleaseOnTouchLeft)
        {
            SetValueByUser(false);
        }
    }

    protected override void InternalOnTouchUp()
    {
        SetValueByUser(false);
    }

    protected override void InternalOnTouchDown()
    {
        SetValueByUser(true);
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
            // Emulate click
            //DoClick();
        }
    }

    private void DesktopTouchUp(object? sender, EventArgs args)
    {
        IsPressed = false;
    }

    #region Style

    public override void ApplyFromStyle(Style? style)
    {
        base.ApplyFromStyle(style);

        // Style für das Content-Label anwenden
        var contentLabelStyle = style?.FindStyle(typeof(Label), "content");
        if (ContentPresenter is Label label)
            label.ApplyFromStyle(contentLabelStyle);

        // Button-spezifische Properties
        style?.Apply(this, (target, sheet, property, value) =>
        {
            switch (property)
            {
                case nameof(Background):
                    target.Background = target.Background.Override(new RegionBrush(sheet.FindRegion(value.RawValue)));
                    break;
                case nameof(HoveredBackground):
                    target.HoveredBackground = target.HoveredBackground.Override(new RegionBrush(sheet.FindRegion(value.RawValue)));
                    break;
                case nameof(PressedBackground):
                    target.PressedBackground = target.PressedBackground.Override(new RegionBrush(sheet.FindRegion(value.RawValue)));
                    break;
            }
        });
    }

    public override void ApplyFrom(ElementBase e)
    {
        base.ApplyFrom(e);

        if (e is not Button source)
            return;

        // Zustand & Interaktionslogik übernehmen
        ReleaseOnTouchLeft = source.ReleaseOnTouchLeft;

        // Hintergrund-Styles
        Background = source.Background;
        HoveredBackground = source.HoveredBackground;
        PressedBackground = source.PressedBackground;

        // Weitere ButtonBase-Eigenschaften
        IsPressed = source.IsPressed;
    }

    protected override ElementBase CreateCloneInstance()
    {
        return new Button();
    }

    #endregion
}
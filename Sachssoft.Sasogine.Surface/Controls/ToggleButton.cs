using Microsoft.Xna.Framework.Input;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;
using System;

namespace Sachssoft.Sasogine.Surface.Controls;

public class ToggleButton : CheckableButtonBase
{
    private readonly SingleItemLayout<Widget> _layout;
    internal bool ReleaseOnTouchLeft;

    public ToggleButton()
    {
        Width = Width.Override(100);
        Height = Width.Override(25);

        _layout = new SingleItemLayout<Widget>(this);
        LayoutContainer  = _layout;
        ReleaseOnTouchLeft = true;
        //UpdateChildren();
    }

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

    public override void ApplyFrom(ElementBase other)
    {
        base.ApplyFrom(other);

        if (other is not ToggleButton toggleButton)
            return;

        // Grundlegende Layout- und Größeinstellungen
        Width = toggleButton.Width;
        Height = toggleButton.Height;

        // Toggle-spezifische Properties
        IsPressed = toggleButton.IsPressed;
        ReleaseOnTouchLeft = toggleButton.ReleaseOnTouchLeft;

        // Optional: Übernehme Presenter-Inhalt (z.B. Label)
        if (ContentPresenter is Label label && toggleButton.ContentPresenter is Label sourceLabel)
        {
            label.ApplyFrom(sourceLabel);
        }
    }

    protected override ElementBase CreateCloneInstance()
    {
        return new ToggleButton();
    }

    #endregion
}

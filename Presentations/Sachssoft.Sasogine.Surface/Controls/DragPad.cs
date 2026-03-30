using System;
using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Behaviors;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Styles;

namespace Sachssoft.Sasogine.Surface.Controls;

public class DragPad : Widget
{


    private readonly SingleItemLayout<Button> _layout;

    private Vector2 _value;

    public Vector2 Minimum { get; set; } = Vector2.Zero;

    public Vector2 Maximum { get; set; } = new Vector2(100f, 100f);

    public DragPadArea Area { get; set; } = DragPadArea.Quad;

    public Vector2 Value
    {
        get => _value;
        set
        {
            var clamped = ClampVector(value, Minimum, Maximum);

            if (_value == clamped)
                return;

            var oldValue = _value;
            _value = clamped;

            SyncHintWithValue();

            ValueChanged?.Invoke(this, new ValueChangedEventArgs<Vector2>(oldValue, _value));
        }
    }

    internal Vector2 Hint
    {
        get => new Vector2(ImageButton.Left, ImageButton.Top);
        set
        {
            var current = Hint;
            if (current == value)
                return;

            ImageButton.Left = (int)value.X;
            ImageButton.Top = (int)value.Y;
        }
    }

    internal Vector2 MaxHint
    {
        get
        {
            return new Vector2(
                Bounds.Width - ImageButton.Bounds.Width,
                Bounds.Height - ImageButton.Bounds.Height
            );
        }
    }

    public override Desktop Desktop
    {
        get => base.Desktop;
        internal set
        {
            if (Desktop != null)
                Desktop.TouchMoved -= DesktopTouchMoved;

            base.Desktop = value;

            if (Desktop != null)
                Desktop.TouchMoved += DesktopTouchMoved;
        }
    }

    public Button ImageButton => _layout.Child;

    public event EventHandler<ValueChangedEventArgs<Vector2>> ValueChanged;
    public event EventHandler<ValueChangedEventArgs<Vector2>> ValueChangedByUser;

    protected DragPad()
    {
        _layout = new SingleItemLayout<Button>(this)
        {
            Child = new Button()
            {
                Content = new Image(),
                ReleaseOnTouchLeft = false
            }
        };

        LayoutContainer  = _layout;
    }

    private Vector2 GetHint()
    {
        if (Desktop == null)
            return Vector2.Zero;

        var pos = ToLocal(Desktop.TouchPosition.Value);
        var bounds = ImageButton.ActualBounds;

        return new Vector2(
            pos.X - bounds.Width / 2,
            pos.Y - bounds.Height / 2
        );
    }

    //public void ApplyDragPadStyle(SliderStyle style)
    //{
    //    ApplyWidgetStyle(style);

    //    if (style.KnobStyle != null)
    //    {
    //        ImageButton.ApplyButtonStyle(style.KnobStyle);

    //        if (style.KnobStyle.ImageStyle != null)
    //        {
    //            var image = (Image)ImageButton.Content;
    //            // image.ApplyPressableImageStyle(style.KnobStyle.ImageStyle);
    //        }
    //    }
    //}

    private void SyncHintWithValue()
    {
        var max = MaxHint;
        var range = Maximum - Minimum;

        var relX = range.X == 0 ? 0f : (_value.X - Minimum.X) / range.X;
        var relY = range.Y == 0 ? 0f : (_value.Y - Minimum.Y) / range.Y;

        Hint = new Vector2(max.X * relX, max.Y * relY);
    }

    protected override void InternalArrange()
    {
        base.InternalArrange();
        SyncHintWithValue();
    }

    internal protected override void OnTouchDown()
    {
        base.OnTouchDown();
        UpdateHint();
        ImageButton.IsPressed = true;
    }

    private void UpdateHint()
    {
        var hint = GetHint();
        var max = MaxHint;

        hint.X = float.Clamp(hint.X, 0, max.X);
        hint.Y = float.Clamp(hint.Y, 0, max.Y);

        if (Area == DragPadArea.Circle)
        {
            // Zentrum
            var center = max / 2f;
            var offset = hint - center;
            var radius = float.Min(max.X, max.Y) / 2f;

            if (offset.Length() > radius)
            {
                offset.Normalize();
                offset *= radius;
            }

            hint = center + offset;
        }

        var oldValue = _value;
        var valueChanged = false;

        var range = Maximum - Minimum;

        var newValue = new Vector2(
            range.X == 0 ? Minimum.X : Minimum.X + hint.X * range.X / max.X,
            range.Y == 0 ? Minimum.Y : Minimum.Y + hint.Y * range.Y / max.Y
        );

        if (_value != newValue)
        {
            _value = newValue;
            valueChanged = true;
        }

        Hint = hint;

        if (valueChanged)
        {
            ValueChanged?.Invoke(this, new ValueChangedEventArgs<Vector2>(oldValue, _value));
            ValueChangedByUser?.Invoke(this, new ValueChangedEventArgs<Vector2>(oldValue, _value));
        }
    }

    private void DesktopTouchMoved(object sender, EventArgs args)
    {
        if (!ImageButton.IsPressed)
            return;

        UpdateHint();
    }

    #region Style

    public override void ApplyFrom(ElementBase other)
    {
        base.ApplyFrom(other);

        if (other is not DragPad source)
            return;

        Minimum = source.Minimum;
        Maximum = source.Maximum;
        Value = source.Value;
    }

    public override void ApplyFromStyle(Style? style)
    {
        base.ApplyFromStyle(style);
    }

    protected override ElementBase CreateCloneInstance()
    {
        return new DragPad();
    }

    #endregion

    private static Vector2 ClampVector(Vector2 v, Vector2 min, Vector2 max)
    {
        return new Vector2(
            float.Clamp(v.X, min.X, max.X),
            float.Clamp(v.Y, min.Y, max.Y)
        );
    }
}
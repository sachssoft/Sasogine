using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Behaviors;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Sachssoft.Sasogine.Surface.Controls;

public class Slider : RangeBase
{
    private StyleProperty<int?> _thumbWidth = new StyleProperty<int?>(null, isUserSet: false);
    private StyleProperty<int?> _thumbHeight = new StyleProperty<int?>(null, isUserSet: false);
    private StyleProperty<Orientation> _orientation = new StyleProperty<Orientation>(Visuals.Orientation.Horizontal, isUserSet: false);

    private readonly Button _thumbButton = new();
    //private readonly Image _thumbImage = new();
    private readonly SingleItemLayout<Button> _layout;

    public Slider()
    {
        //_thumbButton.Content = _thumbImage;
        _thumbButton.ReleaseOnTouchLeft = false;
        //_thumbButton.Background = Color.Red.ToBrush();
        //_thumbButton.HoveredBackground = null;
        //_thumbButton.PressedBackground = null;
        //_thumbButton.FocusedBackground = null;

        _layout = new SingleItemLayout<Button>(this)
        {
            Child = _thumbButton
        };

        LayoutContainer  = _layout;

        HorizontalAlignment = HorizontalAlignment.Override(Visuals.HorizontalAlignment.Left);
        VerticalAlignment = VerticalAlignment.Override(Visuals.VerticalAlignment.Top);

        UpdateOrientation();
    }

    #region Style Properties

    public StyleProperty<int?> ThumbWidth
    {
        get => _thumbWidth;
        set
        {
            if (SetAndNotify(ref _thumbWidth, value))
            {
                InvalidateMeasure();
            }
        }
    }

    public StyleProperty<int?> ThumbHeight
    {
        get => _thumbHeight;
        set
        {
            if (SetAndNotify(ref _thumbHeight, value))
            {
                InvalidateMeasure();
            }
        }
    }

    public StyleProperty<object?> ThumbContent
    {
        get => _thumbButton.Content;
        set
        {
            if (!EqualityComparer<StyleProperty<object?>>.Default.Equals(_thumbButton.Content, value))
            {
                OnPropertyChanging();
                _thumbButton.Content = value;
                OnPropertyChanged();
            }
        }
    }

    public StyleProperty<Orientation> Orientation
    {
        get => _orientation;
        set
        {
            if (SetAndNotify(ref _orientation, value))
            {
                InvalidateMeasure();
            }
        }
    }

    #endregion

    #region Direct Properties

    internal int Hint
    {
        get => Orientation.Value == Visuals.Orientation.Horizontal ? ImageButton.Left : ImageButton.Top;
        set
        {
            if (Hint == value)
                return;

            if (Orientation.Value == Visuals.Orientation.Horizontal)
            {
                ImageButton.Left = value;
            }
            else
            {
                ImageButton.Top = value;
            }
        }
    }

    internal int MaxHint
    {
        get
        {
            return Orientation.Value == Visuals.Orientation.Horizontal
                ? Bounds.Width - ImageButton.Bounds.Width
                : Bounds.Height - ImageButton.Bounds.Height;
        }
    }

    public override Desktop Desktop
    {
        get => base.Desktop;
        internal set
        {
            if (Desktop != null)
            {
                Desktop.TouchMoved -= DesktopTouchMoved;
            }

            base.Desktop = value;

            if (Desktop != null)
            {
                Desktop.TouchMoved += DesktopTouchMoved;
            }
        }
    }

    #endregion

    public Button ImageButton => _thumbButton;

    private int GetHint()
    {
        if (Desktop == null)
            return 0;

        var pos = ToLocal(Desktop.TouchPosition.GetValueOrDefault());
        var bounds = ImageButton.ActualBounds;

        return Orientation == Visuals.Orientation.Horizontal ?
            pos.X - bounds.Width / 2 : pos.Y - bounds.Height / 2;
    }

    internal override void OnValueChanged(ValueChangedEventArgs<float> e)
    {
        base.OnValueChanged(e);
        SyncHintWithValue();
    }

    private void SyncHintWithValue()
    {
        Hint = (int)(MaxHint * ((Value.Value - Minimum.Value) / (Maximum.Value - Minimum.Value)));
    }

    protected override void InternalArrange()
    {
        base.InternalArrange();

        //_thumbButton.Width = ThumbWidth.Value.HasValue ? ThumbWidth.Value.Value : _thumbImage.Bounds.Width;
        //_thumbButton.Height = ThumbHeight.Value.HasValue ? ThumbHeight.Value.Value : _thumbImage.Bounds.Height;

        if (Orientation.Value == Visuals.Orientation.Horizontal)
        {
            _thumbButton.Height = int.Max(_thumbButton.Height.Value.GetValueOrDefault(), Bounds.Height);
            //_thumbImage.Height = _thumbButton.Height.Value;
        }
        else
        {
            _thumbButton.Width = int.Max(_thumbButton.Width.Value.GetValueOrDefault(), Bounds.Width);
            //_thumbImage.Width = _thumbButton.Width.Value;
        }

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
        if (hint < 0)
        {
            hint = 0;
        }

        if (hint > MaxHint)
        {
            hint = MaxHint;
        }

        var oldValue = Value.Value;
        var valueChanged = false;
        // Sync Value with Hint
        if (MaxHint != 0)
        {
            var d = Maximum - Minimum;

            var newValue = Minimum + hint * d / MaxHint;
            if (Value.Value != newValue)
            {
                Value = newValue;
                valueChanged = true;
            }
        }

        Hint = hint;

        if (valueChanged)
        {
            OnValueChanged(new ValueChangedEventArgs<float>(oldValue, Value.Value));
            OnValueChangedByUser(new ValueChangedEventArgs<float>(oldValue, Value.Value));
        }
    }

    private void DesktopTouchMoved(object? sender, EventArgs args)
    {
        if (!ImageButton.IsPressed)
        {
            return;
        }

        UpdateHint();
    }

    #region Style

    public override void ApplyFromStyle(Style? style)
    {
        base.ApplyFromStyle(style);

        //var thumbStyle = style?.FindStyle(typeof(Image));
        //_thumbImage.ApplyFromStyle(thumbStyle);

        var thumbStyle = style?.FindStyle(typeof(Button));
        _thumbButton.ApplyFromStyle(thumbStyle);

        style?.Apply(this, (target, sheet, property, value) =>
        {
            switch (property)
            {
                case nameof(Value):
                    target.Value = target.Value.Override(value.ConvertTo<float>());
                    break;

                case nameof(Minimum):
                    target.Minimum = target.Minimum.Override(value.ConvertTo<float>());
                    break;

                case nameof(Maximum):
                    target.Maximum = target.Maximum.Override(value.ConvertTo<float>());
                    break;

                case nameof(Orientation):
                    target.Orientation = target.Orientation.Override(value.ConvertToEnum<Orientation>());
                    break;

                case nameof(ThumbWidth):
                    target.ThumbWidth = target.ThumbWidth.Override(value.ConvertTo<int>());
                    break;

                case nameof(ThumbHeight):
                    target.ThumbHeight = target.ThumbHeight.Override(value.ConvertTo<int>());
                    break;
            }
        });
    }

    public override void ApplyFrom(ElementBase other)
    {
        base.ApplyFrom(other);

        if (other is not Slider slider)
            return;

        // Slider-spezifische Properties übernehmen
        Value = Value.Override(slider.Value);
        Minimum = Minimum.Override(slider.Minimum);
        Maximum = Maximum.Override(slider.Maximum);
        Orientation = Orientation.Override(slider.Orientation);
        ThumbWidth = ThumbWidth.Override(slider.ThumbWidth);
        ThumbHeight = ThumbHeight.Override(slider.ThumbHeight);
    }

    protected override ElementBase CreateCloneInstance()
    {
        return new Slider();
    }

    #endregion

    #region Helpers

    private void UpdateOrientation()
    {
        StyleId = Orientation.Value switch
        {
            Visuals.Orientation.Vertical => "vertical",
            _ => "horizontal"
        };
    }

    #endregion
}
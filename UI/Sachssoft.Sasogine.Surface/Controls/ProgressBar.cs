using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Utility;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Surface.Controls;

public class ProgressBar : RangeBase
{
    private StyleProperty<IBrush?> _filler = new StyleProperty<IBrush?>(null, isUserSet: false);
    private StyleProperty<bool> _isValueVisible = new StyleProperty<bool>(true, isUserSet: false);
    private StyleProperty<Orientation> _orientation = new StyleProperty<Orientation>(Visuals.Orientation.Horizontal, isUserSet: false);

    private readonly RichTextLayout _textLayout = new();

    public ProgressBar()
    {
        HorizontalAlignment = HorizontalAlignment.Override(Visuals.HorizontalAlignment.Left);
        VerticalAlignment = VerticalAlignment.Override(Visuals.VerticalAlignment.Top);
        TextHorizontalAlignment = TextHorizontalAlignment.Override(Visuals.HorizontalAlignment.Center);
        TextVerticalAlignment = TextVerticalAlignment.Override(Visuals.VerticalAlignment.Center);

        UpdateOrientation();
    }

    #region Style Properties

    public StyleProperty<Orientation> Orientation
    {
        get => _orientation;
        set
        {
            if (SetAndNotify(ref _orientation, value))
            {
                UpdateOrientation();
                InvalidateMeasure();
            }
        }
    }

    public StyleProperty<IBrush?> Filler
    {
        get => _filler;
        set => SetAndNotify(ref _filler, value);
    }

    public StyleProperty<bool> IsValueVisible
    {
        get => _isValueVisible;
        set => SetAndNotify(ref _isValueVisible, value);
    }

    #endregion

    protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        switch (propertyName)
        {
            case nameof(Font):
                UpdateTextLayout();
                break;
            case nameof(Value):
            case nameof(ValueFormat):
            case nameof(ValueFormatCulture):
            case nameof(Minimum):
            case nameof(Maximum):
                UpdateTextLayout();
                break;
        }
    }

    public override void InternalRender(RenderContext context, GameTime t)
    {
        base.InternalRender(context, t);

        var brush = Filler.Value;
        if (brush == null)
            return;

        float v = Value;
        if (v < Minimum) v = Minimum;
        if (v > Maximum) v = Maximum;

        float delta = Maximum - Minimum;
        if (delta.IsZero())
            return;

        float filledPart = (v - Minimum) / delta;
        if (filledPart.EpsilonEquals(0.0f))
            return;

        var bounds = ActualBounds;

        if (Orientation.Value == Visuals.Orientation.Horizontal)
        {
            // Horizontaler Fortschritt: von links nach rechts
            brush.Draw(context,
                new Rectangle(bounds.X, bounds.Y, (int)(filledPart * bounds.Width), bounds.Height),
                Color.White);

            if (IsValueVisible.Value)
            {
                var measure = _textLayout.Measure(null);

                float x = TextHorizontalAlignment.Value switch
                {
                    Visuals.HorizontalAlignment.Center => bounds.X + (bounds.Width - measure.X) / 2,
                    Visuals.HorizontalAlignment.Right => bounds.Right - measure.X,
                    _ => bounds.X, // Left
                };

                float y = TextVerticalAlignment.Value switch
                {
                    Visuals.VerticalAlignment.Center => bounds.Y + (bounds.Height - measure.Y) / 2,
                    Visuals.VerticalAlignment.Bottom => bounds.Bottom - measure.Y,
                    _ => bounds.Y, // Top
                };

                context.DrawRichText(_textLayout, new Vector2(x, y), TextColor.Value);
            }
        }
        else
        {
            // Vertikaler Fortschritt: von unten nach oben
            int filledHeight = (int)(filledPart * bounds.Height);
            brush.Draw(context,
                new Rectangle(bounds.X, bounds.Bottom - filledHeight, bounds.Width, filledHeight),
                Color.White);

            if (IsValueVisible.Value)
            {
                var measure = _textLayout.Measure(null);

                float x = TextHorizontalAlignment.Value switch
                {
                    Visuals.HorizontalAlignment.Center => bounds.X + (bounds.Width - measure.X) / 2,
                    Visuals.HorizontalAlignment.Right => bounds.Right - measure.X,
                    _ => bounds.X, // Left
                };

                float y = TextVerticalAlignment.Value switch
                {
                    Visuals.VerticalAlignment.Center => bounds.Y + (bounds.Height - measure.Y) / 2,
                    Visuals.VerticalAlignment.Bottom => bounds.Bottom - measure.Y,
                    _ => bounds.Y, // Top
                };

                context.DrawRichText(_textLayout, new Vector2(x, y), TextColor.Value);
            }
        }
    }

    #region Style

    public override void ApplyFromStyle(Style? style)
    {
        base.ApplyFromStyle(style);

        style?.Apply(this, (target, sheet, property, value) =>
        {
            switch (property)
            {
                case nameof(Filler):
                    var region = sheet.FindRegion(value.RawValue);
                    target.Filler = target.Filler.Override(region != null ? new RegionBrush(region) : null);
                    break;

                case nameof(IsValueVisible):
                    target.IsValueVisible = target.IsValueVisible.Override(value.ConvertTo<bool>());
                    break;

                case nameof(Orientation):
                    target.Orientation = target.Orientation.Override(value.ConvertToEnum<Orientation>());
                    break;
            }
        });
    }

    public override void ApplyFrom(ElementBase other)
    {
        base.ApplyFrom(other);

        if (other is not ProgressBar progressBar)
            return;

        Filler = progressBar.Filler;
        IsValueVisible = progressBar.IsValueVisible;
        Orientation = progressBar.Orientation;
    }

    protected override ElementBase CreateCloneInstance()
    {
        return new ProgressBar();
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

    private void UpdateTextLayout()
    {
        _textLayout.Text = ValueDisplay;
        _textLayout.Font = Font.Value.GetSpriteFont(Stylesheet.Current);
    }

    #endregion
}
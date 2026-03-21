using FontStashSharp;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using System;
using System.ComponentModel;
using Sachssoft.Sasogine.Surface.Utility;
using Sachssoft.Sasogine.Graphics.Colors;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Styles;
using System.Runtime.CompilerServices;
using Sachssoft.Sasogine.Surface.Basic;

namespace Sachssoft.Sasogine.Surface.Controls;

public class Label : Widget
{
    private StyleProperty<int> _verticalSpacing = new StyleProperty<int>(0, isUserSet: false);
    private StyleProperty<string?> _text = new StyleProperty<string?>(null, isUserSet: false);
    private StyleProperty<TextWrapMode> _wrapMode = new StyleProperty<TextWrapMode>(TextWrapMode.None, isUserSet: false);
    private StyleProperty<AutoEllipsisMethod> _autoEllipsisMethod = new StyleProperty<AutoEllipsisMethod>(FontStashSharp.RichText.AutoEllipsisMethod.None, isUserSet: false);
    private StyleProperty<string?> _autoEllipsisString = new StyleProperty<string?>("...", isUserSet: false);
    private StyleProperty<bool> _isPressed = new StyleProperty<bool>(false, isUserSet: false);

    private readonly RichTextLayout _richTextLayout;

    public Label()
    {
        _richTextLayout = new RichTextLayout()
        {
            SupportsCommands = true
        };

        TextHorizontalAlignment = TextHorizontalAlignment.Override(Visuals.HorizontalAlignment.Left);
        TextVerticalAlignment = TextVerticalAlignment.Override(Visuals.VerticalAlignment.Top);

        UpdateRichTextLayout();
    }

    #region Style Properties

    public StyleProperty<int> VerticalSpacing
    {
        get => _verticalSpacing;
        set
        {
            if (SetAndNotify(ref _verticalSpacing, value))
            {
                UpdateRichTextLayout();
                InvalidateMeasure();
            }
        }
    }

    public StyleProperty<string?> Text
    {
        get => _text;
        set
        {
            if (SetAndNotify(ref _text, value))
            {
                UpdateRichTextLayout();
                InvalidateMeasure();
            }
        }
    }

    public StyleProperty<TextWrapMode> WrapMode
    {
        get => _wrapMode;
        set
        {
            if (SetAndNotify(ref _wrapMode, value))
            {
                UpdateRichTextLayout();
                InvalidateMeasure();
            }
        }
    }

    public StyleProperty<AutoEllipsisMethod> AutoEllipsisMethod
    {
        get => _autoEllipsisMethod;
        set
        {
            if (SetAndNotify(ref _autoEllipsisMethod, value))
                UpdateRichTextLayout();
        }
    }

    public StyleProperty<string?> AutoEllipsisString
    {
        get => _autoEllipsisString;
        set
        {
            if (SetAndNotify(ref _autoEllipsisString, value))
                UpdateRichTextLayout();
        }
    }

    public StyleProperty<bool> IsPressed
    {
        get => _isPressed;
        set => SetAndNotify(ref _isPressed, value);
    }

    #endregion

    public override void InternalRender(RenderContext context, GameTime t)
    {
        if (_richTextLayout.Font == null)
            return;

        // Farbwahl je nach Zustand
        var color = TextColor.Value;
        var useChunkColor = true;

        if (!IsEffectiveEnabled)
        {
            color = DisabledTextColor.Value;
            useChunkColor = false;
        }
        else if (IsPressed.Value)
        {
            color = PressedTextColor.Value;
            useChunkColor = false;
        }
        else if (IsMouseInside)
        {
            color = OverTextColor.Value;
            useChunkColor = false;
        }

        _richTextLayout.IgnoreColorCommand = !useChunkColor;

        var bounds = ActualBounds;
        //var measure = _richTextLayout.Measure(null);

        float x = TextHorizontalAlignment.Value switch
        {
            Visuals.HorizontalAlignment.Center => bounds.X + bounds.Width / 2f,
            Visuals.HorizontalAlignment.Right => bounds.X + bounds.Right,
            _ => bounds.X
        };

        //float y = TextVerticalAlignment.Value switch
        //{
        //    Visuals.VerticalAlignment.Center => bounds.Y + (bounds.Height - measure.Y) / 2f,
        //    Visuals.VerticalAlignment.Bottom => bounds.Bottom - measure.Y,
        //    _ => bounds.Y
        //};

        context.DrawRichText(_richTextLayout, new Vector2(x, bounds.X), color, horizontalAlignment: TextHorizontalAlignment);
    }

    private void UpdateRichTextLayout()
    {
        // Kein Font verfügbar → nichts tun
        if (Font.Value == null)
            return;

        _richTextLayout.Text = _text.Value;
        _richTextLayout.Font = Font.Value.GetSpriteFont(Stylesheet.Current);
        _richTextLayout.VerticalSpacing = _verticalSpacing.Value;

        int? measureWidth = _wrapMode.Value switch
        {
            TextWrapMode.None => null,
            TextWrapMode.WordWrap or TextWrapMode.Ellipsis => ActualBounds.Width > 0 ? ActualBounds.Width : null,
            _ => null
        };

        _richTextLayout.Width = measureWidth;
        _richTextLayout.AutoEllipsisMethod = _autoEllipsisMethod.Value;
        _richTextLayout.AutoEllipsisString = _autoEllipsisString.Value;
    }

    protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        switch (propertyName)
        {
            case nameof(Font):
            case nameof(TextHorizontalAlignment):
            case nameof(TextVerticalAlignment):
                UpdateRichTextLayout();
                break;
        }
    }

    protected override Point InternalMeasure(Point availableSize)
    {
        if (Font.Value == null)
            return Mathematics.PointZero;

        int? measureWidth = _wrapMode.Value switch
        {
            TextWrapMode.None => null,
            TextWrapMode.WordWrap or TextWrapMode.Ellipsis => availableSize.X,
            _ => null
        };

        Point result = _richTextLayout.Measure(measureWidth);

        if (result.Y < _richTextLayout.Font.LineHeight)
            result.Y = _richTextLayout.Font.LineHeight;

        return result;
    }

    protected override void InternalArrange()
    {
        base.InternalArrange();

        int? measureWidth = _wrapMode.Value switch
        {
            TextWrapMode.None => null,
            TextWrapMode.WordWrap or TextWrapMode.Ellipsis => ActualBounds.Width,
            _ => null
        };

        _richTextLayout.Width = measureWidth;

        // Höhe: bei WordWrap gesamte Box, sonst nur LineHeight
        _richTextLayout.Height = _wrapMode.Value == TextWrapMode.WordWrap
            ? ActualBounds.Height
            : _richTextLayout.Font?.LineHeight ?? 0;

        UpdateRichTextLayout();
    }

    #region Style

    public override void ApplyFromStyle(Style? style)
    {
        base.ApplyFromStyle(style);

        style?.Apply<Label>(this, (target, sheet, property, value) =>
        {
            switch (property)
            {
                case nameof(TextColor):
                    target.TextColor = target.TextColor.Override(value.ConvertTo<Color>());
                    break;

                case nameof(DisabledTextColor):
                    target.DisabledTextColor = target.DisabledTextColor.Override(value.ConvertTo<Color>());
                    break;

                case nameof(OverTextColor):
                    target.OverTextColor = target.OverTextColor.Override(value.ConvertTo<Color>());
                    break;

                case nameof(PressedTextColor):
                    target.PressedTextColor = target.PressedTextColor.Override(value.ConvertTo<Color>());
                    break;

                case nameof(Font):
                    target.Font = target.Font.Override(sheet.GetFont(value.RawValue));
                    break;
            }
        });
    }

    public override void ApplyFrom(ElementBase other)
    {
        base.ApplyFrom(other);

        if (other is not Label source)
            return;

        VerticalSpacing = source.VerticalSpacing;
        Text = source.Text;
        Font = source.Font;
        WrapMode = source.WrapMode;
        AutoEllipsisMethod = source.AutoEllipsisMethod;
        AutoEllipsisString = source.AutoEllipsisString;
        TextHorizontalAlignment = source.TextHorizontalAlignment;
        TextColor = source.TextColor;
        DisabledTextColor = source.DisabledTextColor;
        OverTextColor = source.OverTextColor;
        PressedTextColor = source.PressedTextColor;
    }

    protected override ElementBase CreateCloneInstance()
    {
        return new Label();
    }

    #endregion
}
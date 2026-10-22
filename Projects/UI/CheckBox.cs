using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Engine.Controls;
/// <summary>A checkbox with an optional indeterminate state.</summary>
public class CheckBox : ToggleButton
{
    /// <summary>Initializes a new check box instance.</summary>
    public CheckBox() : base()
    {
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "CheckBox";
    /// <summary>Gets or sets is checked.</summary>
    public new bool? IsChecked { get => ConfigIsChecked; set => SetChecked(value); }
    /// <summary>Gets or sets is three state.</summary>
    public bool IsThreeState { get => ConfigIsThreeState; set => ConfigIsThreeState = value; }

    /// <inheritdoc/>
    protected override void ChangeCheckedState() => IsChecked = IsThreeState ? IsChecked is false ? true : IsChecked is true ? null : false : IsChecked != true;
    /// <summary>Gets whether layout reserves space for the selection glyph.</summary>
    protected virtual bool HasGlyph => true;

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        Size2 text = base.MeasureOverride(context, available);
        if (!HasGlyph)
            return text;
        Size2 glyph = ConfigGlyphSize;
        return new Size2(glyph.Width + (ContentPresenter is not null ? ConfigTextSpacing + text.Width : 0), MathF.Max(glyph.Height, text.Height));
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 bounds)
    {
        float offset = HasGlyph ? ConfigGlyphSize.Width + (ContentPresenter is not null ? ConfigTextSpacing : 0) : 0;
        base.ArrangeOverride(new Bounds2(bounds.X + offset, bounds.Y, MathF.Max(0, bounds.Width - offset), bounds.Height));
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        Size2 glyph = ConfigGlyphSize;
        float y = (Bounds.Height - glyph.Height) / 2;
        var box = new Bounds2(Padding.Left, y, glyph.Width, glyph.Height);
        if (!context.DrawSkinPart(this, "Glyph", box, SelectionState))
        {
            context.FillRectangle(box, IsChecked == true ? PressedBackground : Background);
            context.StrokeRectangle(box, TextColor, 1);
            if (IsChecked.HasValue && IsChecked.Value)
                context.DrawText(GetFont(context), "✓", new Vector2(box.X + 3, box.Y), TextColor);
            else if (!IsChecked.HasValue)
                context.FillRectangle(new Bounds2(box.X + 5, box.Y + glyph.Height / 2, glyph.Width - 10, 2), TextColor);
        }
    }

    private StyleProperty<bool> _styleIsThreeState = new(false);
    private static readonly System.ComponentModel.PropertyChangedEventArgs IsThreeStateChangedArgs = new(nameof(IsThreeState));
    internal bool ConfigIsThreeState
    {
        get => _styleIsThreeState.Value;
        set
        {
            if (_styleIsThreeState.SetLocal(value))
                NotifyPropertyChanged(IsThreeStateChangedArgs);
        }
    }

    private StyleProperty<Size2> _styleGlyphSize = new(new Size2(24));
    private static readonly System.ComponentModel.PropertyChangedEventArgs GlyphSizeChangedArgs = new(nameof(GlyphSize));
    internal Size2 ConfigGlyphSize
    {
        get => _styleGlyphSize.Value;
        set
        {
            if (_styleGlyphSize.SetLocal(value))
                NotifyPropertyChanged(GlyphSizeChangedArgs);
        }
    }

    /// <summary>Gets or sets GlyphSize; a local assignment overrides the style.</summary>
    public Size2 GlyphSize { get => ConfigGlyphSize; set => ConfigGlyphSize = value; }

    private StyleProperty<float> _styleTextSpacing = new(8f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs TextSpacingChangedArgs = new(nameof(TextSpacing));
    internal float ConfigTextSpacing
    {
        get => _styleTextSpacing.Value;
        set
        {
            if (_styleTextSpacing.SetLocal(value))
                NotifyPropertyChanged(TextSpacingChangedArgs);
        }
    }

    /// <summary>Gets or sets TextSpacing; a local assignment overrides the style.</summary>
    public float TextSpacing { get => ConfigTextSpacing; set => ConfigTextSpacing = value; }

    /// <summary>Applies this control's explicit style values without replacing local assignments.</summary>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case "IsThreeState":
                    if (_styleIsThreeState.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(IsThreeStateChangedArgs);
                    break;
                case "GlyphSize":
                    if (_styleGlyphSize.Apply(Converters.Convert<Size2>("Size2", entry.Value)))
                        NotifyPropertyChanged(GlyphSizeChangedArgs);
                    break;
                case "TextSpacing":
                    if (_styleTextSpacing.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(TextSpacingChangedArgs);
                    break;
            }
        }
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case "IsThreeState":
                return _styleIsThreeState.HasLocalValue;
            case "GlyphSize":
                return _styleGlyphSize.HasLocalValue;
            case "TextSpacing":
                return _styleTextSpacing.HasLocalValue;
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>Removes a local override and restores the current style value.</summary>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "IsThreeState":
                if (_styleIsThreeState.ClearLocal())
                    NotifyPropertyChanged(IsThreeStateChangedArgs);
                return;
            case "GlyphSize":
                if (_styleGlyphSize.ClearLocal())
                    NotifyPropertyChanged(GlyphSizeChangedArgs);
                return;
            case "TextSpacing":
                if (_styleTextSpacing.ClearLocal())
                    NotifyPropertyChanged(TextSpacingChangedArgs);
                return;
            default:
                base.ResetStyleProperty(property);
                return;
        }
    }

    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();
        if (_styleIsThreeState.BeginStyle(false))
            NotifyPropertyChanged(IsThreeStateChangedArgs);
        if (_styleGlyphSize.BeginStyle(new Size2(24)))
            NotifyPropertyChanged(GlyphSizeChangedArgs);
        if (_styleTextSpacing.BeginStyle(8f))
            NotifyPropertyChanged(TextSpacingChangedArgs);
    }

    /// <summary>Commits final style values and notifies once per effective property change.</summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleIsThreeState.CommitStyle())
            NotifyPropertyChanged(IsThreeStateChangedArgs);
        if (_styleGlyphSize.CommitStyle())
            NotifyPropertyChanged(GlyphSizeChangedArgs);
        if (_styleTextSpacing.CommitStyle())
            NotifyPropertyChanged(TextSpacingChangedArgs);
    }
}

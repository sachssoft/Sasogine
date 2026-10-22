using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Engine.Controls;
/// <summary>Displays text with cached measurement, wrapping, and placement.</summary>
public class TextBlock : Control
{
    /// <summary>Initializes a label.</summary>
    public TextBlock() : base()
    {
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "TextBlock";
    /// <summary>Gets the effective Text value or sets a local override.</summary>
    public string Text { get => ConfigText; set => ConfigText = value; }
    /// <summary>Gets the effective TextHorizontalAlignment value or sets a local override.</summary>
    public ControlAlignment TextHorizontalAlignment { get => ConfigTextHorizontalAlignment; set => ConfigTextHorizontalAlignment = value; }
    /// <summary>Gets the effective TextVerticalAlignment value or sets a local override.</summary>
    public ControlAlignment TextVerticalAlignment { get => ConfigTextVerticalAlignment; set => ConfigTextVerticalAlignment = value; }

    private string _displayText = string.Empty;
    private string? _measuredText;
    private IControlFont? _measuredFont;
    private Size2 _textSize;
    private Vector2 _textPosition;
    private float _wrapWidth;
    private bool _metricsValid, _measuredWrap;
    private bool _pendingTextMetrics;
    /// <inheritdoc/>
    protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs notification)
    {
        // Exact built-in types keep custom subclass measurement contracts unchanged.
        if (notification.PropertyName == nameof(Text) && Text is not null && _metricsValid && !WrapText && !_measuredWrap && Workspace is not null && (GetType() == typeof(TextBlock) || GetType() == typeof(Label)))
        {
            _pendingTextMetrics = true;
            Workspace.InvalidateLayout();
            return;
        }

        base.OnPropertyChanged(notification);
    }

    internal override void Prepare()
    {
        base.Prepare();
        if (!_pendingTextMetrics)
            return;
        _pendingTextMetrics = false;
        if (!_metricsValid || WrapText || _measuredFont is null)
        {
            InvalidateMeasure();
            return;
        }

        try
        {
            Size2 size = _measuredFont.Measure(Text);
            ValidateSize(size, false);
            bool unchanged = size.Width == _textSize.Width && size.Height == _textSize.Height;
            _displayText = _measuredText = Text;
            _textSize = size;
            if (!unchanged)
                InvalidateMeasure();
        }
        catch
        {
            InvalidateMeasure();
            throw;
        }
    }

    /// <inheritdoc/>
    protected override void OnFontChanged()
    {
        _metricsValid = false;
        base.OnFontChanged();
    }

    /// <summary>Invalidates layout and cached text metrics after an external font resource changes.</summary>
    public override void InvalidateLayout()
    {
        _metricsValid = false;
        base.InvalidateLayout();
    }

    /// <summary>Gets or sets wrap text.</summary>
    public bool WrapText { get => ConfigWrapText; set => ConfigWrapText = value; }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        ArgumentNullException.ThrowIfNull(Text);
        if (!IsAlignment(TextHorizontalAlignment) || !IsAlignment(TextVerticalAlignment))
            throw new InvalidOperationException("Invalid text alignment.");
        var font = GetFont(context);
        if (!_metricsValid || !ReferenceEquals(_measuredFont, font) || _measuredText != Text || _measuredWrap != WrapText || WrapText && _wrapWidth != available.Width)
        {
            _metricsValid = false;
            string display = WrapText ? ControlTextLayout.Wrap(font, Text, available.Width) : Text;
            Size2 size = font.Measure(display);
            ValidateSize(size, false);
            _displayText = display;
            _textSize = size;
            _measuredText = Text;
            _measuredFont = font;
            _measuredWrap = WrapText;
            _wrapWidth = available.Width;
            _metricsValid = true;
        }

        return _textSize;
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        base.OnDraw(context);
        DrawText(context);
    }

    /// <summary>Draws the current label text inside the padding using matching metrics.</summary>
    protected void DrawText(ControlDrawContext context)
    {
        context.DrawText(_measuredFont!, _displayText, _textPosition, IsAvailable() ? TextColor : DisabledTextColor);
    }

    /// <summary>Caches the local text position when layout assigns new content bounds.</summary>
    protected override void ArrangeOverride(Bounds2 contentBounds)
    {
        base.ArrangeOverride(contentBounds);
        _textPosition = new Vector2(Padding.Left + Offset(contentBounds.Width - _textSize.Width, TextHorizontalAlignment), Padding.Top + Offset(contentBounds.Height - _textSize.Height, TextVerticalAlignment));
    }

    private static float Offset(float spare, ControlAlignment alignment) => alignment switch
    {
        ControlAlignment.Center => spare / 2,
        ControlAlignment.End => spare,
        _ => 0
    };
    private StyleProperty<string> _styleText = new(string.Empty);
    private static readonly System.ComponentModel.PropertyChangedEventArgs TextChangedArgs = new(nameof(Text));
    internal string ConfigText
    {
        get => _styleText.Value;
        set
        {
            if (_styleText.SetLocal(value))
                NotifyPropertyChanged(TextChangedArgs);
        }
    }

    private StyleProperty<ControlAlignment> _styleTextHorizontalAlignment = new(ControlAlignment.Start);
    private static readonly System.ComponentModel.PropertyChangedEventArgs TextHorizontalAlignmentChangedArgs = new(nameof(TextHorizontalAlignment));
    internal ControlAlignment ConfigTextHorizontalAlignment
    {
        get => _styleTextHorizontalAlignment.Value;
        set
        {
            if (_styleTextHorizontalAlignment.SetLocal(value))
                NotifyPropertyChanged(TextHorizontalAlignmentChangedArgs);
        }
    }

    private StyleProperty<ControlAlignment> _styleTextVerticalAlignment = new(ControlAlignment.Center);
    private static readonly System.ComponentModel.PropertyChangedEventArgs TextVerticalAlignmentChangedArgs = new(nameof(TextVerticalAlignment));
    internal ControlAlignment ConfigTextVerticalAlignment
    {
        get => _styleTextVerticalAlignment.Value;
        set
        {
            if (_styleTextVerticalAlignment.SetLocal(value))
                NotifyPropertyChanged(TextVerticalAlignmentChangedArgs);
        }
    }

    private StyleProperty<bool> _styleWrapText = new(false);
    private static readonly System.ComponentModel.PropertyChangedEventArgs WrapTextChangedArgs = new(nameof(WrapText));
    internal bool ConfigWrapText
    {
        get => _styleWrapText.Value;
        set
        {
            if (_styleWrapText.SetLocal(value))
                NotifyPropertyChanged(WrapTextChangedArgs);
        }
    }

    /// <summary>Applies this control's explicit style values without replacing local assignments.</summary>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case "Text":
                    if (_styleText.Apply(Converters.Convert<string>("String", entry.Value)))
                        NotifyPropertyChanged(TextChangedArgs);
                    break;
                case "TextHorizontalAlignment":
                    if (_styleTextHorizontalAlignment.Apply(Converters.Convert<ControlAlignment>("ControlAlignment", entry.Value)))
                        NotifyPropertyChanged(TextHorizontalAlignmentChangedArgs);
                    break;
                case "TextVerticalAlignment":
                    if (_styleTextVerticalAlignment.Apply(Converters.Convert<ControlAlignment>("ControlAlignment", entry.Value)))
                        NotifyPropertyChanged(TextVerticalAlignmentChangedArgs);
                    break;
                case "WrapText":
                    if (_styleWrapText.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(WrapTextChangedArgs);
                    break;
            }
        }
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case "Text":
                return _styleText.HasLocalValue;
            case "TextHorizontalAlignment":
                return _styleTextHorizontalAlignment.HasLocalValue;
            case "TextVerticalAlignment":
                return _styleTextVerticalAlignment.HasLocalValue;
            case "WrapText":
                return _styleWrapText.HasLocalValue;
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>Removes a local override and restores the current style value.</summary>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "Text":
                if (_styleText.ClearLocal())
                    NotifyPropertyChanged(TextChangedArgs);
                return;
            case "TextHorizontalAlignment":
                if (_styleTextHorizontalAlignment.ClearLocal())
                    NotifyPropertyChanged(TextHorizontalAlignmentChangedArgs);
                return;
            case "TextVerticalAlignment":
                if (_styleTextVerticalAlignment.ClearLocal())
                    NotifyPropertyChanged(TextVerticalAlignmentChangedArgs);
                return;
            case "WrapText":
                if (_styleWrapText.ClearLocal())
                    NotifyPropertyChanged(WrapTextChangedArgs);
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
        if (_styleText.BeginStyle(string.Empty))
            NotifyPropertyChanged(TextChangedArgs);
        if (_styleTextHorizontalAlignment.BeginStyle(ControlAlignment.Start))
            NotifyPropertyChanged(TextHorizontalAlignmentChangedArgs);
        if (_styleTextVerticalAlignment.BeginStyle(ControlAlignment.Center))
            NotifyPropertyChanged(TextVerticalAlignmentChangedArgs);
        if (_styleWrapText.BeginStyle(false))
            NotifyPropertyChanged(WrapTextChangedArgs);
    }

    /// <summary>Commits final style values and notifies once per effective property change.</summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleText.CommitStyle())
            NotifyPropertyChanged(TextChangedArgs);
        if (_styleTextHorizontalAlignment.CommitStyle())
            NotifyPropertyChanged(TextHorizontalAlignmentChangedArgs);
        if (_styleTextVerticalAlignment.CommitStyle())
            NotifyPropertyChanged(TextVerticalAlignmentChangedArgs);
        if (_styleWrapText.CommitStyle())
            NotifyPropertyChanged(WrapTextChangedArgs);
    }
}

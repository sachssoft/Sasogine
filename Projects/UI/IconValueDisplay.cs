using System;
using System.ComponentModel;
using Sachssoft.Engine.Controls.Styling;
using DisplayPlacement = Sachssoft.Engine.Controls.IconPlacement;

namespace Sachssoft.Engine.Controls;
/// <summary>Displays an icon beside a cached, optionally formatted value, such as health, currency or elapsed time.</summary>
/// <remarks>Placement determines the layout axis. Private content hosts are reused throughout the display's lifetime.
/// Controls are presented directly; other objects are converted only when assigned or when formatting changes.</remarks>
public class IconValueDisplay : ContainerControl
{
    private readonly IconValuePresenter _iconHost = new();
    private readonly IconValuePresenter _valueHost = new();
    private object? _value;
    private IFormatProvider? _valueFormatProvider;
    private bool _formatDirty;
    private StyleProperty<IconPlacement> _placement = new(DisplayPlacement.Left);
    private StyleProperty<float> _spacing = new(8);
    private StyleProperty<string> _valueFormat = new(string.Empty);
    private static readonly PropertyChangedEventArgs IconArgs = new(nameof(Icon));
    private static readonly PropertyChangedEventArgs ValueArgs = new(nameof(Value));
    private static readonly PropertyChangedEventArgs IconPresenterArgs = new(nameof(IconPresenter));
    private static readonly PropertyChangedEventArgs ValuePresenterArgs = new(nameof(ValuePresenter));
    private static readonly PropertyChangedEventArgs PlacementArgs = new(nameof(IconPlacement));
    private static readonly PropertyChangedEventArgs SpacingArgs = new(nameof(Spacing));
    private static readonly PropertyChangedEventArgs FormatArgs = new(nameof(ValueFormat));
    private static readonly PropertyChangedEventArgs FormatProviderArgs = new(nameof(ValueFormatProvider));
    /// <summary>Creates an empty display with two persistent, privately owned content hosts.</summary>
    public IconValueDisplay()
    {
        base.Add(_iconHost);
        base.Add(_valueHost);
    }

    /// <summary>Gets or sets icon content. A control is presented directly; other objects use ToString. Null removes the icon.</summary>
    /// <remarks>Reassign a mutable object to refresh its text. Invalid ownership or conversion preserves the previous content.</remarks>
    public object? Icon
    {
        get => _iconHost.Content;
        set
        {
            VerifyTreeMutation();
            object? previous = Icon;
            Control? presenter = IconPresenter;
            _iconHost.Content = value;
            if (!ReferenceEquals(previous, value))
                NotifyPropertyChanged(IconArgs);
            if (!ReferenceEquals(presenter, IconPresenter))
                NotifyPropertyChanged(IconPresenterArgs);
        }
    }

    /// <summary>Gets or sets the displayed source value. Controls are presented directly; null removes the value.</summary>
    /// <remarks>The source object is retained separately from its formatted text. Reassign a mutable object to refresh it.
    /// Invalid ownership or conversion preserves the previous value and presenter.</remarks>
    public object? Value
    {
        get => _value;
        set
        {
            VerifyTreeMutation();
            Control? presenter = ValuePresenter;
            object? content = FormatValue(value, ValueFormat, ValueFormatProvider);
            _valueHost.Content = content;
            bool changed = !ReferenceEquals(_value, value);
            _value = value;
            _formatDirty = false;
            if (changed)
                NotifyPropertyChanged(ValueArgs);
            if (!ReferenceEquals(presenter, ValuePresenter))
                NotifyPropertyChanged(ValuePresenterArgs);
        }
    }

    /// <summary>Gets the actual icon control, or null when no icon content is assigned.</summary>
    public Control? IconPresenter => _iconHost.ContentPresenter;
    /// <summary>Gets the actual value control, or null when no value is assigned.</summary>
    public Control? ValuePresenter => _valueHost.ContentPresenter;

    /// <summary>Gets or sets the icon edge relative to the value. The default is Left; Top and Bottom select a vertical layout.</summary>
    public IconPlacement IconPlacement
    {
        get => _placement.Value;
        set
        {
            VerifyTreeMutation();
            ValidatePlacement(value);
            if (_placement.SetLocal(value))
                NotifyPropertyChanged(PlacementArgs);
        }
    }

    /// <summary>Gets or sets the nonnegative logical gap. The gap is used only when both presenters are visible. Defaults to 8.</summary>
    public float Spacing
    {
        get => _spacing.Value;
        set
        {
            VerifyTreeMutation();
            ValidateSpacing(value);
            if (_spacing.SetLocal(value))
                NotifyPropertyChanged(SpacingArgs);
        }
    }

    /// <summary>Gets or sets the IFormattable format string, such as N0 or mm\:ss. Empty uses the object's text representation.</summary>
    /// <remarks>Formatting applies to data values; explicit controls and objects without IFormattable ignore this format.</remarks>
    public string ValueFormat
    {
        get => _valueFormat.Value;
        set
        {
            VerifyTreeMutation();
            ArgumentNullException.ThrowIfNull(value);
            var candidate = _valueFormat;
            bool changed = candidate.SetLocal(value);
            if (changed)
                SetFormattedContent(FormatValue(_value, value, ValueFormatProvider));
            _valueFormat = candidate;
            if (changed)
                NotifyPropertyChanged(FormatArgs);
            _formatDirty = false;
        }
    }

    /// <summary>Gets or sets the provider for formatted values. Null uses the current culture when the value is converted.</summary>
    /// <remarks>Changing ambient culture is not polled. Reassign Value or the provider to refresh existing text.</remarks>
    public IFormatProvider? ValueFormatProvider
    {
        get => _valueFormatProvider;
        set
        {
            VerifyTreeMutation();
            if (ReferenceEquals(_valueFormatProvider, value))
                return;
            SetFormattedContent(FormatValue(_value, ValueFormat, value));
            _valueFormatProvider = value;
            NotifyPropertyChanged(FormatProviderArgs);
            _formatDirty = false;
        }
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "IconValueDisplay";

    /// <inheritdoc/>
    public override void Add(Control child)
    {
        ArgumentNullException.ThrowIfNull(child);
        ValidateChild(child);
        Value = child;
    }

    /// <inheritdoc/>
    public override void Remove(Control child)
    {
        ArgumentNullException.ThrowIfNull(child);
        VerifyTreeMutation();
        if (ReferenceEquals(child, _iconHost) || ReferenceEquals(child, IconPresenter))
            Icon = null;
        else if (ReferenceEquals(child, _valueHost) || ReferenceEquals(child, ValuePresenter))
            Value = null;
    }

    /// <inheritdoc/>
    public override void Clear()
    {
        VerifyTreeMutation();
        Icon = null;
        Value = null;
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(PropertyChangedEventArgs notification)
    {
        // Content hosts and text metrics invalidate only the geometry that actually changes.
        switch (notification.PropertyName)
        {
            case nameof(ValueFormat):
                _formatDirty = true;
                Workspace?.InvalidateLayout();
                return;
            case nameof(TextColor):
            case nameof(DisabledTextColor):
                UpdateTextColors();
                return;
            case nameof(ValueFormatProvider):
            case nameof(Icon):
            case nameof(Value):
            case nameof(IconPresenter):
            case nameof(ValuePresenter):
                UpdateTextColors();
                return;
            default:
                base.OnPropertyChanged(notification);
                return;
        }
    }

    internal override void Prepare()
    {
        base.Prepare();
        if (_formatDirty)
        {
            SetFormattedContent(FormatValue(_value, ValueFormat, ValueFormatProvider));
            _formatDirty = false;
        }

        UpdateTextColors();
    }

    private void UpdateTextColors()
    {
        if (Icon is not Control && IconPresenter is TextBlock icon)
            SetTextColors(icon);
        if (Value is not Control && ValuePresenter is TextBlock text)
            SetTextColors(text);
    }

    private void SetTextColors(TextBlock text)
    {
        text.TextColor = TextColor;
        text.DisabledTextColor = DisabledTextColor;
    }

    private static object? FormatValue(object? value, string format, IFormatProvider? provider) => value is not Control && format.Length != 0 && value is IFormattable formattable ? formattable.ToString(format, provider) ?? string.Empty : value;
    private void SetFormattedContent(object? content)
    {
        Control? previous = ValuePresenter;
        _valueHost.Content = content;
        if (!ReferenceEquals(previous, ValuePresenter))
            NotifyPropertyChanged(ValuePresenterArgs);
    }

    private bool Horizontal => IconPlacement is DisplayPlacement.Left or DisplayPlacement.Right;
    private bool HasIcon => IconPresenter?.IsVisible == true;
    private bool HasValue => ValuePresenter?.IsVisible == true;

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        ValidatePlacement(IconPlacement);
        ValidateSpacing(Spacing);
        _iconHost.Measure(context, available);
        float gap = HasIcon && HasValue ? Spacing : 0;
        float iconMain = HasIcon ? Main(_iconHost.DesiredSize) : 0;
        float remaining = MathF.Max(0, Main(available) - iconMain - gap);
        _valueHost.Measure(context, Horizontal ? new Size2(remaining, available.Height) : new Size2(available.Width, remaining));
        Size2 icon = HasIcon ? _iconHost.DesiredSize : Size2.Zero;
        Size2 value = HasValue ? _valueHost.DesiredSize : Size2.Zero;
        return Horizontal ? new Size2(icon.Width + gap + value.Width, MathF.Max(icon.Height, value.Height)) : new Size2(MathF.Max(icon.Width, value.Width), icon.Height + gap + value.Height);
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 bounds)
    {
        float available = Horizontal ? bounds.Width : bounds.Height;
        float icon = HasIcon ? MathF.Min(available, Main(_iconHost.DesiredSize)) : 0;
        float gap = HasIcon && HasValue ? MathF.Min(Spacing, MathF.Max(0, available - icon)) : 0;
        float value = HasValue ? MathF.Min(Main(_valueHost.DesiredSize), MathF.Max(0, available - icon - gap)) : 0;
        bool iconFirst = IconPlacement is DisplayPlacement.Left or DisplayPlacement.Top;
        ArrangeHost(_iconHost, bounds, iconFirst ? 0 : value + gap, icon);
        ArrangeHost(_valueHost, bounds, iconFirst ? icon + gap : 0, value);
    }

    private float Main(Size2 size) => Horizontal ? size.Width : size.Height;
    private void ArrangeHost(ContentControl host, Bounds2 bounds, float offset, float extent)
    {
        host.Arrange(Horizontal ? new Bounds2(bounds.X + offset, bounds.Y, extent, bounds.Height) : new Bounds2(bounds.X, bounds.Y + offset, bounds.Width, extent), true);
    }

    private static void ValidatePlacement(IconPlacement value)
    {
        if (value is < DisplayPlacement.Left or > DisplayPlacement.Bottom)
            throw new ArgumentOutOfRangeException(nameof(value));
    }

    private static void ValidateSpacing(float value)
    {
        if (!float.IsFinite(value) || value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Spacing must be finite and nonnegative.");
    }

    /// <inheritdoc/>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case nameof(IconPlacement):
                    if (_placement.Apply(Converters.Convert<IconPlacement>("IconPlacement", entry.Value)))
                        NotifyPropertyChanged(PlacementArgs);
                    break;
                case nameof(Spacing):
                    float spacing = Converters.Convert<float>("Single", entry.Value);
                    ValidateSpacing(spacing);
                    if (_spacing.Apply(spacing))
                        NotifyPropertyChanged(SpacingArgs);
                    break;
                case nameof(ValueFormat):
                    if (_valueFormat.Apply(entry.Value))
                        NotifyPropertyChanged(FormatArgs);
                    break;
            }
        }
    }

    internal override bool IsStyleLocal(string property) => property switch
    {
        nameof(IconPlacement) => _placement.HasLocalValue,
        nameof(Spacing) => _spacing.HasLocalValue,
        nameof(ValueFormat) => _valueFormat.HasLocalValue,
        _ => base.IsStyleLocal(property)};
    /// <inheritdoc/>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case nameof(IconPlacement):
                if (_placement.ClearLocal())
                    NotifyPropertyChanged(PlacementArgs);
                break;
            case nameof(Spacing):
                if (_spacing.ClearLocal())
                    NotifyPropertyChanged(SpacingArgs);
                break;
            case nameof(ValueFormat):
                if (_valueFormat.ClearLocal())
                    NotifyPropertyChanged(FormatArgs);
                break;
            default:
                base.ResetStyleProperty(property);
                break;
        }
    }

    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();
        _placement.BeginStyle(DisplayPlacement.Left);
        _spacing.BeginStyle(8);
        _valueFormat.BeginStyle(string.Empty);
    }

    /// <inheritdoc/>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_placement.CommitStyle())
            NotifyPropertyChanged(PlacementArgs);
        if (_spacing.CommitStyle())
            NotifyPropertyChanged(SpacingArgs);
        if (_valueFormat.CommitStyle())
            NotifyPropertyChanged(FormatArgs);
    }
}

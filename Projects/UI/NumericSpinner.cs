using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;
using System;
using System.Globalization;

namespace Sachssoft.Engine.Controls;
/// <summary>
/// Decimal input with validated text, bounded values and repeating increment/decrement buttons.
/// </summary>
public class NumericSpinner : ContainerControl
{
    /// <summary>Identifies changes to <see cref="NumberFormat"/>.</summary>
    private static readonly System.ComponentModel.PropertyChangedEventArgs NumberFormatArgs = new(nameof(NumberFormat));
    /// <summary>Stores the style value and local override for <see cref="NumberFormat"/>.</summary>
    private StyleProperty<string> _styleNumberFormat = new(string.Empty);

    private readonly TextBox _editor = new()
    {
        StyleId = "NumericSpinner.Editor"
    };
    private readonly RepeatButton _increase = new()
    {
        StyleId = "NumericSpinner.Arrow"
    }, _decrease = new()
    {
        StyleId = "NumericSpinner.Arrow"
    };
    private ChangeTracker<decimal> _value;
    /// <summary>Stores whether the cached text is the editable representation.</summary>
    private bool _textFocused;
    /// <summary>Stores the precision used by the cached editor text.</summary>
    private int _textPlaces = -1;
    /// <summary>Stores the culture of the cached editor text.</summary>
    private string? _textCulture;
    /// <summary>Stores the requested format of the cached editor text.</summary>
    private string? _textFormat;
    /// <summary>Stores the value of the cached editor text.</summary>
    private decimal? _textValue;
    private ChangeTracker<string> _culture;
    private ChangeTracker<int> _places;
    private bool _syncing, _dirty;
    /// <summary>
    /// Creates a number spinner.
    /// </summary>
    public NumericSpinner() : base()
    {
        _increase.Content = new ControlGlyph(ControlGlyphKind.Up);
        _decrease.Content = new ControlGlyph(ControlGlyphKind.Down);
        base.Add(_editor);
        base.Add(_increase);
        base.Add(_decrease);
        _increase.Command = new Command(_ => Change(1), _ => !ConfigIsReadOnly && Value < Maximum);
        _decrease.Command = new Command(_ => Change(-1), _ => !ConfigIsReadOnly && Value > Minimum);
        _editor.Submitted += (_, _) => CommitEdit();
        _editor.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName != nameof(IsFocused))
                return;
            if (!_editor.IsFocused)
                CommitEdit();
            SyncText();
        };
        _editor.TextChanged += (_, _) =>
        {
            if (!_syncing)
            {
                _dirty = true;
                _editor.HasError = !TryRead(out _);
            }
        };
    }

    /// <summary>
    /// Gets or sets minimum value.
    /// </summary>
    public decimal Minimum { get => ConfigNumericMinimum; set => ConfigNumericMinimum = value; }
    /// <summary>
    /// Gets or sets maximum value.
    /// </summary>
    public decimal Maximum { get => ConfigNumericMaximum; set => ConfigNumericMaximum = value; }
    /// <summary>
    /// Gets or sets the increment.
    /// </summary>
    public decimal Increment { get => ConfigNumericIncrement; set => ConfigNumericIncrement = value; }

    /// <summary>
    /// Gets or sets the rounded, clamped decimal value.
    /// </summary>
    public decimal Value
    {
        get
        {
            return ConfigNumericValue;
        }

        set
        {
            Validate();
            SetValue(value);
        }
    }

    /// <summary>
    /// Gets the text editor.
    /// </summary>
    public TextBox Editor => _editor;
    /// <summary>
    /// Gets the increment repeat button.
    /// </summary>
    public RepeatButton IncreaseButton => _increase;
    /// <summary>
    /// Gets the decrement repeat button.
    /// </summary>
    public RepeatButton DecreaseButton => _decrease;

    /// <summary>
    /// Occurs after the decimal value changes.
    /// </summary>
    public event EventHandler? ValueChanged;
    /// <summary>
    /// Gets or sets the standard or custom numeric display format. Formatting does not change the stored value. An empty format uses DecimalPlaces.
    /// </summary>
    public string NumberFormat
    {
        get => _styleNumberFormat.Value;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value != NumberFormat)
                _ = 0m.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
            if (_styleNumberFormat.SetLocal(value))
                NotifyPropertyChanged(NumberFormatArgs);
        }
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "NumericSpinner";
    private CultureInfo Culture => ConfigCultureName.Length == 0 ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(ConfigCultureName);

    private void Validate()
    {
        if (Minimum > Maximum || Increment <= 0 || ConfigDecimalPlaces < 0 || ConfigDecimalPlaces > 28)
            throw new InvalidOperationException("Invalid decimal spinner range or precision.");
    }

    private void SetValue(decimal value)
    {
        decimal next = Math.Clamp(decimal.Round(value, ConfigDecimalPlaces, MidpointRounding.AwayFromZero), Minimum, Maximum);
        ConfigNumericValue = next;
        bool changed = _value.WasEnsured && _value.NewValue != next;
        _value.EnsureChange(next);
        SyncText();
        if (changed)
            ValueChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Refreshes the cached label or plain editable number without replacing pending input.</summary>
    /// <param name="force">Whether pending text and the current formatting cache may be replaced.</param>
    private void SyncText(bool force = false)
    {
        if (_dirty && !force)
            return;
        if (!force && _textValue == Value && _textFormat == NumberFormat &&
            _textCulture == CultureName && _textFocused == _editor.IsFocused && _textPlaces == DecimalPlaces)
            return;
        string format = _editor.IsFocused ? "G29" : NumberFormat.Length == 0 ? "F" + ConfigDecimalPlaces : NumberFormat;
        string text = Value.ToString(format, Culture);
        _textValue = Value;
        _textFormat = NumberFormat;
        _textFocused = _editor.IsFocused;
        _textPlaces = DecimalPlaces;
        _textCulture = CultureName;
        _syncing = true;
        try
        {
            _editor.Text = text;
            _editor.HasError = false;
        }
        finally
        {
            _syncing = false;
        }
    }

    private bool TryRead(out decimal value) => decimal.TryParse(_editor.Text, NumberStyles.Float | NumberStyles.AllowThousands, Culture, out value) && value >= Minimum && value <= Maximum;
    /// <summary>
    /// Validates and commits typed text, preserving invalid text and the old value.
    /// </summary>
    public bool CommitEdit()
    {
        if (!_dirty)
        {
            SyncText();
            return true;
        }
        Validate();
        if (!TryRead(out decimal value))
        {
            _editor.HasError = true;
            return false;
        }

        SetValue(value);
        _dirty = false;
        SyncText(true);

        return true;
    }

    private void Change(int direction)
    {
        if (ConfigIsReadOnly)
            return;
        _dirty = false;
        decimal next;
        try
        {
            next = checked(Value + (direction > 0 ? Increment : -Increment));
        }
        catch (OverflowException)
        {
            next = direction > 0 ? Maximum : Minimum;
        }

        SetValue(next);
        SyncText(true);
    }

    internal override void Prepare()
    {
        base.Prepare();
        Validate();
        _culture.EnsureChange(ConfigCultureName);
        _places.EnsureChange(ConfigDecimalPlaces);
        SetValue(ConfigNumericValue);
        _editor.IsReadOnly = ConfigIsReadOnly;
        _increase.RepeatDelay = _decrease.RepeatDelay = ConfigRepeatDelay;
        _increase.RepeatInterval = _decrease.RepeatInterval = ConfigRepeatInterval;
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        _editor.Measure(context, available);
        _increase.Measure(context, available);
        _decrease.Measure(context, available);
        return new Size2(MathF.Min(160, available.Width), MathF.Min(36, available.Height));
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 b)
    {
        float w = MathF.Min(28, b.Width);
        _editor.Arrange(new Bounds2(b.X, b.Y, MathF.Max(0, b.Width - w), b.Height), true);
        _increase.Arrange(new Bounds2(b.Right - w, b.Y, w, b.Height / 2), true);
        _decrease.Arrange(new Bounds2(b.Right - w, b.Y + b.Height / 2, w, b.Height / 2), true);
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        var e = args.Input;
        if (e.Type == ControlInputEventType.KeyDown && e.Key is Keys.Up or Keys.Down)
        {
            if (!_editor.IsReadOnly)
                Change(e.Key == Keys.Up ? 1 : -1);
            args.Handled = true;
        }
        else if (e.Type == ControlInputEventType.KeyDown && e.Key == Keys.Escape)
        {
            _dirty = false;
            SyncText(true);

            args.Handled = true;
        }
    }

    /// <inheritdoc/>
    protected override void OnUpdate(TimeSpan elapsed)
    {
        SyncText();
    }

    private StyleProperty<bool> _styleIsReadOnly = new(false);
    private static readonly System.ComponentModel.PropertyChangedEventArgs IsReadOnlyChangedArgs = new(nameof(IsReadOnly));
    internal bool ConfigIsReadOnly
    {
        get => _styleIsReadOnly.Value;
        set
        {
            if (_styleIsReadOnly.SetLocal(value))
                NotifyPropertyChanged(IsReadOnlyChangedArgs);
        }
    }

    /// <summary>
    /// Gets or sets IsReadOnly; a local assignment overrides the style.
    /// </summary>
    public bool IsReadOnly { get => ConfigIsReadOnly; set => ConfigIsReadOnly = value; }

    private StyleProperty<float> _styleRepeatDelay = new(0.4f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs RepeatDelayChangedArgs = new(nameof(RepeatDelay));
    internal float ConfigRepeatDelay
    {
        get => _styleRepeatDelay.Value;
        set
        {
            if (_styleRepeatDelay.SetLocal(value))
                NotifyPropertyChanged(RepeatDelayChangedArgs);
        }
    }

    /// <summary>
    /// Gets or sets RepeatDelay; a local assignment overrides the style.
    /// </summary>
    public float RepeatDelay { get => ConfigRepeatDelay; set => ConfigRepeatDelay = value; }

    private StyleProperty<float> _styleRepeatInterval = new(0.075f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs RepeatIntervalChangedArgs = new(nameof(RepeatInterval));
    internal float ConfigRepeatInterval
    {
        get => _styleRepeatInterval.Value;
        set
        {
            if (_styleRepeatInterval.SetLocal(value))
                NotifyPropertyChanged(RepeatIntervalChangedArgs);
        }
    }

    /// <summary>
    /// Gets or sets RepeatInterval; a local assignment overrides the style.
    /// </summary>
    public float RepeatInterval { get => ConfigRepeatInterval; set => ConfigRepeatInterval = value; }

    private StyleProperty<decimal> _styleNumericMinimum = new(0m);
    private static readonly System.ComponentModel.PropertyChangedEventArgs NumericMinimumChangedArgs = new(nameof(NumericMinimum));
    internal decimal ConfigNumericMinimum
    {
        get => _styleNumericMinimum.Value;
        set
        {
            if (_styleNumericMinimum.SetLocal(value))
                NotifyPropertyChanged(NumericMinimumChangedArgs);
        }
    }

    /// <summary>
    /// Gets or sets NumericMinimum; a local assignment overrides the style.
    /// </summary>
    public decimal NumericMinimum { get => ConfigNumericMinimum; set => ConfigNumericMinimum = value; }

    private StyleProperty<decimal> _styleNumericMaximum = new(100m);
    private static readonly System.ComponentModel.PropertyChangedEventArgs NumericMaximumChangedArgs = new(nameof(NumericMaximum));
    internal decimal ConfigNumericMaximum
    {
        get => _styleNumericMaximum.Value;
        set
        {
            if (_styleNumericMaximum.SetLocal(value))
                NotifyPropertyChanged(NumericMaximumChangedArgs);
        }
    }

    /// <summary>
    /// Gets or sets NumericMaximum; a local assignment overrides the style.
    /// </summary>
    public decimal NumericMaximum { get => ConfigNumericMaximum; set => ConfigNumericMaximum = value; }

    private StyleProperty<decimal> _styleNumericValue = new(0m);
    private static readonly System.ComponentModel.PropertyChangedEventArgs NumericValueChangedArgs = new(nameof(NumericValue));
    internal decimal ConfigNumericValue
    {
        get => _styleNumericValue.Value;
        set
        {
            if (_styleNumericValue.SetLocal(value))
                NotifyPropertyChanged(NumericValueChangedArgs);
        }
    }

    /// <summary>
    /// Gets or sets NumericValue; a local assignment overrides the style.
    /// </summary>
    public decimal NumericValue { get => ConfigNumericValue; set => ConfigNumericValue = value; }

    private StyleProperty<decimal> _styleNumericIncrement = new(1m);
    private static readonly System.ComponentModel.PropertyChangedEventArgs NumericIncrementChangedArgs = new(nameof(NumericIncrement));
    internal decimal ConfigNumericIncrement
    {
        get => _styleNumericIncrement.Value;
        set
        {
            if (_styleNumericIncrement.SetLocal(value))
                NotifyPropertyChanged(NumericIncrementChangedArgs);
        }
    }

    /// <summary>
    /// Gets or sets NumericIncrement; a local assignment overrides the style.
    /// </summary>
    public  decimal NumericIncrement
    {
        get => ConfigNumericIncrement;
        set => ConfigNumericIncrement = value;
    }

    private StyleProperty<int> _styleDecimalPlaces = new(0);
    private static readonly System.ComponentModel.PropertyChangedEventArgs DecimalPlacesChangedArgs = new(nameof(DecimalPlaces));
    internal int ConfigDecimalPlaces
    {
        get => _styleDecimalPlaces.Value;
        set
        {
            if (_styleDecimalPlaces.SetLocal(value))
                NotifyPropertyChanged(DecimalPlacesChangedArgs);
        }
    }

    /// <summary>
    /// Gets or sets the stored decimal precision from 0 to 28. Zero rounds values to integers; NumberFormat only changes their display.
    /// </summary>
    public int DecimalPlaces { get => ConfigDecimalPlaces; set => ConfigDecimalPlaces = value; }

    private StyleProperty<string> _styleCultureName = new(string.Empty);
    private static readonly System.ComponentModel.PropertyChangedEventArgs CultureNameChangedArgs = new(nameof(CultureName));
    internal string ConfigCultureName
    {
        get => _styleCultureName.Value;
        set
        {
            if (_styleCultureName.SetLocal(value))
                NotifyPropertyChanged(CultureNameChangedArgs);
        }
    }

    /// <summary>
    /// Gets or sets CultureName; a local assignment overrides the style.
    /// </summary>
    public string CultureName { get => ConfigCultureName; set => ConfigCultureName = value; }

    /// <summary>
    /// Applies this control's explicit style values without replacing local assignments.
    /// </summary>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case nameof(NumberFormat):
                    if (_styleNumberFormat.Apply(Converters.Convert<string>("String", entry.Value)))
                        NotifyPropertyChanged(NumberFormatArgs);
                    break;
                case "IsReadOnly":
                    if (_styleIsReadOnly.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(IsReadOnlyChangedArgs);
                    break;
                case "RepeatDelay":
                    if (_styleRepeatDelay.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(RepeatDelayChangedArgs);
                    break;
                case "RepeatInterval":
                    if (_styleRepeatInterval.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(RepeatIntervalChangedArgs);
                    break;
                case "NumericMinimum":
                    if (_styleNumericMinimum.Apply(Converters.Convert<decimal>("Decimal", entry.Value)))
                        NotifyPropertyChanged(NumericMinimumChangedArgs);
                    break;
                case "NumericMaximum":
                    if (_styleNumericMaximum.Apply(Converters.Convert<decimal>("Decimal", entry.Value)))
                        NotifyPropertyChanged(NumericMaximumChangedArgs);
                    break;
                case "NumericValue":
                    if (_styleNumericValue.Apply(Converters.Convert<decimal>("Decimal", entry.Value)))
                        NotifyPropertyChanged(NumericValueChangedArgs);
                    break;
                case "NumericIncrement":
                    if (_styleNumericIncrement.Apply(Converters.Convert<decimal>("Decimal", entry.Value)))
                        NotifyPropertyChanged(NumericIncrementChangedArgs);
                    break;
                case "DecimalPlaces":
                    if (_styleDecimalPlaces.Apply(Converters.Convert<int>("Int32", entry.Value)))
                        NotifyPropertyChanged(DecimalPlacesChangedArgs);
                    break;
                case "CultureName":
                    if (_styleCultureName.Apply(Converters.Convert<string>("String", entry.Value)))
                        NotifyPropertyChanged(CultureNameChangedArgs);
                    break;
            }
        }
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case nameof(NumberFormat):
                return _styleNumberFormat.HasLocalValue;
            case "IsReadOnly":
                return _styleIsReadOnly.HasLocalValue;
            case "RepeatDelay":
                return _styleRepeatDelay.HasLocalValue;
            case "RepeatInterval":
                return _styleRepeatInterval.HasLocalValue;
            case "NumericMinimum":
                return _styleNumericMinimum.HasLocalValue;
            case "NumericMaximum":
                return _styleNumericMaximum.HasLocalValue;
            case "NumericValue":
                return _styleNumericValue.HasLocalValue;
            case "NumericIncrement":
                return _styleNumericIncrement.HasLocalValue;
            case "DecimalPlaces":
                return _styleDecimalPlaces.HasLocalValue;
            case "CultureName":
                return _styleCultureName.HasLocalValue;
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>
    /// Removes a local override and restores the current style value.
    /// </summary>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case nameof(NumberFormat):
                if (_styleNumberFormat.ClearLocal())
                    NotifyPropertyChanged(NumberFormatArgs);
                return;
            case "IsReadOnly":
                if (_styleIsReadOnly.ClearLocal())
                    NotifyPropertyChanged(IsReadOnlyChangedArgs);
                return;
            case "RepeatDelay":
                if (_styleRepeatDelay.ClearLocal())
                    NotifyPropertyChanged(RepeatDelayChangedArgs);
                return;
            case "RepeatInterval":
                if (_styleRepeatInterval.ClearLocal())
                    NotifyPropertyChanged(RepeatIntervalChangedArgs);
                return;
            case "NumericMinimum":
                if (_styleNumericMinimum.ClearLocal())
                    NotifyPropertyChanged(NumericMinimumChangedArgs);
                return;
            case "NumericMaximum":
                if (_styleNumericMaximum.ClearLocal())
                    NotifyPropertyChanged(NumericMaximumChangedArgs);
                return;
            case "NumericValue":
                if (_styleNumericValue.ClearLocal())
                    NotifyPropertyChanged(NumericValueChangedArgs);
                return;
            case "NumericIncrement":
                if (_styleNumericIncrement.ClearLocal())
                    NotifyPropertyChanged(NumericIncrementChangedArgs);
                return;
            case "DecimalPlaces":
                if (_styleDecimalPlaces.ClearLocal())
                    NotifyPropertyChanged(DecimalPlacesChangedArgs);
                return;
            case "CultureName":
                if (_styleCultureName.ClearLocal())
                    NotifyPropertyChanged(CultureNameChangedArgs);
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
        _styleNumberFormat.BeginStyle(string.Empty);
        if (_styleIsReadOnly.BeginStyle(false))
            NotifyPropertyChanged(IsReadOnlyChangedArgs);
        if (_styleRepeatDelay.BeginStyle(0.4f))
            NotifyPropertyChanged(RepeatDelayChangedArgs);
        if (_styleRepeatInterval.BeginStyle(0.075f))
            NotifyPropertyChanged(RepeatIntervalChangedArgs);
        if (_styleNumericMinimum.BeginStyle(0m))
            NotifyPropertyChanged(NumericMinimumChangedArgs);
        if (_styleNumericMaximum.BeginStyle(100m))
            NotifyPropertyChanged(NumericMaximumChangedArgs);
        if (_styleNumericValue.BeginStyle(0m))
            NotifyPropertyChanged(NumericValueChangedArgs);
        if (_styleNumericIncrement.BeginStyle(1m))
            NotifyPropertyChanged(NumericIncrementChangedArgs);
        if (_styleDecimalPlaces.BeginStyle(0))
            NotifyPropertyChanged(DecimalPlacesChangedArgs);
        if (_styleCultureName.BeginStyle(string.Empty))
            NotifyPropertyChanged(CultureNameChangedArgs);
    }

    /// <summary>
    /// Commits final style values and notifies once per effective property change.
    /// </summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleNumberFormat.CommitStyle())
            NotifyPropertyChanged(NumberFormatArgs);
        if (_styleIsReadOnly.CommitStyle())
            NotifyPropertyChanged(IsReadOnlyChangedArgs);
        if (_styleRepeatDelay.CommitStyle())
            NotifyPropertyChanged(RepeatDelayChangedArgs);
        if (_styleRepeatInterval.CommitStyle())
            NotifyPropertyChanged(RepeatIntervalChangedArgs);
        if (_styleNumericMinimum.CommitStyle())
            NotifyPropertyChanged(NumericMinimumChangedArgs);
        if (_styleNumericMaximum.CommitStyle())
            NotifyPropertyChanged(NumericMaximumChangedArgs);
        if (_styleNumericValue.CommitStyle())
            NotifyPropertyChanged(NumericValueChangedArgs);
        if (_styleNumericIncrement.CommitStyle())
            NotifyPropertyChanged(NumericIncrementChangedArgs);
        if (_styleDecimalPlaces.CommitStyle())
            NotifyPropertyChanged(DecimalPlacesChangedArgs);
        if (_styleCultureName.CommitStyle())
            NotifyPropertyChanged(CultureNameChangedArgs);
    }
}

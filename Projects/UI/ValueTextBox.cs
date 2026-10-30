using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;

namespace Sachssoft.Engine.Controls;
/// <summary>
/// A single-row numeric field with repeat arrows and a shared bar-or-text center.
/// </summary>
public class ValueTextBox : ContainerControl
{
    /// <summary>Identifies changes to <see cref="NumberFormat"/>.</summary>
    private static readonly System.ComponentModel.PropertyChangedEventArgs NumberFormatArgs = new(nameof(NumberFormat));
    /// <summary>Identifies changes to <see cref="CultureName"/>.</summary>
    private static readonly System.ComponentModel.PropertyChangedEventArgs CultureNameArgs = new(nameof(CultureName));
    /// <summary>Stores the style value and local override for <see cref="NumberFormat"/>.</summary>
    private StyleProperty<string> _styleNumberFormat = new("0.###");
    /// <summary>Stores the style value and local override for <see cref="CultureName"/>.</summary>
    private StyleProperty<string> _styleCultureName = new(string.Empty);

    private readonly ValueTextEditor _editor = new()
    {
        StyleId = "ValueTextBox.Editor",
        IsVisible = false
    };
    private readonly ValueBar _bar = new()
    {
        StyleId = "ValueTextBox.Range"
    };
    private readonly RepeatButton _decrease = new()
    {
        StyleId = "ValueTextBox.Arrow"
    }, _increase = new()
    {
        StyleId = "ValueTextBox.Arrow"
    };
    private bool _syncing, _isEditing;
    private float _lastValue = float.NaN;
    private static readonly System.ComponentModel.PropertyChangedEventArgs EditingArgs = new(nameof(IsEditing));
    /// <summary>
    /// Creates the two repeat arrows and the mutually exclusive center controls.
    /// </summary>
    public ValueTextBox()
    {
        _decrease.Content = new ControlGlyph(ControlGlyphKind.Left);
        _increase.Content = new ControlGlyph(ControlGlyphKind.Right);
        base.Add(_decrease);
        base.Add(_bar);
        base.Add(_editor);
        base.Add(_increase);
        _decrease.Command = new Command(_ => Change(-1), _ => IsEditing || Value > Minimum);
        _increase.Command = new Command(_ => Change(1), _ => IsEditing || Value < Maximum);
        _bar.EditRequested += (_, _) => BeginEdit();
        _bar.ValueChanged += (_, _) =>
        {
            if (!_syncing)
                Value = _bar.Value;
        };
        _editor.Submitted += (_, _) =>
        {
            if (CommitEdit())
                EndEdit(true);
        };
        _editor.EditingFocusLost += (_, _) =>
        {
            if (IsEditing)
            {
                if (!CommitEdit())
                    ResetText();
                EndEdit(false);
            }
        };
    }

    /// <summary>
    /// Gets or sets the standard or custom numeric display format. Formatting does not change the stored value.
    /// </summary>
    public string NumberFormat
    {
        get => _styleNumberFormat.Value;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value != NumberFormat)
                _ = 0f.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
            if (_styleNumberFormat.SetLocal(value))
                NotifyPropertyChanged(NumberFormatArgs);
        }
    }

    /// <summary>
    /// Gets or sets the numeric culture name. An empty name selects the invariant culture.
    /// </summary>
    public string CultureName
    {
        get => _styleCultureName.Value;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value != CultureName)
                _ = System.Globalization.CultureInfo.GetCultureInfo(value);
            if (_styleCultureName.SetLocal(value))
                NotifyPropertyChanged(CultureNameArgs);
        }
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "ValueTextBox";
    /// <summary>
    /// Gets the center editor, visible only while entering a number.
    /// </summary>
    public TextBox Editor => _editor;
    /// <summary>
    /// Gets the center range bar, visible outside text editing.
    /// </summary>
    public ValueBar RangeBar => _bar;
    /// <summary>
    /// Gets the repeating decrement button.
    /// </summary>
    public RepeatButton DecreaseButton => _decrease;
    /// <summary>
    /// Gets the repeating increment button.
    /// </summary>
    public RepeatButton IncreaseButton => _increase;
    /// <summary>
    /// Gets whether the center currently accepts text input.
    /// </summary>
    public bool IsEditing => _isEditing;

    /// <summary>
    /// Occurs after the clamped, step-aligned value changes.
    /// </summary>
    public event EventHandler? ValueChanged;
    /// <summary>
    /// Replaces the bar with the editor, focuses it and selects its formatted value.
    /// </summary>
    public void BeginEdit()
    {
        if (IsEditing || !IsAvailable())
            return;
        _editor.Prepare();
        if (!_editor.IsEnabled || !_editor.IsFocusable)
            return;
        ResetText();
        SetNotify(ref _isEditing, true, EditingArgs);
        _bar.IsVisible = false;
        _editor.IsVisible = true;
        Workspace!.Focus(_editor);
        _editor.SelectAll();
    }

    private void EndEdit(bool focusBar)
    {
        SetNotify(ref _isEditing, false, EditingArgs);
        _editor.IsVisible = false;
        _bar.IsVisible = true;
        if (focusBar && _bar.IsAvailable() && _bar.IsFocusable)
            Workspace?.Focus(_bar);
    }

    /// <summary>
    /// Discards uncommitted text and restores the range bar.
    /// </summary>
    public void CancelEdit()
    {
        ResetText();
        EndEdit(_editor.IsFocused);
    }

    private void ResetText()
    {
        _editor.HasError = false;
        Sync(true);
    }

    /// <summary>
    /// Validates and commits plain numeric text using CultureName; invalid focused input remains editable.
    /// </summary>
    public bool CommitEdit()
    {
        if (!float.TryParse(_editor.Text, System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands, System.Globalization.CultureInfo.GetCultureInfo(CultureName), out float value) || !float.IsFinite(value))
        {
            _editor.HasError = true;
            return false;
        }

        Value = Math.Clamp(value, Minimum, Maximum);
        _editor.HasError = false;
        Sync(true);
        return true;
    }

    private void Change(int direction)
    {
        if (IsEditing)
        {
            if (!CommitEdit())
                ResetText();
            EndEdit(_editor.IsFocused);
        }

        double change = Step > 0 ? Step : ((double)Maximum - Minimum) / 100;
        Value = (float)Math.Clamp(Value + direction * change, Minimum, Maximum);
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        if (args.Input.Type == ControlInputEventType.KeyDown && args.Input.Key == Keys.Escape && IsEditing)
        {
            CancelEdit();
            args.Handled = true;
        }
        else if (args.Input.Type == ControlInputEventType.PointerWheel)
        {
            Change(Math.Sign(args.Input.WheelDelta));
            args.Handled = true;
        }
        else
            base.OnInput(context, args);
    }

    private void Sync(bool force = false)
    {
        if (!float.IsFinite(ButtonWidth) || ButtonWidth <= 0 || !float.IsFinite(Minimum) || !float.IsFinite(Maximum) || Maximum < Minimum || !float.IsFinite(Value) || !float.IsFinite(Step) || Step < 0)
            throw new InvalidOperationException("Invalid value range.");
        _syncing = true;
        try
        {
            float next = Math.Clamp(Value, Minimum, Maximum);
            if (next != Value)
                Value = next;
            _bar.NumberFormat = NumberFormat;
            _bar.CultureName = CultureName;
            _bar.Minimum = Minimum;
            _bar.Maximum = Maximum;
            _bar.Step = Step;
            _bar.SmallChange = Step > 0 ? Step : (float)(((double)Maximum - Minimum) / 100);
            _bar.LargeChange = (float)(((double)Maximum - Minimum) / 10);
            _bar.Value = Value;
            if (_bar.Value != Value)
                Value = _bar.Value;
            if (_lastValue != Value)
            {
                _lastValue = Value;
                ValueChanged?.Invoke(this, EventArgs.Empty);
                force = true;
            }

            if (force)
                _editor.Text = Value.ToString("G9", System.Globalization.CultureInfo.GetCultureInfo(CultureName));
        }
        finally
        {
            _syncing = false;
        }
    }

    internal override void Prepare()
    {
        Sync();
        base.Prepare();
    }

    /// <inheritdoc/>
    protected override void OnUpdate(TimeSpan elapsed)
    {
        base.OnUpdate(elapsed);
        Sync();
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        _decrease.Measure(context, available);
        _increase.Measure(context, available);
        _bar.Measure(context, available);
        _editor.Measure(context, available);
        return new Size2(160, MathF.Max(32, MathF.Max(_decrease.DesiredSize.Height, MathF.Max(_bar.DesiredSize.Height, _editor.DesiredSize.Height))));
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 b)
    {
        float arrow = MathF.Min(ButtonWidth, b.Width / 3);
        var center = new Bounds2(b.X + arrow, b.Y, MathF.Max(0, b.Width - 2 * arrow), b.Height);
        _decrease.Arrange(new Bounds2(b.X, b.Y, arrow, b.Height), true);
        _increase.Arrange(new Bounds2(b.Right - arrow, b.Y, arrow, b.Height), true);
        _bar.Arrange(center, true);
        _editor.Arrange(center, true);
    }

    private StyleProperty<float> _buttonWidth = new(28);
    private static readonly System.ComponentModel.PropertyChangedEventArgs ButtonWidthArgs = new(nameof(ButtonWidth));
    /// <summary>
    /// Gets the arrow width in logical pixels, or assigns a local style override.
    /// </summary>
    public float ButtonWidth
    {
        get => _buttonWidth.Value;
        set
        {
            if (_buttonWidth.SetLocal(value))
                NotifyPropertyChanged(ButtonWidthArgs);
        }
    }

    private StyleProperty<float> _Minimum = new(0);
    private static readonly System.ComponentModel.PropertyChangedEventArgs MinimumArgs = new(nameof(Minimum));
    /// <summary>
    /// Gets the effective Minimum value or assigns a local override.
    /// </summary>
    public float Minimum
    {
        get => _Minimum.Value;
        set
        {
            if (_Minimum.SetLocal(value))
                NotifyPropertyChanged(MinimumArgs);
        }
    }

    private StyleProperty<float> _Maximum = new(100);
    private static readonly System.ComponentModel.PropertyChangedEventArgs MaximumArgs = new(nameof(Maximum));
    /// <summary>
    /// Gets the effective Maximum value or assigns a local override.
    /// </summary>
    public float Maximum
    {
        get => _Maximum.Value;
        set
        {
            if (_Maximum.SetLocal(value))
                NotifyPropertyChanged(MaximumArgs);
        }
    }

    private StyleProperty<float> _Step = new(0);
    private static readonly System.ComponentModel.PropertyChangedEventArgs StepArgs = new(nameof(Step));
    /// <summary>
    /// Gets the effective Step value or assigns a local override.
    /// </summary>
    public float Step
    {
        get => _Step.Value;
        set
        {
            if (_Step.SetLocal(value))
                NotifyPropertyChanged(StepArgs);
        }
    }

    private StyleProperty<float> _Value = new(0);
    private static readonly System.ComponentModel.PropertyChangedEventArgs ValueArgs = new(nameof(Value));
    /// <summary>
    /// Gets the effective Value value or assigns a local override.
    /// </summary>
    public float Value
    {
        get => _Value.Value;
        set
        {
            if (!float.IsFinite(value))
                throw new ArgumentOutOfRangeException(nameof(value));
            if (!float.IsFinite(Minimum) || !float.IsFinite(Maximum) || Maximum < Minimum)
                throw new InvalidOperationException("Invalid value range.");
            if (_Value.SetLocal(Math.Clamp(value, Minimum, Maximum)))
                NotifyPropertyChanged(ValueArgs);
            if (!_syncing)
                Sync(true);
        }
    }

    /// <inheritdoc/>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
            switch (entry.Key)
            {
                case nameof(NumberFormat):
                    if (_styleNumberFormat.Apply(Converters.Convert<string>("String", entry.Value)))
                        NotifyPropertyChanged(NumberFormatArgs);
                    break;
                case nameof(CultureName):
                    if (_styleCultureName.Apply(Converters.Convert<string>("String", entry.Value)))
                        NotifyPropertyChanged(CultureNameArgs);
                    break;
                case "ButtonWidth":
                    if (_buttonWidth.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(ButtonWidthArgs);
                    break;
                case "Minimum":
                    if (_Minimum.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(MinimumArgs);
                    break;
                case "Maximum":
                    if (_Maximum.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(MaximumArgs);
                    break;
                case "Step":
                    if (_Step.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(StepArgs);
                    break;
                case "Value":
                    if (_Value.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(ValueArgs);
                    break;
            }
    }

    internal override bool IsStyleLocal(string property) => property switch
    {
        nameof(NumberFormat) => _styleNumberFormat.HasLocalValue,
        nameof(CultureName) => _styleCultureName.HasLocalValue,
        "ButtonWidth" => _buttonWidth.HasLocalValue,
        "Minimum" => _Minimum.HasLocalValue,
        "Maximum" => _Maximum.HasLocalValue,
        "Step" => _Step.HasLocalValue,
        "Value" => _Value.HasLocalValue,
        _ => base.IsStyleLocal(property)};
    /// <inheritdoc/>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case nameof(NumberFormat):
                if (_styleNumberFormat.ClearLocal())
                    NotifyPropertyChanged(NumberFormatArgs);
                return;
            case nameof(CultureName):
                if (_styleCultureName.ClearLocal())
                    NotifyPropertyChanged(CultureNameArgs);
                return;
            case "ButtonWidth":
                if (_buttonWidth.ClearLocal())
                    NotifyPropertyChanged(ButtonWidthArgs);
                break;
            case "Minimum":
                if (_Minimum.ClearLocal())
                    NotifyPropertyChanged(MinimumArgs);
                break;
            case "Maximum":
                if (_Maximum.ClearLocal())
                    NotifyPropertyChanged(MaximumArgs);
                break;
            case "Step":
                if (_Step.ClearLocal())
                    NotifyPropertyChanged(StepArgs);
                break;
            case "Value":
                if (_Value.ClearLocal())
                    NotifyPropertyChanged(ValueArgs);
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
        _styleNumberFormat.BeginStyle("0.###");
        _styleCultureName.BeginStyle(string.Empty);
        _buttonWidth.BeginStyle(28);
        _Minimum.BeginStyle(0);
        _Maximum.BeginStyle(100);
        _Step.BeginStyle(0);
        _Value.BeginStyle(0);
    }

    /// <inheritdoc/>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleNumberFormat.CommitStyle())
            NotifyPropertyChanged(NumberFormatArgs);
        if (_styleCultureName.CommitStyle())
            NotifyPropertyChanged(CultureNameArgs);
        if (_buttonWidth.CommitStyle())
            NotifyPropertyChanged(ButtonWidthArgs);
        if (_Minimum.CommitStyle())
            NotifyPropertyChanged(MinimumArgs);
        if (_Maximum.CommitStyle())
            NotifyPropertyChanged(MaximumArgs);
        if (_Step.CommitStyle())
            NotifyPropertyChanged(StepArgs);
        if (_Value.CommitStyle())
            NotifyPropertyChanged(ValueArgs);
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;
using System;

namespace Sachssoft.Engine.Controls;
/// <summary>Common finite range configuration and clamped, live value state.</summary>
public abstract class RangeControl : Control
{
    private float _notifiedValue;
    /// <summary>Initializes a new range control instance.</summary>
    protected RangeControl() : base()
    {
    }

    /// <summary>Gets or sets minimum.</summary>
    public float Minimum { get => ConfigMinimum; set => ConfigMinimum = value; }
    /// <summary>Gets or sets maximum.</summary>
    public float Maximum { get => ConfigMaximum; set => ConfigMaximum = value; }
    /// <summary>Gets or sets small change.</summary>
    public float SmallChange { get => ConfigSmallChange; set => ConfigSmallChange = value; }
    /// <summary>Gets or sets large change.</summary>
    public float LargeChange { get => ConfigLargeChange; set => ConfigLargeChange = value; }
    /// <summary>Gets or sets orientation.</summary>
    public ControlOrientation Orientation { get => ConfigAxis; set => ConfigAxis = value; }

    /// <summary>Gets or sets value.</summary>
    public float Value
    {
        get
        {
            ValidateRange();
            return Math.Clamp(ConfigValue, Minimum, Maximum);
        }

        set
        {
            if (!float.IsFinite(value))
                throw new ArgumentOutOfRangeException(nameof(value));
            ValidateRange();
            float next = Math.Clamp(value, Minimum, Maximum), step = ConfigStep;
            if (step > 0)
                next = (float)Math.Clamp(Minimum + Math.Round(((double)next - Minimum) / step, MidpointRounding.AwayFromZero) * step, Minimum, Maximum);
            ConfigValue = next;
            Notify();
        }
    }

    /// <summary>Occurs when value changed.</summary>
    public event EventHandler? ValueChanged;
    /// <summary>Performs validate range using the current control state.</summary>
    protected void ValidateRange()
    {
        float step = ConfigStep;
        if (!float.IsFinite(Minimum) || !float.IsFinite(Maximum) || Maximum < Minimum || !float.IsFinite(ConfigValue) || !float.IsFinite(SmallChange) || SmallChange < 0 || !float.IsFinite(LargeChange) || LargeChange < 0 || !float.IsFinite(step) || step < 0 || Orientation is not (ControlOrientation.Horizontal or ControlOrientation.Vertical))
            throw new InvalidOperationException("Invalid control range.");
    }

    private void Notify()
    {
        float value = Value;
        if (value == _notifiedValue)
            return;
        _notifiedValue = value;
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    protected override void OnUpdate(TimeSpan elapsed)
    {
        Notify();
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        var input = args.Input;
        if (input.Type == ControlInputEventType.KeyDown)
        {
            switch (input.Key)
            {
                case Keys.Left:
                case Keys.Down:
                    Value = Value - SmallChange;
                    break;
                case Keys.Right:
                case Keys.Up:
                    Value = Value + SmallChange;
                    break;
                case Keys.PageDown:
                    Value = Value - LargeChange;
                    break;
                case Keys.PageUp:
                    Value = Value + LargeChange;
                    break;
                case Keys.Home:
                    Value = Minimum;
                    break;
                case Keys.End:
                    Value = Maximum;
                    break;
                default:
                    return;
            }

            args.Handled = true;
        }
    }

    /// <summary>Gets fraction.</summary>
    protected float Fraction => Maximum == Minimum ? 0 : (float)(((double)Value - Minimum) / ((double)Maximum - Minimum));

    /// <summary>Performs set fraction using the current control state.</summary>
    protected void SetFraction(double fraction) => Value = (float)(Minimum + Math.Clamp(fraction, 0, 1) * ((double)Maximum - Minimum));
    private StyleProperty<ControlOrientation> _styleAxis = new(ControlOrientation.Horizontal);
    private static readonly System.ComponentModel.PropertyChangedEventArgs AxisChangedArgs = new(nameof(Axis));
    internal ControlOrientation ConfigAxis
    {
        get => _styleAxis.Value;
        set
        {
            if (_styleAxis.SetLocal(value))
                NotifyPropertyChanged(AxisChangedArgs);
        }
    }

    /// <summary>Gets or sets Axis; a local assignment overrides the style.</summary>
    public ControlOrientation Axis { get => ConfigAxis; set => ConfigAxis = value; }

    private StyleProperty<float> _styleMinimum = new(0f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs MinimumChangedArgs = new(nameof(Minimum));
    internal float ConfigMinimum
    {
        get => _styleMinimum.Value;
        set
        {
            if (_styleMinimum.SetLocal(value))
                NotifyPropertyChanged(MinimumChangedArgs);
        }
    }

    private StyleProperty<float> _styleMaximum = new(100f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs MaximumChangedArgs = new(nameof(Maximum));
    internal float ConfigMaximum
    {
        get => _styleMaximum.Value;
        set
        {
            if (_styleMaximum.SetLocal(value))
                NotifyPropertyChanged(MaximumChangedArgs);
        }
    }

    private StyleProperty<float> _styleValue = new(0f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs ValueChangedArgs = new(nameof(Value));
    internal float ConfigValue
    {
        get => _styleValue.Value;
        set
        {
            if (_styleValue.SetLocal(value))
                NotifyPropertyChanged(ValueChangedArgs);
        }
    }

    private StyleProperty<float> _styleSmallChange = new(1f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs SmallChangeChangedArgs = new(nameof(SmallChange));
    internal float ConfigSmallChange
    {
        get => _styleSmallChange.Value;
        set
        {
            if (_styleSmallChange.SetLocal(value))
                NotifyPropertyChanged(SmallChangeChangedArgs);
        }
    }

    private StyleProperty<float> _styleLargeChange = new(10f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs LargeChangeChangedArgs = new(nameof(LargeChange));
    internal float ConfigLargeChange
    {
        get => _styleLargeChange.Value;
        set
        {
            if (_styleLargeChange.SetLocal(value))
                NotifyPropertyChanged(LargeChangeChangedArgs);
        }
    }

    private StyleProperty<float> _styleStep = new(0f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs StepChangedArgs = new(nameof(Step));
    internal float ConfigStep
    {
        get => _styleStep.Value;
        set
        {
            if (_styleStep.SetLocal(value))
                NotifyPropertyChanged(StepChangedArgs);
        }
    }

    /// <summary>Gets or sets Step; a local assignment overrides the style.</summary>
    public float Step { get => ConfigStep; set => ConfigStep = value; }

    /// <summary>Applies this control's explicit style values without replacing local assignments.</summary>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case "Axis":
                    if (_styleAxis.Apply(Converters.Convert<ControlOrientation>("ControlOrientation", entry.Value)))
                        NotifyPropertyChanged(AxisChangedArgs);
                    break;
                case "Minimum":
                    if (_styleMinimum.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(MinimumChangedArgs);
                    break;
                case "Maximum":
                    if (_styleMaximum.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(MaximumChangedArgs);
                    break;
                case "Value":
                    if (_styleValue.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(ValueChangedArgs);
                    break;
                case "SmallChange":
                    if (_styleSmallChange.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(SmallChangeChangedArgs);
                    break;
                case "LargeChange":
                    if (_styleLargeChange.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(LargeChangeChangedArgs);
                    break;
                case "Step":
                    if (_styleStep.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(StepChangedArgs);
                    break;
            }
        }
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case "Axis":
                return _styleAxis.HasLocalValue;
            case "Minimum":
                return _styleMinimum.HasLocalValue;
            case "Maximum":
                return _styleMaximum.HasLocalValue;
            case "Value":
                return _styleValue.HasLocalValue;
            case "SmallChange":
                return _styleSmallChange.HasLocalValue;
            case "LargeChange":
                return _styleLargeChange.HasLocalValue;
            case "Step":
                return _styleStep.HasLocalValue;
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>Removes a local override and restores the current style value.</summary>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "Axis":
                if (_styleAxis.ClearLocal())
                    NotifyPropertyChanged(AxisChangedArgs);
                return;
            case "Minimum":
                if (_styleMinimum.ClearLocal())
                    NotifyPropertyChanged(MinimumChangedArgs);
                return;
            case "Maximum":
                if (_styleMaximum.ClearLocal())
                    NotifyPropertyChanged(MaximumChangedArgs);
                return;
            case "Value":
                if (_styleValue.ClearLocal())
                    NotifyPropertyChanged(ValueChangedArgs);
                return;
            case "SmallChange":
                if (_styleSmallChange.ClearLocal())
                    NotifyPropertyChanged(SmallChangeChangedArgs);
                return;
            case "LargeChange":
                if (_styleLargeChange.ClearLocal())
                    NotifyPropertyChanged(LargeChangeChangedArgs);
                return;
            case "Step":
                if (_styleStep.ClearLocal())
                    NotifyPropertyChanged(StepChangedArgs);
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
        if (_styleAxis.BeginStyle(ControlOrientation.Horizontal))
            NotifyPropertyChanged(AxisChangedArgs);
        if (_styleMinimum.BeginStyle(0f))
            NotifyPropertyChanged(MinimumChangedArgs);
        if (_styleMaximum.BeginStyle(100f))
            NotifyPropertyChanged(MaximumChangedArgs);
        if (_styleValue.BeginStyle(0f))
            NotifyPropertyChanged(ValueChangedArgs);
        if (_styleSmallChange.BeginStyle(1f))
            NotifyPropertyChanged(SmallChangeChangedArgs);
        if (_styleLargeChange.BeginStyle(10f))
            NotifyPropertyChanged(LargeChangeChangedArgs);
        if (_styleStep.BeginStyle(0f))
            NotifyPropertyChanged(StepChangedArgs);
    }

    /// <summary>Commits final style values and notifies once per effective property change.</summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleAxis.CommitStyle())
            NotifyPropertyChanged(AxisChangedArgs);
        if (_styleMinimum.CommitStyle())
            NotifyPropertyChanged(MinimumChangedArgs);
        if (_styleMaximum.CommitStyle())
            NotifyPropertyChanged(MaximumChangedArgs);
        if (_styleValue.CommitStyle())
            NotifyPropertyChanged(ValueChangedArgs);
        if (_styleSmallChange.CommitStyle())
            NotifyPropertyChanged(SmallChangeChangedArgs);
        if (_styleLargeChange.CommitStyle())
            NotifyPropertyChanged(LargeChangeChangedArgs);
        if (_styleStep.CommitStyle())
            NotifyPropertyChanged(StepChangedArgs);
    }
}

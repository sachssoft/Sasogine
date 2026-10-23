
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;
using System;
using System.ComponentModel;

namespace Sachssoft.Engine.Controls;

/// <summary>
/// Represents a button that activates immediately when pressed
/// and repeatedly while the activation input remains held.
/// </summary>
/// <remarks>
/// <para>
/// The first activation occurs immediately. Additional activations
/// begin after <see cref="RepeatDelay"/> and continue at intervals
/// defined by <see cref="RepeatInterval"/>.
/// </para>
/// <para>
/// Repetition uses the elapsed time supplied by the update loop.
/// No independent timers or background threads are created.
/// </para>
/// <para>
/// Each activation follows the standard <see cref="ButtonBase"/>
/// behavior, raising the Clicked event before executing the command.
/// </para>
/// </remarks>
public class RepeatButton : ButtonBase
{
    private ChangeTracker<float> _delay;
    private ChangeTracker<float> _interval;

    private bool _held;
    private bool _keyboard;
    private bool _inside;

    private Keys _key;
    private ControlPointer _pointer;

    private double _elapsed;
    private double _next;

    private StyleProperty<float> _styleRepeatDelay = new(0.4f);
    private StyleProperty<float> _styleRepeatInterval = new(0.075f);

    private static readonly PropertyChangedEventArgs RepeatDelayChangedArgs = new(nameof(RepeatDelay));
    private static readonly PropertyChangedEventArgs RepeatIntervalChangedArgs = new(nameof(RepeatInterval));

    /// <summary>
    /// Initializes a new instance of the <see cref="RepeatButton"/> class.
    /// </summary>
    public RepeatButton()
    {
    }

    /// <summary>
    /// Gets or sets the initial delay, in seconds, before repeated activation begins.
    /// </summary>
    /// <remarks>
    /// The default value is 0.4 seconds.
    /// The value must be finite and greater than or equal to zero.
    /// </remarks>
    public float RepeatDelay
    {
        get => ConfigRepeatDelay;
        set => ConfigRepeatDelay = value;
    }

    /// <summary>
    /// Gets or sets the interval, in seconds, between repeated activations.
    /// </summary>
    /// <remarks>
    /// The default value is 0.075 seconds.
    /// The value must be finite and at least 0.01 seconds.
    /// </remarks>
    public float RepeatInterval
    {
        get => ConfigRepeatInterval;
        set => ConfigRepeatInterval = value;
    }

    /// <inheritdoc/>
    public override bool IsHeld => _held && (_keyboard || _inside);

    /// <inheritdoc/>
    protected override string DefaultStyleName => "RepeatButton";

    /// <inheritdoc/>
    internal override void Prepare()
    {
        base.Prepare();

        _delay.EnsureChange(ConfigRepeatDelay);
        _interval.EnsureChange(ConfigRepeatInterval);

        if (!float.IsFinite(_delay.NewValue) || _delay.NewValue < 0 ||
            !float.IsFinite(_interval.NewValue) || _interval.NewValue < 0.01f)
        {
            throw new InvalidOperationException("Invalid repeat timing.");
        }
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        ControlInputEvent input = args.Input;

        if (input.Type == ControlInputEventType.PointerDown &&
            input.Button == MouseButton.Left && !_held)
        {
            _held = true;
            _inside = true;
            _keyboard = false;
            _pointer = input.Pointer;

            context.CapturePointer(this);
            Begin();

            args.Handled = true;
        }
        else if (input.Type == ControlInputEventType.PointerMove &&
                 _held && !_keyboard && input.Pointer == _pointer)
        {
            _inside = context.Workspace.PointerHits(this, input.Position);
            args.Handled = true;
        }
        else if (input.Type == ControlInputEventType.PointerUp &&
                 input.Button == MouseButton.Left &&
                 _held && !_keyboard && input.Pointer == _pointer)
        {
            _held = false;
            context.ReleasePointer(this);
            args.Handled = true;
        }
        else if (input.Type == ControlInputEventType.KeyDown &&
                 IsFocused && input.Key is Keys.Space or Keys.Enter)
        {
            if (!_held && !input.IsRepeat)
            {
                _held = true;
                _keyboard = true;
                _key = input.Key;

                Begin();
            }

            args.Handled = true;
        }
        else if (input.Type == ControlInputEventType.KeyUp &&
                 _held && _keyboard && input.Key == _key)
        {
            _held = false;
            args.Handled = true;
        }
    }

    /// <summary>
    /// Starts a new activation sequence and performs the initial click.
    /// </summary>
    private void Begin()
    {
        _elapsed = 0;
        _next = ConfigRepeatDelay;

        PerformClick();
    }

    /// <inheritdoc/>
    protected override void OnUpdate(TimeSpan elapsed)
    {
        base.OnUpdate(elapsed);

        if (!_held || !IsAvailable() || !CanExecute)
        {
            _held = false;
            return;
        }

        if (!_keyboard && !_inside)
            return;

        _elapsed += elapsed.TotalSeconds;

        while (_held && _elapsed >= _next)
        {
            _next += ConfigRepeatInterval;
            PerformClick();

            if (!IsAvailable() || !CanExecute)
                _held = false;
        }
    }

    /// <inheritdoc/>
    protected override void OnFocusLost()
    {
        base.OnFocusLost();

        if (_keyboard)
            OnInteractionCancelled();
    }

    /// <inheritdoc/>
    protected override void OnInteractionCancelled()
    {
        _held = false;
        _keyboard = false;
        _inside = false;

        base.OnInteractionCancelled();
    }

    internal float ConfigRepeatDelay
    {
        get => _styleRepeatDelay.Value;
        set
        {
            if (_styleRepeatDelay.SetLocal(value))
                NotifyPropertyChanged(RepeatDelayChangedArgs);
        }
    }

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
    /// Applies the control's explicit style values without replacing
    /// locally assigned property values.
    /// </summary>
    /// <param name="style">The style containing the values to apply.</param>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);

        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case "RepeatDelay":
                    if (_styleRepeatDelay.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(RepeatDelayChangedArgs);
                    break;

                case "RepeatInterval":
                    if (_styleRepeatInterval.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(RepeatIntervalChangedArgs);
                    break;
            }
        }
    }

    /// <inheritdoc/>
    internal override bool IsStyleLocal(string property)
    {
        return property switch
        {
            "RepeatDelay" => _styleRepeatDelay.HasLocalValue,
            "RepeatInterval" => _styleRepeatInterval.HasLocalValue,
            _ => base.IsStyleLocal(property)
        };
    }

    /// <summary>
    /// Removes a local property override and restores its current style value.
    /// </summary>
    /// <param name="property">The name of the property to reset.</param>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "RepeatDelay":
                if (_styleRepeatDelay.ClearLocal())
                    NotifyPropertyChanged(RepeatDelayChangedArgs);
                return;

            case "RepeatInterval":
                if (_styleRepeatInterval.ClearLocal())
                    NotifyPropertyChanged(RepeatIntervalChangedArgs);
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

        if (_styleRepeatDelay.BeginStyle(0.4f))
            NotifyPropertyChanged(RepeatDelayChangedArgs);

        if (_styleRepeatInterval.BeginStyle(0.075f))
            NotifyPropertyChanged(RepeatIntervalChangedArgs);
    }

    /// <inheritdoc/>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();

        if (_styleRepeatDelay.CommitStyle())
            NotifyPropertyChanged(RepeatDelayChangedArgs);

        if (_styleRepeatInterval.CommitStyle())
            NotifyPropertyChanged(RepeatIntervalChangedArgs);
    }
}

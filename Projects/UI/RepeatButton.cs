using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;
using System;

namespace Sachssoft.Engine.Controls;
/// <summary>Activates on press and repeatedly while held, using only supplied frame time.</summary>
public class RepeatButton : ButtonBase
{
    private ChangeTracker<float> _delay, _interval;
    private bool _held, _keyboard, _inside;
    private Keys _key;
    private ControlPointer _pointer;
    private double _elapsed, _next;
    /// <summary>Creates a repeating command button.</summary>
    public RepeatButton() : base()
    {
    }

    /// <summary>Gets or sets the delay in seconds before repeating.</summary>
    public float RepeatDelay { get => ConfigRepeatDelay; set => ConfigRepeatDelay = value; }
    /// <summary>Gets or sets seconds between repeats.</summary>
    public float RepeatInterval { get => ConfigRepeatInterval; set => ConfigRepeatInterval = value; }
    /// <inheritdoc/>
    public override bool IsHeld => _held && (_keyboard || _inside);
    /// <inheritdoc/>
    protected override string DefaultStyleName => "RepeatButton";

    internal override void Prepare()
    {
        base.Prepare();
        _delay.EnsureChange(ConfigRepeatDelay);
        _interval.EnsureChange(ConfigRepeatInterval);
        if (!float.IsFinite(_delay.NewValue) || _delay.NewValue < 0 || !float.IsFinite(_interval.NewValue) || _interval.NewValue < .01f)
            throw new InvalidOperationException("Invalid repeat timing.");
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        var e = args.Input;
        if (e.Type == ControlInputEventType.PointerDown && e.Button == MouseButton.Left && !_held)
        {
            _held = true;
            _inside = true;
            _keyboard = false;
            _pointer = e.Pointer;
            context.CapturePointer(this);
            Begin();
            args.Handled = true;
        }
        else if (e.Type == ControlInputEventType.PointerMove && _held && !_keyboard && e.Pointer == _pointer)
        {
            _inside = context.Workspace.PointerHits(this, e.Position);
            args.Handled = true;
        }
        else if (e.Type == ControlInputEventType.PointerUp && e.Button == MouseButton.Left && _held && !_keyboard && e.Pointer == _pointer)
        {
            _held = false;
            context.ReleasePointer(this);
            args.Handled = true;
        }
        else if (e.Type == ControlInputEventType.KeyDown && IsFocused && e.Key is Keys.Space or Keys.Enter)
        {
            if (!_held && !e.IsRepeat)
            {
                _held = true;
                _keyboard = true;
                _key = e.Key;
                Begin();
            }

            args.Handled = true;
        }
        else if (e.Type == ControlInputEventType.KeyUp && _held && _keyboard && e.Key == _key)
        {
            _held = false;
            args.Handled = true;
        }
    }

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
        if (_keyboard)
            OnInteractionCancelled();
    }

    /// <inheritdoc/>
    protected override void OnInteractionCancelled()
    {
        _held = false;
        base.OnInteractionCancelled();
    }

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

    /// <summary>Applies this control's explicit style values without replacing local assignments.</summary>
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

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case "RepeatDelay":
                return _styleRepeatDelay.HasLocalValue;
            case "RepeatInterval":
                return _styleRepeatInterval.HasLocalValue;
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>Removes a local override and restores the current style value.</summary>
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

    /// <summary>Commits final style values and notifies once per effective property change.</summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleRepeatDelay.CommitStyle())
            NotifyPropertyChanged(RepeatDelayChangedArgs);
        if (_styleRepeatInterval.CommitStyle())
            NotifyPropertyChanged(RepeatIntervalChangedArgs);
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;
using System;
using System.Globalization;
using System.ComponentModel;
using System.Windows.Input;

namespace Sachssoft.Engine.Controls;

/// <summary>Provides an interactive clockwise angle dial with keyboard, pointer, and mouse-wheel input.</summary>
/// <remarks>Zero degrees points right, 90 degrees points down, and all values are normalized to [0, 360). Editing supports optional snapping and command execution after completion.</remarks>
public sealed class RotationControl : Control
{
    private ChangeTracker<float> _observed;
    private ControlPointer? _pointer;
    private float _startAngle, _lastWritten;
    private bool _startLocal;
    private Vector2 _center;
    private float _radius;
    /// <summary>Initializes a new instance of the <see cref="RotationControl"/> class.</summary>
    public RotationControl()
    {
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "RotationControl";

    /// <summary>Normalizes a finite angle to the interval [0, 360) degrees.</summary>
    /// <param name="degrees">The angle in degrees.</param>
    /// <returns>The normalized angle in degrees.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value is NaN or infinite.</exception>
    public static float NormalizeAngle(float degrees)
    {
        if (!float.IsFinite(degrees))
            throw new ArgumentOutOfRangeException(nameof(degrees));
        float angle = degrees % 360;
        if (angle < 0)
            angle += 360;
        return angle >= 360 || angle == 0 ? 0 : angle;
    }

    /// <summary>Gets or sets the clockwise angle in degrees, normalized to [0, 360).</summary>
    /// <exception cref="ArgumentOutOfRangeException">The assigned angle is not finite.</exception>
    public float AngleDegrees
    {
        get => ConfigAngleDegrees;
        set
        {
            float next = NormalizeAngle(value), before = AngleDegrees;
            if (!_observed.WasEnsured)
                _observed.Set(before);
            ConfigAngleDegrees = next;
            Notify();
        }
    }

    /// <summary>Gets or sets the positive angle step used by arrow keys and the mouse wheel.</summary>
    /// <remarks>The default is one degree. Valid values are finite and in (0, 360].</remarks>
    public float AngleIncrement { get => ConfigAngleIncrement; set => ConfigAngleIncrement = value; }
    /// <summary>Gets or sets the angle interval used for Shift snapping and page-key adjustments.</summary>
    /// <remarks>The default is 15 degrees. Valid values are finite and in (0, 360].</remarks>
    public float AngleSnap { get => ConfigAngleSnap; set => ConfigAngleSnap = value; }
    /// <summary>Gets or sets whether user input is prevented from changing the angle.</summary>
    public bool IsReadOnly { get => ConfigIsReadOnly; set => ConfigIsReadOnly = value; }
    /// <summary>Gets or sets the command executed after a completed angle change.</summary>
    /// <remarks>When <see cref="CommandParameter"/> is null, the current angle is passed as the command parameter.</remarks>
    public ICommand? Command { get => ConfigCommand; set => ConfigCommand = value; }
    /// <summary>Gets or sets the parameter passed to the completion command.</summary>
    /// <remarks>A null value uses <see cref="AngleDegrees"/> instead.</remarks>
    public object? CommandParameter { get => ConfigCommandParameter; set => ConfigCommandParameter = value; }
    /// <summary>Gets whether a pointer drag is currently active.</summary>
    public bool IsEditing => _pointer is not null;
    /// <summary>Gets the center of the dial in root logical coordinates.</summary>
    public Vector2 DialCenter => _center;
    /// <summary>Gets the dial radius in logical units.</summary>
    public float DialRadius => _radius;

    /// <summary>Occurs when the effective angle changes, including changes from styles.</summary>
    public event EventHandler? AngleChanged;
    /// <summary>Occurs when a pointer editing session starts, before the first angle change.</summary>
    public event EventHandler? EditStarted;
    /// <summary>Occurs when a pointer editing session completes successfully, before the command executes.</summary>
    public event EventHandler? EditCompleted;
    /// <summary>Occurs after a pointer editing session is cancelled and any eligible local change is restored.</summary>
    public event EventHandler? EditCancelled;
    private object Parameter => CommandParameter ?? AngleDegrees;
    private bool CanEdit => !IsReadOnly && (Command?.CanExecute(Parameter) ?? true);

    private void Validate()
    {
        _ = AngleDegrees;
        if (!float.IsFinite(AngleIncrement) || AngleIncrement <= 0 || AngleIncrement > 360 ||
            !float.IsFinite(AngleSnap) || AngleSnap <= 0 || AngleSnap > 360)
            throw new InvalidOperationException("AngleIncrement and AngleSnap must be finite and within (0,360].");
    }

    private void Notify()
    {
        float angle = AngleDegrees;
        bool ready = _observed.WasEnsured;
        if (_observed.EnsureChange(angle) && ready)
            AngleChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        Validate();
        return new(128, 128);
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 bounds)
    {
        _center = new(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
        _radius = float.Max(0, float.Min(bounds.Width, bounds.Height) / 2 - 6);
    }

    /// <inheritdoc/>
    protected override void OnUpdate(TimeSpan elapsed)
    {
        base.OnUpdate(elapsed);
        Validate();
        if (IsEditing && (!CanEdit || AngleDegrees != _lastWritten))
            Stop(true, AngleDegrees == _lastWritten);
        Notify();
    }

    private void Position(Vector2 point, bool snap)
    {
        Vector2 delta = point - _center;
        // Ignore the unstable angle very near the pivot.
        if (delta.LengthSquared() < 16)
            return;
        float angle = NormalizeAngle(float.Atan2(delta.Y, delta.X) * 180 / float.Pi);
        if (snap)
            angle = NormalizeAngle(float.Round(angle / AngleSnap) * AngleSnap);
        _lastWritten = angle;
        AngleDegrees = angle;
    }

    private void Execute()
    {
        var command = Command;
        var parameter = Parameter;
        if (command?.CanExecute(parameter) == true)
            command.Execute(parameter);
    }

    private void Stop(bool cancel, bool restore = true)
    {
        if (_pointer is not ControlPointer pointer)
            return;
        _pointer = null;
        if (cancel && restore && AngleDegrees == _lastWritten)
        {
            AngleDegrees = _startAngle;
            if (!_startLocal)
            {
                ResetStyleProperty("AngleDegrees");
                Notify();
            }
        }

        Workspace?.ReleasePointer(this, pointer);
        if (cancel)
            EditCancelled?.Invoke(this, EventArgs.Empty);
        else
        {
            EditCompleted?.Invoke(this, EventArgs.Empty);
            if (AngleDegrees != _startAngle)
                Execute();
        }
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        var i = args.Input;
        if (i.Type == ControlInputEventType.KeyDown && i.Key == Keys.Escape && IsEditing)
        {
            Stop(true);
            args.Handled = true;
            return;
        }

        if (!CanEdit)
            return;

        if (i.Type == ControlInputEventType.PointerDown && i.Button == MouseButton.Left &&
            !IsEditing && _radius > 0 &&
            Vector2.DistanceSquared(i.Position, _center) <= (_radius + 6) * (_radius + 6))
        {
            context.Focus(this);
            _startAngle = AngleDegrees;
            _lastWritten = _startAngle;
            _startLocal = IsStyleLocal("AngleDegrees");
            _pointer = i.Pointer;
            context.CapturePointer(this);
            EditStarted?.Invoke(this, EventArgs.Empty);
            if (IsEditing)
                Position(i.Position, i.Shift);
            args.Handled = true;
        }
        else if (IsEditing && i.Pointer == _pointer)
        {
            if (i.Type == ControlInputEventType.PointerMove)
            {
                Position(i.Position, i.Shift);
                args.Handled = true;
            }
            else if (i.Type == ControlInputEventType.PointerUp && i.Button == MouseButton.Left)
            {
                Position(i.Position, i.Shift);
                Stop(false);
                args.Handled = true;
            }
        }

        if (IsEditing)
            return;
        float before = AngleDegrees;
        float step = i.Shift ? AngleSnap : AngleIncrement;
        if (i.Type == ControlInputEventType.KeyDown && IsFocused)
        {
            switch (i.Key)
            {
                case Keys.Left:
                case Keys.Down:
                    AngleDegrees = before - step;
                    break;
                case Keys.Right:
                case Keys.Up:
                    AngleDegrees = before + step;
                    break;
                case Keys.PageDown:
                    AngleDegrees = before - AngleSnap;
                    break;
                case Keys.PageUp:
                    AngleDegrees = before + AngleSnap;
                    break;
                case Keys.Home:
                    AngleDegrees = 0;
                    break;
                default:
                    return;
            }

            args.Handled = true;
        }
        else if (i.Type == ControlInputEventType.PointerWheel && IsPointerOver)
        {
            AngleDegrees = (float)((double)before + i.WheelDelta / 120.0 * step);
            args.Handled = true;
        }

        if (AngleDegrees != before)
            Execute();
    }

    /// <inheritdoc/>
    protected override void OnInteractionCancelled() => Stop(true);

    /// <inheritdoc/>
    protected override void OnPointerCaptureLost(ControlPointer pointer)
    {
        if (_pointer == pointer)
            Stop(true);
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext c)
    {
        base.OnDraw(c);
        if (_radius <= 0)
            return;
        Vector2 center = _center - new Vector2(Bounds.X, Bounds.Y);
        string state = !IsEnabled ? "Disabled" : IsEditing ? "Pressed" : IsPointerOver ? "Hover" : "Normal";
        var area = new Bounds2(center.X - _radius, center.Y - _radius, _radius * 2, _radius * 2);
        Color ink = IsEnabled ? ConfigTextColor : ConfigDisabledTextColor;
        if (!c.DrawSkinPart(this, "Dial", area, state))
        {
            Vector2 previous = center + new Vector2(_radius, 0);
            for (int j = 1; j <= 96; j++)
            {
                float a = j * float.Tau / 96;
                Vector2 next = center + new Vector2(float.Cos(a), float.Sin(a)) * _radius;
                c.DrawLine(previous, next, BorderColor, 2);
                previous = next;
            }
        }

        for (int j = 0; j < 12; j++)
        {
            float a = j * float.Tau / 12;
            Vector2 axis = new(float.Cos(a), float.Sin(a));
            c.DrawLine(center + axis * (_radius - 6), center + axis * _radius, BorderColor, j % 3 == 0 ? 2 : 1);
        }

        float radians = AngleDegrees * float.Pi / 180;
        Vector2 direction = new(float.Cos(radians), float.Sin(radians));
        c.DrawLine(center + direction * (_radius * .48f), center + direction * (_radius - 4), ink, 3);
        Vector2 handle = center + direction * (_radius - 4);
        if (!c.DrawSkinPart(this, "Indicator", new Bounds2(handle.X - 5, handle.Y - 5, 10, 10), state))
            c.FillRectangle(new Bounds2(handle.X - 3, handle.Y - 3, 6, 6), ink);
        string text = AngleDegrees.ToString("0.#", CultureInfo.InvariantCulture) + "°";
        var font = GetFont(c);
        var size = font.Measure(text);
        if (size.Width <= _radius * 1.35f && size.Height <= _radius * .8f)
            c.DrawText(font, text, center - new Vector2(size.Width, size.Height) / 2, ink);
    }

    private StyleProperty<bool> _styleIsReadOnly = new(false);
    private static readonly PropertyChangedEventArgs IsReadOnlyChangedArgs = new(nameof(IsReadOnly));
    internal bool ConfigIsReadOnly
    {
        get => _styleIsReadOnly.Value;
        set
        {
            if (_styleIsReadOnly.SetLocal(value))
                NotifyPropertyChanged(IsReadOnlyChangedArgs);
        }
    }

    private StyleProperty<ICommand?> _styleCommand = new(null);
    private static readonly PropertyChangedEventArgs CommandChangedArgs = new(nameof(Command));
    internal ICommand? ConfigCommand
    {
        get => _styleCommand.Value;
        set
        {
            if (_styleCommand.SetLocal(value))
                NotifyPropertyChanged(CommandChangedArgs);
        }
    }

    private StyleProperty<object?> _styleCommandParameter = new(null);
    private static readonly PropertyChangedEventArgs CommandParameterChangedArgs = new(nameof(CommandParameter));
    internal object? ConfigCommandParameter
    {
        get => _styleCommandParameter.Value;
        set
        {
            if (_styleCommandParameter.SetLocal(value))
                NotifyPropertyChanged(CommandParameterChangedArgs);
        }
    }

    private StyleProperty<float> _styleAngleDegrees = new(0f);
    private static readonly PropertyChangedEventArgs AngleDegreesChangedArgs = new(nameof(AngleDegrees));
    internal float ConfigAngleDegrees
    {
        get => _styleAngleDegrees.Value;
        set
        {
            if (_styleAngleDegrees.SetLocal(value))
                NotifyPropertyChanged(AngleDegreesChangedArgs);
        }
    }

    private StyleProperty<float> _styleAngleIncrement = new(1f);
    private static readonly PropertyChangedEventArgs AngleIncrementChangedArgs = new(nameof(AngleIncrement));
    internal float ConfigAngleIncrement
    {
        get => _styleAngleIncrement.Value;
        set
        {
            if (_styleAngleIncrement.SetLocal(value))
                NotifyPropertyChanged(AngleIncrementChangedArgs);
        }
    }

    private StyleProperty<float> _styleAngleSnap = new(15f);
    private static readonly PropertyChangedEventArgs AngleSnapChangedArgs = new(nameof(AngleSnap));
    internal float ConfigAngleSnap
    {
        get => _styleAngleSnap.Value;
        set
        {
            if (_styleAngleSnap.SetLocal(value))
                NotifyPropertyChanged(AngleSnapChangedArgs);
        }
    }

    /// <summary>Applies style values without overriding locally assigned properties.</summary>
    /// <param name="style">The style containing property values to apply.</param>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case "IsReadOnly":
                    if (_styleIsReadOnly.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(IsReadOnlyChangedArgs);
                    break;
                case "Command":
                    if (_styleCommand.Apply(Converters.Convert<ICommand?>("Command", entry.Value)))
                        NotifyPropertyChanged(CommandChangedArgs);
                    break;
                case "CommandParameter":
                    if (_styleCommandParameter.Apply(Converters.Convert<object?>("CommandParameter", entry.Value)))
                        NotifyPropertyChanged(CommandParameterChangedArgs);
                    break;
                case "AngleDegrees":
                    if (_styleAngleDegrees.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(AngleDegreesChangedArgs);
                    break;
                case "AngleIncrement":
                    if (_styleAngleIncrement.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(AngleIncrementChangedArgs);
                    break;
                case "AngleSnap":
                    if (_styleAngleSnap.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(AngleSnapChangedArgs);
                    break;
            }
        }
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case "IsReadOnly":
                return _styleIsReadOnly.HasLocalValue;
            case "Command":
                return _styleCommand.HasLocalValue;
            case "CommandParameter":
                return _styleCommandParameter.HasLocalValue;
            case "AngleDegrees":
                return _styleAngleDegrees.HasLocalValue;
            case "AngleIncrement":
                return _styleAngleIncrement.HasLocalValue;
            case "AngleSnap":
                return _styleAngleSnap.HasLocalValue;
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>Removes a local property override and restores its effective style value.</summary>
    /// <param name="property">The property name to reset.</param>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "IsReadOnly":
                if (_styleIsReadOnly.ClearLocal())
                    NotifyPropertyChanged(IsReadOnlyChangedArgs);
                return;
            case "Command":
                if (_styleCommand.ClearLocal())
                    NotifyPropertyChanged(CommandChangedArgs);
                return;
            case "CommandParameter":
                if (_styleCommandParameter.ClearLocal())
                    NotifyPropertyChanged(CommandParameterChangedArgs);
                return;
            case "AngleDegrees":
                if (_styleAngleDegrees.ClearLocal())
                    NotifyPropertyChanged(AngleDegreesChangedArgs);
                return;
            case "AngleIncrement":
                if (_styleAngleIncrement.ClearLocal())
                    NotifyPropertyChanged(AngleIncrementChangedArgs);
                return;
            case "AngleSnap":
                if (_styleAngleSnap.ClearLocal())
                    NotifyPropertyChanged(AngleSnapChangedArgs);
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
        if (_styleIsReadOnly.BeginStyle(false))
            NotifyPropertyChanged(IsReadOnlyChangedArgs);
        if (_styleCommand.BeginStyle(null))
            NotifyPropertyChanged(CommandChangedArgs);
        if (_styleCommandParameter.BeginStyle(null))
            NotifyPropertyChanged(CommandParameterChangedArgs);
        if (_styleAngleDegrees.BeginStyle(0f))
            NotifyPropertyChanged(AngleDegreesChangedArgs);
        if (_styleAngleIncrement.BeginStyle(1f))
            NotifyPropertyChanged(AngleIncrementChangedArgs);
        if (_styleAngleSnap.BeginStyle(15f))
            NotifyPropertyChanged(AngleSnapChangedArgs);
    }

    /// <inheritdoc/>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleIsReadOnly.CommitStyle())
            NotifyPropertyChanged(IsReadOnlyChangedArgs);
        if (_styleCommand.CommitStyle())
            NotifyPropertyChanged(CommandChangedArgs);
        if (_styleCommandParameter.CommitStyle())
            NotifyPropertyChanged(CommandParameterChangedArgs);
        if (_styleAngleDegrees.CommitStyle())
            NotifyPropertyChanged(AngleDegreesChangedArgs);
        if (_styleAngleIncrement.CommitStyle())
            NotifyPropertyChanged(AngleIncrementChangedArgs);
        if (_styleAngleSnap.CommitStyle())
            NotifyPropertyChanged(AngleSnapChangedArgs);
    }
}

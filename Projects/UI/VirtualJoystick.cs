using Microsoft.Xna.Framework;
using Sachssoft.Engine.Gameplay.Curves;
using Sachssoft.Engine.Input;
using System;

namespace Sachssoft.Engine.Controls;
/// <summary>
/// Deterministic analog joystick with independent mouse/touch capture and constrained axes.
/// </summary>
public class VirtualJoystick : Control
{
    private ChangeTracker<VirtualJoystickSettings> _settings;
    private Vector2 _center, _position, _raw, _value;
    private ControlPointer? _pointer;
    private float _radius, _thumb;
    private bool _centerReady;
    /// <summary>
    /// Creates a joystick with a typed properties.
    /// </summary>
    public VirtualJoystick() : base()
    {
    }

    /// <summary>
    /// Gets normalized output. Positive Y points down unless InvertY is enabled.
    /// </summary>
    public Vector2 Value => _value;
    /// <summary>
    /// Gets the physical normalized knob displacement before dead zone, curve or inversion.
    /// </summary>
    public Vector2 RawValue => _raw;
    /// <summary>
    /// Gets whether one pointer is currently held.
    /// </summary>
    public bool IsActive => _pointer.HasValue;
    /// <summary>
    /// Gets the held pointer identity.
    /// </summary>
    public ControlPointer? ActivePointer => _pointer;
    /// <summary>
    /// Gets the current center in root coordinates.
    /// </summary>
    public Vector2 Center => _center;
    /// <summary>
    /// Gets the actual travel radius after available layout space is considered.
    /// </summary>
    public float EffectiveRadius => _radius;

    /// <summary>
    /// Occurs before output changes for a newly captured pointer.
    /// </summary>
    public event EventHandler? Started;
    /// <summary>
    /// Occurs only when normalized output changes.
    /// </summary>
    public event EventHandler? ValueChanged;
    /// <summary>
    /// Occurs after release, with capture already released.
    /// </summary>
    public event EventHandler? Completed;
    /// <summary>
    /// Occurs after cancellation; output is always zero.
    /// </summary>
    public event EventHandler? Cancelled;
    /// <inheritdoc/>
    protected override string DefaultStyleName => "VirtualJoystick";

    /// <summary>
    /// Gets or sets joystick MovementMode.
    /// </summary>
    public JoystickMovementMode MovementMode
    {
        get
        {
            return ConfigMovementMode;
        }

        set => ConfigMovementMode = value;
    }

    /// <summary>
    /// Gets or sets joystick PlacementMode.
    /// </summary>
    public JoystickPlacementMode PlacementMode
    {
        get
        {
            return ConfigPlacementMode;
        }

        set => ConfigPlacementMode = value;
    }

    /// <summary>
    /// Gets or sets joystick CustomAxis.
    /// </summary>
    public Vector2 CustomAxis
    {
        get
        {
            return ConfigCustomAxis;
        }

        set => ConfigCustomAxis = value;
    }

    /// <summary>
    /// Gets or sets joystick Radius.
    /// </summary>
    public float Radius
    {
        get
        {
            return ConfigRadius;
        }

        set => ConfigRadius = value;
    }

    /// <summary>
    /// Gets or sets joystick ThumbSize.
    /// </summary>
    public Size2 ThumbSize
    {
        get
        {
            return ConfigThumbSize;
        }

        set => ConfigThumbSize = value;
    }

    /// <summary>
    /// Gets or sets joystick DeadZone.
    /// </summary>
    public float DeadZone
    {
        get
        {
            return ConfigDeadZone;
        }

        set => ConfigDeadZone = value;
    }

    /// <summary>
    /// Gets or sets joystick ReturnToCenter.
    /// </summary>
    public bool ReturnToCenter
    {
        get
        {
            return ConfigReturnToCenter;
        }

        set => ConfigReturnToCenter = value;
    }

    /// <summary>
    /// Gets or sets joystick InvertX.
    /// </summary>
    public bool InvertX
    {
        get
        {
            return ConfigInvertX;
        }

        set => ConfigInvertX = value;
    }

    /// <summary>
    /// Gets or sets joystick InvertY.
    /// </summary>
    public bool InvertY
    {
        get
        {
            return ConfigInvertY;
        }

        set => ConfigInvertY = value;
    }

    /// <summary>
    /// Gets or sets joystick ResponseCurve.
    /// </summary>
    public Sachssoft.Engine.Gameplay.Curves.CurveBase? ResponseCurve
    {
        get
        {
            return ConfigResponseCurve;
        }

        set => ConfigResponseCurve = value;
    }

    internal override void Prepare()
    {
        base.Prepare();
        var d = this;
        if (d.ConfigMovementMode is not (JoystickMovementMode.Free or JoystickMovementMode.Horizontal or JoystickMovementMode.Vertical or JoystickMovementMode.CustomAxis) || d.ConfigPlacementMode is not (JoystickPlacementMode.Fixed or JoystickPlacementMode.Floating))
            throw new InvalidOperationException("Invalid joystick mode.");
        if (!float.IsFinite(d.ConfigRadius) || d.ConfigRadius <= 0 || !float.IsFinite(d.ConfigThumbSize.Width) || !float.IsFinite(d.ConfigThumbSize.Height) || d.ConfigThumbSize.Width <= 0 || d.ConfigThumbSize.Height <= 0 || !float.IsFinite(d.ConfigDeadZone) || d.ConfigDeadZone < 0 || d.ConfigDeadZone >= 1)
            throw new InvalidOperationException("Invalid joystick size or dead zone.");
        if (!float.IsFinite(d.ConfigCustomAxis.X) || !float.IsFinite(d.ConfigCustomAxis.Y) || (d.ConfigMovementMode == JoystickMovementMode.CustomAxis && d.ConfigCustomAxis == Vector2.Zero))
            throw new InvalidOperationException("Custom joystick axis must be finite and nonzero.");
        var next = new VirtualJoystickSettings(d.ConfigMovementMode, d.ConfigPlacementMode, d.ConfigCustomAxis, d.ConfigRadius, d.ConfigThumbSize, d.ConfigDeadZone, d.ConfigReturnToCenter, d.ConfigInvertX, d.ConfigInvertY, d.ConfigResponseCurve);
        bool cancel = _settings.WasEnsured && (next.MovementMode != _settings.NewValue.MovementMode || next.PlacementMode != _settings.NewValue.PlacementMode);
        if (_settings.EnsureChange(next) && cancel)
            Reset();
        if (!IsAvailable())
            Reset();
    }

    private float ThumbDiameter => MathF.Min(ConfigThumbSize.Width, ConfigThumbSize.Height);
    private Vector2 Axis => ConfigMovementMode switch
    {
        JoystickMovementMode.Horizontal => Vector2.UnitX,
        JoystickMovementMode.Vertical => Vector2.UnitY,
        JoystickMovementMode.CustomAxis => Normalize(ConfigCustomAxis),
        _ => Vector2.Zero
    };

    private static Vector2 Normalize(Vector2 v)
    {
        double length = Math.Sqrt((double)v.X * v.X + (double)v.Y * v.Y);
        return new Vector2((float)(v.X / length), (float)(v.Y / length));
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        float diameter = ConfigRadius * 2 + ThumbDiameter;
        return ConfigMovementMode switch
        {
            JoystickMovementMode.Horizontal => new Size2(MathF.Min(available.Width, diameter), MathF.Min(available.Height, ThumbDiameter)),
            JoystickMovementMode.Vertical => new Size2(MathF.Min(available.Width, ThumbDiameter), MathF.Min(available.Height, diameter)),
            _ => new Size2(MathF.Min(available.Width, diameter), MathF.Min(available.Height, diameter))};
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 b)
    {
        _thumb = MathF.Min(ThumbDiameter, MathF.Min(b.Width, b.Height));
        Vector2 axis = Axis;
        float x = MathF.Max(0, (b.Width - _thumb) / 2), y = MathF.Max(0, (b.Height - _thumb) / 2);
        float limit = ConfigMovementMode == JoystickMovementMode.Free ? MathF.Min(x, y) : MathF.Min(MathF.Abs(axis.X) > 0.000001f ? x / MathF.Abs(axis.X) : float.PositiveInfinity, MathF.Abs(axis.Y) > 0.000001f ? y / MathF.Abs(axis.Y) : float.PositiveInfinity);
        _radius = MathF.Min(ConfigRadius, limit);
        if (!_centerReady || ConfigPlacementMode == JoystickPlacementMode.Fixed || (!IsActive && ConfigReturnToCenter))
        {
            _center = new Vector2(b.X + b.Width / 2, b.Y + b.Height / 2);
            _centerReady = true;
        }
        else
            _center = ClampCenter(_center);
        if (IsActive)
            UpdatePointer(_position);
        else
            UpdateOutput();
    }

    private Vector2 ClampCenter(Vector2 point)
    {
        Bounds2 b = Bounds;
        return new Vector2(Math.Clamp(point.X, b.X, b.Right), Math.Clamp(point.Y, b.Y, b.Bottom));
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        var input = args.Input;
        if (input.Type == ControlInputEventType.PointerDown && input.Button == MouseButton.Left)
        {
            if (IsActive || _radius <= 0)
                return;
            _pointer = input.Pointer;
            _position = input.Position;
            if (ConfigPlacementMode == JoystickPlacementMode.Floating)
                _center = ClampCenter(input.Position);
            context.CapturePointer(this);
            args.Handled = true;
            Started?.Invoke(this, EventArgs.Empty);
            if (IsActive)
                UpdatePointer(input.Position);
        }
        else if (IsActive && input.Pointer == _pointer)
        {
            if (input.Type == ControlInputEventType.PointerMove)
            {
                UpdatePointer(input.Position);
                args.Handled = true;
            }
            else if (input.Type == ControlInputEventType.PointerUp && input.Button == MouseButton.Left)
            {
                UpdatePointer(input.Position);
                Stop(false);
                args.Handled = true;
            }
        }
    }

    private void UpdatePointer(Vector2 point)
    {
        _position = point;
        if (_radius <= 0)
        {
            _raw = Vector2.Zero;
            UpdateOutput();
            return;
        }

        double x = ((double)point.X - _center.X) / _radius, y = ((double)point.Y - _center.Y) / _radius;
        if (ConfigMovementMode != JoystickMovementMode.Free)
        {
            Vector2 a = Axis;
            double projection = x * a.X + y * a.Y;
            x = projection * a.X;
            y = projection * a.Y;
        }

        double length = Math.Sqrt(x * x + y * y);
        if (length > 1)
        {
            x /= length;
            y /= length;
        }

        _raw = new Vector2((float)x, (float)y);
        UpdateOutput();
    }

    private void UpdateOutput()
    {
        float magnitude = MathF.Min(1, _raw.Length());
        Vector2 next = Vector2.Zero;
        if (magnitude > ConfigDeadZone)
        {
            float input = (magnitude - ConfigDeadZone) / (1 - ConfigDeadZone);
            float response = ConfigResponseCurve?.GetValue(input) ?? input;
            if (!float.IsFinite(response))
                throw new InvalidOperationException("Joystick response curve returned a nonfinite value.");
            next = _raw / magnitude * Math.Clamp(response, 0, 1);
            if (ConfigInvertX)
                next.X = -next.X;
            if (ConfigInvertY)
                next.Y = -next.Y;
        }

        if (next == _value)
            return;
        _value = next;
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Stop(bool cancelled)
    {
        if (_pointer is not ControlPointer pointer)
            return;
        _pointer = null;
        Workspace?.ReleasePointer(this, pointer);
        if (cancelled || ConfigReturnToCenter)
        {
            _raw = Vector2.Zero;
            UpdateOutput();
        }

        if (cancelled)
            Cancelled?.Invoke(this, EventArgs.Empty);
        else
            Completed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Clears held input and output even when ReturnToCenter is disabled.
    /// </summary>
    public void Reset()
    {
        Stop(true);
        _centerReady = false;
        _raw = Vector2.Zero;
        UpdateOutput();
    }

    /// <inheritdoc/>
    protected override void OnInteractionCancelled() => Reset();
    /// <inheritdoc/>
    protected override void OnPointerCaptureLost(ControlPointer pointer)
    {
        if (_pointer == pointer)
            Stop(true);
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        base.OnDraw(context);
        if (_radius <= 0 || _thumb <= 0)
            return;
        Vector2 center = _center - new Vector2(Bounds.X, Bounds.Y);
        string state = !IsEnabled ? "Disabled" : IsActive ? "Pressed" : IsPointerOver ? "Hover" : "Normal";
        Color guide = IsEnabled ? IsActive ? ConfigPressedBackground : BorderColor : ConfigDisabledBackground;
        if (ConfigMovementMode == JoystickMovementMode.Free)
        {
            var area = new Bounds2(center.X - _radius, center.Y - _radius, _radius * 2, _radius * 2);
            if (!context.DrawSkinPart(this, "Base", area, state))
                Circle(context, center, _radius, guide, false);
        }
        else
        {
            Vector2 axis = Axis;
            bool track = false;
            if (ConfigMovementMode == JoystickMovementMode.Horizontal)
                track = context.DrawSkinPart(this, "TrackHorizontal", new Bounds2(center.X - _radius, center.Y - 3, _radius * 2, 6), state);
            else if (ConfigMovementMode == JoystickMovementMode.Vertical)
                track = context.DrawSkinPart(this, "TrackVertical", new Bounds2(center.X - 3, center.Y - _radius, 6, _radius * 2), state);
            if (!track)
                context.DrawLine(center - axis * _radius, center + axis * _radius, guide, 4);
            Vector2 cap = new Vector2(-axis.Y, axis.X) * 6;
            context.DrawLine(center - axis * _radius - cap, center - axis * _radius + cap, guide, 2);
            context.DrawLine(center + axis * _radius - cap, center + axis * _radius + cap, guide, 2);
        }

        Circle(context, center, 3, ConfigTextColor, true);
        Vector2 thumb = center + _raw * _radius;
        var bounds = new Bounds2(thumb.X - _thumb / 2, thumb.Y - _thumb / 2, _thumb, _thumb);
        if (!context.DrawSkinPart(this, "Thumb", bounds, state))
            Circle(context, thumb, _thumb / 2, IsEnabled ? IsActive ? ConfigPressedBackground : ConfigTextColor : ConfigDisabledBackground, true);
    }

    private static void Circle(ControlDrawContext context, Vector2 center, float radius, Color color, bool fill)
    {
        if (fill)
        {
            const int rows = 32;
            float step = 2 * radius / rows;
            for (int i = 0; i < rows; i++)
            {
                float y = -radius + (i + .5f) * step;
                float x = MathF.Sqrt(MathF.Max(0, radius * radius - y * y));
                context.FillRectangle(new Bounds2(center.X - x, center.Y + y - step / 2, x * 2, step), color);
            }
        }
        else
        {
            Vector2 previous = center + new Vector2(radius, 0);
            for (int i = 1; i <= 64; i++)
            {
                float angle = i * MathF.Tau / 64;
                Vector2 next = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
                context.DrawLine(previous, next, color, 2);
                previous = next;
            }
        }
    }

    private StyleProperty<Size2> _styleThumbSize = new(new Size2(24));
    private static readonly System.ComponentModel.PropertyChangedEventArgs ThumbSizeChangedArgs = new(nameof(ThumbSize));
    internal Size2 ConfigThumbSize
    {
        get => _styleThumbSize.Value;
        set
        {
            if (_styleThumbSize.SetLocal(value))
                NotifyPropertyChanged(ThumbSizeChangedArgs);
        }
    }

    private StyleProperty<JoystickMovementMode> _styleMovementMode = new(JoystickMovementMode.Free);
    private static readonly System.ComponentModel.PropertyChangedEventArgs MovementModeChangedArgs = new(nameof(MovementMode));
    internal JoystickMovementMode ConfigMovementMode
    {
        get => _styleMovementMode.Value;
        set
        {
            if (_styleMovementMode.SetLocal(value))
                NotifyPropertyChanged(MovementModeChangedArgs);
        }
    }

    private StyleProperty<JoystickPlacementMode> _stylePlacementMode = new(JoystickPlacementMode.Fixed);
    private static readonly System.ComponentModel.PropertyChangedEventArgs PlacementModeChangedArgs = new(nameof(PlacementMode));
    internal JoystickPlacementMode ConfigPlacementMode
    {
        get => _stylePlacementMode.Value;
        set
        {
            if (_stylePlacementMode.SetLocal(value))
                NotifyPropertyChanged(PlacementModeChangedArgs);
        }
    }

    private StyleProperty<Vector2> _styleCustomAxis = new(Vector2.UnitY);
    private static readonly System.ComponentModel.PropertyChangedEventArgs CustomAxisChangedArgs = new(nameof(CustomAxis));
    internal Vector2 ConfigCustomAxis
    {
        get => _styleCustomAxis.Value;
        set
        {
            if (_styleCustomAxis.SetLocal(value))
                NotifyPropertyChanged(CustomAxisChangedArgs);
        }
    }

    private StyleProperty<float> _styleRadius = new(64f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs RadiusChangedArgs = new(nameof(Radius));
    internal float ConfigRadius
    {
        get => _styleRadius.Value;
        set
        {
            if (_styleRadius.SetLocal(value))
                NotifyPropertyChanged(RadiusChangedArgs);
        }
    }

    private StyleProperty<float> _styleDeadZone = new(0.12f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs DeadZoneChangedArgs = new(nameof(DeadZone));
    internal float ConfigDeadZone
    {
        get => _styleDeadZone.Value;
        set
        {
            if (_styleDeadZone.SetLocal(value))
                NotifyPropertyChanged(DeadZoneChangedArgs);
        }
    }

    private StyleProperty<bool> _styleReturnToCenter = new(true);
    private static readonly System.ComponentModel.PropertyChangedEventArgs ReturnToCenterChangedArgs = new(nameof(ReturnToCenter));
    internal bool ConfigReturnToCenter
    {
        get => _styleReturnToCenter.Value;
        set
        {
            if (_styleReturnToCenter.SetLocal(value))
                NotifyPropertyChanged(ReturnToCenterChangedArgs);
        }
    }

    private StyleProperty<bool> _styleInvertX = new(false);
    private static readonly System.ComponentModel.PropertyChangedEventArgs InvertXChangedArgs = new(nameof(InvertX));
    internal bool ConfigInvertX
    {
        get => _styleInvertX.Value;
        set
        {
            if (_styleInvertX.SetLocal(value))
                NotifyPropertyChanged(InvertXChangedArgs);
        }
    }

    private StyleProperty<bool> _styleInvertY = new(false);
    private static readonly System.ComponentModel.PropertyChangedEventArgs InvertYChangedArgs = new(nameof(InvertY));
    internal bool ConfigInvertY
    {
        get => _styleInvertY.Value;
        set
        {
            if (_styleInvertY.SetLocal(value))
                NotifyPropertyChanged(InvertYChangedArgs);
        }
    }

    private StyleProperty<Sachssoft.Engine.Gameplay.Curves.CurveBase?> _styleResponseCurve = new(null);
    private static readonly System.ComponentModel.PropertyChangedEventArgs ResponseCurveChangedArgs = new(nameof(ResponseCurve));
    internal Sachssoft.Engine.Gameplay.Curves.CurveBase? ConfigResponseCurve
    {
        get => _styleResponseCurve.Value;
        set
        {
            if (_styleResponseCurve.SetLocal(value))
                NotifyPropertyChanged(ResponseCurveChangedArgs);
        }
    }

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
                case "ThumbSize":
                    if (_styleThumbSize.Apply(Converters.Convert<Size2>("Size2", entry.Value)))
                        NotifyPropertyChanged(ThumbSizeChangedArgs);
                    break;
                case "MovementMode":
                    if (_styleMovementMode.Apply(Converters.Convert<JoystickMovementMode>("JoystickMovementMode", entry.Value)))
                        NotifyPropertyChanged(MovementModeChangedArgs);
                    break;
                case "PlacementMode":
                    if (_stylePlacementMode.Apply(Converters.Convert<JoystickPlacementMode>("JoystickPlacementMode", entry.Value)))
                        NotifyPropertyChanged(PlacementModeChangedArgs);
                    break;
                case "CustomAxis":
                    if (_styleCustomAxis.Apply(Converters.Convert<Vector2>("Vector2", entry.Value)))
                        NotifyPropertyChanged(CustomAxisChangedArgs);
                    break;
                case "Radius":
                    if (_styleRadius.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(RadiusChangedArgs);
                    break;
                case "DeadZone":
                    if (_styleDeadZone.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(DeadZoneChangedArgs);
                    break;
                case "ReturnToCenter":
                    if (_styleReturnToCenter.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(ReturnToCenterChangedArgs);
                    break;
                case "InvertX":
                    if (_styleInvertX.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(InvertXChangedArgs);
                    break;
                case "InvertY":
                    if (_styleInvertY.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(InvertYChangedArgs);
                    break;
                case "ResponseCurve":
                    if (_styleResponseCurve.Apply(Converters.Convert<Sachssoft.Engine.Gameplay.Curves.CurveBase?>("Curve", entry.Value)))
                        NotifyPropertyChanged(ResponseCurveChangedArgs);
                    break;
            }
        }
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case "ThumbSize":
                return _styleThumbSize.HasLocalValue;
            case "MovementMode":
                return _styleMovementMode.HasLocalValue;
            case "PlacementMode":
                return _stylePlacementMode.HasLocalValue;
            case "CustomAxis":
                return _styleCustomAxis.HasLocalValue;
            case "Radius":
                return _styleRadius.HasLocalValue;
            case "DeadZone":
                return _styleDeadZone.HasLocalValue;
            case "ReturnToCenter":
                return _styleReturnToCenter.HasLocalValue;
            case "InvertX":
                return _styleInvertX.HasLocalValue;
            case "InvertY":
                return _styleInvertY.HasLocalValue;
            case "ResponseCurve":
                return _styleResponseCurve.HasLocalValue;
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
            case "ThumbSize":
                if (_styleThumbSize.ClearLocal())
                    NotifyPropertyChanged(ThumbSizeChangedArgs);
                return;
            case "MovementMode":
                if (_styleMovementMode.ClearLocal())
                    NotifyPropertyChanged(MovementModeChangedArgs);
                return;
            case "PlacementMode":
                if (_stylePlacementMode.ClearLocal())
                    NotifyPropertyChanged(PlacementModeChangedArgs);
                return;
            case "CustomAxis":
                if (_styleCustomAxis.ClearLocal())
                    NotifyPropertyChanged(CustomAxisChangedArgs);
                return;
            case "Radius":
                if (_styleRadius.ClearLocal())
                    NotifyPropertyChanged(RadiusChangedArgs);
                return;
            case "DeadZone":
                if (_styleDeadZone.ClearLocal())
                    NotifyPropertyChanged(DeadZoneChangedArgs);
                return;
            case "ReturnToCenter":
                if (_styleReturnToCenter.ClearLocal())
                    NotifyPropertyChanged(ReturnToCenterChangedArgs);
                return;
            case "InvertX":
                if (_styleInvertX.ClearLocal())
                    NotifyPropertyChanged(InvertXChangedArgs);
                return;
            case "InvertY":
                if (_styleInvertY.ClearLocal())
                    NotifyPropertyChanged(InvertYChangedArgs);
                return;
            case "ResponseCurve":
                if (_styleResponseCurve.ClearLocal())
                    NotifyPropertyChanged(ResponseCurveChangedArgs);
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
        if (_styleThumbSize.BeginStyle(new Size2(24)))
            NotifyPropertyChanged(ThumbSizeChangedArgs);
        if (_styleMovementMode.BeginStyle(JoystickMovementMode.Free))
            NotifyPropertyChanged(MovementModeChangedArgs);
        if (_stylePlacementMode.BeginStyle(JoystickPlacementMode.Fixed))
            NotifyPropertyChanged(PlacementModeChangedArgs);
        if (_styleCustomAxis.BeginStyle(Vector2.UnitY))
            NotifyPropertyChanged(CustomAxisChangedArgs);
        if (_styleRadius.BeginStyle(64f))
            NotifyPropertyChanged(RadiusChangedArgs);
        if (_styleDeadZone.BeginStyle(0.12f))
            NotifyPropertyChanged(DeadZoneChangedArgs);
        if (_styleReturnToCenter.BeginStyle(true))
            NotifyPropertyChanged(ReturnToCenterChangedArgs);
        if (_styleInvertX.BeginStyle(false))
            NotifyPropertyChanged(InvertXChangedArgs);
        if (_styleInvertY.BeginStyle(false))
            NotifyPropertyChanged(InvertYChangedArgs);
        if (_styleResponseCurve.BeginStyle(null))
            NotifyPropertyChanged(ResponseCurveChangedArgs);
    }

    /// <summary>
    /// Commits final style values and notifies once per effective property change.
    /// </summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleThumbSize.CommitStyle())
            NotifyPropertyChanged(ThumbSizeChangedArgs);
        if (_styleMovementMode.CommitStyle())
            NotifyPropertyChanged(MovementModeChangedArgs);
        if (_stylePlacementMode.CommitStyle())
            NotifyPropertyChanged(PlacementModeChangedArgs);
        if (_styleCustomAxis.CommitStyle())
            NotifyPropertyChanged(CustomAxisChangedArgs);
        if (_styleRadius.CommitStyle())
            NotifyPropertyChanged(RadiusChangedArgs);
        if (_styleDeadZone.CommitStyle())
            NotifyPropertyChanged(DeadZoneChangedArgs);
        if (_styleReturnToCenter.CommitStyle())
            NotifyPropertyChanged(ReturnToCenterChangedArgs);
        if (_styleInvertX.CommitStyle())
            NotifyPropertyChanged(InvertXChangedArgs);
        if (_styleInvertY.CommitStyle())
            NotifyPropertyChanged(InvertYChangedArgs);
        if (_styleResponseCurve.CommitStyle())
            NotifyPropertyChanged(ResponseCurveChangedArgs);
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;
using System;

namespace Sachssoft.Engine.Controls;
/// <summary>An independent scrollbar with arrow, page and thumb interaction.</summary>
public sealed class ScrollBar : RangeControl
{
    private bool _dragging;
    private float _grab;
    private int _repeat;
    private double _held, _nextRepeat;
    /// <summary>Initializes a new scroll bar instance.</summary>
    public ScrollBar() : base()
    {
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "ScrollBar";
    /// <summary>Gets or sets viewport size.</summary>
    public float ViewportSize { get => ConfigViewportSize; set => ConfigViewportSize = value; }
    private float AxisLength => Orientation == ControlOrientation.Horizontal ? ContentBounds.Width : ContentBounds.Height;
    private float Arrow => MathF.Min(ConfigScrollBarThickness, AxisLength / 2);
    private float TrackLength => MathF.Max(0, AxisLength - 2 * Arrow);

    private float ThumbLength
    {
        get
        {
            double range = (double)Maximum - Minimum;
            float ratio = range + ViewportSize <= 0 ? 1 : (float)(ViewportSize / (range + ViewportSize));
            return Math.Clamp(TrackLength * ratio, MathF.Min(ConfigMinimumThumbExtent, TrackLength), TrackLength);
        }
    }

    private float ThumbStart => Arrow + (TrackLength - ThumbLength) * Fraction;
    /// <summary>Gets thumb bounds.</summary>
    public Bounds2 ThumbBounds => AxisRect(ThumbStart, ThumbLength, true);

    private Bounds2 AxisRect(float offset, float length, bool absolute = false)
    {
        var b = ContentBounds;
        float x = absolute ? b.X : Padding.Left, y = absolute ? b.Y : Padding.Top;
        return Orientation == ControlOrientation.Horizontal ? new Bounds2(x + offset, y + 2, length, MathF.Max(0, b.Height - 4)) : new Bounds2(x + 2, y + offset, MathF.Max(0, b.Width - 4), length);
    }

    private float Coordinate(Vector2 p) => Orientation == ControlOrientation.Horizontal ? p.X - ContentBounds.X : p.Y - ContentBounds.Y;
    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        ValidateRange();
        if (!float.IsFinite(ViewportSize) || ViewportSize < 0)
            throw new InvalidOperationException("Invalid scrollbar viewport.");
        float t = ConfigScrollBarThickness;
        if (!float.IsFinite(t) || t <= 0)
            throw new InvalidOperationException("Invalid scrollbar thickness.");
        return Orientation == ControlOrientation.Horizontal ? new Size2(120, t) : new Size2(t, 120);
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        var input = args.Input;
        float p = Coordinate(input.Position);
        if (input.Type == ControlInputEventType.KeyDown)
        {
            switch (input.Key)
            {
                case Keys.Left:
                case Keys.Up:
                    Value -= SmallChange;
                    break;
                case Keys.Right:
                case Keys.Down:
                    Value += SmallChange;
                    break;
                case Keys.PageUp:
                    Value -= LargeChange;
                    break;
                case Keys.PageDown:
                    Value += LargeChange;
                    break;
                default:
                    base.OnInput(context, args);
                    return;
            }

            args.Handled = true;
            return;
        }

        if (input.Type == ControlInputEventType.PointerDown && input.Button == MouseButton.Left)
        {
            context.CapturePointer(this);
            _held = 0;
            _nextRepeat = .4;
            if (p < Arrow)
            {
                _repeat = -1;
                Value -= SmallChange;
            }
            else if (p >= AxisLength - Arrow)
            {
                _repeat = 1;
                Value += SmallChange;
            }
            else if (p >= ThumbStart && p < ThumbStart + ThumbLength)
            {
                _grab = p - ThumbStart;
                _dragging = true;
            }
            else
                Value += p < ThumbStart ? -LargeChange : LargeChange;
            args.Handled = true;
        }
        else if (input.Type == ControlInputEventType.PointerMove && _dragging)
        {
            float length = TrackLength - ThumbLength;
            if (length > 0)
                SetFraction((p - Arrow - _grab) / length);
            args.Handled = true;
        }
        else if (input.Type == ControlInputEventType.PointerUp && input.Button == MouseButton.Left)
        {
            context.ReleasePointer(this);
            args.Handled = true;
        }
        else if (input.Type == ControlInputEventType.PointerWheel)
        {
            Value -= input.WheelDelta / 120f * SmallChange;
            args.Handled = true;
        }
        else
            base.OnInput(context, args);
    }

    /// <inheritdoc/>
    protected override void OnUpdate(TimeSpan elapsed)
    {
        base.OnUpdate(elapsed);
        if (_repeat == 0)
            return;
        _held += elapsed.TotalSeconds;
        int repeats = 0;
        while (_held >= _nextRepeat && repeats++ < 100)
        {
            Value += _repeat * SmallChange;
            _nextRepeat += .06;
        }

        if (repeats >= 100)
            _nextRepeat = _held + .06;
    }

    /// <inheritdoc/>
    protected override void OnInteractionCancelled()
    {
        _dragging = false;
        _repeat = 0;
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        string style = Orientation == ControlOrientation.Horizontal ? "ScrollBar.Horizontal" : "ScrollBar.Vertical";
        string state = !IsAvailable() ? "Disabled" : _dragging ? "Pressed" : IsPointerOver ? "Hover" : "Normal";
        if (!context.DrawSkinStylePart(style, "Track", AxisRect(Arrow, TrackLength)))
            context.FillRectangle(AxisRect(Arrow, TrackLength), new Color(20, 26, 36));
        if (!context.DrawSkinStylePart(style, "Thumb", AxisRect(ThumbStart, ThumbLength), state))
            context.FillRectangle(AxisRect(ThumbStart, ThumbLength), new Color(90, 120, 145));
        foreach (bool end in new[]
        {
            false,
            true
        }

        )
        {
            Bounds2 b = AxisRect(end ? AxisLength - Arrow : 0, Arrow);
            context.DrawSkinStylePart(style, end ? "IncreaseButton" : "DecreaseButton", b, state);
            context.DrawSkinStylePart(style, end ? "IncreaseGlyph" : "DecreaseGlyph", b, "Normal", IsAvailable() ? Color.White : ConfigDisabledTextColor);
        }
    }

    private StyleProperty<float> _styleViewportSize = new(0f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs ViewportSizeChangedArgs = new(nameof(ViewportSize));
    internal float ConfigViewportSize
    {
        get => _styleViewportSize.Value;
        set
        {
            if (_styleViewportSize.SetLocal(value))
                NotifyPropertyChanged(ViewportSizeChangedArgs);
        }
    }

    private StyleProperty<float> _styleMinimumThumbExtent = new(24f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs MinimumThumbExtentChangedArgs = new(nameof(MinimumThumbExtent));
    internal float ConfigMinimumThumbExtent
    {
        get => _styleMinimumThumbExtent.Value;
        set
        {
            if (_styleMinimumThumbExtent.SetLocal(value))
                NotifyPropertyChanged(MinimumThumbExtentChangedArgs);
        }
    }

    /// <summary>Gets or sets MinimumThumbExtent; a local assignment overrides the style.</summary>
    public float MinimumThumbExtent { get => ConfigMinimumThumbExtent; set => ConfigMinimumThumbExtent = value; }

    private StyleProperty<float> _styleScrollBarThickness = new(20f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs ScrollBarThicknessChangedArgs = new(nameof(ScrollBarThickness));
    internal float ConfigScrollBarThickness
    {
        get => _styleScrollBarThickness.Value;
        set
        {
            if (_styleScrollBarThickness.SetLocal(value))
                NotifyPropertyChanged(ScrollBarThicknessChangedArgs);
        }
    }

    /// <summary>Gets or sets ScrollBarThickness; a local assignment overrides the style.</summary>
    public float ScrollBarThickness { get => ConfigScrollBarThickness; set => ConfigScrollBarThickness = value; }

    /// <summary>Applies this control's explicit style values without replacing local assignments.</summary>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case "ViewportSize":
                    if (_styleViewportSize.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(ViewportSizeChangedArgs);
                    break;
                case "MinimumThumbExtent":
                    if (_styleMinimumThumbExtent.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(MinimumThumbExtentChangedArgs);
                    break;
                case "ScrollBarThickness":
                    if (_styleScrollBarThickness.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(ScrollBarThicknessChangedArgs);
                    break;
            }
        }
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case "ViewportSize":
                return _styleViewportSize.HasLocalValue;
            case "MinimumThumbExtent":
                return _styleMinimumThumbExtent.HasLocalValue;
            case "ScrollBarThickness":
                return _styleScrollBarThickness.HasLocalValue;
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>Removes a local override and restores the current style value.</summary>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "ViewportSize":
                if (_styleViewportSize.ClearLocal())
                    NotifyPropertyChanged(ViewportSizeChangedArgs);
                return;
            case "MinimumThumbExtent":
                if (_styleMinimumThumbExtent.ClearLocal())
                    NotifyPropertyChanged(MinimumThumbExtentChangedArgs);
                return;
            case "ScrollBarThickness":
                if (_styleScrollBarThickness.ClearLocal())
                    NotifyPropertyChanged(ScrollBarThicknessChangedArgs);
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
        if (_styleViewportSize.BeginStyle(0f))
            NotifyPropertyChanged(ViewportSizeChangedArgs);
        if (_styleMinimumThumbExtent.BeginStyle(24f))
            NotifyPropertyChanged(MinimumThumbExtentChangedArgs);
        if (_styleScrollBarThickness.BeginStyle(20f))
            NotifyPropertyChanged(ScrollBarThicknessChangedArgs);
    }

    /// <summary>Commits final style values and notifies once per effective property change.</summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleViewportSize.CommitStyle())
            NotifyPropertyChanged(ViewportSizeChangedArgs);
        if (_styleMinimumThumbExtent.CommitStyle())
            NotifyPropertyChanged(MinimumThumbExtentChangedArgs);
        if (_styleScrollBarThickness.CommitStyle())
            NotifyPropertyChanged(ScrollBarThicknessChangedArgs);
    }
}

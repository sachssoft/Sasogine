using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;

namespace Sachssoft.Engine.Controls;
/// <summary>
/// A numeric range bar that distinguishes a click to edit from a captured drag.
/// </summary>
public class ValueBar : RangeControl
{
    private bool _armed, _isDragging;
    private ControlPointer _pointer;
    private Vector2 _pressPosition;
    private float _pressValue;
    private float _displayValue = float.NaN;
    private string _text = string.Empty;
    private Size2 _textSize;
    private Vector2 _textPosition;
    private static readonly System.ComponentModel.PropertyChangedEventArgs DraggingArgs = new(nameof(IsDragging));
    /// <summary>
    /// Gets whether the current pointer gesture exceeded the drag threshold.
    /// </summary>
    public bool IsDragging { get => _isDragging; private set => SetNotify(ref _isDragging, value, DraggingArgs); }

    /// <summary>
    /// Occurs after a click without dragging, or an Enter/F2 keyboard activation.
    /// </summary>
    public event EventHandler? EditRequested;
    /// <inheritdoc/>
    protected override string DefaultStyleName => "ValueBar";

    /// <summary>
    /// Gets the draggable thumb in root-logical coordinates.
    /// </summary>
    public Bounds2 ThumbBounds
    {
        get
        {
            Bounds2 b = ContentBounds;
            float width = MathF.Min(ThumbWidth, b.Width);
            return new Bounds2(b.X + MathF.Max(0, b.Width - width) * Fraction, b.Y + 3, width, MathF.Max(0, b.Height - 6));
        }
    }

    private void UpdateDrag(Vector2 point)
    {
        Vector2 delta = point - _pressPosition;
        if (!IsDragging && delta.LengthSquared() >= DragThreshold * DragThreshold && delta.LengthSquared() > 0)
            IsDragging = true;
        if (!IsDragging)
            return;
        double length = Math.Max(0, ContentBounds.Width - MathF.Min(ThumbWidth, ContentBounds.Width));
        if (length > 0)
            Value = (float)Math.Clamp(_pressValue + delta.X / length * ((double)Maximum - Minimum), Minimum, Maximum);
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        var e = args.Input;
        if (e.Type == ControlInputEventType.PointerDown && e.Button == MouseButton.Left && !_armed)
        {
            _armed = true;
            _pointer = e.Pointer;
            _pressPosition = e.Position;
            _pressValue = Value;
            IsDragging = false;
            context.CapturePointer(this, _pointer);
            args.Handled = true;
        }
        else if (e.Type == ControlInputEventType.PointerMove && _armed && e.Pointer == _pointer)
        {
            UpdateDrag(e.Position);
            args.Handled = true;
        }
        else if (e.Type == ControlInputEventType.PointerUp && e.Button == MouseButton.Left && _armed && e.Pointer == _pointer)
        {
            UpdateDrag(e.Position);
            bool edit = !IsDragging && context.Workspace.PointerHits(this, e.Position);
            _armed = false;
            IsDragging = false;
            context.ReleasePointer(this, _pointer);
            args.Handled = true;
            if (edit)
                EditRequested?.Invoke(this, EventArgs.Empty);
        }
        else if (e.Type == ControlInputEventType.KeyDown && e.Key == Keys.Escape && _armed)
        {
            Value = Math.Clamp(_pressValue, Minimum, Maximum);
            _armed = false;
            IsDragging = false;
            context.ReleasePointer(this, _pointer);
            args.Handled = true;
        }
        else if (e.Type == ControlInputEventType.KeyDown && !_armed)
        {
            if (e.Key is Keys.Enter or Keys.F2 or Keys.Space)
            {
                if (!e.IsRepeat)
                    EditRequested?.Invoke(this, EventArgs.Empty);
                args.Handled = true;
                return;
            }

            double next = e.Key switch
            {
                Keys.Left or Keys.Down => (double)Value - SmallChange,
                Keys.Right or Keys.Up => (double)Value + SmallChange,
                Keys.PageDown => (double)Value - LargeChange,
                Keys.PageUp => (double)Value + LargeChange,
                Keys.Home => Minimum,
                Keys.End => Maximum,
                _ => double.NaN
            };
            if (!double.IsNaN(next))
            {
                Value = (float)Math.Clamp(next, Minimum, Maximum);
                args.Handled = true;
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnFocusLost()
    {
        if (!_armed)
            base.OnFocusLost();
    }

    /// <inheritdoc/>
    protected override void OnInteractionCancelled()
    {
        _armed = false;
        IsDragging = false;
        base.OnInteractionCancelled();
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        ValidateRange();
        if (!float.IsFinite(DragThreshold) || DragThreshold < 0 || !float.IsFinite(ThumbWidth) || ThumbWidth <= 0)
            throw new InvalidOperationException("Invalid value bar geometry.");
        if (_displayValue != Value)
        {
            _displayValue = Value;
            _text = Value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
        }

        _textSize = (GetFont(context)).Measure(_text);
        return new Size2(MathF.Max(90, _textSize.Width + 20), MathF.Max(28, _textSize.Height + 8));
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 b) => _textPosition = new Vector2(b.X - Bounds.X + (b.Width - _textSize.Width) / 2, b.Y - Bounds.Y + (b.Height - _textSize.Height) / 2);
    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        string state = !IsAvailable() ? "Disabled" : IsDragging ? "Pressed" : IsPointerOver ? "Hover" : "Normal";
        if (!context.DrawSkinPart(this, "Background", LocalBounds, state))
        {
            context.FillRectangle(LocalBounds, Background);
            context.StrokeRectangle(LocalBounds, BorderColor, MathF.Max(1, BorderThickness));
        }

        Bounds2 b = ContentBounds;
        var fill = new Bounds2(b.X - Bounds.X + 3, b.Y - Bounds.Y + 3, MathF.Max(0, b.Width - 6) * Fraction, MathF.Max(0, b.Height - 6));
        if (!context.DrawSkinPart(this, "Fill", fill, state))
            context.FillRectangle(fill, IsAvailable() ? HoverBackground : DisabledBackground);
        Bounds2 thumb = ThumbBounds;
        thumb = new Bounds2(thumb.X - Bounds.X, thumb.Y - Bounds.Y, thumb.Width, thumb.Height);
        if (!context.DrawSkinPart(this, "Thumb", thumb, state))
            context.FillRectangle(thumb, IsAvailable() ? PressedBackground : DisabledBackground);
        context.DrawText(GetFont(context), _text, _textPosition, IsAvailable() ? TextColor : DisabledTextColor);
    }

    private StyleProperty<float> _DragThreshold = new(4);
    private static readonly System.ComponentModel.PropertyChangedEventArgs DragThresholdArgs = new(nameof(DragThreshold));
    /// <summary>
    /// Gets the effective DragThreshold value or assigns a local override.
    /// </summary>
    public float DragThreshold
    {
        get => _DragThreshold.Value;
        set
        {
            if (_DragThreshold.SetLocal(value))
                NotifyPropertyChanged(DragThresholdArgs);
        }
    }

    private StyleProperty<float> _ThumbWidth = new(18);
    private static readonly System.ComponentModel.PropertyChangedEventArgs ThumbWidthArgs = new(nameof(ThumbWidth));
    /// <summary>
    /// Gets the effective ThumbWidth value or assigns a local override.
    /// </summary>
    public float ThumbWidth
    {
        get => _ThumbWidth.Value;
        set
        {
            if (_ThumbWidth.SetLocal(value))
                NotifyPropertyChanged(ThumbWidthArgs);
        }
    }

    /// <inheritdoc/>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
            switch (entry.Key)
            {
                case "DragThreshold":
                    if (_DragThreshold.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(DragThresholdArgs);
                    break;
                case "ThumbWidth":
                    if (_ThumbWidth.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(ThumbWidthArgs);
                    break;
            }
    }

    internal override bool IsStyleLocal(string property) => property switch
    {
        "DragThreshold" => _DragThreshold.HasLocalValue,
        "ThumbWidth" => _ThumbWidth.HasLocalValue,
        _ => base.IsStyleLocal(property)};
    /// <inheritdoc/>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "DragThreshold":
                if (_DragThreshold.ClearLocal())
                    NotifyPropertyChanged(DragThresholdArgs);
                break;
            case "ThumbWidth":
                if (_ThumbWidth.ClearLocal())
                    NotifyPropertyChanged(ThumbWidthArgs);
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
        _DragThreshold.BeginStyle(4);
        _ThumbWidth.BeginStyle(18);
    }

    /// <inheritdoc/>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_DragThreshold.CommitStyle())
            NotifyPropertyChanged(DragThresholdArgs);
        if (_ThumbWidth.CommitStyle())
            NotifyPropertyChanged(ThumbWidthArgs);
    }
}

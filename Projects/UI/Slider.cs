using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;
using System;

namespace Sachssoft.Engine.Controls;
/// <summary>A draggable horizontal or vertical scalar slider.</summary>
public sealed class Slider : RangeControl
{
    private bool _dragging;
    /// <summary>Initializes a new slider instance.</summary>
    public Slider() : base()
    {
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "Slider";
    private float ThumbExtent => Orientation == ControlOrientation.Horizontal ? ConfigThumbSize.Width : ConfigThumbSize.Height;

    /// <summary>Gets thumb bounds.</summary>
    public Bounds2 ThumbBounds
    {
        get
        {
            Size2 size = ConfigThumbSize;
            Bounds2 b = ContentBounds;
            return Orientation == ControlOrientation.Horizontal ? new Bounds2(b.X + MathF.Max(0, b.Width - size.Width) * Fraction, b.Y + (b.Height - size.Height) / 2, size.Width, size.Height) : new Bounds2(b.X + (b.Width - size.Width) / 2, b.Y + MathF.Max(0, b.Height - size.Height) * (1 - Fraction), size.Width, size.Height);
        }
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        ValidateRange();
        Size2 size = ConfigThumbSize;
        ValidateSize(size, false);
        return Orientation == ControlOrientation.Horizontal ? new Size2(120, MathF.Max(size.Height, 24)) : new Size2(MathF.Max(size.Width, 24), 120);
    }

    private void Position(Vector2 point)
    {
        Bounds2 b = ContentBounds;
        bool horizontal = Orientation == ControlOrientation.Horizontal;
        float length = (horizontal ? b.Width : b.Height) - ThumbExtent;
        if (length <= 0)
            return;
        double fraction = ((horizontal ? point.X - b.X : point.Y - b.Y) - ThumbExtent / 2) / length;
        SetFraction(horizontal ? fraction : 1 - fraction);
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        var input = args.Input;
        if (input.Type == ControlInputEventType.PointerDown && input.Button == MouseButton.Left)
        {
            _dragging = true;
            context.CapturePointer(this);
            Position(input.Position);
            args.Handled = true;
        }
        else if (input.Type == ControlInputEventType.PointerMove && _dragging)
        {
            Position(input.Position);
            args.Handled = true;
        }
        else if (input.Type == ControlInputEventType.PointerUp && input.Button == MouseButton.Left && _dragging)
        {
            _dragging = false;
            context.ReleasePointer(this);
            args.Handled = true;
        }
        else
            base.OnInput(context, args);
    }

    /// <inheritdoc/>
    protected override void OnInteractionCancelled() => _dragging = false;
    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        Bounds2 b = ContentBounds;
        float t = ConfigTrackThickness;
        bool horizontal = Orientation == ControlOrientation.Horizontal;
        var track = horizontal ? new Bounds2(Padding.Left + ThumbExtent / 2, Padding.Top + (b.Height - t) / 2, MathF.Max(0, b.Width - ThumbExtent), t) : new Bounds2(Padding.Left + (b.Width - t) / 2, Padding.Top + ThumbExtent / 2, t, MathF.Max(0, b.Height - ThumbExtent));
        string variant = horizontal ? "Slider.Horizontal" : "Slider.Vertical";
        DrawVariant(context, variant, "Track", track, "Normal", new Color(30, 38, 50));
        var fill = horizontal ? new Bounds2(track.X, track.Y, track.Width * Fraction, track.Height) : new Bounds2(track.X, track.Bottom - track.Height * Fraction, track.Width, track.Height * Fraction);
        DrawVariant(context, variant, "Fill", fill, IsAvailable() ? "Normal" : "Disabled", new Color(65, 199, 223));
        Bounds2 thumb = ThumbBounds;
        thumb = new Bounds2(thumb.X - Bounds.X, thumb.Y - Bounds.Y, thumb.Width, thumb.Height);
        if (!context.DrawSkinPart(this, "Thumb", thumb, !IsAvailable() ? "Disabled" : _dragging ? "Pressed" : IsPointerOver ? "Hover" : "Normal"))
            context.FillRectangle(thumb, Color.LightGray);
    }

    private void DrawVariant(ControlDrawContext context, string variant, string part, Bounds2 bounds, string state, Color fallback)
    {
        if (!context.DrawSkinStylePart(variant, part, bounds, state))
            context.FillRectangle(bounds, fallback);
    }

    private StyleProperty<float> _styleTrackThickness = new(8f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs TrackThicknessChangedArgs = new(nameof(TrackThickness));
    internal float ConfigTrackThickness
    {
        get => _styleTrackThickness.Value;
        set
        {
            if (_styleTrackThickness.SetLocal(value))
                NotifyPropertyChanged(TrackThicknessChangedArgs);
        }
    }

    /// <summary>Gets or sets TrackThickness; a local assignment overrides the style.</summary>
    public float TrackThickness { get => ConfigTrackThickness; set => ConfigTrackThickness = value; }

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

    /// <summary>Gets or sets ThumbSize; a local assignment overrides the style.</summary>
    public Size2 ThumbSize { get => ConfigThumbSize; set => ConfigThumbSize = value; }

    /// <summary>Applies this control's explicit style values without replacing local assignments.</summary>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case "TrackThickness":
                    if (_styleTrackThickness.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(TrackThicknessChangedArgs);
                    break;
                case "ThumbSize":
                    if (_styleThumbSize.Apply(Converters.Convert<Size2>("Size2", entry.Value)))
                        NotifyPropertyChanged(ThumbSizeChangedArgs);
                    break;
            }
        }
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case "TrackThickness":
                return _styleTrackThickness.HasLocalValue;
            case "ThumbSize":
                return _styleThumbSize.HasLocalValue;
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>Removes a local override and restores the current style value.</summary>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "TrackThickness":
                if (_styleTrackThickness.ClearLocal())
                    NotifyPropertyChanged(TrackThicknessChangedArgs);
                return;
            case "ThumbSize":
                if (_styleThumbSize.ClearLocal())
                    NotifyPropertyChanged(ThumbSizeChangedArgs);
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
        if (_styleTrackThickness.BeginStyle(8f))
            NotifyPropertyChanged(TrackThicknessChangedArgs);
        if (_styleThumbSize.BeginStyle(new Size2(24)))
            NotifyPropertyChanged(ThumbSizeChangedArgs);
    }

    /// <summary>Commits final style values and notifies once per effective property change.</summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleTrackThickness.CommitStyle())
            NotifyPropertyChanged(TrackThicknessChangedArgs);
        if (_styleThumbSize.CommitStyle())
            NotifyPropertyChanged(ThumbSizeChangedArgs);
    }
}

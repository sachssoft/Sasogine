using System.ComponentModel;

namespace Sachssoft.Engine.Controls;
/// <summary>A two-state mobile-style switch with an optional content label and a skinnable track and thumb.</summary>
/// <remarks>Touch taps, mouse clicks, Space, Enter and commands use the existing ToggleButton activation contract.
/// The default style reserves a 44-pixel logical touch target. IsChecked and IsPressed remain deterministic update state.</remarks>
public class Switch : ToggleButton
{
    /// <summary>Creates an unchecked switch with a horizontal track.</summary>
    public Switch()
    {
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "Switch";
    /// <summary>Gets the track bounds in workspace coordinates.</summary>
    public Bounds2 TrackBounds => new(ContentBounds.X, ContentBounds.Y + (ContentBounds.Height - TrackSize.Height) / 2, TrackSize.Width, TrackSize.Height);

    /// <summary>Gets the thumb bounds in workspace coordinates; checked places the thumb on the right.</summary>
    public Bounds2 ThumbBounds
    {
        get
        {
            Bounds2 track = TrackBounds;
            float inset = (track.Height - ThumbSize) / 2;
            return new Bounds2(IsChecked ? track.Right - inset - ThumbSize : track.X + inset, track.Y + inset, ThumbSize, ThumbSize);
        }
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        if (!float.IsFinite(TrackSize.Width) || !float.IsFinite(TrackSize.Height) || TrackSize.Height <= 0 || TrackSize.Width < TrackSize.Height || !float.IsFinite(ThumbSize) || ThumbSize <= 0 || ThumbSize > TrackSize.Height || !float.IsFinite(TextSpacing) || TextSpacing < 0)
            throw new InvalidOperationException("Switch dimensions must be finite and nonnegative, with a positive thumb fitting inside a horizontal track.");
        float offset = TrackSize.Width + (ContentPresenter is null ? 0 : TextSpacing);
        Size2 content = base.MeasureOverride(context, new Size2(MathF.Max(0, available.Width - offset), available.Height));
        return new Size2(offset + content.Width, MathF.Max(TrackSize.Height, content.Height));
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 bounds)
    {
        float offset = TrackSize.Width + (ContentPresenter is null ? 0 : TextSpacing);
        base.ArrangeOverride(new Bounds2(bounds.X + offset, bounds.Y, MathF.Max(0, bounds.Width - offset), bounds.Height));
    }

    // Literal states avoid ToggleButton.SelectionState's concatenation in the draw loop.
    private string SwitchState => IsChecked ? !IsAvailable() ? "Checked.Disabled" : IsHeld ? "Checked.Pressed" : IsPointerOver ? "Checked.Hover" : "Checked.Normal" : !IsAvailable() ? "Unchecked.Disabled" : IsHeld ? "Unchecked.Pressed" : IsPointerOver ? "Unchecked.Hover" : "Unchecked.Normal";

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        string state = SwitchState;
        bool enabled = IsAvailable();
        Color trackColor = !enabled ? DisabledBackground : IsChecked ? PressedBackground : Background;
        if (enabled && IsPointerOver)
            trackColor = Color.Lerp(trackColor, Color.White, IsHeld ? .16f : .08f);
        Bounds2 track = TrackBounds, thumb = ThumbBounds;
        track = new Bounds2(track.X - Bounds.X, track.Y - Bounds.Y, track.Width, track.Height);
        thumb = new Bounds2(thumb.X - Bounds.X, thumb.Y - Bounds.Y, thumb.Width, thumb.Height);
        if (!context.DrawSkinPart(this, "Track", track, state, trackColor))
            DrawCapsule(context, track, trackColor);
        Color thumbColor = enabled ? ThumbColor : DisabledTextColor;
        if (!context.DrawSkinPart(this, "Thumb", thumb, state, thumbColor))
            DrawCapsule(context, thumb, thumbColor);
    }

    private static void DrawCapsule(ControlDrawContext context, Bounds2 bounds, Color color)
    {
        float radius = bounds.Height / 2;
        const int bands = 32;
        float step = bounds.Height / bands;
        for (int i = 0; i < bands; i++)
        {
            float y = (i + .5f) * step - radius;
            float inset = radius - MathF.Sqrt(MathF.Max(0, radius * radius - y * y));
            context.FillRectangle(new Bounds2(bounds.X + inset, bounds.Y + i * step, bounds.Width - 2 * inset, step), color);
        }
    }

    private StyleProperty<Size2> _styleTrackSize = new(new Size2(52, 28));
    private static readonly PropertyChangedEventArgs TrackSizeArgs = new(nameof(TrackSize));
    /// <summary>Gets or sets the logical track size. Width must be at least its height; both dimensions must be positive.</summary>
    public Size2 TrackSize
    {
        get => _styleTrackSize.Value;
        set
        {
            VerifyTreeMutation();
            if (_styleTrackSize.SetLocal(value))
                NotifyPropertyChanged(TrackSizeArgs);
        }
    }

    private StyleProperty<float> _styleThumbSize = new(24f);
    private static readonly PropertyChangedEventArgs ThumbSizeArgs = new(nameof(ThumbSize));
    /// <summary>Gets or sets the logical thumb diameter. It must be positive and fit inside the track.</summary>
    public float ThumbSize
    {
        get => _styleThumbSize.Value;
        set
        {
            VerifyTreeMutation();
            if (_styleThumbSize.SetLocal(value))
                NotifyPropertyChanged(ThumbSizeArgs);
        }
    }

    private StyleProperty<float> _styleTextSpacing = new(10f);
    private static readonly PropertyChangedEventArgs TextSpacingArgs = new(nameof(TextSpacing));
    /// <summary>Gets or sets the nonnegative logical gap between the track and optional content.</summary>
    public float TextSpacing
    {
        get => _styleTextSpacing.Value;
        set
        {
            VerifyTreeMutation();
            if (_styleTextSpacing.SetLocal(value))
                NotifyPropertyChanged(TextSpacingArgs);
        }
    }

    private StyleProperty<Color> _styleThumbColor = new(Color.White);
    private static readonly PropertyChangedEventArgs ThumbColorArgs = new(nameof(ThumbColor));
    /// <summary>Gets or sets the enabled thumb tint. DisabledTextColor is used when the switch is unavailable.</summary>
    public Color ThumbColor
    {
        get => _styleThumbColor.Value;
        set
        {
            VerifyTreeMutation();
            if (_styleThumbColor.SetLocal(value))
                NotifyPropertyChanged(ThumbColorArgs);
        }
    }

    /// <inheritdoc/>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case "TrackSize":
                    if (_styleTrackSize.Apply(Converters.Convert<Size2>("Size2", entry.Value)))
                        NotifyPropertyChanged(TrackSizeArgs);
                    break;
                case "ThumbSize":
                    if (_styleThumbSize.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(ThumbSizeArgs);
                    break;
                case "TextSpacing":
                    if (_styleTextSpacing.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(TextSpacingArgs);
                    break;
                case "ThumbColor":
                    if (_styleThumbColor.Apply(Converters.Convert<Color>("Color", entry.Value)))
                        NotifyPropertyChanged(ThumbColorArgs);
                    break;
            }
        }
    }

    internal override bool IsStyleLocal(string property) => property switch
    {
        "TrackSize" => _styleTrackSize.HasLocalValue,
        "ThumbSize" => _styleThumbSize.HasLocalValue,
        "TextSpacing" => _styleTextSpacing.HasLocalValue,
        "ThumbColor" => _styleThumbColor.HasLocalValue,
        _ => base.IsStyleLocal(property)};
    /// <inheritdoc/>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "TrackSize":
                if (_styleTrackSize.ClearLocal())
                    NotifyPropertyChanged(TrackSizeArgs);
                break;
            case "ThumbSize":
                if (_styleThumbSize.ClearLocal())
                    NotifyPropertyChanged(ThumbSizeArgs);
                break;
            case "TextSpacing":
                if (_styleTextSpacing.ClearLocal())
                    NotifyPropertyChanged(TextSpacingArgs);
                break;
            case "ThumbColor":
                if (_styleThumbColor.ClearLocal())
                    NotifyPropertyChanged(ThumbColorArgs);
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
        if (_styleTrackSize.BeginStyle(new Size2(52, 28)))
            NotifyPropertyChanged(TrackSizeArgs);
        if (_styleThumbSize.BeginStyle(24f))
            NotifyPropertyChanged(ThumbSizeArgs);
        if (_styleTextSpacing.BeginStyle(10f))
            NotifyPropertyChanged(TextSpacingArgs);
        if (_styleThumbColor.BeginStyle(Color.White))
            NotifyPropertyChanged(ThumbColorArgs);
    }

    /// <inheritdoc/>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleTrackSize.CommitStyle())
            NotifyPropertyChanged(TrackSizeArgs);
        if (_styleThumbSize.CommitStyle())
            NotifyPropertyChanged(ThumbSizeArgs);
        if (_styleTextSpacing.CommitStyle())
            NotifyPropertyChanged(TextSpacingArgs);
        if (_styleThumbColor.CommitStyle())
            NotifyPropertyChanged(ThumbColorArgs);
    }
}

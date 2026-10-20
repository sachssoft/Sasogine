using System;

namespace Sachssoft.Engine.Controls.Layouts;
/// <summary>Wraps children into successive rows or columns in stable insertion order.</summary>
public sealed class WrapPanel : ContainerControl
{
    /// <summary>Initializes a new wrap panel instance.</summary>
    public WrapPanel() : base()
    {
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "WrapPanel";
    /// <summary>Gets or sets orientation.</summary>
    public ControlOrientation Orientation { get => ConfigAxis; set => ConfigAxis = value; }
    /// <summary>Gets or sets spacing.</summary>
    public float Spacing { get => ConfigSpacing; set => ConfigSpacing = value; }
    /// <summary>Gets or sets line spacing.</summary>
    public float LineSpacing { get => ConfigLineSpacing; set => ConfigLineSpacing = value; }

    private Size2 LayoutChildren(Size2 size, Bounds2? bounds)
    {
        if (!float.IsFinite(Spacing) || Spacing < 0 || !float.IsFinite(LineSpacing) || LineSpacing < 0)
            throw new InvalidOperationException("Invalid wrapping spacing.");
        bool horizontal = Orientation == ControlOrientation.Horizontal;
        float limit = horizontal ? size.Width : size.Height;
        float main = 0, cross = 0, lineCross = 0, max = 0;
        bool occupied = false;
        foreach (Control child in Children)
        {
            if (!child.IsVisible)
                continue;
            float childMain = horizontal ? child.DesiredSize.Width : child.DesiredSize.Height, childCross = horizontal ? child.DesiredSize.Height : child.DesiredSize.Width;
            if (occupied && main + Spacing + childMain > limit)
            {
                max = MathF.Max(max, main);
                cross += lineCross + LineSpacing;
                main = 0;
                lineCross = 0;
                occupied = false;
            }

            if (occupied)
                main += Spacing;
            if (bounds is Bounds2 b)
                child.Arrange(horizontal ? new Bounds2(b.X + main, b.Y + cross, childMain, childCross) : new Bounds2(b.X + cross, b.Y + main, childCross, childMain));
            main += childMain;
            lineCross = MathF.Max(lineCross, childCross);
            occupied = true;
        }

        max = MathF.Max(max, main);
        cross += lineCross;
        return horizontal ? new Size2(max, cross) : new Size2(cross, max);
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        foreach (Control child in Children)
            child.Measure(context, available);
        return LayoutChildren(available, null);
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 contentBounds) => LayoutChildren(contentBounds.Size, contentBounds);
    private StyleProperty<float> _styleSpacing = new(0f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs SpacingChangedArgs = new(nameof(Spacing));
    internal float ConfigSpacing
    {
        get => _styleSpacing.Value;
        set
        {
            if (_styleSpacing.SetLocal(value))
                NotifyPropertyChanged(SpacingChangedArgs);
        }
    }

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

    private StyleProperty<float> _styleLineSpacing = new(8f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs LineSpacingChangedArgs = new(nameof(LineSpacing));
    internal float ConfigLineSpacing
    {
        get => _styleLineSpacing.Value;
        set
        {
            if (_styleLineSpacing.SetLocal(value))
                NotifyPropertyChanged(LineSpacingChangedArgs);
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
                case "Spacing":
                    if (_styleSpacing.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(SpacingChangedArgs);
                    break;
                case "Axis":
                    if (_styleAxis.Apply(Converters.Convert<ControlOrientation>("ControlOrientation", entry.Value)))
                        NotifyPropertyChanged(AxisChangedArgs);
                    break;
                case "LineSpacing":
                    if (_styleLineSpacing.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(LineSpacingChangedArgs);
                    break;
            }
        }
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case "Spacing":
                return _styleSpacing.HasLocalValue;
            case "Axis":
                return _styleAxis.HasLocalValue;
            case "LineSpacing":
                return _styleLineSpacing.HasLocalValue;
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>Removes a local override and restores the current style value.</summary>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "Spacing":
                if (_styleSpacing.ClearLocal())
                    NotifyPropertyChanged(SpacingChangedArgs);
                return;
            case "Axis":
                if (_styleAxis.ClearLocal())
                    NotifyPropertyChanged(AxisChangedArgs);
                return;
            case "LineSpacing":
                if (_styleLineSpacing.ClearLocal())
                    NotifyPropertyChanged(LineSpacingChangedArgs);
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
        if (_styleSpacing.BeginStyle(0f))
            NotifyPropertyChanged(SpacingChangedArgs);
        if (_styleAxis.BeginStyle(ControlOrientation.Horizontal))
            NotifyPropertyChanged(AxisChangedArgs);
        if (_styleLineSpacing.BeginStyle(8f))
            NotifyPropertyChanged(LineSpacingChangedArgs);
    }

    /// <summary>Commits final style values and notifies once per effective property change.</summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleSpacing.CommitStyle())
            NotifyPropertyChanged(SpacingChangedArgs);
        if (_styleAxis.CommitStyle())
            NotifyPropertyChanged(AxisChangedArgs);
        if (_styleLineSpacing.CommitStyle())
            NotifyPropertyChanged(LineSpacingChangedArgs);
    }
}

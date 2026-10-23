using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Engine.Controls.Layouts;
/// <summary>
/// Stacks controls in insertion order. ZIndex changes painting, not layout.
/// </summary>
public class StackPanel : ContainerControl
{
    private readonly StackOrientation _defaultOrientation;
    /// <summary>
    /// Initializes a stack panel.
    /// </summary>
    public StackPanel() : this(StackOrientation.Vertical)
    {
    }

    /// <summary>
    /// Creates a stack with a style-resettable orientation fallback.
    /// </summary>
    protected StackPanel(StackOrientation defaultOrientation)
    {
        _defaultOrientation = defaultOrientation;
        _styleOrientation = new(defaultOrientation);
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "StackPanel";
    /// <summary>
    /// Gets the effective Orientation value or sets a local override.
    /// </summary>
    public StackOrientation Orientation { get => ConfigOrientation; set => ConfigOrientation = value; }
    /// <summary>
    /// Gets the effective Spacing value or sets a local override.
    /// </summary>
    public float Spacing { get => ConfigSpacing; set => ConfigSpacing = value; }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        if (!float.IsFinite(Spacing) || Spacing < 0 || Orientation is not (StackOrientation.Vertical or StackOrientation.Horizontal))
            throw new InvalidOperationException("Invalid stack configuration.");
        bool vertical = Orientation == StackOrientation.Vertical;
        float main = 0, cross = 0;
        int count = 0;
        foreach (Control child in Children)
        {
            child.Measure(context, new Size2(vertical ? available.Width : float.PositiveInfinity, vertical ? float.PositiveInfinity : available.Height));
            if (!child.IsVisible)
                continue;
            if (count++ > 0)
                main += Spacing;
            main += vertical ? child.DesiredSize.Height : child.DesiredSize.Width;
            cross = MathF.Max(cross, vertical ? child.DesiredSize.Width : child.DesiredSize.Height);
        }

        return vertical ? new Size2(cross, main) : new Size2(main, cross);
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 contentBounds)
    {
        bool vertical = Orientation == StackOrientation.Vertical;
        float offset = 0;
        foreach (Control child in Children)
        {
            if (!child.IsVisible)
            {
                child.Arrange(Bounds2.Zero);
                continue;
            }

            float extent = vertical ? child.DesiredSize.Height : child.DesiredSize.Width;
            child.Arrange(vertical ? new Bounds2(contentBounds.X, contentBounds.Y + offset, contentBounds.Width, extent) : new Bounds2(contentBounds.X + offset, contentBounds.Y, extent, contentBounds.Height));
            offset += extent + Spacing;
        }
    }

    private StyleProperty<StackOrientation> _styleOrientation = new(StackOrientation.Vertical);
    private static readonly System.ComponentModel.PropertyChangedEventArgs OrientationChangedArgs = new(nameof(Orientation));
    internal StackOrientation ConfigOrientation
    {
        get => _styleOrientation.Value;
        set
        {
            if (_styleOrientation.SetLocal(value))
                NotifyPropertyChanged(OrientationChangedArgs);
        }
    }

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
                case "Orientation":
                    if (_styleOrientation.Apply(Converters.Convert<StackOrientation>("StackOrientation", entry.Value)))
                        NotifyPropertyChanged(OrientationChangedArgs);
                    break;
                case "Spacing":
                    if (_styleSpacing.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(SpacingChangedArgs);
                    break;
            }
        }
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case "Orientation":
                return _styleOrientation.HasLocalValue;
            case "Spacing":
                return _styleSpacing.HasLocalValue;
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
            case "Orientation":
                if (_styleOrientation.ClearLocal())
                    NotifyPropertyChanged(OrientationChangedArgs);
                return;
            case "Spacing":
                if (_styleSpacing.ClearLocal())
                    NotifyPropertyChanged(SpacingChangedArgs);
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
        if (_styleOrientation.BeginStyle(_defaultOrientation))
            NotifyPropertyChanged(OrientationChangedArgs);
        if (_styleSpacing.BeginStyle(0f))
            NotifyPropertyChanged(SpacingChangedArgs);
    }

    /// <summary>
    /// Commits final style values and notifies once per effective property change.
    /// </summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleOrientation.CommitStyle())
            NotifyPropertyChanged(OrientationChangedArgs);
        if (_styleSpacing.CommitStyle())
            NotifyPropertyChanged(SpacingChangedArgs);
    }
}

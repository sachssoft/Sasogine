using System;

namespace Sachssoft.Engine.Controls;

/// <summary>Arranges status strips using automatic, fixed or proportional lengths along its orientation.</summary>
public class StatusBar : StackPanel
{
    /// <summary>Creates a horizontal status bar without a local orientation override.</summary>
    public StatusBar() : base(StackOrientation.Horizontal)
    {
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => nameof(StatusBar);

    /// <inheritdoc/>
    protected override bool DrawsDecoration => true;

    /// <summary>Adds a status strip, reserving its ownership immediately.</summary>
    /// <param name="child">The status strip to add.</param>
    /// <exception cref="ArgumentNullException">The child is null.</exception>
    /// <exception cref="ArgumentException">The child is not a StatusStrip.</exception>
    public override void Add(Control child)
    {
        ArgumentNullException.ThrowIfNull(child);
        if (child is not StatusStrip)
            throw new ArgumentException("StatusBar children must be StatusStrip hosts.", nameof(child));
        base.Add(child);
    }

    /// <summary>Adds a status strip with its current length.</summary>
    /// <param name="statusStrip">The status strip to add.</param>
    public void Add(StatusStrip statusStrip) => Add((Control)statusStrip);

    /// <summary>Adds a status strip and assigns its length after ownership validation succeeds.</summary>
    /// <param name="statusStrip">The status strip to add.</param>
    /// <param name="length">Its fixed length, automatic content size or proportional weight.</param>
    public void Add(StatusStrip statusStrip, GridLength length)
    {
        Add(statusStrip);
        statusStrip.Length = length;
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        if (!float.IsFinite(Spacing) || Spacing < 0 || Orientation is not (StackOrientation.Vertical or StackOrientation.Horizontal))
            throw new InvalidOperationException("Invalid status bar configuration.");
        bool vertical = Orientation == StackOrientation.Vertical;
        float availableMain = vertical ? available.Height : available.Width;
        float availableCross = vertical ? available.Width : available.Height;
        double occupied = 0;
        double weight = 0;
        float cross = 0;
        int count = 0;
        foreach (StatusStrip strip in Children)
        {
            if (!strip.IsVisible)
            {
                strip.Measure(context, Size2.Zero);
                continue;
            }
            count++;
            GridLength length = strip.Length;
            if (length.Unit == GridUnitType.Proportion)
            {
                weight += length.Value;
                continue;
            }
            float constraint = length.Unit == GridUnitType.Pixel ? length.Value : float.PositiveInfinity;
            strip.Measure(context, vertical ? new Size2(availableCross, constraint) : new Size2(constraint, availableCross));
            occupied += length.Unit == GridUnitType.Pixel ? length.Value : vertical ? strip.DesiredSize.Height : strip.DesiredSize.Width;
            cross = MathF.Max(cross, vertical ? strip.DesiredSize.Width : strip.DesiredSize.Height);
        }
        occupied += Math.Max(0, count - 1) * (double)Spacing;
        double remaining = float.IsFinite(availableMain) ? Math.Max(0, availableMain - occupied) : double.PositiveInfinity;
        double desired = occupied;
        foreach (StatusStrip strip in Children)
        {
            if (!strip.IsVisible || strip.Length.Unit != GridUnitType.Proportion)
                continue;
            float extent = strip.Length.Value == 0 ? 0 : float.IsFinite(availableMain)
                ? (float)(remaining * (strip.Length.Value / weight)) : float.PositiveInfinity;
            strip.Measure(context, vertical ? new Size2(availableCross, extent) : new Size2(extent, availableCross));
            desired += float.IsFinite(extent) ? extent : vertical ? strip.DesiredSize.Height : strip.DesiredSize.Width;
            cross = MathF.Max(cross, vertical ? strip.DesiredSize.Width : strip.DesiredSize.Height);
        }
        float main = (float)Math.Min(float.MaxValue, desired);
        return vertical ? new Size2(cross, main) : new Size2(main, cross);
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 bounds)
    {
        bool vertical = Orientation == StackOrientation.Vertical;
        double occupied = 0;
        double weight = 0;
        int count = 0;
        foreach (StatusStrip strip in Children)
        {
            if (!strip.IsVisible)
                continue;
            count++;
            if (strip.Length.Unit == GridUnitType.Proportion)
                weight += strip.Length.Value;
            else
                occupied += strip.Length.Unit == GridUnitType.Pixel ? strip.Length.Value
                    : vertical ? strip.DesiredSize.Height : strip.DesiredSize.Width;
        }
        occupied += Math.Max(0, count - 1) * (double)Spacing;
        double remaining = Math.Max(0, (vertical ? bounds.Height : bounds.Width) - occupied);
        float offset = 0;
        foreach (StatusStrip strip in Children)
        {
            if (!strip.IsVisible)
            {
                strip.Arrange(Bounds2.Zero);
                continue;
            }
            float extent = strip.Length.Unit switch
            {
                GridUnitType.Pixel => strip.Length.Value,
                GridUnitType.Proportion => weight > 0 ? (float)(remaining * (strip.Length.Value / weight)) : 0,
                _ => vertical ? strip.DesiredSize.Height : strip.DesiredSize.Width
            };
            strip.Arrange(vertical ? new Bounds2(bounds.X, bounds.Y + offset, bounds.Width, extent)
                : new Bounds2(bounds.X + offset, bounds.Y, extent, bounds.Height), true);
            offset += extent + Spacing;
        }
    }
}

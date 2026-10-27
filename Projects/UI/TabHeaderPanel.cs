using Sachssoft.Engine.Controls.Drawing;
using Sachssoft.Engine.Controls.Layouts;
using System;
using System.ComponentModel;

namespace Sachssoft.Engine.Controls;

/// <summary>Preserves natural stack layout or shares a tab strip's finite main axis equally among visible headers.</summary>
internal sealed class TabHeaderPanel : StackPanel
{
    /// <summary>Caches notifications for the bounded main-axis extent.</summary>
    private static readonly PropertyChangedEventArgs StretchExtentArgs = new(nameof(StretchExtent));

    /// <summary>Caches notifications when equal header sizing is toggled.</summary>
    private static readonly PropertyChangedEventArgs StretchHeadersArgs = new(nameof(StretchHeaders));

    /// <summary>Tracks whether visible headers share the available main axis.</summary>
    private bool _stretchHeaders;

    /// <summary>Retains the owner's bounded extent while the scroll viewer measures content with an unbounded axis.</summary>
    private float _stretchExtent = float.PositiveInfinity;

    /// <summary>Gets or sets the owning strip's main-axis extent; infinity retains natural sizing until arrangement.</summary>
    internal float StretchExtent
    {
        get => _stretchExtent;
        set
        {
            if (float.IsNaN(value) || value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            SetNotify(ref _stretchExtent, value, StretchExtentArgs);
        }
    }

    /// <summary>Gets or sets whether visible headers share the bounded main axis equally.</summary>
    internal bool StretchHeaders
    {
        get => _stretchHeaders;
        set => SetNotify(ref _stretchHeaders, value, StretchHeadersArgs);
    }

    /// <summary>Updates the arrangement constraint without propagating a new measure invalidation to the arranging owner.</summary>
    /// <param name="extent">The finite, nonnegative main-axis length of the arranged strip.</param>
    /// <returns>True when the caller must explicitly remeasure the header panel for the new constraint.</returns>
    internal bool SetArrangeExtent(float extent)
    {
        if (!float.IsFinite(extent) || extent < 0)
            throw new ArgumentOutOfRangeException(nameof(extent));

        if (_stretchExtent == extent)
            return false;

        _stretchExtent = extent;
        return true;
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        if (!StretchHeaders || !float.IsFinite(StretchExtent))
            return base.MeasureOverride(context, available);
        ValidateStack();
        int count = CountVisibleHeaders();
        if (count == 0)
            return Size2.Zero;
        bool vertical = Orientation == StackOrientation.Vertical;
        float spacing = GetEffectiveSpacing(StretchExtent, count);
        float extent = MathF.Max(0, (StretchExtent - spacing * (count - 1)) / count);
        float cross = 0;
        foreach (Control child in Children)
        {
            child.Measure(context, vertical ? new Size2(available.Width, extent) : new Size2(extent, available.Height));
            if (child.IsVisible)
                cross = MathF.Max(cross, vertical ? child.DesiredSize.Width : child.DesiredSize.Height);
        }

        return vertical ? new Size2(cross, StretchExtent) : new Size2(StretchExtent, cross);
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 contentBounds)
    {
        if (!StretchHeaders)
        {
            base.ArrangeOverride(contentBounds);
            return;
        }

        ValidateStack();
        int count = CountVisibleHeaders();
        bool vertical = Orientation == StackOrientation.Vertical;
        float main = vertical ? contentBounds.Height : contentBounds.Width;
        float spacing = GetEffectiveSpacing(main, count);
        float extent = count == 0 ? 0 : MathF.Max(0, (main - spacing * (count - 1)) / count);
        int index = 0;
        foreach (Control child in Children)
        {
            if (!child.IsVisible)
            {
                child.Arrange(Bounds2.Zero);
                continue;
            }

            float offset = index * (extent + spacing);
            float size = index == count - 1 ? MathF.Max(0, main - offset) : extent;
            child.Arrange(vertical ? new Bounds2(contentBounds.X, contentBounds.Y + offset, contentBounds.Width, size) : new Bounds2(contentBounds.X + offset, contentBounds.Y, size, contentBounds.Height), true);
            index++;
        }
    }

    /// <summary>Counts visible buttons without allocating a filtered collection.</summary>
    /// <returns>The number of headers participating in layout.</returns>
    private int CountVisibleHeaders()
    {
        int count = 0;
        foreach (Control child in Children)
            if (child.IsVisible)
                count++;
        return count;
    }

    /// <summary>Restricts gaps to the available axis so tiny strips never create negative header extents.</summary>
    /// <param name = "extent">The finite main-axis length.</param>
    /// <param name = "count">The visible header count.</param>
    /// <returns>The configured spacing, reduced only when gaps alone would exceed the strip.</returns>
    private float GetEffectiveSpacing(float extent, int count) => count < 2 ? 0 : MathF.Min(Spacing, extent / (count - 1));

    /// <summary>Rejects invalid inherited stack geometry before equal sizing is calculated.</summary>
    private void ValidateStack()
    {
        if (!float.IsFinite(Spacing) || Spacing < 0 || Orientation is not (StackOrientation.Horizontal or StackOrientation.Vertical))
            throw new InvalidOperationException("Invalid tab header stack configuration.");
    }
}

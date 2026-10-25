using Microsoft.Xna.Framework;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;

/// <summary>Exposes arranged text placement and counts layout work for invalidation checks.</summary>
internal sealed class InvalidationTextProbe : TextBlock
{
    /// <summary>Counts actual measure overrides rather than cached Measure calls.</summary>
    internal int Measurements;
    /// <summary>Counts actual arrange overrides rather than cached Arrange calls.</summary>
    internal int Arrangements;
    /// <summary>Gets the cached text origin in local logical coordinates.</summary>
    internal Vector2 Position => TextPosition;

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        Measurements++;
        return base.MeasureOverride(context, available);
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 bounds)
    {
        Arrangements++;
        base.ArrangeOverride(bounds);
    }
}

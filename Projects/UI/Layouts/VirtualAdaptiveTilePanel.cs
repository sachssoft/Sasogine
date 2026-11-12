using Sachssoft.Engine;

namespace Sachssoft.Engine.Controls.Layouts;

/// <summary>Virtualizes uniform tiles and adapts their column count and size to the viewport.</summary>
public sealed class VirtualAdaptiveTilePanel : VirtualizingPanel
{
    /// <summary>Gets the preferred tile size used to determine columns and aspect ratio.</summary>
    public Size2 PreferredTileSize { get; init; } = new(128);

    /// <inheritdoc/>
    protected override Size2 GetSlot(float width, out int columns)
    {
        Size2 preferred = PreferredTileSize;
        if (!float.IsFinite(preferred.Width) || !float.IsFinite(preferred.Height) ||
            preferred.Width <= 0 || preferred.Height <= 0)
            throw new InvalidOperationException("The preferred tile size must be finite and positive.");

        columns = (int)Math.Clamp(Math.Floor((width + (double)Spacing) / (preferred.Width + (double)Spacing)), 1, int.MaxValue);
        float tileWidth = width > 0 ? MathF.Max(0, (width - (columns - 1) * Spacing) / columns) : preferred.Width;
        return new Size2(tileWidth, tileWidth / preferred.Width * preferred.Height);
    }
}

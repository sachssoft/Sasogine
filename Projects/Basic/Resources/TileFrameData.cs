using Sachssoft.Engine;

namespace Sachssoft.Engine.Resources;

/// <summary>
/// Represents a single tile frame within a texture atlas.
/// </summary>
/// <remarks>
/// A tile frame defines the atlas cell position, tile size, and the resulting
/// pixel bounds used to identify the frame within the source texture.
///
/// The structure is immutable and optimized for frequent usage during rendering
/// and batching operations.
/// </remarks>
public readonly struct TileFrameData : ISourceRegion
{
    /// <summary>
    /// Creates a new tile frame.
    /// </summary>
    /// <param name="size">
    /// Size of the tile in pixels.
    /// </param>
    /// <param name="cell">
    /// Atlas cell coordinate of the tile frame.
    /// </param>
    public TileFrameData(
        PixelSize2 size,
        Coordinate2 cell)
        : this(size, cell, 0)
    {
    }

    /// <summary>Creates a tile region on a specified atlas page.</summary>
    /// <param name="size">The tile size in pixels.</param>
    /// <param name="cell">The atlas cell coordinate.</param>
    /// <param name="textureIndex">The nonnegative, zero-based atlas page index.</param>
    /// <exception cref="System.ArgumentOutOfRangeException">The page index is negative.</exception>
    public TileFrameData(PixelSize2 size, Coordinate2 cell, int textureIndex)
    {
        if (textureIndex < 0)
            throw new System.ArgumentOutOfRangeException(nameof(textureIndex));
        TextureIndex = textureIndex;
        Size = size;
        Cell = cell;

        SourceBounds = new PixelBounds2(
            cell.X * size.Width,
            cell.Y * size.Height,
            size.Width,
            size.Height);
    }

    /// <summary>
    /// Gets the size of the tile in pixels.
    /// </summary>
    public PixelSize2 Size
    {
        get;
    }

    /// <summary>
    /// Gets the cell coordinate of the frame inside the texture atlas.
    /// </summary>
    public Coordinate2 Cell
    {
        get;
    }

    /// <summary>
    /// Gets the pixel bounds identifying this frame within the source texture.
    /// </summary>
    public PixelBounds2 SourceBounds
    {
        get;
    }

    /// <summary>Gets the zero-based index of the texture containing this tile.</summary>
    public int TextureIndex
    {
        get;
    }
}

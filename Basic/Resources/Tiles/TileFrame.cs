using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Assets.Graphics;
using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Resources;

/// <summary>
/// Represents a single tile frame within a texture atlas.
///
/// A tile frame defines the atlas cell position, tile size, and the resulting
/// source rectangle used for rendering.
///
/// The structure is immutable and optimized for frequent usage during rendering
/// and batching operations.
/// </summary>
public readonly struct TileFrame
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
    public TileFrame(
        PixelSize size,
        Coordinate2 cell)
    {
        Size = size;
        Cell = cell;

        SourceRectangle = new Rectangle(
            cell.X * size.Width,
            cell.Y * size.Height,
            size.Width,
            size.Height);
    }


    /// <summary>
    /// Gets the size of the tile in pixels.
    /// </summary>
    public PixelSize Size { get; }


    /// <summary>
    /// Gets the cell coordinate of the frame inside the texture atlas.
    /// </summary>
    public Coordinate2 Cell { get; }


    /// <summary>
    /// Gets the source rectangle used to draw this frame from the texture atlas.
    /// </summary>
    public Rectangle SourceRectangle { get; }
}
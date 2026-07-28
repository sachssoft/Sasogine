using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Resources;

/// <summary>
/// Represents a single frame region inside a texture atlas.
/// 
/// A frame defines the pixel location, size, and source rectangle
/// used to extract a region from the texture during rendering.
/// 
/// The structure is immutable and optimized for frequent access
/// during rendering and batching operations.
/// </summary>
public readonly struct FrameData : ISourceRegion
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FrameData"/> struct.
    /// </summary>
    /// <param name="location">
    /// Pixel location of the frame inside the texture atlas.
    /// </param>
    /// <param name="size">
    /// Size of the frame in pixels.
    /// </param>
    public FrameData(
        Point location,
        PixelSize size)
    {
        Location = location;
        Size = size;

        SourceRectangle = new Rectangle(
            location.X,
            location.Y,
            size.Width,
            size.Height);
    }


    /// <summary>
    /// Gets the pixel location of the frame inside the texture atlas.
    /// </summary>
    public Point Location { get; }


    /// <summary>
    /// Gets the size of the frame in pixels.
    /// </summary>
    public PixelSize Size { get; }


    /// <summary>
    /// Gets the rectangular source region used for rendering.
    /// </summary>
    public Rectangle SourceRectangle { get; }
}
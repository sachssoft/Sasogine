using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Resources;

/// <summary>
/// Represents a single frame region inside a texture atlas.
/// </summary>
/// <remarks>
/// A frame defines the pixel location, size, and source bounds
/// used to identify a region within the source texture.
///
/// The structure is immutable and optimized for frequent access
/// during rendering and batching operations.
/// </remarks>
public readonly struct FrameData : ISourceRegion
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FrameData"/> structure.
    /// </summary>
    /// <param name="location">
    /// Pixel location of the frame inside the texture atlas.
    /// </param>
    /// <param name="size">
    /// Size of the frame in pixels.
    /// </param>
    public FrameData(
        PixelPoint2 location,
        PixelSize2 size)
    {
        Location = location;
        Size = size;

        SourceBounds = new PixelBounds2(
            location.X,
            location.Y,
            size.Width,
            size.Height);
    }

    /// <summary>
    /// Gets the pixel location of the frame inside the texture atlas.
    /// </summary>
    public PixelPoint2 Location { get; }

    /// <summary>
    /// Gets the size of the frame in pixels.
    /// </summary>
    public PixelSize2 Size { get; }

    /// <summary>
    /// Gets the pixel bounds identifying the frame within the source texture.
    /// </summary>
    public PixelBounds2 SourceBounds { get; }
}
using Sachssoft.Engine;

namespace Sachssoft.Engine.Resources;

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
        : this(location, size, 0)
    {
    }

    /// <summary>Creates a region on a specified atlas page.</summary>
    /// <param name="location">The pixel location within the atlas page.</param>
    /// <param name="size">The region size in pixels.</param>
    /// <param name="textureIndex">The nonnegative, zero-based atlas page index.</param>
    /// <exception cref="System.ArgumentOutOfRangeException">The page index is negative.</exception>
    public FrameData(PixelPoint2 location, PixelSize2 size, int textureIndex)
    {
        if (textureIndex < 0)
            throw new System.ArgumentOutOfRangeException(nameof(textureIndex));
        TextureIndex = textureIndex;
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
    public PixelPoint2 Location
    {
        get;
    }

    /// <summary>
    /// Gets the size of the frame in pixels.
    /// </summary>
    public PixelSize2 Size
    {
        get;
    }

    /// <summary>
    /// Gets the pixel bounds identifying the frame within the source texture.
    /// </summary>
    public PixelBounds2 SourceBounds
    {
        get;
    }

    /// <summary>Gets the zero-based index of the texture containing this region.</summary>
    public int TextureIndex
    {
        get;
    }
}

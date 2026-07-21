namespace Sachssoft.Sasogine.Resources;

using Microsoft.Xna.Framework;

/// <summary>
/// Represents a single sprite frame within a texture atlas.
///
/// A frame contains the position and size of a texture region inside the atlas,
/// optional transformation information, and additional data used for rendering
/// and shader-based processing.
///
/// The class supports common atlas features such as trimmed frames, pivot points,
/// and rotated atlas entries (for example Starling or LibGDX style formats).
/// </summary>
public sealed class TextureAtlasFrame
{
    /// <summary>
    /// Creates a new <see cref="TextureAtlasFrame"/>.
    /// </summary>
    /// <param name="name">
    /// Logical name of the frame.
    /// </param>
    /// <param name="sourceRect">
    /// Position and size of the frame inside the texture atlas.
    /// </param>
    /// <param name="rotated">
    /// Indicates whether the frame is stored rotated inside the atlas.
    /// </param>
    /// <param name="originalFrame">
    /// Original sprite bounds before possible atlas optimizations.
    /// </param>
    public TextureAtlasFrame(
        string name,
        Rectangle sourceRect,
        bool rotated = false,
        Rectangle originalFrame = default)
    {
        Name = name;
        SourceRectangle = sourceRect;
        Rotated = rotated;
        OriginalFrame = originalFrame;
    }


    /// <summary>
    /// Gets the logical name of the frame.
    ///
    /// This is mainly used for editor references, asset management,
    /// or animation definitions.
    /// </summary>
    public string Name { get; }


    /// <summary>
    /// Gets the source rectangle of the frame inside the texture atlas.
    ///
    /// The values are specified in pixel coordinates of the underlying texture.
    /// </summary>
    public Rectangle SourceRectangle { get; }


    /// <summary>
    /// Gets or sets the origin/pivot point of the frame.
    ///
    /// This point is used as the reference point for rotation and scaling
    /// during rendering.
    /// </summary>
    public Vector2 Origin { get; set; } = Vector2.Zero;


    /// <summary>
    /// Gets a value indicating whether the frame is stored rotated by 90 degrees
    /// inside the texture atlas.
    ///
    /// Some atlas formats use rotated sprites to reduce texture memory usage.
    /// </summary>
    public bool Rotated { get; }


    /// <summary>
    /// Gets the original sprite bounds before atlas optimizations such as
    /// trimming or cropping were applied.
    ///
    /// This allows restoring the original sprite size and position during
    /// rendering.
    /// </summary>
    public Rectangle OriginalFrame { get; }


    /// <summary>
    /// Gets the width of the associated atlas texture in pixels.
    ///
    /// This value is assigned internally by <see cref="TextureAtlas"/>
    /// and is used for calculating normalized UV coordinates.
    /// </summary>
    internal int TextureWidth { get; set; }


    /// <summary>
    /// Gets the height of the associated atlas texture in pixels.
    ///
    /// This value is assigned internally by <see cref="TextureAtlas"/>
    /// and is used for calculating normalized UV coordinates.
    /// </summary>
    internal int TextureHeight { get; set; }


    /// <summary>
    /// Gets the normalized UV coordinate of the top-left corner of the frame.
    ///
    /// The value is in the range from 0 to 1 and can be used directly for
    /// shader-based rendering and manual vertex batching.
    /// </summary>
    public Vector2 UVTopLeft => new Vector2(
        (float)SourceRectangle.X / TextureWidth,
        (float)SourceRectangle.Y / TextureHeight);


    /// <summary>
    /// Gets the normalized UV coordinate of the bottom-right corner of the frame.
    ///
    /// The value is in the range from 0 to 1 and can be used directly for
    /// shader-based rendering and manual vertex batching.
    /// </summary>
    public Vector2 UVBottomRight => new Vector2(
        (float)(SourceRectangle.X + SourceRectangle.Width) / TextureWidth,
        (float)(SourceRectangle.Y + SourceRectangle.Height) / TextureHeight);


    /// <summary>
    /// Gets the width of the frame in pixels.
    /// </summary>
    public int Width => SourceRectangle.Width;


    /// <summary>
    /// Gets the height of the frame in pixels.
    /// </summary>
    public int Height => SourceRectangle.Height;


    /// <summary>
    /// Gets the frame size as a <see cref="Vector2"/>.
    /// </summary>
    public Vector2 Size => new Vector2(
        SourceRectangle.Width,
        SourceRectangle.Height);
}
using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Graphics.Rendering;

/// <summary>
/// Represents a local transformation applied to an individual tile.
/// </summary>
/// <remarks>
/// A tile transformation is applied after the tile grid position has been
/// calculated by the tile batch.
/// 
/// The tile coordinate defines the world position inside the grid, while this
/// transformation provides local modifications for the individual tile.
/// 
/// Supported transformations:
/// <list type="bullet">
/// <item>
/// <description>
/// Offset moves the tile relative to its grid position.
/// </description>
/// </item>
/// <item>
/// <description>
/// Scale changes the local tile size.
/// </description>
/// </item>
/// <item>
/// <description>
/// Rotation rotates the tile around its local pivot.
/// </description>
/// </item>
/// <item>
/// <description>
/// Pivot defines the normalized point used for scaling and rotation.
/// </description>
/// </item>
/// </list>
/// 
/// The local pivot is independent from the tile grid pivot configured by the
/// tile batch.
/// </remarks>
public readonly struct TileTransform
{
    /// <summary>
    /// Creates a default tile transformation.
    /// </summary>
    /// <remarks>
    /// Default values:
    /// <list type="bullet">
    /// <item>
    /// <description>Scale = (1,1)</description>
    /// </item>
    /// <item>
    /// <description>Rotation = 0</description>
    /// </item>
    /// <item>
    /// <description>Offset = (0,0)</description>
    /// </item>
    /// <item>
    /// <description>Pivot = (0.5,0.5)</description>
    /// </item>
    /// </list>
    /// </remarks>
    public TileTransform()
    {
        Scale = Vector2.One;
        Rotation = 0f;
        Offset = Vector2.Zero;
        Pivot = new Vector2(0.5f);
    }


    /// <summary>
    /// Creates a tile transformation.
    /// </summary>
    /// <param name="scale">
    /// Local scale of the tile.
    /// </param>
    /// <param name="rotation">
    /// Rotation angle in radians.
    /// </param>
    /// <param name="offset">
    /// Local position offset.
    /// </param>
    public TileTransform(
        Vector2 scale,
        float rotation,
        Vector2 offset)
    {
        Scale = scale;
        Rotation = rotation;
        Offset = offset;
        Pivot = new Vector2(0.5f);
    }


    /// <summary>
    /// Creates a tile transformation with a custom pivot.
    /// </summary>
    /// <param name="scale">
    /// Local scale of the tile.
    /// </param>
    /// <param name="rotation">
    /// Rotation angle in radians.
    /// </param>
    /// <param name="offset">
    /// Local position offset.
    /// </param>
    /// <param name="pivot">
    /// Normalized local pivot point used for rotation and scaling.
    /// </param>
    public TileTransform(
        Vector2 scale,
        float rotation,
        Vector2 offset,
        Vector2 pivot)
    {
        Scale = scale;
        Rotation = rotation;
        Offset = offset;
        Pivot = pivot;
    }


    /// <summary>
    /// Gets an identity tile transformation.
    /// </summary>
    public static readonly TileTransform Identity = new();


    /// <summary>
    /// Gets the local scale of the tile.
    /// </summary>
    public Vector2 Scale { get; init; }


    /// <summary>
    /// Gets the local rotation angle in radians.
    /// </summary>
    public float Rotation { get; init; }


    /// <summary>
    /// Gets the normalized local pivot point used for scaling and rotation.
    /// </summary>
    /// <remarks>
    /// Values range from 0 to 1.
    /// 
    /// Examples:
    /// <list type="bullet">
    /// <item>
    /// <description>(0,0) = top-left</description>
    /// </item>
    /// <item>
    /// <description>(0.5,0.5) = center</description>
    /// </item>
    /// <item>
    /// <description>(1,1) = bottom-right</description>
    /// </item>
    /// </list>
    /// 
    /// This pivot affects only local transformation and does not modify
    /// the tile grid position.
    /// </remarks>
    public Vector2 Pivot { get; init; }


    /// <summary>
    /// Gets the local offset inside the tile grid position.
    /// </summary>
    /// <remarks>
    /// The offset is applied after the tile coordinate has been converted
    /// into a world position.
    /// 
    /// The offset only moves the tile and does not change the rotation or
    /// scaling origin.
    /// </remarks>
    public Vector2 Offset { get; init; }
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Graphics.Rendering.Batches;

// Achtung! Zwei unterschiedliche Pivot-Systeme:
//
// Grid Pivot  -> Positionierung der Tile im Raster
// Tile Pivot  -> lokaler Dreh-/Skalierungspunkt der Tile

/// <summary>
/// Renders tiles using a traditional orthogonal grid layout.
/// </summary>
/// <remarks>
/// Converts tile coordinates into world positions using a fixed tile size.
///
/// The batch supports a configurable tile pivot which defines the anchor
/// position of a tile relative to its grid coordinate.
///
/// Each tile can additionally use <see cref="TileTransform"/> for local
/// transformations such as offset, scaling, rotation and transformation pivot.
///
/// The tile pivot and transformation pivot have different purposes:
/// <list type="bullet">
/// <item>
/// <description>
/// The tile pivot defines where the grid coordinate is located inside the tile.
/// For example, (0,0) represents the top-left corner and (0.5,0.5)
/// represents the tile center.
/// </description>
/// </item>
/// <item>
/// <description>
/// The transformation pivot defines the normalized local point used for scaling
/// and rotation of an individual tile.
/// </description>
/// </item>
/// </list>
///
/// Supports default tile sizes as well as custom tile sizes per tile.
/// </remarks>
public sealed class OrthogonalTileBatch : QuadBatchBase
{
    private readonly Size2 _tileSize;
    private readonly Vector2 _tileGridPivot;

    /// <summary>
    /// Creates a new orthogonal tile batch.
    /// </summary>
    /// <param name="graphicsDevice">
    /// Graphics device used for rendering.
    /// </param>
    /// <param name="tileSize">
    /// Default size of a tile in world units.
    /// </param>
    /// <param name="tileGridPivot">
    /// Normalized anchor position of the tile relative to its grid coordinate.
    /// Values range from 0 to 1.
    ///
    /// Examples:
    /// (0,0) = top-left corner,
    /// (0.5,0.5) = center,
    /// (1,1) = bottom-right corner.
    /// </param>
    /// <param name="initialCapacity">
    /// Initial number of tiles the batch can store before resizing.
    /// </param>
    public OrthogonalTileBatch(
        GraphicsDevice graphicsDevice,
        Size2 tileSize,
        Vector2 tileGridPivot,
        int initialCapacity = 1024)
        : base(
            graphicsDevice,
            initialCapacity)
    {
        _tileSize = tileSize;
        _tileGridPivot = tileGridPivot;
    }

    /// <summary>
    /// Creates a new orthogonal tile batch using a default tile pivot.
    /// </summary>
    /// <param name="graphicsDevice">
    /// Graphics device used for rendering.
    /// </param>
    /// <param name="tileSize">
    /// Default size of a tile in world units.
    /// </param>
    /// <param name="initialCapacity">
    /// Initial number of tiles the batch can store before resizing.
    /// </param>
    public OrthogonalTileBatch(
        GraphicsDevice graphicsDevice,
        Size2 tileSize,
        int initialCapacity = 1024)
        : this(
            graphicsDevice,
            tileSize,
            new Vector2(0.5f),
            initialCapacity)
    {
    }

    /// <summary>
    /// Adds a tile at the specified grid coordinate.
    /// </summary>
    /// <remarks>
    /// The grid coordinate is converted into a world position using the configured
    /// tile size.
    ///
    /// A custom transformation can be applied to the tile using
    /// <see cref="TileTransform"/>. The transformation supports:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// Offset for local tile movement.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Scale for resizing the tile.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Rotation around the transformation pivot.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Pivot defining the normalized local transformation anchor.
    /// </description>
    /// </item>
    /// </list>
    ///
    /// A custom tile size changes the rendered size while keeping the grid layout
    /// unchanged.
    /// </remarks>
    /// <param name="coordinate">
    /// Grid coordinate of the tile.
    /// </param>
    /// <param name="sourceBounds">
    /// Pixel bounds inside the texture atlas.
    /// </param>
    /// <param name="customTransform">
    /// Optional local transformation applied to the tile.
    /// </param>
    /// <param name="customTileSize">
    /// Optional custom rendering size of the tile.
    /// </param>
    /// <param name="color">
    /// Optional color tint.
    /// </param>
    public void AddTile(
        Coordinate2 coordinate,
        PixelBounds2 sourceBounds,
        TileTransform? customTransform = null,
        Size2? customTileSize = null,
        Color? color = null)
    {
        TileTransform transform =
            customTransform ?? TileTransform.Identity;

        var position = new Point2(
            coordinate.X * _tileSize.Width,
            coordinate.Y * _tileSize.Height);

        var tileOffset = new Vector2(
            _tileSize.Width * _tileGridPivot.X,
            _tileSize.Height * _tileGridPivot.Y);

        position = new Point2(
            position.X + transform.Offset.X + tileOffset.X,
            position.Y + transform.Offset.Y + tileOffset.Y);

        var scale = transform.Scale;

        if (customTileSize.HasValue)
        {
            scale *= new Vector2(
                customTileSize.Value.Width / _tileSize.Width,
                customTileSize.Value.Height / _tileSize.Height);
        }

        var pivot = transform.Pivot;

        Matrix matrix =
            Matrix.CreateTranslation(
                -pivot.X,
                -pivot.Y,
                0f)
            *
            Matrix.CreateScale(
                _tileSize.Width * scale.X,
                _tileSize.Height * scale.Y,
                1f)
            *
            Matrix.CreateRotationZ(
                transform.Rotation)
            *
            Matrix.CreateTranslation(
                position.X + pivot.X,
                position.Y + pivot.Y,
                0f);

        AddQuad(
            matrix,
            sourceBounds,
            color ?? Color.White);
    }
}
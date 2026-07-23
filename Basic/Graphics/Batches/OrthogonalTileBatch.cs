using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics.Cameras;
using Sachssoft.Sasogine.Graphics.Rendering;

namespace Sachssoft.Sasogine.Graphics.Rendering.Batches;

/// <summary>
/// Renders tiles using a traditional orthogonal grid layout.
/// </summary>
/// <remarks>
/// Converts integer tile coordinates into world positions and uses
/// the shared rendering functionality from <see cref="TileBatchBase"/>.
/// </remarks>
public sealed class OrthogonalTileBatch : TileBatchBase
{
    private readonly Vector2 _tileSize;


    /// <summary>
    /// Creates a new orthogonal tile batch.
    /// </summary>
    /// <param name="graphicsDevice">
    /// Graphics device used for rendering.
    /// </param>
    /// <param name="tileSize">
    /// Size of a single tile in world units.
    /// </param>
    /// <param name="initialCapacity">
    /// Initial tile capacity.
    /// </param>
    public OrthogonalTileBatch(
        GraphicsDevice graphicsDevice,
        Vector2 tileSize,
        int initialCapacity = 1024)
        : base(
            graphicsDevice,
            initialCapacity)
    {
        _tileSize = tileSize;
    }


    /// <summary>
    /// Adds a tile at a grid coordinate.
    /// </summary>
    /// <param name="coordinate">
    /// Tile grid coordinate.
    /// </param>
    /// <param name="sourceRect">
    /// Texture region inside the tile atlas.
    /// </param>
    /// <param name="color">
    /// Tile color multiplier.
    /// </param>
    public void AddTile(
        Point coordinate,
        Rectangle sourceRect,
        Color color)
    {
        AddTile(
            coordinate.X,
            coordinate.Y,
            sourceRect,
            color);
    }


    /// <summary>
    /// Adds a tile at a grid coordinate.
    /// </summary>
    /// <param name="x">
    /// Horizontal tile coordinate.
    /// </param>
    /// <param name="y">
    /// Vertical tile coordinate.
    /// </param>
    /// <param name="sourceRect">
    /// Texture region inside the tile atlas.
    /// </param>
    /// <param name="color">
    /// Tile color multiplier.
    /// </param>
    public void AddTile(
        int x,
        int y,
        Rectangle sourceRect,
        Color color)
    {
        AddTile(
            new Vector2(
                x * _tileSize.X,
                y * _tileSize.Y),
            sourceRect,
            color);
    }


    /// <summary>
    /// Adds a tile at a world position.
    /// </summary>
    public void AddTile(
        Vector2 position,
        Rectangle sourceRect,
        Color color)
    {
        Matrix transform =
            Matrix.CreateScale(
                _tileSize.X,
                _tileSize.Y,
                1f)
            *
            Matrix.CreateTranslation(
                position.X,
                position.Y,
                0f);


        AddQuad(
            transform,
            sourceRect,
            color);
    }
}
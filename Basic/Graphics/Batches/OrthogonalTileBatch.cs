using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Common;

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
    public void AddTile(
        Coordinate2 coordinate,
        Rectangle sourceRect,
        Color color)
    {
        AddTile(
            coordinate.X,
            coordinate.Y,
            new TileTransform(),
            sourceRect,
            color);
    }


    /// <summary>
    /// Adds a tile at a grid coordinate.
    /// </summary>
    public void AddTile(
        Coordinate2 coordinate,
        TileTransform transform,
        Rectangle sourceRect,
        Color color)
    {
        AddTile(
            coordinate.X,
            coordinate.Y,
            transform,
            sourceRect,
            color);
    }


    /// <summary>
    /// Adds a tile at a grid coordinate.
    /// </summary>
    public void AddTile(
        int x,
        int y,
        Rectangle sourceRect,
        Color color)
    {
        AddTile(
            x,
            y,
            new TileTransform(),
            sourceRect,
            color);
    }


    /// <summary>
    /// Adds a tile at a grid coordinate with transformation.
    /// </summary>
    public void AddTile(
        int x,
        int y,
        TileTransform transform,
        Rectangle sourceRect,
        Color color)
    {
        Vector2 position =
            new Vector2(
                x * _tileSize.X,
                y * _tileSize.Y);


        TileTransform tileTransform =
            transform with
            {
                Position = position
            };


        Matrix matrix =
            Matrix.CreateScale(
                _tileSize.X,
                _tileSize.Y,
                1f)
            *
            tileTransform.ToMatrix();


        AddQuad(
            matrix,
            sourceRect,
            color);
    }
}
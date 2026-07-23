using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Graphics.Rendering;

namespace Sachssoft.Sasogine.Graphics.Rendering;

/// <summary>
/// Represents a transformation for tile based rendering.
/// </summary>
/// <remarks>
/// A tile transform uses a tile size instead of a generic scale value.
/// It supports position, offset, rotation and origin based transformations
/// and can be converted into a rendering matrix.
/// </remarks>
public readonly struct TileTransform : IMatrixProvider
{
    /// <summary>
    /// Creates an empty tile transform.
    /// </summary>
    public TileTransform()
    {
        Position = Vector2.Zero;
        Offset = Vector2.Zero;
        Rotation = 0f;
        Origin = Vector2.Zero;
    }


    /// <summary>
    /// Creates a tile transform with position and tile size.
    /// </summary>
    /// <param name="position">
    /// World position of the tile.
    /// </param>
    /// <param name="tileSize">
    /// Size of the tile.
    /// </param>
    public TileTransform(
        Vector2 position,
        Vector2 tileSize)
    {
        Position = position;
        Offset = Vector2.Zero;
        Rotation = 0f;
        Origin = Vector2.Zero;
    }


    /// <summary>
    /// Creates a tile transform with full transformation settings.
    /// </summary>
    /// <param name="position">
    /// World position of the tile.
    /// </param>
    /// <param name="rotation">
    /// Rotation in radians.
    /// </param>
    /// <param name="origin">
    /// Rotation origin.
    /// </param>
    public TileTransform(
        Vector2 position,
        float rotation,
        Vector2 origin)
    {
        Position = position;
        Offset = Vector2.Zero;
        Rotation = rotation;
        Origin = origin;
    }


    /// <summary>
    /// Gets the world position of the tile.
    /// </summary>
    public Vector2 Position { get; init; }


    /// <summary>
    /// Gets an additional offset applied to the tile position.
    /// </summary>
    public Vector2 Offset { get; init; }


    /// <summary>
    /// Gets the rotation angle in radians.
    /// </summary>
    public float Rotation { get; init; }


    /// <summary>
    /// Gets the origin point used as rotation and scaling pivot.
    /// </summary>
    public Vector2 Origin { get; init; }


    /// <summary>
    /// Converts this tile transformation into a graphics matrix.
    /// </summary>
    /// <returns>
    /// A matrix representing the tile transformation.
    /// </returns>
    public Matrix ToMatrix()
    {
        return
            Matrix.CreateTranslation(
                -Origin.X,
                -Origin.Y,
                0f)
            *
            Matrix.CreateRotationZ(
                Rotation)
            *
            Matrix.CreateTranslation(
                Position.X + Offset.X,
                Position.Y + Offset.Y,
                0f);
    }
}
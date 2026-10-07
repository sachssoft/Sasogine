using Microsoft.Xna.Framework;
using Sachssoft.Engine.Graphics.Rendering.Batches;

namespace Sachssoft.Engine.Graphics;

/// <summary>
/// Provides coordinate information used by a <see cref="Brush"/> while
/// configuring a <see cref="ShapeVertex"/>.
/// </summary>
public readonly struct BrushVertexContext
{
    /// <summary>
    /// Initializes a new brush vertex context.
    /// </summary>
    /// <param name="position">The transformed world position.</param>
    /// <param name="localPosition">The untransformed local position.</param>
    /// <param name="textureCoordinate">The normalized shape coordinate.</param>
    /// <param name="boundsMin">The minimum local mapping bounds.</param>
    /// <param name="boundsMax">The maximum local mapping bounds.</param>
    public BrushVertexContext(
        Vector2 position,
        Vector2 localPosition,
        Vector2 textureCoordinate,
        Vector2 boundsMin,
        Vector2 boundsMax)
    {
        Position = position;
        LocalPosition = localPosition;
        TextureCoordinate = textureCoordinate;
        BoundsMin = boundsMin;
        BoundsMax = boundsMax;
    }

    /// <summary>
    /// Gets the transformed world position of the vertex.
    /// </summary>
    public Vector2 Position { get; }

    /// <summary>
    /// Gets the untransformed local position of the vertex.
    /// </summary>
    public Vector2 LocalPosition { get; }

    /// <summary>
    /// Gets the normalized shape coordinate of the vertex.
    /// </summary>
    public Vector2 TextureCoordinate { get; }

    /// <summary>
    /// Gets the minimum local mapping bounds of the shape.
    /// </summary>
    public Vector2 BoundsMin { get; }

    /// <summary>
    /// Gets the maximum local mapping bounds of the shape.
    /// </summary>
    public Vector2 BoundsMax { get; }

    /// <summary>
    /// Gets the vertex position represented in the requested brush coordinate space.
    /// </summary>
    /// <param name="coordinateSpace">The coordinate space to use.</param>
    /// <returns>The corresponding two-dimensional coordinate.</returns>
    public Vector2 GetPosition(BrushCoordinateSpace coordinateSpace) =>
        coordinateSpace switch
        {
            BrushCoordinateSpace.Normalized => TextureCoordinate,
            BrushCoordinateSpace.Local => LocalPosition,
            BrushCoordinateSpace.World => Position,
            _ => TextureCoordinate
        };
}

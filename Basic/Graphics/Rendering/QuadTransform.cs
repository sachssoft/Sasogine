using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Graphics.Rendering;

namespace Sachssoft.Sasogine.Graphics.Rendering;

/// <summary>
/// Represents a 2D transformation for rendering quad-based objects.
/// </summary>
/// <remarks>
/// The transformation contains position, scale, rotation, origin and offset
/// information and can be converted into a graphics transformation matrix.
/// </remarks>
public readonly struct QuadTransform : IMatrixProvider
{
    /// <summary>
    /// Creates a new quad transformation.
    /// </summary>
    public QuadTransform()
    {
        Position = Vector2.Zero;
        Offset = Vector2.Zero;
        Scale = Vector2.One;
        Origin = Vector2.Zero;
        Rotation = 0f;
    }

    /// <summary>
    /// Creates a quad transformation with position and scale.
    /// </summary>
    public QuadTransform(
        Vector2 position,
        Vector2 scale)
    {
        Position = position;
        Offset = Vector2.Zero;
        Scale = scale;
        Origin = Vector2.Zero;
        Rotation = 0f;
    }


    /// <summary>
    /// Creates a quad transformation with position, scale and rotation.
    /// </summary>
    public QuadTransform(
        Vector2 position,
        Vector2 scale,
        float rotation)
    {
        Position = position;
        Offset = Vector2.Zero;
        Scale = scale;
        Origin = Vector2.Zero;
        Rotation = rotation;
    }


    /// <summary>
    /// Creates a quad transformation with complete transformation data.
    /// </summary>
    public QuadTransform(
        Vector2 position,
        Vector2 scale,
        float rotation,
        Vector2 origin,
        Vector2 offset)
    {
        Position = position;
        Offset = offset;
        Scale = scale;
        Origin = origin;
        Rotation = rotation;
    }


    /// <summary>
    /// Gets the world position of the quad.
    /// </summary>
    public Vector2 Position { get; init; }


    /// <summary>
    /// Gets an additional local offset applied after the transformation.
    /// </summary>
    public Vector2 Offset { get; init; }


    /// <summary>
    /// Gets the scale of the quad.
    /// </summary>
    public Vector2 Scale { get; init; }


    /// <summary>
    /// Gets the origin point used as rotation and scaling pivot.
    /// </summary>
    public Vector2 Origin { get; init; }


    /// <summary>
    /// Gets the rotation angle in radians.
    /// </summary>
    public float Rotation { get; init; }


    /// <summary>
    /// Converts this quad transformation into a graphics matrix.
    /// </summary>
    /// <returns>
    /// A matrix containing the combined scale, rotation and translation.
    /// </returns>
    public Matrix ToMatrix()
    {
        return
            Matrix.CreateTranslation(
                -Origin.X,
                -Origin.Y,
                0f)
            *
            Matrix.CreateScale(
                Scale.X,
                Scale.Y,
                1f)
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
using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Graphics.Rendering;

/// <summary>
/// Represents a complete 2D transformation for rendering quad-based objects.
/// </summary>
public readonly struct QuadTransform
{
    /// <summary>
    /// Creates an identity transformation.
    /// </summary>
    public QuadTransform()
    {
        Position = Point2.Zero;
        Scale = Vector2.One;
        Origin = Vector2.Zero;
        Rotation = 0f;
        Skew = Vector2.Zero;
    }

    /// <summary>
    /// Creates a transformation with position and scale.
    /// </summary>
    /// <param name="position">World position of the quad.</param>
    /// <param name="scale">Local scale factor of the quad.</param>
    public QuadTransform(
        Point2 position,
        Vector2 scale)
    {
        Position = position;
        Scale = scale;
        Origin = Vector2.Zero;
        Rotation = 0f;
        Skew = Vector2.Zero;
    }

    /// <summary>
    /// Creates a transformation with position, scale and rotation.
    /// </summary>
    /// <param name="position">World position of the quad.</param>
    /// <param name="scale">Local scale factor of the quad.</param>
    /// <param name="rotation">Rotation angle in radians.</param>
    public QuadTransform(
        Point2 position,
        Vector2 scale,
        float rotation)
    {
        Position = position;
        Scale = scale;
        Origin = Vector2.Zero;
        Rotation = rotation;
        Skew = Vector2.Zero;
    }

    /// <summary>
    /// Creates a complete quad transformation.
    /// </summary>
    /// <param name="position">World position of the quad.</param>
    /// <param name="scale">Local scale factor of the quad.</param>
    /// <param name="rotation">Rotation angle in radians.</param>
    /// <param name="origin">
    /// Local transformation origin used for rotation and scaling.
    /// </param>
    public QuadTransform(
        Point2 position,
        Vector2 scale,
        float rotation,
        Vector2 origin)
    {
        Position = position;
        Scale = scale;
        Rotation = rotation;
        Origin = origin;
        Skew = Vector2.Zero;
    }

    /// <summary>
    /// Creates a transformation with only a world position.
    /// </summary>
    /// <param name="position">World position of the quad.</param>
    public QuadTransform(Point2 position)
    {
        Position = position;
        Scale = Vector2.One;
        Origin = Vector2.Zero;
        Rotation = 0f;
        Skew = Vector2.Zero;
    }

    /// <summary>
    /// Gets an identity transformation.
    /// </summary>
    public static readonly QuadTransform Identity = new();

    /// <summary>
    /// Gets the world position of the quad.
    /// </summary>
    public Point2 Position { get; init; }

    /// <summary>
    /// Gets the local scale of the quad.
    /// </summary>
    public Vector2 Scale { get; init; }

    /// <summary>
    /// Gets the local transformation origin used for rotation and scaling.
    /// </summary>
    public Vector2 Origin { get; init; }

    /// <summary>
    /// Gets the rotation angle of the quad in radians.
    /// </summary>
    public float Rotation { get; init; }

    /// <summary>
    /// Gets the local skew/shear transformation.
    /// </summary>
    public Vector2 Skew { get; init; }

    /// <summary>
    /// Converts this transformation into a graphics matrix.
    /// </summary>
    /// <returns>A matrix representing the complete transformation.</returns>
    public Matrix ToMatrix()
    {
        Matrix matrix = Matrix.Identity;

        if (Origin != Vector2.Zero)
        {
            matrix *= Matrix.CreateTranslation(
                -Origin.X,
                -Origin.Y,
                0f);
        }

        if (Scale != Vector2.One)
        {
            matrix *= Matrix.CreateScale(
                Scale.X,
                Scale.Y,
                1f);
        }

        if (Skew != Vector2.Zero)
        {
            matrix *= CreateSkew(
                Skew.X,
                Skew.Y);
        }

        if (Rotation != 0f)
        {
            matrix *= Matrix.CreateRotationZ(Rotation);
        }

        if (Position != Point2.Zero)
        {
            matrix *= Matrix.CreateTranslation(
                Position.X,
                Position.Y,
                0f);
        }

        return matrix;
    }

    private static Matrix CreateSkew(
        float skewX,
        float skewY)
    {
        return new Matrix(
            1f, skewY, 0f, 0f,
            skewX, 1f, 0f, 0f,
            0f, 0f, 1f, 0f,
            0f, 0f, 0f, 1f);
    }
}
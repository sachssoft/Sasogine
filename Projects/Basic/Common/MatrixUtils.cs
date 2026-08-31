using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Provides helper methods for creating common 2D transformation matrices.
/// </summary>
public static class MatrixUtils
{
    /// <summary>
    /// Creates a transformation matrix using position, scale, rotation, and depth.
    /// </summary>
    /// <param name="position">World position.</param>
    /// <param name="scale">Scale factors.</param>
    /// <param name="rotation">Rotation around the Z axis in radians.</param>
    /// <param name="depth">Depth offset along the Z axis.</param>
    /// <returns>The resulting transformation matrix.</returns>
    public static Matrix Create(
        Vector2 position,
        Vector2 scale,
        float rotation,
        float depth = 0f)
    {
        return
            Matrix.CreateScale(scale.X, scale.Y, 1f) *
            Matrix.CreateRotationZ(rotation) *
            Matrix.CreateTranslation(position.X, position.Y, depth);
    }

    /// <summary>
    /// Creates a transformation matrix using an absolute origin.
    /// </summary>
    /// <param name="position">World position.</param>
    /// <param name="scale">Scale factors.</param>
    /// <param name="rotation">Rotation around the Z axis in radians.</param>
    /// <param name="origin">Absolute transformation origin.</param>
    /// <param name="depth">Depth offset along the Z axis.</param>
    /// <returns>The resulting transformation matrix.</returns>
    public static Matrix Create(
        Vector2 position,
        Vector2 scale,
        float rotation,
        Vector2 origin,
        float depth = 0f)
    {
        return
            Matrix.CreateTranslation(-origin.X, -origin.Y, 0f) *
            Matrix.CreateScale(scale.X, scale.Y, 1f) *
            Matrix.CreateRotationZ(rotation) *
            Matrix.CreateTranslation(position.X, position.Y, depth);
    }

    /// <summary>
    /// Creates a transformation matrix using an origin that can optionally
    /// be converted from normalized coordinates to absolute coordinates.
    /// </summary>
    /// <param name="position">World position.</param>
    /// <param name="scale">Scale factors.</param>
    /// <param name="rotation">Rotation around the Z axis in radians.</param>
    /// <param name="origin">
    /// Normalized origin when <paramref name="size"/> is specified;
    /// otherwise an absolute origin.
    /// </param>
    /// <param name="size">
    /// Optional size used to convert the normalized origin to absolute coordinates.
    /// </param>
    /// <param name="depth">Depth offset along the Z axis.</param>
    /// <returns>The resulting transformation matrix.</returns>
    public static Matrix Create(
        Vector2 position,
        Vector2 scale,
        float rotation,
        Vector2 origin,
        Vector2? size = null,
        float depth = 0f)
    {
        var originPx = size.HasValue
            ? origin * size.Value
            : origin;

        return
            Matrix.CreateTranslation(-originPx.X, -originPx.Y, 0f) *
            Matrix.CreateScale(scale.X, scale.Y, 1f) *
            Matrix.CreateRotationZ(rotation) *
            Matrix.CreateTranslation(position.X, position.Y, depth);
    }

    /// <summary>
    /// Creates a transformation matrix using the center of the specified size
    /// as the transformation origin.
    /// </summary>
    /// <param name="position">World position.</param>
    /// <param name="scale">Scale factors.</param>
    /// <param name="rotation">Rotation around the Z axis in radians.</param>
    /// <param name="size">Size used to calculate the center origin.</param>
    /// <param name="depth">Depth offset along the Z axis.</param>
    /// <returns>The resulting transformation matrix.</returns>
    public static Matrix CreateCenter(
        Vector2 position,
        Vector2 scale,
        float rotation,
        Vector2 size,
        float depth = 0f)
    {
        return Create(
            position,
            scale,
            rotation,
            new Vector2(0.5f),
            size,
            depth);
    }

    /// <summary>
    /// Creates a translation matrix that applies only a depth offset.
    /// </summary>
    /// <param name="z">Depth offset along the Z axis.</param>
    /// <returns>The resulting translation matrix.</returns>
    public static Matrix CreateDepth(float z)
    {
        return Matrix.CreateTranslation(0f, 0f, z);
    }

    /// <summary>
    /// Creates a two-dimensional skew transformation matrix.
    /// </summary>
    /// <param name="skewX">Skew factor along the X axis.</param>
    /// <param name="skewY">Skew factor along the Y axis.</param>
    /// <returns>The resulting skew matrix.</returns>
    public static Matrix CreateSkew(float skewX, float skewY)
    {
        return new Matrix(
            1f, skewY, 0f, 0f,
            skewX, 1f, 0f, 0f,
            0f, 0f, 1f, 0f,
            0f, 0f, 0f, 1f);
    }
}
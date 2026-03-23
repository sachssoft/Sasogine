using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Graphics;

/// <summary>
/// Provides helper methods for creating 2D transformation matrices 
/// with position, scale, rotation, origin, and depth.
/// </summary>
public static class MatrixHelper
{
    public static Matrix Create(Vector2 position, Vector2 scale, float rotation, float depth = 0f)
    {
        return
            Matrix.CreateScale(scale.X, scale.Y, 1f) *
            Matrix.CreateRotationZ(rotation) *
            Matrix.CreateTranslation(position.X, position.Y, depth);
    }


    public static Matrix Create(Vector2 position, Vector2 scale, float rotation, Vector2 origin, float depth = 0f)
    {
        // Origin in Weltkoordinaten
        Vector2 originPx = origin;

        return
            Matrix.CreateTranslation(-originPx.X, -originPx.Y, 0f) * // Punkte vom Pivot weg
            Matrix.CreateScale(scale.X, scale.Y, 1f) *               // Skalierung
            Matrix.CreateRotationZ(rotation) *                        // Rotation
            Matrix.CreateTranslation(position.X, position.Y, depth);      // Position / Offset
    }
    
    /// <summary>
     /// Creates a transformation matrix using relative origin (0..1) and optional size.
     /// </summary>
     /// <param name="position">World position.</param>
     /// <param name="scale">Scale factors.</param>
     /// <param name="rotation">Rotation in radians.</param>
     /// <param name="origin">Pivot in 0..1 coordinates (relative to size) or absolute pixels if size not provided).</param>
     /// <param name="size">Optional size to convert origin from relative to absolute pixels.</param>
     /// <param name="depth">Optional depth (Z coordinate).</param>
     /// <returns>Transformation matrix.</returns>
    public static Matrix Create(Vector2 position, Vector2 scale, float rotation, Vector2 origin, Vector2? size = null, float depth = 0f)
    {
        // Convert relative origin to pixel coordinates if size is provided
        Vector2 originPx = size.HasValue ? origin * size.Value : origin;

        return
            Matrix.CreateTranslation(-originPx.X, -originPx.Y, 0f) * // Pivot
            Matrix.CreateScale(scale.X, scale.Y, 1f) *               // Scale
            Matrix.CreateRotationZ(rotation) *                        // Rotate
            Matrix.CreateTranslation(position.X, position.Y, depth); // Translate
    }


    /// <summary>
    /// Creates a transformation matrix with the origin set to the normalized center (0.5, 0.5).
    /// </summary>
    public static Matrix CreateCenter(Vector2 position, Vector2 scale, float rotation, float depth = 0f)
    {
        return Create(position, scale, rotation, new Vector2(0.5f), depth);
    }

    /// <summary>
    /// Creates a translation matrix that applies only a depth (Z) offset.
    /// </summary>
    public static Matrix CreateDepth(float z)
    {
        return Matrix.CreateTranslation(0f, 0f, z);
    }

    public static Matrix CreateSkew(float skewX, float skewY)
    {
        return new Matrix(
            1f, skewY, 0f, 0f,  // Row 1
            skewX, 1f, 0f, 0f,  // Row 2
            0f, 0f, 1f, 0f,     // Row 3
            0f, 0f, 0f, 1f      // Row 4
        );
    }
}

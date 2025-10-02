using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Graphics;

/// <summary>
/// Provides helper methods for creating 2D transformation matrices 
/// with position, scale, rotation, origin, and depth.
/// </summary>
public static class MatrixHelper
{
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
}

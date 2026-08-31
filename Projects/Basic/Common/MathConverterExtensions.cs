using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Provides conversion and transformation extensions for common
/// MonoGame mathematical types.
/// </summary>
public static class MathConverterExtensions
{
    /// <summary>
    /// Converts a <see cref="Vector3"/> to a <see cref="Vector2"/>
    /// by discarding the Z component.
    /// </summary>
    /// <param name="vector">Vector to convert.</param>
    /// <returns>The converted two-dimensional vector.</returns>
    public static Vector2 ToVector2(this Vector3 vector)
    {
        return new Vector2(vector.X, vector.Y);
    }

    /// <summary>
    /// Converts a <see cref="Vector2"/> to a <see cref="Vector3"/>.
    /// </summary>
    /// <param name="vector">Vector to convert.</param>
    /// <param name="z">Value of the Z component.</param>
    /// <returns>The converted three-dimensional vector.</returns>
    public static Vector3 ToVector3(this Vector2 vector, float z = 0f)
    {
        return new Vector3(vector.X, vector.Y, z);
    }

    /// <summary>
    /// Creates a translation matrix from the specified two-dimensional vector.
    /// </summary>
    /// <param name="vector">Translation vector.</param>
    /// <returns>The resulting translation matrix.</returns>
    public static Matrix ToTranslationMatrix(this Vector2 vector)
    {
        return ToTranslationMatrix(vector.ToVector3());
    }

    /// <summary>
    /// Creates a translation matrix from the specified three-dimensional vector.
    /// </summary>
    /// <param name="vector">Translation vector.</param>
    /// <returns>The resulting translation matrix.</returns>
    public static Matrix ToTranslationMatrix(this Vector3 vector)
    {
        return Matrix.CreateTranslation(vector);
    }

    /// <summary>
    /// Creates a scale matrix from the specified two-dimensional vector.
    /// </summary>
    /// <param name="vector">Scale vector.</param>
    /// <returns>The resulting scale matrix.</returns>
    public static Matrix ToScaleMatrix(this Vector2 vector)
    {
        return ToScaleMatrix(vector.ToVector3(1f));
    }

    /// <summary>
    /// Creates a scale matrix from the specified three-dimensional vector.
    /// </summary>
    /// <param name="vector">Scale vector.</param>
    /// <returns>The resulting scale matrix.</returns>
    public static Matrix ToScaleMatrix(this Vector3 vector)
    {
        return Matrix.CreateScale(vector);
    }

    /// <summary>
    /// Creates a rotation matrix around the X axis.
    /// </summary>
    /// <param name="value">Rotation angle in radians.</param>
    /// <returns>The resulting rotation matrix.</returns>
    public static Matrix ToRotationXMatrix(this float value)
    {
        return Matrix.CreateRotationX(value);
    }

    /// <summary>
    /// Creates a rotation matrix around the Y axis.
    /// </summary>
    /// <param name="value">Rotation angle in radians.</param>
    /// <returns>The resulting rotation matrix.</returns>
    public static Matrix ToRotationYMatrix(this float value)
    {
        return Matrix.CreateRotationY(value);
    }

    /// <summary>
    /// Creates a rotation matrix around the Z axis.
    /// </summary>
    /// <param name="value">Rotation angle in radians.</param>
    /// <returns>The resulting rotation matrix.</returns>
    public static Matrix ToRotationZMatrix(this float value)
    {
        return Matrix.CreateRotationZ(value);
    }

    /// <summary>
    /// Creates a rotation matrix from the specified two-dimensional rotation vector.
    /// </summary>
    /// <param name="vector">
    /// Rotation angles in radians around the X and Y axes.
    /// </param>
    /// <returns>The resulting rotation matrix.</returns>
    public static Matrix ToRotationMatrix(this Vector2 vector)
    {
        return ToRotationMatrix(vector.ToVector3());
    }

    /// <summary>
    /// Creates a rotation matrix from the specified three-dimensional rotation vector.
    /// </summary>
    /// <param name="vector">
    /// Rotation angles in radians around the X, Y, and Z axes.
    /// </param>
    /// <returns>The resulting rotation matrix.</returns>
    public static Matrix ToRotationMatrix(this Vector3 vector)
    {
        return Matrix.CreateRotationX(vector.X) *
               Matrix.CreateRotationY(vector.Y) *
               Matrix.CreateRotationZ(vector.Z);
    }
}
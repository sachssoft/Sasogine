using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Provides extension methods for common operations on three-dimensional points.
/// </summary>
public static class Point3Extensions
{
    /// <summary>
    /// Returns the normalized direction from this point
    /// to the specified target point.
    /// </summary>
    /// <param name="point">The source point.</param>
    /// <param name="target">The target point.</param>
    /// <returns>The normalized direction vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 DirectionTo(
        this Point3 point,
        Point3 target)
    {
        return VectorMath.SafeNormalize(
            target - point);
    }

    /// <summary>
    /// Rotates the point around the X-axis relative to the specified origin.
    /// </summary>
    /// <param name="point">The point to rotate.</param>
    /// <param name="origin">The rotation origin.</param>
    /// <param name="rotation">The rotation angle in radians.</param>
    /// <returns>The rotated point.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point3 RotateAroundX(
        this Point3 point,
        Point3 origin,
        float rotation)
    {
        Vector3 translated = point - origin;

        float cos = float.Cos(rotation);
        float sin = float.Sin(rotation);

        Vector3 rotated = new Vector3(
            translated.X,
            translated.Y * cos - translated.Z * sin,
            translated.Y * sin + translated.Z * cos);

        return origin + rotated;
    }

    /// <summary>
    /// Rotates the point around the Y-axis relative to the specified origin.
    /// </summary>
    /// <param name="point">The point to rotate.</param>
    /// <param name="origin">The rotation origin.</param>
    /// <param name="rotation">The rotation angle in radians.</param>
    /// <returns>The rotated point.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point3 RotateAroundY(
        this Point3 point,
        Point3 origin,
        float rotation)
    {
        Vector3 translated = point - origin;

        float cos = float.Cos(rotation);
        float sin = float.Sin(rotation);

        Vector3 rotated = new Vector3(
            translated.X * cos + translated.Z * sin,
            translated.Y,
            -translated.X * sin + translated.Z * cos);

        return origin + rotated;
    }

    /// <summary>
    /// Rotates the point around the Z-axis relative to the specified origin.
    /// </summary>
    /// <param name="point">The point to rotate.</param>
    /// <param name="origin">The rotation origin.</param>
    /// <param name="rotation">The rotation angle in radians.</param>
    /// <returns>The rotated point.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point3 RotateAroundZ(
        this Point3 point,
        Point3 origin,
        float rotation)
    {
        Vector3 translated = point - origin;

        float cos = float.Cos(rotation);
        float sin = float.Sin(rotation);

        Vector3 rotated = new Vector3(
            translated.X * cos - translated.Y * sin,
            translated.X * sin + translated.Y * cos,
            translated.Z);

        return origin + rotated;
    }
}
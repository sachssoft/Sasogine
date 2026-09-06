using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Provides extension methods for common operations on two-dimensional points.
/// </summary>
public static class Point2Extensions
{
    /// <summary>
    /// Calculates a point on a circle around the specified center.
    /// </summary>
    /// <param name="position">The center point of the circle.</param>
    /// <param name="rotation">The rotation angle in radians.</param>
    /// <param name="distance">The distance from the center.</param>
    /// <returns>The calculated point on the circle.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point2 UnitCircle(
        this Point2 position,
        float rotation,
        float distance)
    {
        return position + new Vector2(
            float.Cos(rotation) * distance,
            float.Sin(rotation) * distance);
    }

    /// <summary>
    /// Returns the normalized direction from this point
    /// to the specified target point.
    /// </summary>
    /// <param name="point">The source point.</param>
    /// <param name="target">The target point.</param>
    /// <returns>The normalized direction vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 DirectionTo(
        this Point2 point,
        Point2 target)
    {
        return VectorMath.SafeNormalize(
            target - point);
    }

    /// <summary>
    /// Returns the angle from this point to the specified target point.
    /// </summary>
    /// <param name="point">The source point.</param>
    /// <param name="target">The target point.</param>
    /// <returns>The angle to the target in radians.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float AngleTo(
        this Point2 point,
        Point2 target)
    {
        Vector2 delta = target - point;

        return float.Atan2(
            delta.Y,
            delta.X);
    }

    /// <summary>
    /// Rotates the point around the specified origin.
    /// </summary>
    /// <param name="point">The point to rotate.</param>
    /// <param name="origin">The rotation origin.</param>
    /// <param name="rotation">The rotation angle in radians.</param>
    /// <returns>The rotated point.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point2 RotateAround(
        this Point2 point,
        Point2 origin,
        float rotation)
    {
        Vector2 translated = point - origin;
        Vector2 rotated = VectorMath.Rotate(
            translated,
            rotation);

        return origin + rotated;
    }
}
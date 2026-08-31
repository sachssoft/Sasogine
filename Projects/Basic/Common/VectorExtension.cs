using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Provides utility extensions for common two-dimensional vector operations.
/// </summary>
public static class VectorExtension
{
    /// <summary>
    /// Projects the specified vector onto another vector.
    /// </summary>
    /// <param name="vector">Vector to project.</param>
    /// <param name="normal">Vector onto which the projection is performed.</param>
    /// <returns>The projected vector.</returns>
    public static Vector2 Projection(this Vector2 vector, Vector2 normal)
    {
        var lengthSquared = normal.LengthSquared();

        if (lengthSquared == 0f)
            return Vector2.Zero;

        return Vector2.Dot(vector, normal) / lengthSquared * normal;
    }

    /// <summary>
    /// Calculates the rejection of the specified vector from another vector.
    /// </summary>
    /// <param name="vector">Vector to reject.</param>
    /// <param name="normal">Vector from which the rejection is calculated.</param>
    /// <returns>The rejected component of the vector.</returns>
    public static Vector2 Rejection(this Vector2 vector, Vector2 normal)
    {
        return vector - vector.Projection(normal);
    }

    /// <summary>
    /// Calculates a position on a circle around the specified position.
    /// </summary>
    /// <param name="position">Center position of the circle.</param>
    /// <param name="rotation">Rotation angle in radians.</param>
    /// <param name="distance">Distance from the center.</param>
    /// <returns>The calculated position on the circle.</returns>
    public static Vector2 UnitCircle(
        this Vector2 position,
        float rotation,
        float distance)
    {
        return position + new Vector2(
            float.Cos(rotation) * distance,
            float.Sin(rotation) * distance);
    }

    /// <summary>
    /// Returns a vector containing the absolute value of each component.
    /// </summary>
    /// <param name="vector">Vector to convert.</param>
    /// <returns>A vector containing the absolute component values.</returns>
    public static Vector2 Abs(this Vector2 vector)
    {
        return new Vector2(
            float.Abs(vector.X),
            float.Abs(vector.Y));
    }

    /// <summary>
    /// Returns a vector perpendicular to the specified vector
    /// in clockwise direction.
    /// </summary>
    /// <param name="vector">Source vector.</param>
    /// <returns>The clockwise perpendicular vector.</returns>
    public static Vector2 PerpendicularRight(this Vector2 vector)
    {
        return new Vector2(vector.Y, -vector.X);
    }

    /// <summary>
    /// Returns a vector perpendicular to the specified vector
    /// in counter-clockwise direction.
    /// </summary>
    /// <param name="vector">Source vector.</param>
    /// <returns>The counter-clockwise perpendicular vector.</returns>
    public static Vector2 PerpendicularLeft(this Vector2 vector)
    {
        return new Vector2(-vector.Y, vector.X);
    }

    /// <summary>
    /// Returns a safely normalized vector.
    /// </summary>
    /// <param name="vector">Vector to normalize.</param>
    /// <returns>
    /// The normalized vector, or <see cref="Vector2.Zero"/>
    /// when the vector has no length.
    /// </returns>
    public static Vector2 NormalizeSafe(this Vector2 vector)
    {
        var lengthSquared = vector.LengthSquared();

        if (lengthSquared == 0f)
            return Vector2.Zero;

        return vector / float.Sqrt(lengthSquared);
    }

    /// <summary>
    /// Returns the normalized direction from this vector
    /// to the specified target.
    /// </summary>
    /// <param name="vector">Source position.</param>
    /// <param name="target">Target position.</param>
    /// <returns>The normalized direction vector.</returns>
    public static Vector2 DirectionTo(this Vector2 vector, Vector2 target)
    {
        return (target - vector).NormalizeSafe();
    }

    /// <summary>
    /// Returns the angle of the vector in radians.
    /// </summary>
    /// <param name="vector">Source vector.</param>
    /// <returns>The vector angle in radians.</returns>
    public static float Angle(this Vector2 vector)
    {
        return float.Atan2(vector.Y, vector.X);
    }

    /// <summary>
    /// Returns the angle from this position to the specified target
    /// in radians.
    /// </summary>
    /// <param name="vector">Source position.</param>
    /// <param name="target">Target position.</param>
    /// <returns>The angle to the target in radians.</returns>
    public static float AngleTo(this Vector2 vector, Vector2 target)
    {
        var delta = target - vector;

        return float.Atan2(delta.Y, delta.X);
    }

    /// <summary>
    /// Rotates the vector around the origin by the specified angle.
    /// </summary>
    /// <param name="vector">Vector to rotate.</param>
    /// <param name="rotation">Rotation angle in radians.</param>
    /// <returns>The rotated vector.</returns>
    public static Vector2 Rotate(this Vector2 vector, float rotation)
    {
        var cos = float.Cos(rotation);
        var sin = float.Sin(rotation);

        return new Vector2(
            vector.X * cos - vector.Y * sin,
            vector.X * sin + vector.Y * cos);
    }

    /// <summary>
    /// Rotates the vector around the specified origin.
    /// </summary>
    /// <param name="vector">Vector to rotate.</param>
    /// <param name="origin">Rotation origin.</param>
    /// <param name="rotation">Rotation angle in radians.</param>
    /// <returns>The rotated vector.</returns>
    public static Vector2 RotateAround(
        this Vector2 vector,
        Vector2 origin,
        float rotation)
    {
        var translated = vector - origin;
        var cos = float.Cos(rotation);
        var sin = float.Sin(rotation);

        return new Vector2(
            translated.X * cos - translated.Y * sin,
            translated.X * sin + translated.Y * cos) + origin;
    }

    /// <summary>
    /// Returns the distance from this vector to the specified target.
    /// </summary>
    /// <param name="vector">Source position.</param>
    /// <param name="target">Target position.</param>
    /// <returns>The distance between both vectors.</returns>
    public static float DistanceTo(this Vector2 vector, Vector2 target)
    {
        return Vector2.Distance(vector, target);
    }

    /// <summary>
    /// Returns the squared distance from this vector to the specified target.
    /// </summary>
    /// <param name="vector">Source position.</param>
    /// <param name="target">Target position.</param>
    /// <returns>The squared distance between both vectors.</returns>
    public static float DistanceSquaredTo(this Vector2 vector, Vector2 target)
    {
        return Vector2.DistanceSquared(vector, target);
    }

    /// <summary>
    /// Returns a vector with each component clamped
    /// between the specified minimum and maximum values.
    /// </summary>
    /// <param name="vector">Vector to clamp.</param>
    /// <param name="min">Minimum component values.</param>
    /// <param name="max">Maximum component values.</param>
    /// <returns>The clamped vector.</returns>
    public static Vector2 Clamp(
        this Vector2 vector,
        Vector2 min,
        Vector2 max)
    {
        return Vector2.Clamp(vector, min, max);
    }

    /// <summary>
    /// Returns a vector with each component rounded
    /// to the nearest integral value.
    /// </summary>
    /// <param name="vector">Vector to round.</param>
    /// <returns>The rounded vector.</returns>
    public static Vector2 Round(this Vector2 vector)
    {
        return new Vector2(
            float.Round(vector.X),
            float.Round(vector.Y));
    }

    /// <summary>
    /// Returns a vector with each component rounded down.
    /// </summary>
    /// <param name="vector">Vector to round down.</param>
    /// <returns>The floored vector.</returns>
    public static Vector2 Floor(this Vector2 vector)
    {
        return new Vector2(
            float.Floor(vector.X),
            float.Floor(vector.Y));
    }

    /// <summary>
    /// Returns a vector with each component rounded up.
    /// </summary>
    /// <param name="vector">Vector to round up.</param>
    /// <returns>The ceiled vector.</returns>
    public static Vector2 Ceiling(this Vector2 vector)
    {
        return new Vector2(
            float.Ceiling(vector.X),
            float.Ceiling(vector.Y));
    }
}
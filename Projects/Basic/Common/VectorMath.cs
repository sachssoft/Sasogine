using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Geometry;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Provides utility methods for common vector and line-segment calculations.
/// </summary>
public static class VectorMath
{
    /// <summary>
    /// Safely normalizes the specified vector.
    /// </summary>
    /// <param name="value">
    /// The vector to normalize.
    /// </param>
    /// <returns>
    /// The normalized vector, or <see cref="Vector2.Zero"/> when the vector
    /// is too small to normalize safely.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 SafeNormalize(Vector2 value)
    {
        var lengthSquared = value.LengthSquared();

        if (lengthSquared < 1e-12f)
            return Vector2.Zero;

        return value / float.Sqrt(lengthSquared);
    }

    /// <summary>
    /// Safely normalizes the specified vector.
    /// </summary>
    /// <param name="value">
    /// The vector to normalize.
    /// </param>
    /// <returns>
    /// The normalized vector, or <see cref="Vector3.Zero"/> when the vector
    /// is too small to normalize safely.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 SafeNormalize(Vector3 value)
    {
        var lengthSquared = value.LengthSquared();

        if (lengthSquared < 1e-12f)
            return Vector3.Zero;

        return value / float.Sqrt(lengthSquared);
    }

    /// <summary>
    /// Returns a perpendicular vector rotated 90 degrees counterclockwise.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Perpendicular(Vector2 value)
    {
        return new Vector2(-value.Y, value.X);
    }

    /// <summary>
    /// Returns a perpendicular vector rotated 90 degrees clockwise.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 PerpendicularClockwise(Vector2 value)
    {
        return new Vector2(value.Y, -value.X);
    }

    /// <summary>
    /// Returns the angle of the specified vector in radians.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Angle(Vector2 value)
    {
        return float.Atan2(value.Y, value.X);
    }

    /// <summary>
    /// Creates a unit direction vector from the specified angle in radians.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 FromAngle(float angle)
    {
        return new Vector2(
            float.Cos(angle),
            float.Sin(angle));
    }

    /// <summary>
    /// Returns the normalized direction from one point to another.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Direction(
        Vector2 from,
        Vector2 to)
    {
        return SafeNormalize(to - from);
    }

    /// <summary>
    /// Projects a point onto a line segment and returns the normalized
    /// segment parameter and projected position.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SegmentProjectionResult ProjectPointOntoSegment(
        Vector2 point,
        Vector2 segmentStart,
        Vector2 segmentEnd)
    {
        var segment = segmentEnd - segmentStart;
        var lengthSquared = segment.LengthSquared();

        if (lengthSquared < 1e-6f)
            return new SegmentProjectionResult(0f, segmentStart);

        var t = Vector2.Dot(point - segmentStart, segment) / lengthSquared;
        t = float.Clamp(t, 0f, 1f);

        var position = segmentStart + t * segment;

        return new SegmentProjectionResult(t, position);
    }

    /// <summary>
    /// Projects a point onto an infinite line.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SegmentProjectionResult ProjectPointOntoLine(
        Vector2 point,
        Vector2 lineStart,
        Vector2 lineEnd)
    {
        var line = lineEnd - lineStart;
        var lengthSquared = line.LengthSquared();

        if (lengthSquared < 1e-6f)
            return new SegmentProjectionResult(0f, lineStart);

        var t = Vector2.Dot(point - lineStart, line) / lengthSquared;
        var position = lineStart + t * line;

        return new SegmentProjectionResult(t, position);
    }

    /// <summary>
    /// Returns the closest point on a line segment.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 ClosestPointOnSegment(
        Vector2 point,
        Vector2 segmentStart,
        Vector2 segmentEnd)
    {
        return ProjectPointOntoSegment(
            point,
            segmentStart,
            segmentEnd).ProjectedPoint;
    }

    /// <summary>
    /// Returns the squared distance between a point and a line segment.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DistanceSquaredToSegment(
        Vector2 point,
        Vector2 segmentStart,
        Vector2 segmentEnd)
    {
        var projection = ProjectPointOntoSegment(
            point,
            segmentStart,
            segmentEnd);

        return Vector2.DistanceSquared(
            point,
            projection.ProjectedPoint);
    }

    /// <summary>
    /// Returns the distance between a point and a line segment.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DistanceToSegment(
        Vector2 point,
        Vector2 segmentStart,
        Vector2 segmentEnd)
    {
        return float.Sqrt(
            DistanceSquaredToSegment(
                point,
                segmentStart,
                segmentEnd));
    }

    /// <summary>
    /// Returns the squared distance between a point and an infinite line.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DistanceSquaredToLine(
        Vector2 point,
        Vector2 lineStart,
        Vector2 lineEnd)
    {
        var projection = ProjectPointOntoLine(
            point,
            lineStart,
            lineEnd);

        return Vector2.DistanceSquared(
            point,
            projection.ProjectedPoint);
    }

    /// <summary>
    /// Returns the distance between a point and an infinite line.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DistanceToLine(
        Vector2 point,
        Vector2 lineStart,
        Vector2 lineEnd)
    {
        return float.Sqrt(
            DistanceSquaredToLine(
                point,
                lineStart,
                lineEnd));
    }

    /// <summary>
    /// Calculates a point along a line segment using a normalized parameter.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 PointOnSegment(
        Vector2 segmentStart,
        Vector2 segmentEnd,
        float t)
    {
        return Vector2.Lerp(
            segmentStart,
            segmentEnd,
            float.Clamp(t, 0f, 1f));
    }

    /// <summary>
    /// Returns the normalized direction of a line segment.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 SegmentDirection(
        Vector2 segmentStart,
        Vector2 segmentEnd)
    {
        return SafeNormalize(segmentEnd - segmentStart);
    }

    /// <summary>
    /// Returns the length of a line segment.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SegmentLength(
        Vector2 segmentStart,
        Vector2 segmentEnd)
    {
        return Vector2.Distance(
            segmentStart,
            segmentEnd);
    }

    /// <summary>
    /// Returns the squared length of a line segment.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SegmentLengthSquared(
        Vector2 segmentStart,
        Vector2 segmentEnd)
    {
        return Vector2.DistanceSquared(
            segmentStart,
            segmentEnd);
    }

    /// <summary>
    /// Returns the midpoint of a line segment.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 SegmentMidpoint(
        Vector2 segmentStart,
        Vector2 segmentEnd)
    {
        return (segmentStart + segmentEnd) * 0.5f;
    }

    /// <summary>
    /// Returns the signed two-dimensional cross product.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Cross(
        Vector2 a,
        Vector2 b)
    {
        return a.X * b.Y - a.Y * b.X;
    }

    /// <summary>
    /// Returns the signed orientation of three points.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Orientation(
        Vector2 a,
        Vector2 b,
        Vector2 c)
    {
        return Cross(
            b - a,
            c - a);
    }

    /// <summary>
    /// Determines whether a point lies on a line segment.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPointOnSegment(
        Vector2 point,
        Vector2 segmentStart,
        Vector2 segmentEnd,
        float tolerance = 1e-5f)
    {
        var projection = ProjectPointOntoSegment(
            point,
            segmentStart,
            segmentEnd);

        return Vector2.DistanceSquared(
            point,
            projection.ProjectedPoint) <= tolerance * tolerance;
    }

    /// <summary>
    /// Returns the signed side of a point relative to a directed line.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SideOfLine(
        Vector2 point,
        Vector2 lineStart,
        Vector2 lineEnd)
    {
        return Cross(
            lineEnd - lineStart,
            point - lineStart);
    }

    /// <summary>
    /// Determines whether two line segments intersect.
    /// </summary>
    /// <param name="aStart">The start point of the first segment.</param>
    /// <param name="aEnd">The end point of the first segment.</param>
    /// <param name="bStart">The start point of the second segment.</param>
    /// <param name="bEnd">The end point of the second segment.</param>
    /// <param name="intersection">
    /// Receives the intersection point when the segments intersect.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the segments intersect; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public static bool IntersectSegments(
        Vector2 aStart,
        Vector2 aEnd,
        Vector2 bStart,
        Vector2 bEnd,
        out Vector2 intersection)
    {
        var r = aEnd - aStart;
        var s = bEnd - bStart;

        var denominator = Cross(r, s);
        var delta = bStart - aStart;

        if (float.Abs(denominator) < 1e-6f)
        {
            intersection = default;
            return false;
        }

        var t = Cross(delta, s) / denominator;
        var u = Cross(delta, r) / denominator;

        if (t < 0f || t > 1f ||
            u < 0f || u > 1f)
        {
            intersection = default;
            return false;
        }

        intersection = aStart + t * r;
        return true;
    }
}
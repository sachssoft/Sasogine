using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Geometry;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Provides utility methods for common vector and line-segment calculations.
    /// </summary>
    public static class VectorMath
    {
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
            var segment = segmentEnd - segmentStart;
            var lengthSquared = segment.LengthSquared();

            if (lengthSquared < 1e-6f)
                return Vector2.Zero;

            return segment / float.Sqrt(lengthSquared);
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
        /// <returns>
        /// A positive value when the point lies on one side of the line,
        /// a negative value when it lies on the opposite side,
        /// or zero when it lies on the line.
        /// </returns>
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
}
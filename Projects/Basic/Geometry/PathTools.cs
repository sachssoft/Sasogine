using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry
{
    /// <summary>
    /// Provides utility methods for processing, smoothing, simplifying,
    /// resampling, rounding, and measuring paths represented by point sequences.
    /// </summary>
    public static class PathTools
    {
        /// <summary>
        /// Smooths a sequence of points by generating intermediate points using
        /// quadratic Bézier curves while preserving sufficiently sharp corners.
        /// </summary>
        /// <param name="points">The points that define the path.</param>
        /// <param name="smoothFactor">
        /// The interpolation factor used to determine the smoothing control points.
        /// </param>
        /// <param name="maxAngleDeg">
        /// The maximum angle in degrees for a point to be smoothed.
        /// </param>
        /// <param name="segments">
        /// The number of generated segments for each smoothed corner.
        /// </param>
        /// <returns>A new list containing the smoothed path.</returns>
        public static List<Point2> SmoothPath(
            List<Point2> points,
            float smoothFactor = 0.5f,
            float maxAngleDeg = 170f,
            int segments = 3)
        {
            if (points == null || points.Count < 3)
                return points != null
                    ? new List<Point2>(points)
                    : new List<Point2>();

            var result = new List<Point2>
            {
                points[0]
            };

            for (int i = 1; i < points.Count - 1; i++)
            {
                Point2 prev = points[i - 1];
                Point2 current = points[i];
                Point2 next = points[i + 1];

                Vector2 dir1 = VectorMath.SafeNormalize(current - prev);
                Vector2 dir2 = VectorMath.SafeNormalize(next - current);

                float dot = float.Clamp(
                    Vector2.Dot(dir1, dir2),
                    -1f,
                    1f);

                float angleDeg =
                    float.Acos(dot) *
                    (180f / float.Pi);

                if (angleDeg < maxAngleDeg)
                {
                    Point2 p1 =
                        Point2.Lerp(
                            current,
                            prev,
                            smoothFactor);

                    Point2 p2 =
                        Point2.Lerp(
                            current,
                            next,
                            smoothFactor);

                    for (int s = 0; s <= segments; s++)
                    {
                        float t = s / (float)segments;
                        float mt = 1f - t;

                        result.Add(
                            new Point2(
                                mt * mt * p1.X +
                                2f * mt * t * current.X +
                                t * t * p2.X,

                                mt * mt * p1.Y +
                                2f * mt * t * current.Y +
                                t * t * p2.Y));
                    }
                }
                else
                {
                    result.Add(current);
                }
            }

            result.Add(points[^1]);
            return result;
        }

        /// <summary>
        /// Applies Chaikin's corner-cutting algorithm to smooth a sequence of points.
        /// </summary>
        /// <param name="points">The points that define the path.</param>
        /// <param name="iterations">
        /// The number of smoothing iterations to perform.
        /// </param>
        /// <returns>A new list containing the smoothed path.</returns>
        public static List<Point2> ChaikinSmooth(
            List<Point2> points,
            int iterations = 1)
        {
            if (points == null || points.Count < 3)
            {
                return points != null
                    ? new List<Point2>(points)
                    : new List<Point2>();
            }

            var segment = new List<Point2>(points);

            for (int it = 0; it < iterations; it++)
            {
                var newSegment = new List<Point2>
                {
                    segment[0]
                };

                for (int i = 0; i < segment.Count - 1; i++)
                {
                    Point2 p0 = segment[i];
                    Point2 p1 = segment[i + 1];

                    Point2 q = Point2.Lerp(p0, p1, 0.25f);
                    Point2 r = Point2.Lerp(p0, p1, 0.75f);

                    newSegment.Add(q);
                    newSegment.Add(r);
                }

                newSegment.Add(segment[^1]);
                segment = newSegment;
            }

            return segment;
        }

        /// <summary>
        /// Simplifies a path using the Douglas-Peucker algorithm while attempting
        /// to preserve its original shape.
        /// </summary>
        /// <param name="points">The points that define the path.</param>
        /// <param name="tolerance">
        /// The maximum allowed deviation from the simplified path.
        /// </param>
        /// <returns>A new list containing the simplified path.</returns>
        public static List<Point2> SimplifyDouglasPeucker(
            List<Point2> points,
            float tolerance)
        {
            if (points == null || points.Count < 3)
            {
                return points != null
                    ? new List<Point2>(points)
                    : new List<Point2>();
            }

            var result = new List<Point2>();

            SimplifyDouglasPeuckerRecursive(
                points,
                0,
                points.Count - 1,
                tolerance,
                result);

            result.Insert(0, points[0]);
            result.Add(points[^1]);

            return result;
        }

        private static void SimplifyDouglasPeuckerRecursive(
            List<Point2> points,
            int start,
            int end,
            float tolerance,
            List<Point2> result)
        {
            if (end <= start + 1)
                return;

            float maxDistance = 0f;
            int index = -1;

            Point2 a = points[start];
            Point2 b = points[end];

            for (int i = start + 1; i < end; i++)
            {
                float distance =
                    PerpendicularDistance(
                        points[i],
                        a,
                        b);

                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    index = i;
                }
            }

            if (maxDistance > tolerance)
            {
                SimplifyDouglasPeuckerRecursive(
                    points,
                    start,
                    index,
                    tolerance,
                    result);

                result.Add(points[index]);

                SimplifyDouglasPeuckerRecursive(
                    points,
                    index,
                    end,
                    tolerance,
                    result);
            }
        }

        private static float PerpendicularDistance(
            Point2 point,
            Point2 a,
            Point2 b)
        {
            if (a == b)
                return Point2.Distance(point, a);

            float numerator =
                float.Abs(
                    (b.Y - a.Y) * point.X -
                    (b.X - a.X) * point.Y +
                    b.X * a.Y -
                    b.Y * a.X);

            float denominator =
                Point2.Distance(a, b);

            return numerator / denominator;
        }

        /// <summary>
        /// Resamples a section of a path using linear interpolation to insert
        /// a fixed number of additional points between two existing points.
        /// </summary>
        /// <param name="points">The points that define the path.</param>
        /// <param name="startIndex">
        /// The index of the first point of the segment.
        /// </param>
        /// <param name="endIndex">
        /// The index of the last point of the segment.
        /// </param>
        /// <param name="newPointCount">
        /// The number of additional points to generate.
        /// </param>
        /// <returns>A new list containing the resampled path.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the specified indices are outside the valid range
        /// or the start index is not before the end index.
        /// </exception>
        public static List<Point2> ResampleLinear(
            List<Point2> points,
            int startIndex,
            int endIndex,
            int newPointCount)
        {
            if (startIndex < 0 ||
                endIndex >= points.Count ||
                startIndex >= endIndex)
            {
                throw new ArgumentOutOfRangeException();
            }

            var result =
                new List<Point2>(
                    points.Count +
                    newPointCount);

            for (int i = 0; i < startIndex; i++)
                result.Add(points[i]);

            Point2 start = points[startIndex];
            Point2 end = points[endIndex];

            int steps = newPointCount + 1;

            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;

                result.Add(
                    Point2.Lerp(
                        start,
                        end,
                        t));
            }

            for (int i = endIndex + 1; i < points.Count; i++)
                result.Add(points[i]);

            return result;
        }

        /// <summary>
        /// Rounds a single corner of a path by replacing it with a circular arc.
        /// </summary>
        /// <param name="points">The points that define the path.</param>
        /// <param name="cornerIndex">
        /// The index of the corner to round.
        /// </param>
        /// <param name="radius">
        /// The radius of the rounded corner.
        /// </param>
        /// <param name="segments">
        /// The number of segments used to approximate the arc.
        /// </param>
        /// <returns>
        /// A new list containing the path with the specified corner rounded.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the path contains fewer than three points.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the corner index refers to the first or last point.
        /// </exception>
        public static List<Point2> RoundCorner(
            List<Point2> points,
            int cornerIndex,
            float radius,
            int segments = 6)
        {
            if (points == null || points.Count < 3)
            {
                throw new ArgumentException(
                    "Pfad muss mindestens 3 Punkte haben.");
            }

            if (cornerIndex <= 0 ||
                cornerIndex >= points.Count - 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cornerIndex),
                    "Ecke darf nicht erster oder letzter Punkt sein.");
            }

            if (radius <= 0f || segments < 1)
                return new List<Point2>(points);

            Point2 prev = points[cornerIndex - 1];
            Point2 corner = points[cornerIndex];
            Point2 next = points[cornerIndex + 1];

            Vector2 dir1 =
                VectorMath.SafeNormalize(
                    corner - prev);

            Vector2 dir2 =
                VectorMath.SafeNormalize(
                    next - corner);

            float angle1 =
                float.Atan2(
                    dir1.Y,
                    dir1.X);

            float angle2 =
                float.Atan2(
                    dir2.Y,
                    dir2.X);

            float cutLength =
                radius /
                float.Tan(
                    float.Abs(angle2 - angle1) /
                    2f);

            Point2 p1 =
                corner -
                dir1 * cutLength;

            Point2 p2 =
                corner +
                dir2 * cutLength;

            Vector2 bisectorDir =
                VectorMath.SafeNormalize(
                    dir1 + dir2);

            Point2 center =
                corner -
                bisectorDir * radius;

            float startAngle =
                float.Atan2(
                    p1.Y - center.Y,
                    p1.X - center.X);

            float endAngle =
                float.Atan2(
                    p2.Y - center.Y,
                    p2.X - center.X);

            if (endAngle < startAngle)
                endAngle += MathHelper.TwoPi;

            var result = new List<Point2>();

            for (int i = 0; i < cornerIndex; i++)
                result.Add(points[i]);

            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;

                float angle =
                    MathHelper.Lerp(
                        startAngle,
                        endAngle,
                        t);

                result.Add(
                    new Point2(
                        center.X +
                        radius * float.Cos(angle),

                        center.Y +
                        radius * float.Sin(angle)));
            }

            for (int i = cornerIndex + 1; i < points.Count; i++)
                result.Add(points[i]);

            return result;
        }

        /// <summary>
        /// Automatically rounds corners whose angle is below the specified maximum angle.
        /// </summary>
        /// <param name="points">The points that define the path.</param>
        /// <param name="radius">The radius used for rounded corners.</param>
        /// <param name="maxAngleDeg">
        /// The maximum corner angle in degrees for automatic rounding.
        /// </param>
        /// <param name="segments">
        /// The number of segments used to approximate each rounded corner.
        /// </param>
        /// <returns>
        /// A new list containing the path with eligible corners rounded.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the path contains fewer than three points.
        /// </exception>
        public static List<Point2> RoundCornersAuto(
            List<Point2> points,
            float radius,
            float maxAngleDeg = 150f,
            int segments = 6)
        {
            if (points == null || points.Count < 3)
            {
                throw new ArgumentException(
                    "Pfad muss mindestens 3 Punkte haben.");
            }

            var result = new List<Point2>
            {
                points[0]
            };

            for (int i = 1; i < points.Count - 1; i++)
            {
                Point2 prev = points[i - 1];
                Point2 corner = points[i];
                Point2 next = points[i + 1];

                Vector2 dir1 =
                    VectorMath.SafeNormalize(
                        corner - prev);

                Vector2 dir2 =
                    VectorMath.SafeNormalize(
                        next - corner);

                float dot =
                    float.Clamp(
                        Vector2.Dot(dir1, dir2),
                        -1f,
                        1f);

                float angleDeg =
                    float.Acos(dot) *
                    (180f / float.Pi);

                if (angleDeg < maxAngleDeg)
                {
                    var temp =
                        RoundCorner(
                            new List<Point2>
                            {
                                prev,
                                corner,
                                next
                            },
                            1,
                            radius,
                            segments);

                    for (int k = 1; k < temp.Count - 1; k++)
                        result.Add(temp[k]);
                }
                else
                {
                    result.Add(corner);
                }
            }

            result.Add(points[^1]);

            return result;
        }

        /// <summary>
        /// Calculates the total length of a path.
        /// </summary>
        /// <param name="points">The points that define the path.</param>
        /// <returns>The total length of the path.</returns>
        public static float PathLength(
            List<Point2> points)
        {
            if (points == null || points.Count < 2)
                return 0f;

            float length = 0f;

            for (int i = 1; i < points.Count; i++)
            {
                length +=
                    Point2.Distance(
                        points[i - 1],
                        points[i]);
            }

            return length;
        }

        /// <summary>
        /// Gets the point located at a specified distance along a path.
        /// </summary>
        /// <param name="points">The points that define the path.</param>
        /// <param name="distance">
        /// The distance from the beginning of the path.
        /// </param>
        /// <returns>
        /// The interpolated point at the specified distance,
        /// or the last point when the distance exceeds the path length.
        /// </returns>
        public static Point2 GetPointAtDistance(
            List<Point2> points,
            float distance)
        {
            if (points == null || points.Count < 2)
                return Point2.Zero;

            float traveled = 0f;

            for (int i = 1; i < points.Count; i++)
            {
                float segmentLength =
                    Point2.Distance(
                        points[i - 1],
                        points[i]);

                if (traveled + segmentLength >= distance)
                {
                    if (segmentLength <= 0f)
                        return points[i];

                    float t =
                        (distance - traveled) /
                        segmentLength;

                    return Point2.Lerp(
                        points[i - 1],
                        points[i],
                        t);
                }

                traveled += segmentLength;
            }

            return points[^1];
        }
    }
}
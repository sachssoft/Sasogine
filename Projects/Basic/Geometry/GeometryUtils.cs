using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry;

/// <summary>
/// Provides utility methods for common geometry operations.
/// </summary>
public static class GeometryUtils
{
    /// <summary>
    /// Compresses a sequence of points by removing points that are too close
    /// together or form nearly straight segments.
    /// </summary>
    /// <param name="points">
    /// The points to compress.
    /// </param>
    /// <param name="distanceThreshold">
    /// The minimum distance required between retained points.
    /// </param>
    /// <param name="angleDegreeThreshold">
    /// The minimum angular difference, in degrees, required to retain
    /// an intermediate point.
    /// </param>
    /// <returns>
    /// A new list containing the compressed point sequence.
    /// </returns>
    public static List<Vector2> CompressPoints(
        List<Vector2> points,
        float distanceThreshold = 0.2f,
        float angleDegreeThreshold = 5f)
    {
        if (points == null || points.Count < 3)
        {
            return points != null
                ? new List<Vector2>(points)
                : new List<Vector2>();
        }

        distanceThreshold = float.Max(0f, distanceThreshold);
        angleDegreeThreshold = float.Max(0f, angleDegreeThreshold);

        float distanceThresholdSquared =
            distanceThreshold * distanceThreshold;

        float angleThreshold =
            MathHelper.ToRadians(angleDegreeThreshold);

        var compressed = new List<Vector2>(points.Count)
        {
            points[0]
        };

        for (int i = 1; i < points.Count - 1; i++)
        {
            Vector2 previous = compressed[^1];
            Vector2 current = points[i];
            Vector2 next = points[i + 1];

            if (Vector2.DistanceSquared(
                    previous,
                    current) < distanceThresholdSquared)
            {
                continue;
            }

            Vector2 directionA =
                VectorMath.SafeNormalize(current - previous);

            Vector2 directionB =
                VectorMath.SafeNormalize(next - current);

            if (directionA == Vector2.Zero ||
                directionB == Vector2.Zero)
            {
                continue;
            }

            float dot = float.Clamp(
                Vector2.Dot(directionA, directionB),
                -1f,
                1f);

            float angle = float.Acos(dot);

            if (angle >= angleThreshold)
                compressed.Add(current);
        }

        compressed.Add(points[^1]);

        return compressed;
    }

    /// <summary>
    /// Creates a rectangular polygon representing a line with symmetric thickness.
    /// </summary>
    /// <param name="start">
    /// The start point of the line.
    /// </param>
    /// <param name="end">
    /// The end point of the line.
    /// </param>
    /// <param name="thickness">
    /// The total thickness of the generated line.
    /// </param>
    /// <returns>
    /// Four vertices representing the generated line polygon.
    /// </returns>
    public static Vector2[] CreateWidedLine(
        Vector2 start,
        Vector2 end,
        float thickness)
    {
        thickness = float.Max(0f, thickness);

        float halfThickness = thickness * 0.5f;

        return CreateWidedLine(
            start,
            end,
            halfThickness,
            halfThickness);
    }

    /// <summary>
    /// Creates a rectangular polygon representing a line with independently
    /// configurable widths on both sides.
    /// </summary>
    /// <param name="start">
    /// The start point of the line.
    /// </param>
    /// <param name="end">
    /// The end point of the line.
    /// </param>
    /// <param name="positiveWidth">
    /// The width applied to the positive perpendicular side.
    /// </param>
    /// <param name="negativeWidth">
    /// The width applied to the negative perpendicular side.
    /// </param>
    /// <returns>
    /// Four vertices representing the generated line polygon.
    /// </returns>
    public static Vector2[] CreateWidedLine(
        Vector2 start,
        Vector2 end,
        float positiveWidth,
        float negativeWidth)
    {
        positiveWidth = float.Max(0f, positiveWidth);
        negativeWidth = float.Max(0f, negativeWidth);

        Vector2 direction =
            VectorMath.SafeNormalize(end - start);

        if (direction == Vector2.Zero)
        {
            return new[]
            {
                start,
                start,
                end,
                end
            };
        }

        Vector2 perpendicular =
            VectorMath.Perpendicular(direction);

        return new[]
        {
            start - perpendicular * negativeWidth,
            start + perpendicular * positiveWidth,
            end + perpendicular * positiveWidth,
            end - perpendicular * negativeWidth
        };
    }
}
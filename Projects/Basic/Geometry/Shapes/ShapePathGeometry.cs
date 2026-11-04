using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Geometry.Shapes;

/// <summary>
/// Validates generated shape parameters and closes sampled contours.
/// </summary>
internal static class ShapePathGeometry
{
    /// <summary>
    /// Validates a finite shape parameter.
    /// </summary>
    /// <param name="value">The parameter value.</param>
    /// <param name="name">The property name used in the diagnostic.</param>
    /// <exception cref="InvalidOperationException">The value is not finite.</exception>
    internal static void ValidateFinite(float value, string name)
    {
        if (!float.IsFinite(value))
            throw new InvalidOperationException($"{name} must be finite.");
    }

    /// <summary>
    /// Validates a curve sampling count before allocating point arrays.
    /// </summary>
    /// <param name="segments">The requested segment count.</param>
    /// <param name="minimum">The smallest supported segment count.</param>
    /// <exception cref="InvalidOperationException">The count is outside the supported range.</exception>
    internal static void ValidateSegments(int segments, int minimum = 1)
    {
        if (segments < minimum || segments == int.MaxValue)
        {
            throw new InvalidOperationException(
                $"Segments must be at least {minimum} and less than {int.MaxValue}.");
        }
    }

    /// <summary>
    /// Validates a supported corner interpolation type.
    /// </summary>
    /// <param name="value">The interpolation type.</param>
    /// <param name="name">The property name used in the diagnostic.</param>
    /// <exception cref="InvalidOperationException">The interpolation type is unsupported.</exception>
    internal static void ValidateRoundingType(RoundingType value, string name)
    {
        if (value != RoundingType.Linear &&
            value != RoundingType.Quadratic &&
            value != RoundingType.Cubic)
        {
            throw new InvalidOperationException($"{name} is not a supported rounding type.");
        }
    }

    /// <summary>
    /// Removes consecutive duplicate points and closes a sampled shape contour.
    /// </summary>
    /// <param name="points">The generated contour, modified in place.</param>
    /// <returns>A path containing the closed contour.</returns>
    /// <exception cref="InvalidOperationException">
    /// A generated coordinate is not finite or fewer than three contour vertices remain.
    /// </exception>
    internal static Path CreatePath(List<Vector2> points)
    {
        int count = 0;

        for (int i = 0; i < points.Count; i++)
        {
            Vector2 point = points[i];

            if (!float.IsFinite(point.X) || !float.IsFinite(point.Y))
                throw new InvalidOperationException("The generated shape contains a non-finite coordinate.");

            if (count == 0 || points[count - 1] != point)
                points[count++] = point;
        }

        if (count < points.Count)
            points.RemoveRange(count, points.Count - count);

        int vertexCount = points.Count;

        if (vertexCount > 1 && points[^1] == points[0])
            vertexCount--;

        if (vertexCount < 3)
            throw new InvalidOperationException("The generated shape must contain at least three contour vertices.");

        if (points[^1] != points[0])
            points.Add(points[0]);

        return new Path(new[] { points.ToArray() });
    }
}

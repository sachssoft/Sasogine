using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Sasogine.Geometry { 

/// <summary>
/// Provides sampling methods for spline and curve geometry.
/// </summary>
public static partial class GeometrySampler
{
    // =========================================================================
    // Catmull-Rom
    // =========================================================================

    /// <summary>
    /// Samples a Catmull-Rom spline through the specified points.
    /// </summary>
    /// <param name="points">
    /// The control points of the spline.
    /// </param>
    /// <param name="segmentsPerSpan">
    /// The number of sampled segments between two consecutive control points.
    /// </param>
    /// <param name="closed">
    /// <see langword="true"/> to create a closed spline;
    /// otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>
    /// The sampled points of the spline.
    /// </returns>
    public static Vector2[] SampleCatmullRom(
        Vector2[] points,
        int segmentsPerSpan,
        bool closed = false)
    {
        if (points is null)
        {
            throw new ArgumentNullException(
                nameof(points));
        }

        if (points.Length < 2)
        {
            return Array.Empty<Vector2>();
        }

        segmentsPerSpan =
            Math.Max(
                1,
                segmentsPerSpan);

        int spanCount =
            closed
                ? points.Length
                : points.Length - 1;

        Vector2[] result =
            new Vector2[
                spanCount *
                segmentsPerSpan +
                1];

        int resultIndex = 0;

        for (int span = 0;
             span < spanCount;
             span++)
        {
            Vector2 p0 =
                GetCatmullRomPoint(
                    points,
                    span - 1,
                    closed);

            Vector2 p1 =
                GetCatmullRomPoint(
                    points,
                    span,
                    closed);

            Vector2 p2 =
                GetCatmullRomPoint(
                    points,
                    span + 1,
                    closed);

            Vector2 p3 =
                GetCatmullRomPoint(
                    points,
                    span + 2,
                    closed);

            for (int i = 0;
                 i < segmentsPerSpan;
                 i++)
            {
                float t =
                    i /
                    (float)segmentsPerSpan;

                result[resultIndex++] =
                    EvaluateCatmullRom(
                        p0,
                        p1,
                        p2,
                        p3,
                        t);
            }
        }

        result[resultIndex] =
            closed
                ? points[0]
                : points[points.Length - 1];

        return result;
    }

    /// <summary>
    /// Gets a control point used by a Catmull-Rom span.
    /// </summary>
    private static Vector2 GetCatmullRomPoint(
        Vector2[] points,
        int index,
        bool closed)
    {
        if (closed)
        {
            index %=
                points.Length;

            if (index < 0)
            {
                index +=
                    points.Length;
            }

            return points[index];
        }

        if (index < 0)
        {
            return points[0];
        }

        if (index >= points.Length)
        {
            return points[
                points.Length - 1];
        }

        return points[index];
    }

    /// <summary>
    /// Evaluates a single Catmull-Rom spline span.
    /// </summary>
    private static Vector2 EvaluateCatmullRom(
        Vector2 p0,
        Vector2 p1,
        Vector2 p2,
        Vector2 p3,
        float t)
    {
        float t2 =
            t * t;

        float t3 =
            t2 * t;

        return
            0.5f *
            (
                2f * p1 +

                (-p0 + p2) * t +

                (2f * p0 -
                 5f * p1 +
                 4f * p2 -
                 p3) * t2 +

                (-p0 +
                 3f * p1 -
                 3f * p2 +
                 p3) * t3
            );
    }

    // =========================================================================
    // B-Spline
    // =========================================================================

    /// <summary>
    /// Samples a clamped uniform B-spline.
    /// </summary>
    /// <param name="controlPoints">
    /// The control points of the spline.
    /// </param>
    /// <param name="degree">
    /// The polynomial degree of the spline.
    /// A degree of 3 creates a cubic B-spline.
    /// </param>
    /// <param name="segments">
    /// The total number of sampled segments.
    /// </param>
    /// <returns>
    /// The sampled points of the B-spline.
    /// </returns>
    public static Vector2[] SampleBSpline(
        Vector2[] controlPoints,
        int degree,
        int segments)
    {
        if (controlPoints is null)
        {
            throw new ArgumentNullException(
                nameof(controlPoints));
        }

        if (controlPoints.Length == 0)
        {
            return Array.Empty<Vector2>();
        }

        if (degree < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(degree));
        }

        if (degree >= controlPoints.Length)
        {
            throw new ArgumentException(
                "Degree must be smaller than the number of control points.",
                nameof(degree));
        }

        segments =
            Math.Max(
                1,
                segments);

        double[] knots =
            CreateUniformClampedKnots(
                controlPoints.Length,
                degree);

        Vector2[] result =
            new Vector2[
                segments + 1];

        double start =
            knots[degree];

        double end =
            knots[
                controlPoints.Length];

        for (int i = 0;
             i <= segments;
             i++)
        {
            double t =
                start +
                (end - start) *
                (i /
                 (double)segments);

            result[i] =
                EvaluateBSpline(
                    controlPoints,
                    degree,
                    knots,
                    t);
        }

        return result;
    }

    /// <summary>
    /// Creates a uniform clamped knot vector for a B-spline.
    /// </summary>
    private static double[] CreateUniformClampedKnots(
        int controlPointCount,
        int degree)
    {
        int knotCount =
            controlPointCount +
            degree +
            1;

        double[] knots =
            new double[knotCount];

        int interiorCount =
            controlPointCount -
            degree -
            1;

        for (int i = 0;
             i < knotCount;
             i++)
        {
            if (i <= degree)
            {
                knots[i] =
                    0d;
            }
            else if (i >= controlPointCount)
            {
                knots[i] =
                    1d;
            }
            else
            {
                knots[i] =
                    (i - degree) /
                    (double)(interiorCount + 1);
            }
        }

        return knots;
    }

    /// <summary>
    /// Evaluates a B-spline using the de Boor algorithm.
    /// </summary>
    private static Vector2 EvaluateBSpline(
        Vector2[] controlPoints,
        int degree,
        double[] knots,
        double t)
    {
        int n =
            controlPoints.Length - 1;

        int span =
            FindKnotSpan(
                n,
                degree,
                t,
                knots);

        Vector2[] local =
            new Vector2[
                degree + 1];

        for (int j = 0;
             j <= degree;
             j++)
        {
            local[j] =
                controlPoints[
                    span -
                    degree +
                    j];
        }

        for (int r = 1;
             r <= degree;
             r++)
        {
            for (int j = degree;
                 j >= r;
                 j--)
            {
                int index =
                    span -
                    degree +
                    j;

                double denominator =
                    knots[
                        index +
                        degree -
                        r +
                        1] -
                    knots[index];

                double alpha =
                    denominator == 0d
                        ? 0d
                        : (t -
                           knots[index]) /
                          denominator;

                local[j] =
                    Vector2.Lerp(
                        local[j - 1],
                        local[j],
                        (float)alpha);
            }
        }

        return local[degree];
    }

    /// <summary>
    /// Finds the knot span containing the specified parameter.
    /// </summary>
    private static int FindKnotSpan(
        int n,
        int degree,
        double t,
        double[] knots)
    {
        if (t >= knots[n + 1])
        {
            return n;
        }

        if (t <= knots[degree])
        {
            return degree;
        }

        int low =
            degree;

        int high =
            n + 1;

        int middle =
            (low + high) / 2;

        while (
            t < knots[middle] ||
            t >= knots[middle + 1])
        {
            if (t < knots[middle])
            {
                high =
                    middle;
            }
            else
            {
                low =
                    middle;
            }

            middle =
                (low + high) / 2;
        }

        return middle;
    }

    // =========================================================================
    // Hermite
    // =========================================================================

    /// <summary>
    /// Samples a cubic Hermite curve.
    /// </summary>
    /// <param name="start">
    /// The start position.
    /// </param>
    /// <param name="startTangent">
    /// The tangent at the start position.
    /// </param>
    /// <param name="end">
    /// The end position.
    /// </param>
    /// <param name="endTangent">
    /// The tangent at the end position.
    /// </param>
    /// <param name="segments">
    /// The number of sampled segments.
    /// </param>
    /// <returns>
    /// The sampled points of the Hermite curve.
    /// </returns>
    public static Vector2[] SampleHermite(
        Vector2 start,
        Vector2 startTangent,
        Vector2 end,
        Vector2 endTangent,
        int segments)
    {
        segments =
            Math.Max(
                1,
                segments);

        Vector2[] result =
            new Vector2[
                segments + 1];

        for (int i = 0;
             i <= segments;
             i++)
        {
            float t =
                i /
                (float)segments;

            float t2 =
                t * t;

            float t3 =
                t2 * t;

            float h00 =
                2f * t3 -
                3f * t2 +
                1f;

            float h10 =
                t3 -
                2f * t2 +
                t;

            float h01 =
                -2f * t3 +
                3f * t2;

            float h11 =
                t3 -
                t2;

            result[i] =
                h00 * start +
                h10 * startTangent +
                h01 * end +
                h11 * endTangent;
        }

        return result;
    }
}
}
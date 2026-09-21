using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry.Shapes;

/// <summary>
/// Represents an elliptical shape path defined within normalized
/// coordinates and approximated using configurable path segments.
/// </summary>
public class EllipsePath : ShapePathBase
{
    /// <summary>
    /// Gets the number of segments used to construct the full ellipse.
    /// Higher values produce a more detailed approximation of the curve.
    /// </summary>
    public int Segments { get; init; } = 64;

    /// <summary>
    /// Gets the rounding method used to construct the individual
    /// segments of the ellipse.
    /// </summary>
    /// <remarks>
    /// Linear rounding connects consecutive ellipse points directly,
    /// while quadratic and cubic rounding use Bézier interpolation
    /// between the generated points.
    /// </remarks>
    public RoundingType RoundingType { get; init; } =
        RoundingType.Linear;

    /// <inheritdoc/>
    protected override Path BuildDefinedPath()
    {
        var points = new List<Vector2>();

        const float cx = 0.5f;
        const float cy = 0.5f;
        const float rx = 0.5f;
        const float ry = 0.5f;

        Vector2 center =
            new(cx, cy);

        Vector2 prevPoint =
            new(cx + rx, cy);

        points.Add(prevPoint);

        for (int i = 1; i <= Segments; i++)
        {
            float t =
                i / (float)Segments;

            float angle =
                t * MathHelper.TwoPi;

            Vector2 nextPoint =
                new(
                    cx + rx * MathF.Cos(angle),
                    cy + ry * MathF.Sin(angle));

            Vector2[] sampledPoints =
                RoundingType switch
                {
                    RoundingType.Linear =>
                        GeometrySampler.SampleLinear(
                            prevPoint,
                            nextPoint,
                            1),

                    RoundingType.Quadratic =>
                        GeometrySampler.SampleQuadraticBezier(
                            prevPoint,
                            center,
                            nextPoint,
                            1),

                    RoundingType.Cubic =>
                        GeometrySampler.SampleCubicBezier(
                            prevPoint,
                            prevPoint + (center - prevPoint) * 0.5f,
                            nextPoint + (center - nextPoint) * 0.5f,
                            nextPoint,
                            1),

                    _ => throw new NotImplementedException()
                };

            for (int j = 1; j < sampledPoints.Length; j++)
                points.Add(sampledPoints[j]);

            prevPoint = nextPoint;
        }

        if (points.Count > 0 &&
            points[0] != points[^1])
        {
            points.Add(points[0]);
        }

        return new Path(
            new[]
            {
                points.ToArray()
            });
    }
}
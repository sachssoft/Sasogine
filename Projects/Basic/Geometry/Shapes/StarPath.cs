using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry.Shapes;

/// <summary>
/// Represents a star-shaped path with configurable inner and outer radii
/// and optional rounded points.
/// </summary>
/// <remarks>
/// The star is generated around the normalized center point
/// <c>(0.5, 0.5)</c>.
/// </remarks>
public class StarPath : ShapePathBase
{
    /// <summary>
    /// Gets the number of outer points of the star.
    /// </summary>
    public int Spikes { get; init; } = 5;

    /// <summary>
    /// Gets the radius of the outer points.
    /// </summary>
    public float OuterRadius { get; init; } = 1f;

    /// <summary>
    /// Gets the radius of the inner points.
    /// </summary>
    public float InnerRadius { get; init; } = 0.5f;

    /// <summary>
    /// Gets the rotation angle of the star, in degrees.
    /// </summary>
    public float Angle { get; init; }

    /// <summary>
    /// Gets the rounding amount applied to the outer points.
    /// </summary>
    /// <remarks>
    /// The effective value is clamped to the range <c>0</c> to <c>1</c>.
    /// </remarks>
    public float OuterRounding { get; init; }

    /// <summary>
    /// Gets the rounding amount applied to the inner points.
    /// </summary>
    /// <remarks>
    /// The effective value is clamped to the range <c>0</c> to <c>1</c>.
    /// </remarks>
    public float InnerRounding { get; init; }

    /// <summary>
    /// Gets the rounding type used for the outer points.
    /// </summary>
    public RoundingType OuterRoundingType { get; init; } =
        RoundingType.Quadratic;

    /// <summary>
    /// Gets the rounding type used for the inner points.
    /// </summary>
    public RoundingType InnerRoundingType { get; init; } =
        RoundingType.Quadratic;

    /// <summary>
    /// Gets the number of segments used to sample rounded points.
    /// </summary>
    public int Segments { get; init; } = 8;

    /// <summary>
    /// Builds the path representing the star.
    /// </summary>
    /// <returns>
    /// A <see cref="Path"/> containing the generated star geometry.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="Spikes"/> is less than <c>2</c>.
    /// </exception>
    protected override Path BuildDefinedPath()
    {
        if (Spikes < 2)
        {
            throw new InvalidOperationException(
                "Star must have at least 2 spikes.");
        }

        float outerRounding =
            MathHelper.Clamp(OuterRounding, 0f, 1f);

        float innerRounding =
            MathHelper.Clamp(InnerRounding, 0f, 1f);

        float rotation = MathHelper.ToRadians(Angle);
        float step = MathHelper.TwoPi / Spikes;
        float halfStep = step * 0.5f;

        Vector2 center = new(0.5f, 0.5f);

        var points = new Vector2[Spikes * 2];

        for (int i = 0; i < Spikes; i++)
        {
            float outerAngle = rotation + i * step;
            float innerAngle = outerAngle + halfStep;

            points[i * 2] =
                center +
                new Vector2(
                    MathF.Cos(outerAngle),
                    MathF.Sin(outerAngle)) *
                OuterRadius;

            points[i * 2 + 1] =
                center +
                new Vector2(
                    MathF.Cos(innerAngle),
                    MathF.Sin(innerAngle)) *
                InnerRadius;
        }

        var polygon = new List<Vector2>();

        for (int i = 0; i < points.Length; i++)
        {
            Vector2 previous =
                points[(i + points.Length - 1) % points.Length];

            Vector2 current = points[i];

            Vector2 next =
                points[(i + 1) % points.Length];

            bool outerPoint = i % 2 == 0;

            float rounding =
                outerPoint
                    ? outerRounding
                    : innerRounding;

            RoundingType roundingType =
                outerPoint
                    ? OuterRoundingType
                    : InnerRoundingType;

            AddPoint(
                polygon,
                previous,
                current,
                next,
                rounding,
                roundingType);
        }

        if (polygon.Count > 0 &&
            polygon[0] != polygon[^1])
        {
            polygon.Add(polygon[0]);
        }

        return new Path(
            new[]
            {
                polygon.ToArray()
            });
    }

    private void AddPoint(
        List<Vector2> polygon,
        Vector2 previous,
        Vector2 current,
        Vector2 next,
        float rounding,
        RoundingType roundingType)
    {
        if (rounding <= 0f)
        {
            polygon.Add(current);
            return;
        }

        Vector2 previousDirection =
            Vector2.Normalize(current - previous);

        Vector2 nextDirection =
            Vector2.Normalize(next - current);

        float previousDistance =
            Vector2.Distance(previous, current) * rounding;

        float nextDistance =
            Vector2.Distance(current, next) * rounding;

        Vector2 start =
            current - previousDirection * previousDistance;

        Vector2 end =
            current + nextDirection * nextDistance;

        Vector2[] sampledPoints = roundingType switch
        {
            RoundingType.Linear =>
                GeometrySampler.SampleLinear(
                    start,
                    end,
                    Segments),

            RoundingType.Quadratic =>
                GeometrySampler.SampleQuadraticBezier(
                    start,
                    current,
                    end,
                    Segments),

            RoundingType.Cubic =>
                GeometrySampler.SampleCubicBezier(
                    start,
                    start + previousDirection * previousDistance * 0.5f,
                    end - nextDirection * nextDistance * 0.5f,
                    end,
                    Segments),

            _ => throw new NotImplementedException()
        };

        polygon.AddRange(sampledPoints);
    }
}
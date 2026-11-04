using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Geometry.Shapes;

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
    public int Spikes
    {
        get;
        init;
    } = 5;

    /// <summary>
    /// Gets the positive, finite radius of the outer points.
    /// </summary>
    public float OuterRadius
    {
        get;
        init;
    } = 1f;

    /// <summary>
    /// Gets the nonnegative, finite radius of the inner points.
    /// </summary>
    public float InnerRadius
    {
        get;
        init;
    } = 0.5f;

    /// <summary>
    /// Gets the rotation angle of the star, in degrees.
    /// </summary>
    public float Angle
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the rounding amount applied to the outer points.
    /// </summary>
    /// <remarks>
    /// The value is clamped to zero through one. Inner and outer rounding amounts
    /// are proportionally reduced when their sum would exceed one adjacent edge.
    /// </remarks>
    public float OuterRounding
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the rounding amount applied to the inner points.
    /// </summary>
    /// <remarks>
    /// The value is clamped to zero through one. Inner and outer rounding amounts
    /// are proportionally reduced when their sum would exceed one adjacent edge.
    /// </remarks>
    public float InnerRounding
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the rounding type used for the outer points.
    /// </summary>
    public RoundingType OuterRoundingType
    {
        get;
        init;
    } = RoundingType.Quadratic;

    /// <summary>
    /// Gets the rounding type used for the inner points.
    /// </summary>
    public RoundingType InnerRoundingType
    {
        get;
        init;
    } = RoundingType.Quadratic;

    /// <summary>
    /// Gets the number of segments used to sample rounded points; must be at least one.
    /// </summary>
    public int Segments
    {
        get;
        init;
    } = 8;

    /// <summary>
    /// Builds the path representing the star.
    /// </summary>
    /// <returns>
    /// A <see cref="Path"/> containing the generated star geometry.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// A spike count, segment count, radius, angle, rounding amount, or interpolation type is invalid.
    /// </exception>
    protected override Path BuildDefinedPath()
    {
        if (Spikes < 2 || Spikes > (int.MaxValue - 1) / 2)
        {
            throw new InvalidOperationException(
                "Spikes must be at least 2 and permit allocating twice as many points.");
        }

        ShapePathGeometry.ValidateSegments(Segments);
        ShapePathGeometry.ValidateFinite(Angle, nameof(Angle));
        ShapePathGeometry.ValidateFinite(OuterRadius, nameof(OuterRadius));
        ShapePathGeometry.ValidateFinite(InnerRadius, nameof(InnerRadius));
        ShapePathGeometry.ValidateFinite(OuterRounding, nameof(OuterRounding));
        ShapePathGeometry.ValidateFinite(InnerRounding, nameof(InnerRounding));
        ShapePathGeometry.ValidateRoundingType(OuterRoundingType, nameof(OuterRoundingType));
        ShapePathGeometry.ValidateRoundingType(InnerRoundingType, nameof(InnerRoundingType));

        if (OuterRadius <= 0f || InnerRadius < 0f)
            throw new InvalidOperationException("OuterRadius must be positive and InnerRadius must be nonnegative.");

        float outerRounding =
            MathHelper.Clamp(OuterRounding, 0f, 1f);

        float innerRounding =
            MathHelper.Clamp(InnerRounding, 0f, 1f);

        float roundingSum = outerRounding + innerRounding;

        if (roundingSum > 1f)
        {
            outerRounding /= roundingSum;
            innerRounding /= roundingSum;
        }

        float rotation = MathHelper.ToRadians(Angle % 360f);
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

        return ShapePathGeometry.CreatePath(polygon);
    }

    /// <summary>
    /// Appends a sharp or rounded star vertex to the contour.
    /// </summary>
    /// <param name="polygon">The contour receiving the sampled points.</param>
    /// <param name="previous">The previous star vertex.</param>
    /// <param name="current">The vertex being sampled.</param>
    /// <param name="next">The next star vertex.</param>
    /// <param name="rounding">The non-overlapping fraction of each adjacent edge.</param>
    /// <param name="roundingType">The corner interpolation type.</param>
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

        Vector2 start =
            previous * rounding + current * (1f - rounding);

        Vector2 end =
            current * (1f - rounding) + next * rounding;

        Vector2[] sampledPoints = roundingType switch
        {
            RoundingType.Linear =>
                new[] { start, end },

            RoundingType.Quadratic =>
                GeometrySampler.SampleQuadraticBezier(
                    start,
                    current,
                    end,
                    Segments),

            RoundingType.Cubic =>
                GeometrySampler.SampleCubicBezier(
                    start,
                    start * 0.5f + current * 0.5f,
                    end * 0.5f + current * 0.5f,
                    end,
                    Segments),

            _ => throw new InvalidOperationException("The rounding type is unsupported.")
        };

        polygon.AddRange(sampledPoints);
    }
}
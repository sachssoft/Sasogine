using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Geometry.Shapes;

/// <summary>
/// Represents a regular polygonal path with optional rounded corners.
/// </summary>
/// <remarks>
/// The polygon is generated within normalized coordinates ranging from
/// <c>0</c> to <c>1</c> and supports linear, quadratic, and cubic corner rounding.
/// </remarks>
public class PolygonPath : ShapePathBase
{
    /// <summary>
    /// Gets the number of segments used to sample each rounded corner; must be at least one.
    /// </summary>
    public int Segments
    {
        get;
        init;
    } = 8;

    /// <summary>
    /// Gets the number of sides of the polygon.
    /// </summary>
    /// <remarks>
    /// The value must be at least <c>3</c>.
    /// </remarks>
    public int Sides
    {
        get;
        init;
    } = 5;

    /// <summary>
    /// Gets the rotation angle of the polygon, in degrees.
    /// </summary>
    public float Angle
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the amount of corner rounding.
    /// </summary>
    /// <remarks>
    /// The effective value is clamped to the range <c>0</c> to <c>1</c>.
    /// A value of <c>0</c> produces sharp corners, while higher values
    /// increase the rounding amount.
    /// </remarks>
    public float Rounding
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the interpolation type used to generate rounded corners.
    /// </summary>
    public RoundingType RoundingType
    {
        get;
        init;
    } = RoundingType.Quadratic;

    /// <summary>
    /// Builds the path representing the polygon.
    /// </summary>
    /// <returns>
    /// A <see cref="Path"/> containing the generated polygon geometry.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// A side count, segment count, angle, rounding amount, or interpolation type is invalid.
    /// </exception>
    protected override Path BuildDefinedPath()
    {
        if (Sides < 3 || Sides == int.MaxValue)
            throw new InvalidOperationException(
                "Sides must be at least 3 and less than int.MaxValue.");

        ShapePathGeometry.ValidateSegments(Segments);
        ShapePathGeometry.ValidateFinite(Angle, nameof(Angle));
        ShapePathGeometry.ValidateFinite(Rounding, nameof(Rounding));
        ShapePathGeometry.ValidateRoundingType(RoundingType, nameof(RoundingType));

        float rounding = MathHelper.Clamp(Rounding, 0f, 1f) / 2f;
        float rotationRad = MathHelper.ToRadians(Angle % 360f);
        float step = MathHelper.TwoPi / Sides;
        float radius = 0.5f;

        Vector2[] points = new Vector2[Sides];

        for (int i = 0; i < Sides; i++)
        {
            float theta = rotationRad + i * step;

            points[i] = new Vector2(
                0.5f + radius * MathF.Cos(theta),
                0.5f + radius * MathF.Sin(theta));
        }

        var polygon = new List<Vector2>();

        for (int i = 0; i < Sides; i++)
        {
            Vector2 prev = points[(i + Sides - 1) % Sides];
            Vector2 curr = points[i];
            Vector2 next = points[(i + 1) % Sides];

            if (rounding == 0f)
            {
                polygon.Add(curr);
                continue;
            }

            Vector2 start =
                prev * rounding + curr * (1f - rounding);

            Vector2 end =
                curr * (1f - rounding) + next * rounding;

            Vector2[] sampledPoints = RoundingType switch
            {
                RoundingType.Linear =>
                    new[] { start, end },

                RoundingType.Quadratic =>
                    GeometrySampler.SampleQuadraticBezier(
                        start,
                        curr,
                        end,
                        Segments),

                RoundingType.Cubic =>
                    GeometrySampler.SampleCubicBezier(
                        start,
                        start * 0.5f + curr * 0.5f,
                        end * 0.5f + curr * 0.5f,
                        end,
                        Segments),

                _ => throw new InvalidOperationException("The rounding type is unsupported.")
            };

            polygon.AddRange(sampledPoints);
        }

        return ShapePathGeometry.CreatePath(polygon);
    }
}

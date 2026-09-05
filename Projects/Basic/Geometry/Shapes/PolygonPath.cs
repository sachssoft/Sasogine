using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry.Shapes;

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
    /// Gets the number of segments used to sample each rounded corner.
    /// </summary>
    public int Segments { get; init; } = 8;

    /// <summary>
    /// Gets the number of sides of the polygon.
    /// </summary>
    /// <remarks>
    /// The value must be at least <c>3</c>.
    /// </remarks>
    public int Sides { get; init; } = 5;

    /// <summary>
    /// Gets the rotation angle of the polygon, in degrees.
    /// </summary>
    public float Angle { get; init; }

    /// <summary>
    /// Gets the amount of corner rounding.
    /// </summary>
    /// <remarks>
    /// The effective value is clamped to the range <c>0</c> to <c>1</c>.
    /// A value of <c>0</c> produces sharp corners, while higher values
    /// increase the rounding amount.
    /// </remarks>
    public float Rounding { get; init; }

    /// <summary>
    /// Gets the interpolation type used to generate rounded corners.
    /// </summary>
    public RoundingType RoundingType { get; init; } =
        RoundingType.Quadratic;

    /// <summary>
    /// Builds the path representing the polygon.
    /// </summary>
    /// <returns>
    /// A <see cref="Path"/> containing the generated polygon geometry.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="Sides"/> is less than <c>3</c>.
    /// </exception>
    /// <exception cref="NotImplementedException">
    /// Thrown when the specified <see cref="RoundingType"/> is not supported.
    /// </exception>
    protected override Path BuildDefinedPath()
    {
        if (Sides < 3)
            throw new InvalidOperationException(
                "Polygon must have at least 3 sides.");

        float rounding = MathHelper.Clamp(Rounding, 0f, 1f) / 2f;
        float rotationRad = MathHelper.ToRadians(Angle);
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

            Vector2 dirPrev = Vector2.Normalize(curr - prev);
            Vector2 dirNext = Vector2.Normalize(next - curr);

            float distPrev =
                Vector2.Distance(curr, prev) * rounding;

            float distNext =
                Vector2.Distance(next, curr) * rounding;

            Vector2 start =
                curr - dirPrev * distPrev;

            Vector2 end =
                curr + dirNext * distNext;

            Vector2[] sampledPoints = RoundingType switch
            {
                RoundingType.Linear =>
                    GeometrySampler.SampleLinear(
                        start,
                        end,
                        Segments),

                RoundingType.Quadratic =>
                    GeometrySampler.SampleQuadraticBezier(
                        start,
                        curr,
                        end,
                        Segments),

                RoundingType.Cubic =>
                    GeometrySampler.SampleCubicBezier(
                        start,
                        start + dirPrev * distPrev * 0.5f,
                        end - dirNext * distNext * 0.5f,
                        end,
                        Segments),

                _ => throw new NotImplementedException()
            };

            polygon.AddRange(sampledPoints);
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
}
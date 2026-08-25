using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry.Shapes
{
    /// <summary>
    /// Represents a polygonal path with optional rounded corners.
    /// Supports linear, quadratic, and cubic rounding types.
    /// </summary>
    public class PolygonPath : ShapePathBase
    {
        /// <summary>
        /// Number of segments used when sampling curves for rounded corners.
        /// </summary>
        public int Segments { get; init; } = 8;

        /// <summary>
        /// Number of sides of the polygon. Must be >= 3.
        /// </summary>
        public int Sides { get; init; } = 5;

        /// <summary>
        /// Rotation of the polygon in degrees.
        /// </summary>
        public float Angle { get; init; }

        /// <summary>
        /// Amount of rounding for the corners. 0..1, representing a percentage of edge length.
        /// </summary>
        public float Rounding { get; init; }

        /// <summary>
        /// Type of rounding applied to corners.
        /// </summary>
        public RoundingType RoundingType { get; init; } = RoundingType.Quadratic;

        /// <summary>
        /// Builds the polygon path with the specified sides, rounding, and rotation.
        /// </summary>
        /// <returns>A <see cref="Path"/> representing the polygon shape.</returns>
        protected override Path BuildDefinedPath()
        {
            if (Sides < 3) throw new InvalidOperationException("Polygon must have at least 3 sides.");

            float rounding = MathHelper.Clamp(Rounding, 0f, 1f) / 2f;
            float rotationRad = MathHelper.ToRadians(Angle);
            float step = MathHelper.TwoPi / Sides;
            float radius = 0.5f;

            // Compute polygon vertices
            Vector2[] points = new Vector2[Sides];
            for (int i = 0; i < Sides; i++)
            {
                float theta = rotationRad + i * step;
                points[i] = new Vector2(0.5f + radius * MathF.Cos(theta),
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

                float distPrev = Vector2.Distance(curr, prev) * rounding;
                float distNext = Vector2.Distance(next, curr) * rounding;

                Vector2 start = curr - dirPrev * distPrev;
                Vector2 end = curr + dirNext * distNext;

                // Sample points for the corner based on rounding type
                Vector2[] sampledPoints = RoundingType switch
                {
                    RoundingType.Linear => GeometrySampler.SampleLinear(start, end, Segments),
                    RoundingType.Quadratic => GeometrySampler.SampleQuadraticBezier(start, curr, end, Segments),
                    RoundingType.Cubic => GeometrySampler.SampleCubicBezier(start, start + dirPrev * distPrev * 0.5f, end - dirNext * distNext * 0.5f, end, Segments),
                    _ => throw new NotImplementedException()
                };

                polygon.AddRange(sampledPoints);
            }

            // Close the polygon
            if (polygon.Count > 0 && polygon[0] != polygon[^1])
                polygon.Add(polygon[0]);

            return new Path(new[] { polygon.ToArray() });
        }
    }
}

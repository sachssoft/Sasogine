using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry.Shapes
{
    public class EllipsePath : ShapePathBase
    {
        /// <summary>
        /// Number of segments used for the full ellipse.
        /// Higher values result in smoother curves.
        /// </summary>
        public int Segments { get; init; } = 64;

        /// <summary>
        /// Determines the curve type used for the ellipse.
        /// Linear = straight lines, Quadratic/Cubic = Bezier curves.
        /// </summary>
        public RoundingType RoundingType { get; init; } = RoundingType.Linear;

        protected override Path BuildDefinedPath()
        {
            var points = new List<Vector2>();

            const float cx = 0.5f;
            const float cy = 0.5f;
            const float rx = 0.5f;
            const float ry = 0.5f;

            Vector2 prevPoint = new Vector2(cx + rx, cy);
            points.Add(prevPoint);

            for (int i = 1; i <= Segments; i++)
            {
                float t = i / (float)Segments;
                float angle = t * MathHelper.TwoPi;
                Vector2 nextPoint = new Vector2(cx + rx * MathF.Cos(angle), cy + ry * MathF.Sin(angle));

                Vector2[] sampledPoints = RoundingType switch
                {
                    RoundingType.Linear => GeometrySampler.SampleLinear(prevPoint, nextPoint, 1),
                    RoundingType.Quadratic => GeometrySampler.SampleQuadraticBezier(prevPoint,
                                                                                     new Vector2(cx, cy),
                                                                                     nextPoint,
                                                                                     1),
                    RoundingType.Cubic => GeometrySampler.SampleCubicBezier(prevPoint,
                                                                            prevPoint + (new Vector2(cx, cy) - prevPoint) * 0.5f,
                                                                            nextPoint + (new Vector2(cx, cy) - nextPoint) * 0.5f,
                                                                            nextPoint,
                                                                            1),
                    _ => throw new NotImplementedException()
                };

                // Add sampled points (skip first to avoid duplicate)
                for (int j = 1; j < sampledPoints.Length; j++)
                    points.Add(sampledPoints[j]);

                prevPoint = nextPoint;
            }

            // Close the ellipse
            if (points.Count > 0 && points[0] != points[^1])
                points.Add(points[0]);

            return new Path(new[] { points.ToArray() });
        }
    }
}

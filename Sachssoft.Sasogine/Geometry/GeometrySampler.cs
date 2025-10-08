using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Sasogine.Geometry
{
    public static class GeometrySampler
    {
        /// <summary>
        /// Sample a cubic bezier curve with n segments.
        /// </summary>
        public static Vector2[] SampleCubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, int segments)
        {
            segments = Math.Max(1, segments);
            Vector2[] points = new Vector2[segments + 1];

            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                float mt = 1f - t;

                points[i] =
                    mt * mt * mt * p0 +
                    3f * mt * mt * t * p1 +
                    3f * mt * t * t * p2 +
                    t * t * t * p3;
            }

            return points;
        }

        /// <summary>
        /// Sample a quadratic bezier curve with n segments.
        /// </summary>
        public static Vector2[] SampleQuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, int segments)
        {
            segments = Math.Max(1, segments);
            Vector2[] points = new Vector2[segments + 1];

            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                float mt = 1f - t;

                points[i] =
                    mt * mt * p0 +
                    2f * mt * t * p1 +
                    t * t * p2;
            }

            return points;
        }

        /// <summary>
        /// Sample an SVG-style elliptical arc with n segments.
        /// </summary>
        public static Vector2[] SampleArc(Vector2 start, Vector2 end, float rx, float ry, float xAxisRotation, bool largeArc, bool sweep, int segments)
        {
            segments = Math.Max(1, segments);

            // Convert to radians
            float phi = MathHelper.ToRadians(xAxisRotation);
            float cosPhi = (float)Math.Cos(phi);
            float sinPhi = (float)Math.Sin(phi);

            // Step 1: Compute (x1', y1')
            float dx = (start.X - end.X) / 2f;
            float dy = (start.Y - end.Y) / 2f;
            float x1p = cosPhi * dx + sinPhi * dy;
            float y1p = -sinPhi * dx + cosPhi * dy;

            // Step 2: Correct radii
            rx = Math.Abs(rx);
            ry = Math.Abs(ry);
            float rx_sq = rx * rx;
            float ry_sq = ry * ry;
            float x1p_sq = x1p * x1p;
            float y1p_sq = y1p * y1p;

            float lambda = x1p_sq / rx_sq + y1p_sq / ry_sq;
            if (lambda > 1f)
            {
                float scale = (float)Math.Sqrt(lambda);
                rx *= scale;
                ry *= scale;
                rx_sq = rx * rx;
                ry_sq = ry * ry;
            }

            // Step 3: Compute center
            float sign = (largeArc == sweep) ? -1f : 1f;
            float numerator = rx_sq * ry_sq - rx_sq * y1p_sq - ry_sq * x1p_sq;
            numerator = Math.Max(0, numerator);
            float factor = sign * (float)Math.Sqrt(numerator / (rx_sq * y1p_sq + ry_sq * x1p_sq));

            float cxp = factor * (rx * y1p / ry);
            float cyp = factor * -(ry * x1p / rx);

            float cx = cosPhi * cxp - sinPhi * cyp + (start.X + end.X) / 2f;
            float cy = sinPhi * cxp + cosPhi * cyp + (start.Y + end.Y) / 2f;

            // Step 4: Compute angles
            float theta1 = (float)Math.Atan2((y1p - cyp) / ry, (x1p - cxp) / rx);
            float deltaTheta = (float)Math.Atan2((-y1p - cyp) / ry, (-x1p - cxp) / rx) - theta1;

            if (sweep && deltaTheta < 0)
                deltaTheta += MathHelper.TwoPi;
            else if (!sweep && deltaTheta > 0)
                deltaTheta -= MathHelper.TwoPi;

            Vector2[] points = new Vector2[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                float angle = theta1 + t * deltaTheta;
                float x = cosPhi * rx * (float)Math.Cos(angle) - sinPhi * ry * (float)Math.Sin(angle) + cx;
                float y = sinPhi * rx * (float)Math.Cos(angle) + cosPhi * ry * (float)Math.Sin(angle) + cy;
                points[i] = new Vector2(x, y);
            }

            return points;
        }
    }
}

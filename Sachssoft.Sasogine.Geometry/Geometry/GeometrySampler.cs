using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Sasogine.Geometry
{
    /// <summary>
    /// Provides sampling functions for common 2D geometric curves.
    /// Supports linear segments, quadratic and cubic Bezier curves, and SVG-style arcs.
    /// </summary>
    public static class GeometrySampler
    {
        /// <summary>
        /// Samples a linear segment between two points.
        /// </summary>
        /// <param name="start">Start point.</param>
        /// <param name="end">End point.</param>
        /// <param name="segments">Number of segments to divide the line into. Must be >= 1.</param>
        /// <returns>An array of points along the line.</returns>
        public static Vector2[] SampleLinear(Vector2 start, Vector2 end, int segments)
        {
            segments = Math.Max(1, segments);
            Vector2[] points = new Vector2[segments + 1];

            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                points[i] = start * (1 - t) + end * t;
            }

            return points;
        }

        /// <summary>
        /// Samples a quadratic Bezier curve with a specified number of segments.
        /// </summary>
        /// <param name="p0">Start point.</param>
        /// <param name="p1">Control point.</param>
        /// <param name="p2">End point.</param>
        /// <param name="segments">Number of segments to divide the curve into. Must be >= 1.</param>
        /// <returns>An array of points along the curve.</returns>
        public static Vector2[] SampleQuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, int segments)
        {
            segments = Math.Max(1, segments);
            Vector2[] points = new Vector2[segments + 1];

            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                float mt = 1f - t;
                points[i] = mt * mt * p0 + 2f * mt * t * p1 + t * t * p2;
            }

            return points;
        }

        /// <summary>
        /// Samples a cubic Bezier curve with a specified number of segments.
        /// </summary>
        /// <param name="p0">Start point.</param>
        /// <param name="c1">First control point.</param>
        /// <param name="c2">Second control point.</param>
        /// <param name="p3">End point.</param>
        /// <param name="segments">Number of segments to divide the curve into. Must be >= 1.</param>
        /// <returns>An array of points along the curve.</returns>
        public static Vector2[] SampleCubicBezier(Vector2 p0, Vector2 c1, Vector2 c2, Vector2 p3, int segments)
        {
            segments = Math.Max(1, segments);
            Vector2[] points = new Vector2[segments + 1];

            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                float mt = 1f - t;
                points[i] = mt * mt * mt * p0 +
                            3f * mt * mt * t * c1 +
                            3f * mt * t * t * c2 +
                            t * t * t * p3;
            }

            return points;
        }

        /// <summary>
        /// Samples an SVG-style elliptical arc with a specified number of segments.
        /// Handles degenerate cases where the radius is zero or start equals end.
        /// </summary>
        /// <param name="start">Start point.</param>
        /// <param name="end">End point.</param>
        /// <param name="rx">Ellipse radius in X.</param>
        /// <param name="ry">Ellipse radius in Y.</param>
        /// <param name="xAxisRotation">Rotation of the ellipse in degrees.</param>
        /// <param name="largeArc">Large arc flag (true = use the larger arc).</param>
        /// <param name="sweep">Sweep flag (true = positive angle direction).</param>
        /// <param name="segments">Number of segments to divide the arc into. Must be >= 1.</param>
        /// <returns>An array of points along the arc.</returns>
        public static Vector2[] SampleArc(Vector2 start, Vector2 end, float rx, float ry, float xAxisRotation, bool largeArc, bool sweep, int segments)
        {
            segments = Math.Max(1, segments);

            if (rx <= 0f || ry <= 0f || start == end)
            {
                // Degenerate case: straight line or zero-size arc
                return new Vector2[] { start, end };
            }

            float phi = MathHelper.ToRadians(xAxisRotation);
            float cosPhi = (float)Math.Cos(phi);
            float sinPhi = (float)Math.Sin(phi);

            // Transform coordinates to arc space
            float dx = (start.X - end.X) / 2f;
            float dy = (start.Y - end.Y) / 2f;
            float x1p = cosPhi * dx + sinPhi * dy;
            float y1p = -sinPhi * dx + cosPhi * dy;

            rx = Math.Abs(rx);
            ry = Math.Abs(ry);
            float rxSq = rx * rx;
            float rySq = ry * ry;
            float x1pSq = x1p * x1p;
            float y1pSq = y1p * y1p;

            float lambda = x1pSq / rxSq + y1pSq / rySq;
            if (lambda > 1f)
            {
                float scale = (float)Math.Sqrt(lambda);
                rx *= scale;
                ry *= scale;
                rxSq = rx * rx;
                rySq = ry * ry;
            }

            float numerator = Math.Max(0f, rxSq * rySq - rxSq * y1pSq - rySq * x1pSq);
            float denominator = rxSq * y1pSq + rySq * x1pSq;
            float factor = (denominator == 0f) ? 0f : (largeArc == sweep ? -1f : 1f) * (float)Math.Sqrt(numerator / denominator);

            float cxp = factor * (rx * y1p / ry);
            float cyp = factor * -(ry * x1p / rx);

            float cx = cosPhi * cxp - sinPhi * cyp + (start.X + end.X) / 2f;
            float cy = sinPhi * cxp + cosPhi * cyp + (start.Y + end.Y) / 2f;

            float theta1 = (float)Math.Atan2((y1p - cyp) / ry, (x1p - cxp) / rx);
            float deltaTheta = (float)Math.Atan2((-y1p - cyp) / ry, (-x1p - cxp) / rx) - theta1;

            if (sweep && deltaTheta < 0) deltaTheta += MathHelper.TwoPi;
            else if (!sweep && deltaTheta > 0) deltaTheta -= MathHelper.TwoPi;

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

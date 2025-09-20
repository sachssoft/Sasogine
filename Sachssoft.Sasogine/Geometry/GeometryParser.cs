using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Sachssoft.Sasogine.Geometry
{
    public static class GeometryParser
    {
        public static PathCollection Parse(string data, int curveSegments = 8)
        {
            if (string.IsNullOrWhiteSpace(data))
                return new PathCollection();

            var paths = new List<Path>();
            var currentPath = new List<Vector2>();
            Vector2 lastPoint = Vector2.Zero;
            Vector2? lastControl = null;
            char command = ' ';
            int index = 0;

            var regex = new Regex(@"([MmLlHhVvCcSsQqTtAaZz])|(-?\d*\.?\d+)", RegexOptions.Compiled);
            var matches = regex.Matches(data);

            List<float> numbers = new List<float>();

            void FlushNumbers()
            {
                if (numbers.Count > 0)
                {
                    int i = 0;
                    while (i < numbers.Count)
                    {
                        switch (command)
                        {
                            case 'M':
                                lastPoint = new Vector2(numbers[i++], numbers[i++]);
                                currentPath.Add(lastPoint);
                                command = 'L'; // nach moveto automatisch lineto
                                break;
                            case 'm':
                                lastPoint += new Vector2(numbers[i++], numbers[i++]);
                                currentPath.Add(lastPoint);
                                command = 'l';
                                break;
                            case 'L':
                                lastPoint = new Vector2(numbers[i++], numbers[i++]);
                                currentPath.Add(lastPoint);
                                break;
                            case 'l':
                                lastPoint += new Vector2(numbers[i++], numbers[i++]);
                                currentPath.Add(lastPoint);
                                break;
                            case 'H':
                                lastPoint = new Vector2(numbers[i++], lastPoint.Y);
                                currentPath.Add(lastPoint);
                                break;
                            case 'h':
                                lastPoint += new Vector2(numbers[i++], 0);
                                currentPath.Add(lastPoint);
                                break;
                            case 'V':
                                lastPoint = new Vector2(lastPoint.X, numbers[i++]);
                                currentPath.Add(lastPoint);
                                break;
                            case 'v':
                                lastPoint += new Vector2(0, numbers[i++]);
                                currentPath.Add(lastPoint);
                                break;
                            case 'C':
                                {
                                    Vector2 c1 = new Vector2(numbers[i++], numbers[i++]);
                                    Vector2 c2 = new Vector2(numbers[i++], numbers[i++]);
                                    Vector2 p = new Vector2(numbers[i++], numbers[i++]);
                                    var segment = SampleCubicBezier(lastPoint, c1, c2, p, curveSegments);
                                    currentPath.AddRange(segment[1..]);
                                    lastPoint = p;
                                    lastControl = c2;
                                }
                                break;
                            case 'c':
                                {
                                    Vector2 c1 = lastPoint + new Vector2(numbers[i++], numbers[i++]);
                                    Vector2 c2 = lastPoint + new Vector2(numbers[i++], numbers[i++]);
                                    Vector2 p = lastPoint + new Vector2(numbers[i++], numbers[i++]);
                                    var segment = SampleCubicBezier(lastPoint, c1, c2, p, curveSegments);
                                    currentPath.AddRange(segment[1..]);
                                    lastPoint = p;
                                    lastControl = c2;
                                }
                                break;
                            case 'S':
                                {
                                    Vector2 c1 = lastControl.HasValue ? Reflect(lastControl.Value, lastPoint) : lastPoint;
                                    Vector2 c2 = new Vector2(numbers[i++], numbers[i++]);
                                    Vector2 p = new Vector2(numbers[i++], numbers[i++]);
                                    var segment = SampleCubicBezier(lastPoint, c1, c2, p, curveSegments);
                                    currentPath.AddRange(segment[1..]);
                                    lastPoint = p;
                                    lastControl = c2;
                                }
                                break;
                            case 's':
                                {
                                    Vector2 c1 = lastControl.HasValue ? Reflect(lastControl.Value, lastPoint) : lastPoint;
                                    Vector2 c2 = lastPoint + new Vector2(numbers[i++], numbers[i++]);
                                    Vector2 p = lastPoint + new Vector2(numbers[i++], numbers[i++]);
                                    var segment = SampleCubicBezier(lastPoint, c1, c2, p, curveSegments);
                                    currentPath.AddRange(segment[1..]);
                                    lastPoint = p;
                                    lastControl = c2;
                                }
                                break;
                            case 'Q':
                                {
                                    Vector2 c = new Vector2(numbers[i++], numbers[i++]);
                                    Vector2 p = new Vector2(numbers[i++], numbers[i++]);
                                    var segment = SampleQuadraticBezier(lastPoint, c, p, curveSegments);
                                    currentPath.AddRange(segment[1..]);
                                    lastPoint = p;
                                    lastControl = c;
                                }
                                break;
                            case 'q':
                                {
                                    Vector2 c = lastPoint + new Vector2(numbers[i++], numbers[i++]);
                                    Vector2 p = lastPoint + new Vector2(numbers[i++], numbers[i++]);
                                    var segment = SampleQuadraticBezier(lastPoint, c, p, curveSegments);
                                    currentPath.AddRange(segment[1..]);
                                    lastPoint = p;
                                    lastControl = c;
                                }
                                break;
                            case 'T':
                                {
                                    Vector2 c = lastControl.HasValue ? Reflect(lastControl.Value, lastPoint) : lastPoint;
                                    Vector2 p = new Vector2(numbers[i++], numbers[i++]);
                                    var segment = SampleQuadraticBezier(lastPoint, c, p, curveSegments);
                                    currentPath.AddRange(segment[1..]);
                                    lastPoint = p;
                                    lastControl = c;
                                }
                                break;
                            case 't':
                                {
                                    Vector2 c = lastControl.HasValue ? Reflect(lastControl.Value, lastPoint) : lastPoint;
                                    Vector2 p = lastPoint + new Vector2(numbers[i++], numbers[i++]);
                                    var segment = SampleQuadraticBezier(lastPoint, c, p, curveSegments);
                                    currentPath.AddRange(segment[1..]);
                                    lastPoint = p;
                                    lastControl = c;
                                }
                                break;
                            case 'A':
                                {
                                    float rx = numbers[i++];
                                    float ry = numbers[i++];
                                    float xAxisRot = numbers[i++];
                                    bool largeArc = numbers[i++] != 0;
                                    bool sweep = numbers[i++] != 0;
                                    Vector2 p = new Vector2(numbers[i++], numbers[i++]);
                                    var segment = SampleArc(lastPoint, p, rx, ry, xAxisRot, largeArc, sweep, curveSegments);
                                    currentPath.AddRange(segment[1..]);
                                    lastPoint = p;
                                    lastControl = null;
                                }
                                break;
                            case 'a':
                                {
                                    float rx = numbers[i++];
                                    float ry = numbers[i++];
                                    float xAxisRot = numbers[i++];
                                    bool largeArc = numbers[i++] != 0;
                                    bool sweep = numbers[i++] != 0;
                                    Vector2 p = lastPoint + new Vector2(numbers[i++], numbers[i++]);
                                    var segment = SampleArc(lastPoint, p, rx, ry, xAxisRot, largeArc, sweep, curveSegments);
                                    currentPath.AddRange(segment[1..]);
                                    lastPoint = p;
                                    lastControl = null;
                                }
                                break;
                            case 'Z':
                            case 'z':
                                if (currentPath.Count > 0)
                                {
                                    paths.Add(new Path(currentPath.ToArray()));
                                    currentPath.Clear();
                                    lastControl = null;
                                }
                                break;
                        }
                    }
                    numbers.Clear();
                }
            }

            // Parsing loop
            foreach (Match m in matches)
            {
                if (m.Groups[1].Success)
                {
                    // command
                    FlushNumbers();
                    command = m.Groups[1].Value[0];
                }
                else if (m.Groups[2].Success)
                {
                    numbers.Add(float.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture));
                }
            }
            FlushNumbers();

            // letzte Punkte
            if (currentPath.Count > 0)
                paths.Add(new Path(currentPath.ToArray()));

            return new PathCollection(paths);
        }

        private static Vector2 Reflect(Vector2 control, Vector2 anchor) => anchor * 2f - control;

        private static Vector2[] SampleCubicBezier(Vector2 p0, Vector2 c1, Vector2 c2, Vector2 p1, int segments)
        {
            var points = new Vector2[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                float it = 1f - t;
                points[i] = it * it * it * p0 +
                            3f * it * it * t * c1 +
                            3f * it * t * t * c2 +
                            t * t * t * p1;
            }
            return points;
        }

        private static Vector2[] SampleQuadraticBezier(Vector2 p0, Vector2 c, Vector2 p1, int segments)
        {
            var points = new Vector2[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                float it = 1f - t;
                points[i] = it * it * p0 + 2f * it * t * c + t * t * p1;
            }
            return points;
        }

        private static Vector2[] SampleArc(Vector2 start, Vector2 end, float rx, float ry, float xAxisRotation, bool largeArc, bool sweep, int segments)
        {
            segments = Math.Max(1, segments);

            if (Vector2.DistanceSquared(start, end) < 1e-6f)
                return new Vector2[] { start, end };

            float phi = MathHelper.ToRadians(xAxisRotation % 360f);
            float cosPhi = MathF.Cos(phi);
            float sinPhi = MathF.Sin(phi);

            float dx = (start.X - end.X) / 2f;
            float dy = (start.Y - end.Y) / 2f;
            float x1p = cosPhi * dx + sinPhi * dy;
            float y1p = -sinPhi * dx + cosPhi * dy;

            rx = MathF.Abs(rx);
            ry = MathF.Abs(ry);

            float rxSq = rx * rx;
            float rySq = ry * ry;
            float x1pSq = x1p * x1p;
            float y1pSq = y1p * y1p;

            float lambda = x1pSq / rxSq + y1pSq / rySq;
            if (lambda > 1f)
            {
                float factor = MathF.Sqrt(lambda);
                rx *= factor;
                ry *= factor;
                rxSq = rx * rx;
                rySq = ry * ry;
            }

            float sign = (largeArc == sweep) ? -1f : 1f;
            float num = rxSq * rySq - rxSq * y1pSq - rySq * x1pSq;
            float denom = rxSq * y1pSq + rySq * x1pSq;
            float coef = (num < 0 ? 0 : sign * MathF.Sqrt(num / denom));

            float cxp = coef * (rx * y1p / ry);
            float cyp = coef * (-ry * x1p / rx);

            float cx = cosPhi * cxp - sinPhi * cyp + (start.X + end.X) / 2f;
            float cy = sinPhi * cxp + cosPhi * cyp + (start.Y + end.Y) / 2f;

            float theta1 = VectorAngle(1f, 0f, (x1p - cxp) / rx, (y1p - cyp) / ry);
            float deltaTheta = VectorAngle((x1p - cxp) / rx, (y1p - cyp) / ry, (-x1p - cxp) / rx, (-y1p - cyp) / ry);

            if (!sweep && deltaTheta > 0) deltaTheta -= MathHelper.TwoPi;
            else if (sweep && deltaTheta < 0) deltaTheta += MathHelper.TwoPi;

            var points = new Vector2[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                float angle = theta1 + deltaTheta * t;
                float cosA = MathF.Cos(angle);
                float sinA = MathF.Sin(angle);
                float x = cosPhi * rx * cosA - sinPhi * ry * sinA + cx;
                float y = sinPhi * rx * cosA + cosPhi * ry * sinA + cy;
                points[i] = new Vector2(x, y);
            }

            return points;
        }

        private static float VectorAngle(float ux, float uy, float vx, float vy)
        {
            float dot = ux * vx + uy * vy;
            float det = ux * vy - uy * vx;
            return MathF.Atan2(det, dot);
        }
    }
}

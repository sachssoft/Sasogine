using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry
{
    /// <summary>
    /// Hilfsklasse mit Werkzeugen zur Pfadbearbeitung (Glätten, Vereinfachen, Abrunden, Resampling).
    /// </summary>
    public static class PathTools
    {
        // ============================================================
        // 🔹 GLÄTTUNG
        // ============================================================

        /// <summary>
        /// Glättet eine Sequenz von Punkten, indem Zwischenpunkte berechnet werden.
        /// Ecken über maxAngleDeg bleiben erhalten (z. B. 90°).
        /// </summary>
        public static List<Vector2> SmoothPath(List<Vector2> points, float smoothFactor = 0.5f, float maxAngleDeg = 170f, int segments = 3)
        {
            if (points == null || points.Count < 3)
                return new List<Vector2>(points);

            var result = new List<Vector2>();
            result.Add(points[0]); // erster Punkt bleibt

            for (int i = 1; i < points.Count - 1; i++)
            {
                Vector2 prev = points[i - 1];
                Vector2 current = points[i];
                Vector2 next = points[i + 1];

                Vector2 dir1 = Vector2.Normalize(current - prev);
                Vector2 dir2 = Vector2.Normalize(next - current);

                float dot = Vector2.Dot(dir1, dir2);
                dot = MathHelper.Clamp(dot, -1f, 1f);
                float angleDeg = MathF.Acos(dot) * (180f / MathF.PI);

                if (angleDeg < maxAngleDeg) // nur glätten, wenn Ecke spitz genug
                {
                    Vector2 p1 = Vector2.Lerp(current, prev, smoothFactor);
                    Vector2 p2 = Vector2.Lerp(current, next, smoothFactor);

                    for (int s = 0; s <= segments; s++)
                    {
                        float t = s / (float)segments;
                        Vector2 pt =
                            (1 - t) * (1 - t) * p1 +
                            2 * (1 - t) * t * current +
                            t * t * p2;
                        result.Add(pt);
                    }
                }
                else
                {
                    result.Add(current);
                }
            }

            result.Add(points[^1]); // letzter Punkt
            return result;
        }

        /// <summary>
        /// Chaikin's Corner Cutting (einfaches Glätten durch "Ecken abschneiden").
        /// </summary>
        public static List<Vector2> ChaikinSmooth(List<Vector2> points, int iterations = 1)
        {
            if (points == null || points.Count < 3) return new List<Vector2>(points);

            var segment = new List<Vector2>(points);

            for (int it = 0; it < iterations; it++)
            {
                var newSegment = new List<Vector2> { segment[0] };

                for (int i = 0; i < segment.Count - 1; i++)
                {
                    Vector2 p0 = segment[i];
                    Vector2 p1 = segment[i + 1];

                    Vector2 q = Vector2.Lerp(p0, p1, 0.25f);
                    Vector2 r = Vector2.Lerp(p0, p1, 0.75f);

                    newSegment.Add(q);
                    newSegment.Add(r);
                }

                newSegment.Add(segment[^1]);
                segment = newSegment;
            }

            return segment;
        }

        // ============================================================
        // 🔹 VEREINFACHUNG
        // ============================================================

        /// <summary>
        /// Douglas-Peucker-Vereinfachung (reduziert Punkte, behält Form).
        /// </summary>
        public static List<Vector2> SimplifyDouglasPeucker(List<Vector2> points, float tolerance)
        {
            if (points == null || points.Count < 3) return new List<Vector2>(points);

            var result = new List<Vector2>();
            SimplifyDouglasPeuckerRecursive(points, 0, points.Count - 1, tolerance, result);
            result.Insert(0, points[0]);
            result.Add(points[^1]);
            return result;
        }

        private static void SimplifyDouglasPeuckerRecursive(List<Vector2> points, int start, int end, float tolerance, List<Vector2> result)
        {
            if (end <= start + 1) return;

            float maxDistance = 0f;
            int index = -1;

            Vector2 a = points[start];
            Vector2 b = points[end];

            for (int i = start + 1; i < end; i++)
            {
                float distance = PerpendicularDistance(points[i], a, b);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    index = i;
                }
            }

            if (maxDistance > tolerance)
            {
                SimplifyDouglasPeuckerRecursive(points, start, index, tolerance, result);
                result.Add(points[index]);
                SimplifyDouglasPeuckerRecursive(points, index, end, tolerance, result);
            }
        }

        private static float PerpendicularDistance(Vector2 p, Vector2 a, Vector2 b)
        {
            if (a == b) return Vector2.Distance(p, a);
            float num = float.Abs((b.Y - a.Y) * p.X - (b.X - a.X) * p.Y + b.X * a.Y - b.Y * a.X);
            float den = Vector2.Distance(a, b);
            return num / den;
        }

        // ============================================================
        // 🔹 RESAMPLING
        // ============================================================

        /// <summary>
        /// Resampling eines Segments auf eine feste Anzahl Punkte (linear).
        /// </summary>
        public static List<Vector2> ResampleLinear(List<Vector2> points, int startIndex, int endIndex, int newPointCount)
        {
            if (startIndex < 0 || endIndex >= points.Count || startIndex >= endIndex)
                throw new ArgumentOutOfRangeException();

            var result = new List<Vector2>(points.Count + newPointCount);

            for (int i = 0; i < startIndex; i++)
                result.Add(points[i]);

            Vector2 start = points[startIndex];
            Vector2 end = points[endIndex];
            int steps = newPointCount + 1;

            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                result.Add(Vector2.Lerp(start, end, t));
            }

            for (int i = endIndex + 1; i < points.Count; i++)
                result.Add(points[i]);

            return result;
        }

        // ============================================================
        // 🔹 ECKEN ABRUNDEN
        // ============================================================

        public static List<Vector2> RoundCorner(List<Vector2> points, int cornerIndex, float radius, int segments = 6)
        {
            if (points == null || points.Count < 3)
                throw new ArgumentException("Pfad muss mindestens 3 Punkte haben.");
            if (cornerIndex <= 0 || cornerIndex >= points.Count - 1)
                throw new ArgumentOutOfRangeException(nameof(cornerIndex), "Ecke darf nicht erster oder letzter Punkt sein.");
            if (radius <= 0f || segments < 1)
                return new List<Vector2>(points);

            Vector2 prev = points[cornerIndex - 1];
            Vector2 corner = points[cornerIndex];
            Vector2 next = points[cornerIndex + 1];

            Vector2 dir1 = Vector2.Normalize(corner - prev);
            Vector2 dir2 = Vector2.Normalize(next - corner);

            float angle1 = MathF.Atan2(dir1.Y, dir1.X);
            float angle2 = MathF.Atan2(dir2.Y, dir2.X);

            float cutLength = radius / MathF.Tan(MathF.Abs(angle2 - angle1) / 2f);

            Vector2 p1 = corner - dir1 * cutLength;
            Vector2 p2 = corner + dir2 * cutLength;

            Vector2 bisectorDir = Vector2.Normalize(dir1 + dir2);
            Vector2 center = corner - bisectorDir * radius;

            float startAngle = MathF.Atan2(p1.Y - center.Y, p1.X - center.X);
            float endAngle = MathF.Atan2(p2.Y - center.Y, p2.X - center.X);

            if (endAngle < startAngle)
                endAngle += MathHelper.TwoPi;

            var result = new List<Vector2>();
            for (int i = 0; i < cornerIndex; i++)
                result.Add(points[i]);

            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                float angle = MathHelper.Lerp(startAngle, endAngle, t);
                result.Add(new Vector2(
                    center.X + radius * MathF.Cos(angle),
                    center.Y + radius * MathF.Sin(angle)
                ));
            }

            for (int i = cornerIndex + 1; i < points.Count; i++)
                result.Add(points[i]);

            return result;
        }

        public static List<Vector2> RoundCornersAuto(List<Vector2> points, float radius, float maxAngleDeg = 150f, int segments = 6)
        {
            if (points == null || points.Count < 3)
                throw new ArgumentException("Pfad muss mindestens 3 Punkte haben.");

            var result = new List<Vector2> { points[0] };

            for (int i = 1; i < points.Count - 1; i++)
            {
                Vector2 prev = points[i - 1];
                Vector2 corner = points[i];
                Vector2 next = points[i + 1];

                Vector2 dir1 = Vector2.Normalize(corner - prev);
                Vector2 dir2 = Vector2.Normalize(next - corner);

                float dot = Vector2.Dot(dir1, dir2);
                dot = MathHelper.Clamp(dot, -1f, 1f);
                float angleDeg = MathF.Acos(dot) * (180f / MathF.PI);

                if (angleDeg < maxAngleDeg)
                {
                    var temp = RoundCorner(new List<Vector2> { prev, corner, next }, 1, radius, segments);
                    for (int k = 1; k < temp.Count - 1; k++)
                        result.Add(temp[k]);
                }
                else
                {
                    result.Add(corner);
                }
            }

            result.Add(points[^1]);
            return result;
        }

        // ============================================================
        // 🔹 EXTRA IDEEN
        // ============================================================

        /// <summary>
        /// Berechnet die Gesamtlänge eines Pfades.
        /// </summary>
        public static float PathLength(List<Vector2> points)
        {
            if (points == null || points.Count < 2) return 0f;

            float length = 0f;
            for (int i = 1; i < points.Count; i++)
                length += Vector2.Distance(points[i - 1], points[i]);
            return length;
        }

        /// <summary>
        /// Holt einen Punkt entlang des Pfades bei bestimmter Distanz.
        /// </summary>
        public static Vector2 GetPointAtDistance(List<Vector2> points, float distance)
        {
            if (points == null || points.Count < 2) return Vector2.Zero;

            float traveled = 0f;
            for (int i = 1; i < points.Count; i++)
            {
                float segLen = Vector2.Distance(points[i - 1], points[i]);
                if (traveled + segLen >= distance)
                {
                    float t = (distance - traveled) / segLen;
                    return Vector2.Lerp(points[i - 1], points[i], t);
                }
                traveled += segLen;
            }

            return points[^1];
        }
    }
}

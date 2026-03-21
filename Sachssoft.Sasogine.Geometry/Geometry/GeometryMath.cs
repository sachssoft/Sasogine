using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Common;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Geometry
{
    public static class GeometryUtils
    {
        /// <summary>
        /// Projects a point onto a line segment and returns the parameter t and the projection position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SegmentProjectionResult ProjectPointOntoSegment(Vector2 point, Vector2 segmentStart, Vector2 segmentEnd)
        {
            // Vektor vom Start- zum Endpunkt des Segments
            Vector2 ab = segmentEnd - segmentStart;
            float lenSq = ab.LengthSquared();

            // Degenerierter Fall: Segment zu kurz
            if (lenSq < 1e-6f)
                return new SegmentProjectionResult(0f, segmentStart);

            // Projektion auf Segment berechnen
            float t = Vector2.Dot(point - segmentStart, ab) / lenSq;
            t = Math.Clamp(t, 0f, 1f);

            Vector2 tpos = segmentStart + t * ab;

            return new SegmentProjectionResult(t, tpos);
        }

        /// <summary>
        /// Finds the nearest polygon index to a location in a Path.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetNearestPolygon(Vector2 location, Sasogine.Geometry.Path path, out int nearest_polygon_index)
        {
            float nearest_distance = float.PositiveInfinity;
            nearest_polygon_index = -1;

            // Alle Polygone durchsuchen
            for (int i = 0; i < path.GetPolygonCount(); i++)
            {
                for (int j = 0; j < path.GetPointCount(i); j++)
                {
                    var point = path.GetPoint(i, j);
                    var distance = (location - point).Length();

                    // Kleinster Abstand merken
                    if (distance < nearest_distance)
                    {
                        nearest_distance = distance;
                        nearest_polygon_index = i;
                    }
                }
            }
        }

        /// <summary>
        /// Creates a wide line polygon with symmetric thickness.
        /// </summary>
        // Rechteck-Polygon mit gleicher Breite auf beiden Seiten
        public static Vertices CreateWidedLine(Vector2 start, Vector2 end, float thickness)
        {
            return CreateWidedLine(start, end, thickness / 2f, thickness / 2f);
        }

        /// <summary>
        /// Creates a wide line polygon with asymmetric thickness.
        /// </summary>
        // Rechteck-Polygon mit unterschiedlichen Breiten
        public static Vertices CreateWidedLine(Vector2 start, Vector2 end, float positiveWidth, float negativeWidth)
        {
            // Breiten auf mindestens 0 begrenzen
            positiveWidth = MathF.Max(0f, positiveWidth);
            negativeWidth = MathF.Max(0f, negativeWidth);

            // Richtung von start nach end
            var delta = end - start;
            var length = delta.Length();
            if (length < 1e-6f)
                length = 1f; // Division durch Null vermeiden

            // Normalisierter senkrechter Vektor
            var perp = new Vector2(-delta.Y, delta.X) / length;

            // Vertices erzeugen
            var vertices = new Vertices
            {
                start - perp * negativeWidth,
                start + perp * positiveWidth,
                end + perp * positiveWidth,
                end - perp * negativeWidth
            };

            return vertices;
        }

        /// <summary>
        /// Compresses a list of points by removing short or nearly straight segments.
        /// </summary>
        // Punkte komprimieren: kurze oder gerade Linien entfernen
        public static List<Vector2> CompressPoints(List<Vector2> points, float distanceThreshold = 0.2f, float angleDegreeThreshold = 5f)
        {
            if (points == null || points.Count < 3)
                return points != null ? new List<Vector2>(points) : new List<Vector2>();

            var compressed = new List<Vector2> { points[0] }; // erster Punkt immer behalten
            float angleThresholdRad = MathHelper.ToRadians(angleDegreeThreshold);

            // Punkte durchgehen, außer erster und letzter
            for (int i = 1; i < points.Count - 1; i++)
            {
                var prev = compressed[compressed.Count - 1];
                var current = points[i];
                var next = points[i + 1];

                // Segmentlänge prüfen
                if (Vector2.Distance(prev, current) < distanceThreshold)
                    continue;

                // Richtungen der Segmente
                var dir1 = Vector2.Normalize(current - prev);
                var dir2 = Vector2.Normalize(next - current);

                // Winkel zwischen Segmenten
                float angle = (float)Math.Acos(MathHelper.Clamp(Vector2.Dot(dir1, dir2), -1f, 1f));

                if (angle >= angleThresholdRad)
                    compressed.Add(current); // Punkt behalten
            }

            compressed.Add(points[points.Count - 1]); // letzter Punkt immer behalten
            return compressed;
        }
    }
}

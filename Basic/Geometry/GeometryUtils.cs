using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Geometry
{
    public static class GeometryUtils
    {
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
                float angle = float.Acos(MathHelper.Clamp(Vector2.Dot(dir1, dir2), -1f, 1f));

                if (angle >= angleThresholdRad)
                    compressed.Add(current); // Punkt behalten
            }

            compressed.Add(points[points.Count - 1]); // letzter Punkt immer behalten
            return compressed;
        }


        /// <summary>
        /// Creates a wide line polygon with symmetric thickness.
        /// </summary>
        // Rechteck-Polygon mit gleicher Breite auf beiden Seiten
        public static Vector2[] CreateWidedLine(Vector2 start, Vector2 end, float thickness)
        {
            return CreateWidedLine(start, end, thickness / 2f, thickness / 2f);
        }

        /// <summary>
        /// Creates a wide line polygon with asymmetric thickness.
        /// </summary>
        // Rechteck-Polygon mit unterschiedlichen Breiten
        public static Vector2[] CreateWidedLine(Vector2 start, Vector2 end, float positiveWidth, float negativeWidth)
        {
            // Breiten auf mindestens 0 begrenzen
            positiveWidth = float.Max(0f, positiveWidth);
            negativeWidth = float.Max(0f, negativeWidth);

            // Richtung von start nach end
            var delta = end - start;
            var length = delta.Length();
            if (length < 1e-6f)
                length = 1f; // Division durch Null vermeiden

            // Normalisierter senkrechter Vektor
            var perp = new Vector2(-delta.Y, delta.X) / length;

            // Vertices erzeugen
            var vertices = new Vector2[]
            {
                start - perp * negativeWidth,
                start + perp * positiveWidth,
                end + perp * positiveWidth,
                end - perp * negativeWidth
            };

            return vertices;
        }
    }
}

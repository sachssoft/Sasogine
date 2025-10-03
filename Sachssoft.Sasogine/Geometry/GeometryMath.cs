using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Sasogine.Geometry
{
    public static class GeometryMath
    {

        /// <summary>
        /// Projects a point onto a line segment and returns the closest point on the segment.
        /// </summary>
        /// <param name="point">The point to project.</param>
        /// <param name="segmentStart">Start of the segment.</param>
        /// <param name="segmentEnd">End of the segment.</param>
        /// <returns>The closest point on the segment to the given point.</returns>
        public static Vector2 ProjectPointOntoSegment(Vector2 point, Vector2 segmentStart, Vector2 segmentEnd)
        {
            // Berechne Richtungsvektor des Segments
            Vector2 AB = segmentEnd - segmentStart;

            // Länge des Segments quadriert
            float lenSq = AB.LengthSquared();

            // Projektion des Punktes auf die unendliche Linie
            float t = lenSq > 1e-6f ? Vector2.Dot(point - segmentStart, AB) / lenSq : 0f;

            // Clamp t, damit der Punkt innerhalb des Segments liegt
            t = Math.Clamp(t, 0f, 1f);

            // Berechne den nächsten Punkt auf dem Segment
            return segmentStart + t * AB;
        }



    }
}

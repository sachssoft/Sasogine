using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Geometry;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Math
{
    public static class VectorMath
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
            t = float.Clamp(t, 0f, 1f);

            Vector2 tpos = segmentStart + t * ab;

            return new SegmentProjectionResult(t, tpos);
        }
    }
}

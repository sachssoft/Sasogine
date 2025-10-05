using Microsoft.Xna.Framework;
using System;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Geometry
{
    public static class GeometryMath
    {
        /// <summary>
        /// Projektiert einen Punkt auf ein Liniensegment und gibt den Parameter t
        /// und die Projektion (tpos) in einer strukturierten Form zurück.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SegmentProjectionResult ProjectPointOntoSegment(Vector2 point, Vector2 segmentStart, Vector2 segmentEnd)
        {
            Vector2 ab = segmentEnd - segmentStart;
            float lenSq = ab.LengthSquared();

            if (lenSq < 1e-6f)
                return new SegmentProjectionResult(0f, segmentStart); // degenerierter Fall

            float t = Vector2.Dot(point - segmentStart, ab) / lenSq;
            t = Math.Clamp(t, 0f, 1f);
            Vector2 tpos = segmentStart + t * ab;

            return new SegmentProjectionResult(t, tpos);
        }

    }
}

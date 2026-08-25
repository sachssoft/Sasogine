using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Geometry
{

    public readonly struct SegmentProjectionResult
    {
        /// <summary>
        /// Normalisierte Position entlang des Segments (0 = Start, 1 = Ende).
        /// </summary>
        public readonly float SegmentFactor;

        /// <summary>
        /// Der projizierte Punkt auf dem Segment.
        /// </summary>
        public readonly Vector2 ProjectedPoint;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SegmentProjectionResult(float t, Vector2 tpos)
        {
            this.SegmentFactor = t;
            this.ProjectedPoint = tpos;
        }

        /// <summary>
        /// True, wenn der Punkt innerhalb des Segmentbereichs liegt (0 ≤ t ≤ 1).
        /// </summary>
        public bool IsOnSegment => SegmentFactor >= 0f && SegmentFactor <= 1f;

        /// <summary>
        /// Berechnet die Distanz zwischen der Projektion und einem gegebenen Punkt.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float DistanceTo(Vector2 point) => Vector2.Distance(point, ProjectedPoint);
    }
}

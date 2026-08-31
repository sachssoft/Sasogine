using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Geometry
{
    /// <summary>
    /// Represents the result of projecting a point onto a line segment.
    /// </summary>
    public readonly struct SegmentProjectionResult
    {
        /// <summary>
        /// Gets the normalized position along the segment,
        /// where 0 represents the start and 1 represents the end.
        /// </summary>
        public readonly float SegmentFactor;

        /// <summary>
        /// Gets the projected point on the segment or line.
        /// </summary>
        public readonly Vector2 ProjectedPoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="SegmentProjectionResult"/> struct.
        /// </summary>
        /// <param name="t">Normalized or unbounded segment factor.</param>
        /// <param name="tpos">Projected position.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SegmentProjectionResult(float t, Vector2 tpos)
        {
            SegmentFactor = t;
            ProjectedPoint = tpos;
        }

        /// <summary>
        /// Gets whether the projection lies within the segment range.
        /// </summary>
        public bool IsOnSegment =>
            SegmentFactor >= 0f &&
            SegmentFactor <= 1f;

        /// <summary>
        /// Calculates the distance between the projected point
        /// and the specified point.
        /// </summary>
        /// <param name="point">Point used for the distance calculation.</param>
        /// <returns>The distance to the projected point.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float DistanceTo(Vector2 point)
        {
            return Vector2.Distance(point, ProjectedPoint);
        }
    }
}
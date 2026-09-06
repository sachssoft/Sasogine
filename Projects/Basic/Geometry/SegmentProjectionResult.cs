using Sachssoft.Sasogine.Common;
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
        public readonly Point2 ProjectedPoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="SegmentProjectionResult"/> struct.
        /// </summary>
        /// <param name="segmentFactor">
        /// The normalized or unbounded position along the segment.
        /// </param>
        /// <param name="projectedPoint">
        /// The projected point on the segment or line.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SegmentProjectionResult(
            float segmentFactor,
            Point2 projectedPoint)
        {
            SegmentFactor = segmentFactor;
            ProjectedPoint = projectedPoint;
        }

        /// <summary>
        /// Gets a value indicating whether the projection lies within
        /// the segment range.
        /// </summary>
        public bool IsOnSegment =>
            SegmentFactor >= 0f &&
            SegmentFactor <= 1f;

        /// <summary>
        /// Calculates the distance between the projected point
        /// and the specified point.
        /// </summary>
        /// <param name="point">
        /// The point used for the distance calculation.
        /// </param>
        /// <returns>The distance to the projected point.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float DistanceTo(Point2 point)
        {
            return Point2.Distance(
                point,
                ProjectedPoint);
        }
    }
}
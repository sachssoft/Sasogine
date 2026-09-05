using System;

namespace Sachssoft.Sasogine.Geometry
{
    /// <summary>
    /// Defines options used when generating an offset from polygonal geometry.
    /// </summary>
    public sealed class PolygonOffsetOptions
    {
        /// <summary>
        /// Gets the distance by which the geometry is offset.
        /// </summary>
        /// <remarks>
        /// Positive values typically expand the geometry, while negative values
        /// typically contract it.
        /// </remarks>
        public float Delta { get; }

        /// <summary>
        /// Gets the maximum allowed miter ratio for miter joins.
        /// </summary>
        public float MiterLimit { get; }

        /// <summary>
        /// Gets the decimal precision used during offset calculations.
        /// </summary>
        public int Precision { get; }

        /// <summary>
        /// Gets the tolerance used when approximating rounded joins and ends.
        /// </summary>
        public float ArcTolerance { get; }

        /// <summary>
        /// Gets the join type used at polygon corners.
        /// </summary>
        public PolygonOffsetJoinType JoinType { get; }

        /// <summary>
        /// Gets the end type used when offsetting the geometry.
        /// </summary>
        public PolygonOffsetEndType EndType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolygonOffsetOptions"/> class.
        /// </summary>
        /// <param name="delta">
        /// The distance by which the geometry is offset.
        /// </param>
        /// <param name="miterLimit">
        /// The maximum allowed miter ratio for miter joins.
        /// </param>
        /// <param name="precision">
        /// The decimal precision used during offset calculations.
        /// Must be between <c>-8</c> and <c>8</c>.
        /// </param>
        /// <param name="arcTolerance">
        /// The tolerance used when approximating rounded joins and ends.
        /// </param>
        /// <param name="joinType">
        /// The join type used at polygon corners.
        /// </param>
        /// <param name="endType">
        /// The end type used when offsetting the geometry.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="miterLimit"/> or
        /// <paramref name="arcTolerance"/> is negative, or when
        /// <paramref name="precision"/> is outside the supported range.
        /// </exception>
        public PolygonOffsetOptions(
            float delta = 0f,
            float miterLimit = 2f,
            int precision = 2,
            float arcTolerance = 0f,
            PolygonOffsetJoinType joinType = PolygonOffsetJoinType.Miter,
            PolygonOffsetEndType endType = PolygonOffsetEndType.Polygon)
        {
            if (miterLimit < 0f)
                throw new ArgumentOutOfRangeException(nameof(miterLimit));

            if (precision < -8 || precision > 8)
                throw new ArgumentOutOfRangeException(nameof(precision));

            if (arcTolerance < 0f)
                throw new ArgumentOutOfRangeException(nameof(arcTolerance));

            Delta = delta;
            MiterLimit = miterLimit;
            Precision = precision;
            ArcTolerance = arcTolerance;
            JoinType = joinType;
            EndType = endType;
        }
    }
}
using System;

namespace Sachssoft.Sasogine.Geometry
{
    /// <summary>
    /// Defines options used when generating stroked polygon geometry.
    /// </summary>
    public sealed class PolygonStrokeOptions
    {
        /// <summary>
        /// Gets the total thickness of the generated stroke.
        /// </summary>
        public float Thickness { get; }

        /// <summary>
        /// Gets the maximum allowed miter ratio for miter joins.
        /// </summary>
        public float MiterLimit { get; }

        /// <summary>
        /// Gets the decimal precision used during stroke calculations.
        /// </summary>
        public int Precision { get; }

        /// <summary>
        /// Gets the tolerance used when approximating rounded joins and caps.
        /// </summary>
        public float ArcTolerance { get; }

        /// <summary>
        /// Gets the join type used where adjacent stroke segments meet.
        /// </summary>
        public PolygonOffsetJoinType JoinType { get; }

        /// <summary>
        /// Gets the cap type used at the ends of open strokes.
        /// </summary>
        public PolygonStrokeCapType CapType { get; }

        /// <summary>
        /// Gets the end type that determines whether the stroked path
        /// is treated as open or closed.
        /// </summary>
        public PolygonStrokeEndType EndType { get; }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="PolygonStrokeOptions"/> class.
        /// </summary>
        /// <param name="thickness">
        /// The total thickness of the generated stroke.
        /// Must be greater than <c>0</c>.
        /// </param>
        /// <param name="miterLimit">
        /// The maximum allowed miter ratio for miter joins.
        /// </param>
        /// <param name="precision">
        /// The decimal precision used during stroke calculations.
        /// Must be between <c>-8</c> and <c>8</c>.
        /// </param>
        /// <param name="arcTolerance">
        /// The tolerance used when approximating rounded joins and caps.
        /// </param>
        /// <param name="joinType">
        /// The join type used where adjacent stroke segments meet.
        /// </param>
        /// <param name="capType">
        /// The cap type used at the ends of open strokes.
        /// </param>
        /// <param name="endType">
        /// Determines whether the stroked path is treated as open or closed.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="thickness"/> is not greater than zero,
        /// when <paramref name="miterLimit"/> or <paramref name="arcTolerance"/>
        /// is negative, or when <paramref name="precision"/> is outside the
        /// supported range.
        /// </exception>
        public PolygonStrokeOptions(
            float thickness,
            float miterLimit = 2f,
            int precision = 2,
            float arcTolerance = 0f,
            PolygonOffsetJoinType joinType = PolygonOffsetJoinType.Miter,
            PolygonStrokeCapType capType = PolygonStrokeCapType.Butt,
            PolygonStrokeEndType endType = PolygonStrokeEndType.Open)
        {
            if (thickness <= 0f)
                throw new ArgumentOutOfRangeException(
                    nameof(thickness));

            if (miterLimit < 0f)
                throw new ArgumentOutOfRangeException(
                    nameof(miterLimit));

            if (precision < -8 || precision > 8)
                throw new ArgumentOutOfRangeException(
                    nameof(precision));

            if (arcTolerance < 0f)
                throw new ArgumentOutOfRangeException(
                    nameof(arcTolerance));

            Thickness = thickness;
            MiterLimit = miterLimit;
            Precision = precision;
            ArcTolerance = arcTolerance;
            JoinType = joinType;
            CapType = capType;
            EndType = endType;
        }
    }
}
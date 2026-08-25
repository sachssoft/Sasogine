using System;

namespace Sachssoft.Sasogine.Geometry
{
    public sealed class PolygonOffsetOptions
    {
        public float Delta { get; }

        public float MiterLimit { get; }

        public int Precision { get; }

        public float ArcTolerance { get; }

        public PolygonOffsetJoinType JoinType { get; }

        public PolygonOffsetEndType EndType { get; }

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
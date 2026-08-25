using System;

namespace Sachssoft.Sasogine.Geometry
{
    public sealed class PolygonStrokeOptions
    {
        public float Thickness { get; }

        public float MiterLimit { get; }

        public int Precision { get; }

        public float ArcTolerance { get; }

        public PolygonOffsetJoinType JoinType { get; }

        public PolygonStrokeCapType CapType { get; }

        public PolygonStrokeEndType EndType { get; }

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

namespace Sachssoft.Sasogine.Geometry
{
    public enum PolygonStrokeCapType
    {
        Butt,
        Square,
        Round
    }
}

namespace Sachssoft.Sasogine.Geometry
{
    public enum PolygonStrokeEndType
    {
        Open,
        Closed
    }
}
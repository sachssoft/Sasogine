using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry.Shapes
{
    public class RectanglePath : ShapePathBase
    {
        public int Segments { get; init; } = 8;

        public RoundingType TopLeftEdgeType { get; init; } = RoundingType.Linear;
        public RoundingType TopRightEdgeType { get; init; } = RoundingType.Linear;
        public RoundingType BottomLeftEdgeType { get; init; } = RoundingType.Linear;
        public RoundingType BottomRightEdgeType { get; init; } = RoundingType.Linear;

        private Vector2 _topLeftEdgeSize = Vector2.Zero;
        public Vector2 TopLeftEdgeSize
        {
            get => _topLeftEdgeSize;
            init => _topLeftEdgeSize = CoerceSize(value);
        }

        private Vector2 _topRightEdgeSize = Vector2.Zero;
        public Vector2 TopRightEdgeSize
        {
            get => _topRightEdgeSize;
            init => _topRightEdgeSize = CoerceSize(value);
        }

        private Vector2 _bottomLeftEdgeSize = Vector2.Zero;
        public Vector2 BottomLeftEdgeSize
        {
            get => _bottomLeftEdgeSize;
            init => _bottomLeftEdgeSize = CoerceSize(value);
        }

        private Vector2 _bottomRightEdgeSize = Vector2.Zero;
        public Vector2 BottomRightEdgeSize
        {
            get => _bottomRightEdgeSize;
            init => _bottomRightEdgeSize = CoerceSize(value);
        }

        /// <summary>
        /// Ensure the corner size is within [0,1].
        /// Later, this can also clamp based on rectangle size.
        /// </summary>
        private static Vector2 CoerceSize(Vector2 size)
        {
            return new Vector2(
                MathHelper.Clamp(size.X, 0f, 1f),
                MathHelper.Clamp(size.Y, 0f, 1f)
            );
        }

        protected override Path BuildDefinedPath()
        {
            var polygon = new List<Vector2>();

            // Eckpunkte normiert
            Vector2 tl = new(0f, 0f);
            Vector2 tr = new(1f, 0f);
            Vector2 br = new(1f, 1f);
            Vector2 bl = new(0f, 1f);

            // Hilfsmethode: Ecke abrunden
            Vector2[] RoundCorner(Vector2 corner, Vector2 next, Vector2 prev, Vector2 size, RoundingType type)
            {
                if (size == Vector2.Zero)
                    return new Vector2[] { corner };

                Vector2 dirPrev = (corner - prev).SafeNormalize();
                Vector2 dirNext = (next - corner).SafeNormalize();

                Vector2 start = corner - dirPrev * size.Length();
                Vector2 end = corner + dirNext * size.Length();

                return type switch
                {
                    RoundingType.Linear => new Vector2[] { start, end }, // Bevel
                    RoundingType.Quadratic => GeometrySampler.SampleQuadraticBezier(start, corner, end, Segments),
                    RoundingType.Cubic => GeometrySampler.SampleCubicBezier(start, start + dirPrev * size.Length() * 0.5f, end - dirNext * size.Length() * 0.5f, end, Segments),
                    _ => new Vector2[] { corner }
                };
            }


            // Ecken nacheinander
            polygon.AddRange(RoundCorner(tl, tr, bl, TopLeftEdgeSize, TopLeftEdgeType));
            polygon.AddRange(RoundCorner(tr, br, tl, TopRightEdgeSize, TopRightEdgeType));
            polygon.AddRange(RoundCorner(br, bl, tr, BottomRightEdgeSize, BottomRightEdgeType));
            polygon.AddRange(RoundCorner(bl, tl, br, BottomLeftEdgeSize, BottomLeftEdgeType));

            // Polygon schließen
            if (polygon.Count > 0 && polygon[0] != polygon[^1])
                polygon.Add(polygon[0]);

            return new Path(new[] { polygon.ToArray() });
        }
    }

    public static class VectorExtensions
    {
        public static Vector2 SafeNormalize(this Vector2 v)
        {
            float len = v.Length();
            return len < 1e-8f ? Vector2.Zero : v / len;
        }
    }
}

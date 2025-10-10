using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Geometry.Shapes
{
    public class StarPath : ShapePathBase
    {
        public int Points { get; init; } = 5;
        public float OuterRadius { get; init; } = 50f;
        public float InnerRadius { get; init; } = 25f;
        public float Rotation { get; init; } = 0f;

        public float OuterSmoothRadius { get; init; } = 0f;
        public float InnerSmoothRadius { get; init; } = 0f;
        public int SmoothSegments { get; init; } = 4;

        protected override Path BuildDefinedPath()
        {
            var raw = new List<Vector2>();
            int count = Math.Max(2, Points);
            float step = MathHelper.Pi / count;

            for (int i = 0; i < count * 2; i++)
            {
                float angle = Rotation + i * step;
                float radius = (i % 2 == 0) ? OuterRadius : InnerRadius;
                raw.Add(new Vector2(
                    (float)Math.Cos(angle) * radius,
                    (float)Math.Sin(angle) * radius
                ));
            }

            if (OuterSmoothRadius <= 0f && InnerSmoothRadius <= 0f)
                return new Path(new List<Vector2[]> { raw.ToArray() });

            var smooth = new List<Vector2>();
            int total = raw.Count;

            for (int i = 0; i < total; i++)
            {
                Vector2 prev = raw[(i - 1 + total) % total];
                Vector2 corner = raw[i];
                Vector2 next = raw[(i + 1) % total];

                bool isOuter = (i % 2 == 0);
                float r = isOuter ? OuterSmoothRadius : InnerSmoothRadius;

                AddRoundedCorner(smooth, prev, corner, next, r, SmoothSegments);
            }

            return new Path(new List<Vector2[]> { smooth.ToArray() });
        }

        private void AddRoundedCorner(List<Vector2> output, Vector2 prev, Vector2 corner, Vector2 next, float radius, int segments)
        {
            if (radius <= 0f)
            {
                output.Add(corner);
                return;
            }

            Vector2 v1 = Vector2.Normalize(prev - corner);
            Vector2 v2 = Vector2.Normalize(next - corner);

            float angle1 = (float)Math.Atan2(v1.Y, v1.X);
            float angle2 = (float)Math.Atan2(v2.Y, v2.X);

            // Richtung korrigieren (von v1 nach v2)
            float diff = angle2 - angle1;
            if (diff <= -MathHelper.Pi) diff += MathHelper.TwoPi;
            else if (diff > MathHelper.Pi) diff -= MathHelper.TwoPi;

            // Tangentenpunkte
            Vector2 p1 = corner + v1 * radius;
            Vector2 p2 = corner + v2 * radius;

            // Bogenpunkte
            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                float angle = angle1 + diff * t;
                var p = corner + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                output.Add(p);
            }
        }
    }
}

using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Geometry;
using System;

namespace Sachssoft.Sasogine.Example.Primitives
{
    public static class PolygonHelper
    {
        public static Path RegularPolygon(Vector2 center, float radius, int sides, float rotation = 0f)
        {
            if (sides < 3)
                throw new ArgumentException("Polygon muss mindestens 3 Seiten haben.");

            Vector2[] points = new Vector2[sides];
            float angleStep = MathHelper.TwoPi / sides;
            float rotRad = MathHelper.ToRadians(rotation);

            for (int i = 0; i < sides; i++)
            {
                float angle = i * angleStep + rotRad;
                float x = center.X + radius * MathF.Cos(angle);
                float y = center.Y + radius * MathF.Sin(angle);
                points[i] = new Vector2(x, y);
            }

            return new Path(points);
        }
    }
}

using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Sasogine.Geometry.Shapes
{
    public class EllipsePath : ShapePathBase
    {
        /// <summary>
        /// Anzahl der Segmente, die für den vollständigen Ellipsenbogen verwendet werden.
        /// Je höher, desto glatter.
        /// </summary>
        public int Segments { get; init; } = 64;

        protected override Path BuildDefinedPath()
        {
            var builder = new PathBuilder();

            const float cx = 0.5f;
            const float cy = 0.5f;
            const float rx = 0.5f;
            const float ry = 0.5f;

            builder.Start(new Vector2(cx + rx, cy));

            for (int i = 1; i <= Segments; i++)
            {
                float t = i / (float)Segments;
                float angle = t * MathHelper.TwoPi;

                float x = cx + rx * MathF.Cos(angle);
                float y = cy + ry * MathF.Sin(angle);

                builder.AddLine(x, y);
            }

            builder.Close();
            return builder.ToPath();
        }

    }
}

using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Enums;
using System;

namespace Sachssoft.Sasogine.Geometry.Shapes
{
    public class RectanglePath : ShapePathBase
    {
        public int Segments { get; init; } = 8;

        public EdgeType TopLeftEdgeType { get; init; } = EdgeType.None;
        public EdgeType TopRightEdgeType { get; init; } = EdgeType.None;
        public EdgeType BottomLeftEdgeType { get; init; } = EdgeType.None;
        public EdgeType BottomRightEdgeType { get; init; } = EdgeType.None;

        public Vector2 TopLeftEdgeSize { get; init; }
        public Vector2 TopRightEdgeSize { get; init; }
        public Vector2 BottomLeftEdgeSize { get; init; }
        public Vector2 BottomRightEdgeSize { get; init; }

        protected override Path BuildDefinedPath()
        {
            var builder = new PathBuilder();

            // Normierte Eckpunkte
            Vector2 tl = new(0f, 0f); // Top-Left
            Vector2 tr = new(1f, 0f); // Top-Right
            Vector2 br = new(1f, 1f); // Bottom-Right
            Vector2 bl = new(0f, 1f); // Bottom-Left

            // Hilfsfunktion: Ecke hinzufügen
            void AddCorner(Vector2 corner, EdgeType type, Vector2 offset, Vector2 next)
            {
                switch (type)
                {
                    case EdgeType.None:
                        builder.AddLine(next.X, next.Y);
                        break;

                    case EdgeType.Below:
                        // „Abgesenkte“ Ecke: Linie zum Offset-Punkt, dann zum nächsten Eckpunkt
                        var lowered = corner + offset;
                        builder.AddLine(lowered.X, lowered.Y);
                        builder.AddLine(next.X, next.Y);
                        break;

                    case EdgeType.Rounded:
                        // Abgerundete Ecke: Quadratic Bezier
                        var control = corner + offset;
                        builder.AddQuadraticBezier(control, next, Segments);
                        break;
                }
            }

            // Startpunkt oben links
            builder.Start(tl);

            // Ecken nacheinander
            AddCorner(tl, TopLeftEdgeType, TopLeftEdgeSize, tr);
            AddCorner(tr, TopRightEdgeType, TopRightEdgeSize, br);
            AddCorner(br, BottomRightEdgeType, BottomRightEdgeSize, bl);
            AddCorner(bl, BottomLeftEdgeType, BottomLeftEdgeSize, tl);

            // Pfad schließen
            builder.Close();

            return builder.ToPath();
        }
    }
}

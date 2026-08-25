using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry
{
    public sealed class PolygonTriangulationResult
    {
        public IReadOnlyList<Vector2> Vertices { get; }

        public IReadOnlyList<int> Indices { get; }

        public PolygonTriangulationResult(
            IReadOnlyList<Vector2> vertices,
            IReadOnlyList<int> indices)
        {
            Vertices = vertices;
            Indices = indices;
        }
    }
}
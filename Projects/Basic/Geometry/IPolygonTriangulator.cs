using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry
{
    public interface IPolygonTriangulator
    {
        PolygonTriangulationResult Triangulate(
            IReadOnlyList<IReadOnlyList<Vector2>> contours,
            PolygonTriangulationOptions options);
    }
}

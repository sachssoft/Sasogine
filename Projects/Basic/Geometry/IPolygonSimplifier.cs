using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry
{
    public interface IPolygonSimplifier
    {
        IReadOnlyList<IReadOnlyList<Vector2>> Simplify(
            IReadOnlyList<IReadOnlyList<Vector2>> contours,
            PolygonSimplificationOptions options);
    }
}
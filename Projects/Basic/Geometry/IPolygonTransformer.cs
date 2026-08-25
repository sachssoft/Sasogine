using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry
{
    public interface IPolygonTransformer
    {
        IReadOnlyList<IReadOnlyList<Vector2>> Transform(
            IReadOnlyList<IReadOnlyList<Vector2>> contours,
            Matrix transform);
    }
}
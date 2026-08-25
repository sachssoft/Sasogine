using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry
{
    public interface IPolygonClipper
    {
        IReadOnlyList<IReadOnlyList<Vector2>> Clip(
            IReadOnlyList<IReadOnlyList<Vector2>> subject,
            IReadOnlyList<IReadOnlyList<Vector2>> clip,
            PolygonClipOperation operation);
    }
}

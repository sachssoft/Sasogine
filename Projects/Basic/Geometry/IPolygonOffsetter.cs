using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry
{
    public interface IPolygonOffsetter
    {
        IReadOnlyList<IReadOnlyList<Vector2>> Offset(
            IReadOnlyList<IReadOnlyList<Vector2>> contours,
            PolygonOffsetOptions options);
    }
}
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Geometry
{
    public interface IPolygonStroker
    {
        IReadOnlyList<IReadOnlyList<Vector2>> Stroke(
            IReadOnlyList<IReadOnlyList<Vector2>> contours,
            PolygonStrokeOptions options);
    }
}
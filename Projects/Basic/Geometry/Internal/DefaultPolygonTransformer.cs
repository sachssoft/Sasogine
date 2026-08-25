using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry.Internal
{
    internal class DefaultPolygonTransformer : IPolygonTransformer
    {
        public IReadOnlyList<IReadOnlyList<Vector2>> Transform(
            IReadOnlyList<IReadOnlyList<Vector2>> contours,
            Matrix transform)
        {
            var result = new List<IReadOnlyList<Vector2>>(contours.Count);

            foreach (var contour in contours)
            {
                var transformed = new Vector2[contour.Count];

                for (var i = 0; i < contour.Count; i++)
                    transformed[i] = Vector2.Transform(contour[i], transform);

                result.Add(transformed);
            }

            return result;
        }
    }
}
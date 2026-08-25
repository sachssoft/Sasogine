using Clipper2Lib;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry.Internal
{
    internal class Clipper2PolygonSimplifier : IPolygonSimplifier
    {
        public IReadOnlyList<IReadOnlyList<Vector2>> Simplify(
            IReadOnlyList<IReadOnlyList<Vector2>> contours,
            PolygonSimplificationOptions options)
        {
            var paths = ToPaths(contours);

            var result = Clipper.SimplifyPaths(
                paths,
                options.Tolerance);

            return ToContours(result);
        }

        private static PathsD ToPaths(
            IReadOnlyList<IReadOnlyList<Vector2>> contours)
        {
            var paths = new PathsD(contours.Count);

            foreach (var contour in contours)
            {
                if (contour.Count < 3)
                    continue;

                var path = new PathD(contour.Count);

                foreach (var position in contour)
                {
                    path.Add(new PointD(
                        position.X,
                        position.Y));
                }

                paths.Add(path);
            }

            return paths;
        }

        private static IReadOnlyList<IReadOnlyList<Vector2>> ToContours(
            PathsD paths)
        {
            var contours = new List<IReadOnlyList<Vector2>>(paths.Count);

            foreach (var path in paths)
            {
                if (path.Count < 3)
                    continue;

                var contour = new Vector2[path.Count];

                for (var i = 0; i < path.Count; i++)
                {
                    contour[i] = new Vector2(
                        (float)path[i].x,
                        (float)path[i].y);
                }

                contours.Add(contour);
            }

            return contours;
        }
    }
}
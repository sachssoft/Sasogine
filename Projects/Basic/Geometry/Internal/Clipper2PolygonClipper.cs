using Clipper2Lib;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry.Internal
{
    internal class Clipper2PolygonClipper : IPolygonClipper
    {
        public IReadOnlyList<IReadOnlyList<Vector2>> Clip(
            IReadOnlyList<IReadOnlyList<Vector2>> subject,
            IReadOnlyList<IReadOnlyList<Vector2>> clip,
            PolygonClipOperation operation)
        {
            var subjectPaths = ToPaths(subject);
            var clipPaths = ToPaths(clip);

            var clipType = operation switch
            {
                PolygonClipOperation.Intersection => ClipType.Intersection,
                PolygonClipOperation.Union => ClipType.Union,
                PolygonClipOperation.Difference => ClipType.Difference,
                PolygonClipOperation.Xor => ClipType.Xor,
                _ => throw new ArgumentOutOfRangeException(nameof(operation))
            };

            var result = Clipper.BooleanOp(
                clipType,
                subjectPaths,
                clipPaths,
                FillRule.EvenOdd);

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
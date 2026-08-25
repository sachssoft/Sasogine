using Clipper2Lib;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry.Internal
{
    internal sealed class Clipper2PolygonOffsetter : IPolygonOffsetter
    {
        public IReadOnlyList<IReadOnlyList<Vector2>> Offset(
            IReadOnlyList<IReadOnlyList<Vector2>> contours,
            PolygonOffsetOptions options)
        {
            var paths =
                ToPaths(
                    contours,
                    options.EndType);

            if (paths.Count == 0)
                return Array.Empty<IReadOnlyList<Vector2>>();

            var result =
                Clipper.InflatePaths(
                    paths,
                    options.Delta,
                    ToJoinType(options.JoinType),
                    ToEndType(options.EndType),
                    options.MiterLimit,
                    options.Precision,
                    options.ArcTolerance);

            return ToContours(result);
        }

        private static PathsD ToPaths(
            IReadOnlyList<IReadOnlyList<Vector2>> contours,
            PolygonOffsetEndType endType)
        {
            bool closed =
                endType == PolygonOffsetEndType.Polygon;

            int minimumPointCount =
                closed ? 3 : 2;

            var paths =
                new PathsD(
                    contours.Count);

            foreach (var contour in contours)
            {
                if (contour is null ||
                    contour.Count < minimumPointCount)
                {
                    continue;
                }

                var path =
                    new PathD(
                        contour.Count);

                foreach (var position in contour)
                {
                    path.Add(
                        new PointD(
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
            var contours =
                new List<IReadOnlyList<Vector2>>(
                    paths.Count);

            foreach (var path in paths)
            {
                if (path.Count < 3)
                    continue;

                var contour =
                    new Vector2[path.Count];

                for (int i = 0;
                     i < path.Count;
                     i++)
                {
                    contour[i] =
                        new Vector2(
                            (float)path[i].x,
                            (float)path[i].y);
                }

                contours.Add(contour);
            }

            return contours;
        }

        private static JoinType ToJoinType(
            PolygonOffsetJoinType joinType)
        {
            return joinType switch
            {
                PolygonOffsetJoinType.Square =>
                    JoinType.Square,

                PolygonOffsetJoinType.Bevel =>
                    JoinType.Bevel,

                PolygonOffsetJoinType.Round =>
                    JoinType.Round,

                PolygonOffsetJoinType.Miter =>
                    JoinType.Miter,

                _ =>
                    throw new ArgumentOutOfRangeException(
                        nameof(joinType))
            };
        }

        private static EndType ToEndType(
            PolygonOffsetEndType endType)
        {
            return endType switch
            {
                PolygonOffsetEndType.Polygon =>
                    EndType.Polygon,

                PolygonOffsetEndType.Joined =>
                    EndType.Joined,

                PolygonOffsetEndType.Butt =>
                    EndType.Butt,

                PolygonOffsetEndType.Square =>
                    EndType.Square,

                PolygonOffsetEndType.Round =>
                    EndType.Round,

                _ =>
                    throw new ArgumentOutOfRangeException(
                        nameof(endType))
            };
        }
    }
}
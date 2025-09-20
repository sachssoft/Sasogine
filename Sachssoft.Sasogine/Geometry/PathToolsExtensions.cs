using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry
{
    public static class PathToolsExtensions
    {
        /// <summary>
        /// Glättet alle Polygone in der PathCollection mit maximalem Winkel.
        /// </summary>
        public static PathCollection Smooth(this PathCollection collection, float smoothFactor = 0.5f, float maxAngleDeg = 170f, int segments = 3)
        {
            var resultPaths = new List<Path>();
            foreach (var path in collection)
            {
                var smoothed = PathTools.SmoothPath(path.GetPolygon(0) as List<Vector2>, smoothFactor, maxAngleDeg, segments);
                resultPaths.Add(new Path(smoothed.ToArray()));
            }
            return new PathCollection(resultPaths);
        }

        /// <summary>
        /// Rundet alle Ecken, die spitz genug sind.
        /// </summary>
        public static PathCollection RoundCorners(this PathCollection collection, float radius, float maxAngleDeg = 150f, int segments = 6)
        {
            var resultPaths = new List<Path>();
            foreach (var path in collection)
            {
                var rounded = PathTools.RoundCornersAuto(path.GetPolygon(0) as List<Vector2>, radius, maxAngleDeg, segments);
                resultPaths.Add(new Path(rounded.ToArray()));
            }
            return new PathCollection(resultPaths);
        }

        /// <summary>
        /// Resample das erste Polygon auf eine feste Anzahl Punkte
        /// </summary>
        public static PathCollection Resample(this PathCollection collection, int startIndex, int endIndex, int newPointCount)
        {
            var resultPaths = new List<Path>();
            foreach (var path in collection)
            {
                var resampled = PathTools.ResampleLinear(path.GetPolygon(0) as List<Vector2>, startIndex, endIndex, newPointCount);
                resultPaths.Add(new Path(resampled.ToArray()));
            }
            return new PathCollection(resultPaths);
        }

        /// <summary>
        /// Vereinfachung der Punkte mit Douglas-Peucker
        /// </summary>
        public static PathCollection Simplify(this PathCollection collection, float tolerance)
        {
            var resultPaths = new List<Path>();
            foreach (var path in collection)
            {
                var simplified = PathTools.SimplifyDouglasPeucker(path.GetPolygon(0) as List<Vector2>, tolerance);
                resultPaths.Add(new Path(simplified.ToArray()));
            }
            return new PathCollection(resultPaths);
        }

        /// <summary>
        /// Konvertiert alle Pfade in eine Liste von Vector2 für Rendering oder Editor.
        /// </summary>
        public static List<Vector2> Flatten(this PathCollection collection)
        {
            var allPoints = new List<Vector2>();
            foreach (var path in collection)
            {
                for (int i = 0; i < path.GetPolygonCount(); i++)
                    allPoints.AddRange(path.GetPolygon(i));
            }
            return allPoints;
        }
    }
}

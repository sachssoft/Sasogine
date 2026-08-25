using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry
{
    /// <summary>
    /// Erweiterungsmethoden für die Bearbeitung und Optimierung von PathCollection-Objekten.
    /// </summary>
    public static class PathToolsExtensions
    {
        /// <summary>
        /// Glättet alle Polygone der PathCollection mit dem angegebenen Glättungsfaktor
        /// und maximal zulässigen Winkel.
        /// </summary>
        /// <param name="collection">Die zu glättende PathCollection.</param>
        /// <param name="smoothFactor">Der Glättungsfaktor.</param>
        /// <param name="maxAngleDeg">Der maximale Winkel in Grad, bis zu dem Ecken geglättet werden.</param>
        /// <param name="segments">Die Anzahl der Segmente für erzeugte Kurven.</param>
        /// <returns>Eine neue PathCollection mit den geglätteten Polygonen.</returns>
        public static PathCollection Smooth(this PathCollection collection, float smoothFactor = 0.5f, float maxAngleDeg = 170f, int segments = 3)
        {
            var resultPaths = new List<Path>(collection.Count);

            foreach (var path in collection)
            {
                var polygons = new List<Vector2[]>(path.GetPolygonCount());

                for (int i = 0; i < path.GetPolygonCount(); i++)
                {
                    var points = path.GetPolygonPoints(i);
                    var input = new List<Vector2>(points.Count);

                    for (int j = 0; j < points.Count; j++)
                        input.Add(points[j]);

                    var smoothed = PathTools.SmoothPath(input, smoothFactor, maxAngleDeg, segments);
                    polygons.Add(smoothed.ToArray());
                }

                resultPaths.Add(new Path(polygons));
            }

            return new PathCollection(resultPaths);
        }

        /// <summary>
        /// Rundet alle Ecken aller Polygone der PathCollection,
        /// deren Winkel kleiner als der angegebene maximale Winkel ist.
        /// </summary>
        /// <param name="collection">Die zu bearbeitende PathCollection.</param>
        /// <param name="radius">Der Radius der abgerundeten Ecken.</param>
        /// <param name="maxAngleDeg">Der maximale Winkel in Grad für eine Rundung.</param>
        /// <param name="segments">Die Anzahl der Segmente für den Rundungsbogen.</param>
        /// <returns>Eine neue PathCollection mit abgerundeten Ecken.</returns>
        public static PathCollection RoundCorners(this PathCollection collection, float radius, float maxAngleDeg = 150f, int segments = 6)
        {
            var resultPaths = new List<Path>(collection.Count);

            foreach (var path in collection)
            {
                var polygons = new List<Vector2[]>(path.GetPolygonCount());

                for (int i = 0; i < path.GetPolygonCount(); i++)
                {
                    var points = path.GetPolygonPoints(i);
                    var input = new List<Vector2>(points.Count);

                    for (int j = 0; j < points.Count; j++)
                        input.Add(points[j]);

                    var rounded = PathTools.RoundCornersAuto(input, radius, maxAngleDeg, segments);
                    polygons.Add(rounded.ToArray());
                }

                resultPaths.Add(new Path(polygons));
            }

            return new PathCollection(resultPaths);
        }

        /// <summary>
        /// Resampelt die angegebenen Bereiche aller Polygone der PathCollection
        /// auf eine feste Anzahl zusätzlicher Punkte.
        /// </summary>
        /// <param name="collection">Die zu resampelnde PathCollection.</param>
        /// <param name="startIndex">Der Startindex des Bereichs.</param>
        /// <param name="endIndex">Der Endindex des Bereichs.</param>
        /// <param name="newPointCount">Die Anzahl der zusätzlichen Punkte.</param>
        /// <returns>Eine neue PathCollection mit resampelten Polygonen.</returns>
        public static PathCollection Resample(this PathCollection collection, int startIndex, int endIndex, int newPointCount)
        {
            var resultPaths = new List<Path>(collection.Count);

            foreach (var path in collection)
            {
                var polygons = new List<Vector2[]>(path.GetPolygonCount());

                for (int i = 0; i < path.GetPolygonCount(); i++)
                {
                    var points = path.GetPolygonPoints(i);
                    var input = new List<Vector2>(points.Count);

                    for (int j = 0; j < points.Count; j++)
                        input.Add(points[j]);

                    var resampled = PathTools.ResampleLinear(input, startIndex, endIndex, newPointCount);
                    polygons.Add(resampled.ToArray());
                }

                resultPaths.Add(new Path(polygons));
            }

            return new PathCollection(resultPaths);
        }

        /// <summary>
        /// Vereinfacht alle Polygone der PathCollection mit dem
        /// Douglas-Peucker-Verfahren.
        /// </summary>
        /// <param name="collection">Die zu vereinfachende PathCollection.</param>
        /// <param name="tolerance">Die maximale Abweichung für die Vereinfachung.</param>
        /// <returns>Eine neue PathCollection mit vereinfachten Polygonen.</returns>
        public static PathCollection Simplify(this PathCollection collection, float tolerance)
        {
            var resultPaths = new List<Path>(collection.Count);

            foreach (var path in collection)
            {
                var polygons = new List<Vector2[]>(path.GetPolygonCount());

                for (int i = 0; i < path.GetPolygonCount(); i++)
                {
                    var points = path.GetPolygonPoints(i);
                    var input = new List<Vector2>(points.Count);

                    for (int j = 0; j < points.Count; j++)
                        input.Add(points[j]);

                    var simplified = PathTools.SimplifyDouglasPeucker(input, tolerance);
                    polygons.Add(simplified.ToArray());
                }

                resultPaths.Add(new Path(polygons));
            }

            return new PathCollection(resultPaths);
        }

        /// <summary>
        /// Konvertiert alle Punkte aller Polygone der PathCollection
        /// in eine gemeinsame Punktliste.
        /// </summary>
        /// <param name="collection">Die PathCollection.</param>
        /// <returns>Eine Liste mit allen Polygonpunkten.</returns>
        public static List<Vector2> Flatten(this PathCollection collection)
        {
            var count = 0;

            foreach (var path in collection)
            {
                for (int i = 0; i < path.GetPolygonCount(); i++)
                    count += path.GetPointCount(i);
            }

            var allPoints = new List<Vector2>(count);

            foreach (var path in collection)
            {
                for (int i = 0; i < path.GetPolygonCount(); i++)
                {
                    var points = path.GetPolygonPoints(i);

                    for (int j = 0; j < points.Count; j++)
                        allPoints.Add(points[j]);
                }
            }

            return allPoints;
        }

        /// <summary>
        /// Optimiert alle Polygone der PathCollection für die weitere Verarbeitung
        /// oder Darstellung.
        /// </summary>
        /// <param name="collection">Die zu optimierende PathCollection.</param>
        /// <param name="simplifyTolerance">Die Toleranz für die Douglas-Peucker-Vereinfachung.</param>
        /// <param name="targetPointCount">Optionale Anzahl zusätzlicher Punkte beim Resampling.</param>
        /// <param name="smoothFactor">Der Glättungsfaktor. Ein Wert von 0 deaktiviert die Glättung.</param>
        /// <param name="smoothMaxAngleDeg">Der maximale Winkel für die Glättung.</param>
        /// <param name="smoothIterations">Die Anzahl der Glättungsdurchläufe.</param>
        /// <returns>Eine neue optimierte PathCollection.</returns>
        public static PathCollection Optimize(
            this PathCollection collection,
            float simplifyTolerance = 0.5f,
            int? targetPointCount = null,
            float smoothFactor = 0.0f,
            float smoothMaxAngleDeg = 45f,
            int smoothIterations = 1)
        {
            var resultPaths = new List<Path>(collection.Count);

            foreach (var path in collection)
            {
                var polygons = new List<Vector2[]>(path.GetPolygonCount());

                for (int polygonIndex = 0; polygonIndex < path.GetPolygonCount(); polygonIndex++)
                {
                    var points = path.GetPolygonPoints(polygonIndex);
                    var input = new List<Vector2>(points.Count);

                    for (int i = 0; i < points.Count; i++)
                        input.Add(points[i]);

                    var optimized = PathTools.SimplifyDouglasPeucker(input, simplifyTolerance);

                    if (targetPointCount.HasValue && targetPointCount.Value > 2 && optimized.Count >= 2)
                    {
                        optimized = PathTools.ResampleLinear(
                            optimized,
                            0,
                            optimized.Count - 1,
                            targetPointCount.Value);
                    }

                    if (smoothFactor > 0f && smoothIterations > 0)
                    {
                        for (int iteration = 0; iteration < smoothIterations; iteration++)
                        {
                            optimized = PathTools.SmoothPath(
                                optimized,
                                smoothFactor,
                                smoothMaxAngleDeg,
                                2);
                        }
                    }

                    polygons.Add(optimized.ToArray());
                }

                resultPaths.Add(new Path(polygons));
            }

            return new PathCollection(resultPaths);
        }
    }
}
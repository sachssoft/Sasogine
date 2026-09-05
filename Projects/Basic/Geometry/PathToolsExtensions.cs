using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry;

/// <summary>
/// Provides extension methods for transforming, simplifying,
/// smoothing, resampling, and optimizing path collections.
/// </summary>
public static class PathToolsExtensions
{
    /// <summary>
    /// Smooths all polygons in the specified path collection.
    /// </summary>
    /// <param name="collection">
    /// The path collection to smooth.
    /// </param>
    /// <param name="smoothFactor">
    /// The smoothing factor applied to the polygon points.
    /// </param>
    /// <param name="maxAngleDeg">
    /// The maximum corner angle, in degrees, affected by smoothing.
    /// </param>
    /// <param name="segments">
    /// The number of segments used when generating smoothed curves.
    /// </param>
    /// <returns>
    /// A new <see cref="PathCollection"/> containing the smoothed polygons.
    /// </returns>
    public static PathCollection Smooth(
        this PathCollection collection,
        float smoothFactor = 0.5f,
        float maxAngleDeg = 170f,
        int segments = 3)
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

                var smoothed = PathTools.SmoothPath(
                    input,
                    smoothFactor,
                    maxAngleDeg,
                    segments);

                polygons.Add(smoothed.ToArray());
            }

            resultPaths.Add(new Path(polygons));
        }

        return new PathCollection(resultPaths);
    }

    /// <summary>
    /// Rounds eligible corners of all polygons in the specified path collection.
    /// </summary>
    /// <param name="collection">
    /// The path collection to process.
    /// </param>
    /// <param name="radius">
    /// The radius used for rounded corners.
    /// </param>
    /// <param name="maxAngleDeg">
    /// The maximum corner angle, in degrees, eligible for rounding.
    /// </param>
    /// <param name="segments">
    /// The number of segments used to generate each rounded corner.
    /// </param>
    /// <returns>
    /// A new <see cref="PathCollection"/> containing polygons with rounded corners.
    /// </returns>
    public static PathCollection RoundCorners(
        this PathCollection collection,
        float radius,
        float maxAngleDeg = 150f,
        int segments = 6)
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

                var rounded = PathTools.RoundCornersAuto(
                    input,
                    radius,
                    maxAngleDeg,
                    segments);

                polygons.Add(rounded.ToArray());
            }

            resultPaths.Add(new Path(polygons));
        }

        return new PathCollection(resultPaths);
    }

    /// <summary>
    /// Resamples the specified point range of every polygon
    /// in the path collection.
    /// </summary>
    /// <param name="collection">
    /// The path collection to resample.
    /// </param>
    /// <param name="startIndex">
    /// The index of the first point in the range to resample.
    /// </param>
    /// <param name="endIndex">
    /// The index of the last point in the range to resample.
    /// </param>
    /// <param name="newPointCount">
    /// The number of points used for the resampled range.
    /// </param>
    /// <returns>
    /// A new <see cref="PathCollection"/> containing the resampled polygons.
    /// </returns>
    public static PathCollection Resample(
        this PathCollection collection,
        int startIndex,
        int endIndex,
        int newPointCount)
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

                var resampled = PathTools.ResampleLinear(
                    input,
                    startIndex,
                    endIndex,
                    newPointCount);

                polygons.Add(resampled.ToArray());
            }

            resultPaths.Add(new Path(polygons));
        }

        return new PathCollection(resultPaths);
    }

    /// <summary>
    /// Simplifies all polygons in the specified path collection
    /// using the Douglas-Peucker algorithm.
    /// </summary>
    /// <param name="collection">
    /// The path collection to simplify.
    /// </param>
    /// <param name="tolerance">
    /// The maximum allowed deviation used by the simplification algorithm.
    /// </param>
    /// <returns>
    /// A new <see cref="PathCollection"/> containing the simplified polygons.
    /// </returns>
    public static PathCollection Simplify(
        this PathCollection collection,
        float tolerance)
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

                var simplified =
                    PathTools.SimplifyDouglasPeucker(
                        input,
                        tolerance);

                polygons.Add(simplified.ToArray());
            }

            resultPaths.Add(new Path(polygons));
        }

        return new PathCollection(resultPaths);
    }

    /// <summary>
    /// Flattens all polygon points from the specified path collection
    /// into a single point list.
    /// </summary>
    /// <param name="collection">
    /// The path collection to flatten.
    /// </param>
    /// <returns>
    /// A list containing all points from all polygons in the collection.
    /// </returns>
    public static List<Vector2> Flatten(
        this PathCollection collection)
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
    /// Optimizes all polygons in the specified path collection
    /// by applying simplification, optional resampling, and optional smoothing.
    /// </summary>
    /// <param name="collection">
    /// The path collection to optimize.
    /// </param>
    /// <param name="simplifyTolerance">
    /// The tolerance used for Douglas-Peucker simplification.
    /// </param>
    /// <param name="targetPointCount">
    /// The optional point count used when resampling the simplified path.
    /// </param>
    /// <param name="smoothFactor">
    /// The smoothing factor. A value of <c>0</c> disables smoothing.
    /// </param>
    /// <param name="smoothMaxAngleDeg">
    /// The maximum corner angle, in degrees, affected by smoothing.
    /// </param>
    /// <param name="smoothIterations">
    /// The number of smoothing passes to perform.
    /// </param>
    /// <returns>
    /// A new <see cref="PathCollection"/> containing the optimized paths.
    /// </returns>
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

            for (int polygonIndex = 0;
                 polygonIndex < path.GetPolygonCount();
                 polygonIndex++)
            {
                var points = path.GetPolygonPoints(polygonIndex);
                var input = new List<Vector2>(points.Count);

                for (int i = 0; i < points.Count; i++)
                    input.Add(points[i]);

                var optimized =
                    PathTools.SimplifyDouglasPeucker(
                        input,
                        simplifyTolerance);

                if (targetPointCount.HasValue &&
                    targetPointCount.Value > 2 &&
                    optimized.Count >= 2)
                {
                    optimized = PathTools.ResampleLinear(
                        optimized,
                        0,
                        optimized.Count - 1,
                        targetPointCount.Value);
                }

                if (smoothFactor > 0f &&
                    smoothIterations > 0)
                {
                    for (int iteration = 0;
                         iteration < smoothIterations;
                         iteration++)
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
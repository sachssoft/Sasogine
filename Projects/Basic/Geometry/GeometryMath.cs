using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Geometry;

/// <summary>
/// Provides utility methods for common geometric calculations on paths
/// and polygonal geometry.
/// </summary>
public static class GeometryMath
{
    /// <summary>
    /// Finds the polygon in the specified path containing the point
    /// nearest to the specified location.
    /// </summary>
    /// <param name="location">
    /// The location used for the search.
    /// </param>
    /// <param name="path">
    /// The path containing the polygons to search.
    /// </param>
    /// <param name="nearestPolygonIndex">
    /// Receives the index of the polygon containing the nearest point,
    /// or <c>-1</c> when the path contains no points.
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetNearestPolygon(
        Point2 location,
        Path path,
        out int nearestPolygonIndex)
    {
        float nearestDistanceSquared = float.PositiveInfinity;
        nearestPolygonIndex = -1;

        Vector2 locationVector = location;

        for (int i = 0; i < path.GetPolygonCount(); i++)
        {
            for (int j = 0; j < path.GetPointCount(i); j++)
            {
                Point2 point = path.GetPoint(i, j);
                Vector2 pointVector = point;

                float distanceSquared =
                    Vector2.DistanceSquared(
                        locationVector,
                        pointVector);

                if (distanceSquared >= nearestDistanceSquared)
                    continue;

                nearestDistanceSquared = distanceSquared;
                nearestPolygonIndex = i;
            }
        }
    }

    /// <summary>
    /// Finds the point in the specified path nearest to the specified location.
    /// </summary>
    /// <param name="location">
    /// The location used for the search.
    /// </param>
    /// <param name="path">
    /// The path containing the points to search.
    /// </param>
    /// <param name="nearestPoint">
    /// Receives the nearest point when one is found.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a point was found; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetNearestPoint(
        Point2 location,
        Path path,
        out Point2 nearestPoint)
    {
        float nearestDistanceSquared = float.PositiveInfinity;

        nearestPoint = default;
        bool found = false;

        Vector2 locationVector = location;

        for (int i = 0; i < path.GetPolygonCount(); i++)
        {
            for (int j = 0; j < path.GetPointCount(i); j++)
            {
                Point2 point = path.GetPoint(i, j);
                Vector2 pointVector = point;

                float distanceSquared =
                    Vector2.DistanceSquared(
                        locationVector,
                        pointVector);

                if (distanceSquared >= nearestDistanceSquared)
                    continue;

                nearestDistanceSquared = distanceSquared;
                nearestPoint = point;
                found = true;
            }
        }

        return found;
    }

    /// <summary>
    /// Finds the nearest point in the specified path and returns its polygon
    /// and point indices.
    /// </summary>
    /// <param name="location">
    /// The location used for the search.
    /// </param>
    /// <param name="path">
    /// The path containing the points to search.
    /// </param>
    /// <param name="polygonIndex">
    /// Receives the index of the polygon containing the nearest point.
    /// </param>
    /// <param name="pointIndex">
    /// Receives the index of the nearest point within the polygon.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a point was found; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetNearestPointIndex(
        Point2 location,
        Path path,
        out int polygonIndex,
        out int pointIndex)
    {
        float nearestDistanceSquared = float.PositiveInfinity;

        polygonIndex = -1;
        pointIndex = -1;

        Vector2 locationVector = location;

        for (int i = 0; i < path.GetPolygonCount(); i++)
        {
            for (int j = 0; j < path.GetPointCount(i); j++)
            {
                Point2 point = path.GetPoint(i, j);
                Vector2 pointVector = point;

                float distanceSquared =
                    Vector2.DistanceSquared(
                        locationVector,
                        pointVector);

                if (distanceSquared >= nearestDistanceSquared)
                    continue;

                nearestDistanceSquared = distanceSquared;
                polygonIndex = i;
                pointIndex = j;
            }
        }

        return polygonIndex >= 0;
    }

    /// <summary>
    /// Finds the point on any path segment nearest to the specified location.
    /// </summary>
    /// <param name="location">
    /// The location used for the search.
    /// </param>
    /// <param name="path">
    /// The path containing the segments to search.
    /// </param>
    /// <param name="nearestPoint">
    /// Receives the nearest point on the path.
    /// </param>
    /// <param name="polygonIndex">
    /// Receives the index of the polygon containing the nearest segment.
    /// </param>
    /// <param name="segmentIndex">
    /// Receives the index of the start point of the nearest segment.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a segment was found; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public static bool TryGetNearestPointOnPath(
        Point2 location,
        Path path,
        out Point2 nearestPoint,
        out int polygonIndex,
        out int segmentIndex)
    {
        float nearestDistanceSquared = float.PositiveInfinity;

        nearestPoint = default;
        polygonIndex = -1;
        segmentIndex = -1;

        Vector2 locationVector = location;

        for (int i = 0; i < path.GetPolygonCount(); i++)
        {
            int pointCount = path.GetPointCount(i);

            if (pointCount < 2)
                continue;

            for (int j = 0; j < pointCount - 1; j++)
            {
                Point2 start = path.GetPoint(i, j);
                Point2 end = path.GetPoint(i, j + 1);

                Vector2 projectedPoint =
                    VectorMath.ClosestPointOnSegment(
                        locationVector,
                        start,
                        end);

                float distanceSquared =
                    Vector2.DistanceSquared(
                        locationVector,
                        projectedPoint);

                if (distanceSquared >= nearestDistanceSquared)
                    continue;

                nearestDistanceSquared = distanceSquared;
                nearestPoint = projectedPoint;
                polygonIndex = i;
                segmentIndex = j;
            }
        }

        return polygonIndex >= 0;
    }

    /// <summary>
    /// Calculates the axis-aligned bounds of all points in the specified path.
    /// </summary>
    /// <param name="path">
    /// The path whose bounds should be calculated.
    /// </param>
    /// <param name="minimum">
    /// Receives the minimum coordinate of the bounds.
    /// </param>
    /// <param name="maximum">
    /// Receives the maximum coordinate of the bounds.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the path contains at least one point;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryGetBounds(
        Path path,
        out Point2 minimum,
        out Point2 maximum)
    {
        float minX = float.PositiveInfinity;
        float minY = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float maxY = float.NegativeInfinity;

        bool found = false;

        for (int i = 0; i < path.GetPolygonCount(); i++)
        {
            for (int j = 0; j < path.GetPointCount(i); j++)
            {
                Point2 point = path.GetPoint(i, j);

                minX = float.Min(minX, point.X);
                minY = float.Min(minY, point.Y);
                maxX = float.Max(maxX, point.X);
                maxY = float.Max(maxY, point.Y);

                found = true;
            }
        }

        if (found)
        {
            minimum = new Point2(minX, minY);
            maximum = new Point2(maxX, maxY);
            return true;
        }

        minimum = default;
        maximum = default;

        return false;
    }
}
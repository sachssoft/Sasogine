using Sachssoft.Engine;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Geometry.Internal;

/// <summary>
/// Provides allocation-bounded simplification for closed polygon contours.
/// </summary>
internal static class ClosedPolygonSimplifier
{
    /// <summary>
    /// Simplifies a closed polygon while preserving at least three points.
    /// </summary>
    public static Point2[] Simplify(
        IReadOnlyList<Point2> points,
        float tolerance)
    {
        ArgumentNullException.ThrowIfNull(points);

        if (!float.IsFinite(tolerance) || tolerance < 0f)
            throw new ArgumentOutOfRangeException(nameof(tolerance));

        if (points.Count <= 3 || tolerance <= 0f)
            return CopyPoints(points);

        int first = FindFarthestPoint(points, 0);
        int second = FindFarthestPoint(points, first);

        if (first == second)
            return CopyPoints(points);

        Point2[] source = CopyPoints(points);
        Point2[] chainA = BuildCircularRun(source, first, second);
        Point2[] chainB = BuildCircularRun(source, second, first);
        Point2[] simplifiedA = SimplifyOpenPolyline(chainA, tolerance);
        Point2[] simplifiedB = SimplifyOpenPolyline(chainB, tolerance);

        var result = new List<Point2>(
            simplifiedA.Length +
            simplifiedB.Length - 2);

        result.AddRange(simplifiedA);

        for (int i = 1; i < simplifiedB.Length - 1; i++)
            result.Add(simplifiedB[i]);

        RemoveCollinearPoints(result);

        return result.Count >= 3
            ? result.ToArray()
            : source;
    }

    private static Point2[] SimplifyOpenPolyline(
        Point2[] points,
        float tolerance)
    {
        if (points.Length <= 2 || tolerance <= 0f)
            return (Point2[])points.Clone();

        double toleranceSquared =
            (double)tolerance *
            tolerance;

        var keep = new bool[points.Length];
        keep[0] = true;
        keep[^1] = true;

        var stack = new Stack<(int First, int Last)>();
        stack.Push((0, points.Length - 1));

        while (stack.Count > 0)
        {
            (int first, int last) = stack.Pop();
            int bestIndex = -1;
            double bestDistanceSquared = toleranceSquared;

            for (int i = first + 1; i < last; i++)
            {
                double distanceSquared = DistanceToSegmentSquared(
                    points[i],
                    points[first],
                    points[last]);

                if (distanceSquared <= bestDistanceSquared)
                    continue;

                bestDistanceSquared = distanceSquared;
                bestIndex = i;
            }

            if (bestIndex < 0)
                continue;

            keep[bestIndex] = true;
            stack.Push((first, bestIndex));
            stack.Push((bestIndex, last));
        }

        var result = new List<Point2>();

        for (int i = 0; i < points.Length; i++)
        {
            if (keep[i])
                result.Add(points[i]);
        }

        return result.ToArray();
    }

    private static double DistanceToSegmentSquared(
        Point2 point,
        Point2 start,
        Point2 end)
    {
        double dx = (double)end.X - start.X;
        double dy = (double)end.Y - start.Y;
        double lengthSquared = dx * dx + dy * dy;

        if (lengthSquared <= 1e-20d)
        {
            double px = (double)point.X - start.X;
            double py = (double)point.Y - start.Y;
            return px * px + py * py;
        }

        double t =
            (((double)point.X - start.X) * dx +
             ((double)point.Y - start.Y) * dy) /
            lengthSquared;

        t = Math.Clamp(t, 0d, 1d);

        double nearestX = start.X + dx * t;
        double nearestY = start.Y + dy * t;
        double x = point.X - nearestX;
        double y = point.Y - nearestY;

        return x * x + y * y;
    }

    private static int FindFarthestPoint(
        IReadOnlyList<Point2> points,
        int originIndex)
    {
        Point2 origin = points[originIndex];
        int bestIndex = originIndex;
        double bestDistanceSquared = -1d;

        for (int i = 0; i < points.Count; i++)
        {
            double x = (double)points[i].X - origin.X;
            double y = (double)points[i].Y - origin.Y;
            double distanceSquared = x * x + y * y;

            if (distanceSquared <= bestDistanceSquared)
                continue;

            bestDistanceSquared = distanceSquared;
            bestIndex = i;
        }

        return bestIndex;
    }

    private static Point2[] BuildCircularRun(
        Point2[] points,
        int first,
        int last)
    {
        var run = new List<Point2>();
        int index = first;
        run.Add(points[index]);

        while (index != last)
        {
            index = (index + 1) % points.Length;
            run.Add(points[index]);

            if (run.Count > points.Length + 1)
            {
                throw new InvalidOperationException(
                    "Failed to construct a closed simplification run.");
            }
        }

        return run.ToArray();
    }

    private static void RemoveCollinearPoints(List<Point2> points)
    {
        if (points.Count <= 3)
            return;

        bool changed;

        do
        {
            changed = false;

            for (int i = points.Count - 1; i >= 0; i--)
            {
                if (points.Count <= 3)
                    return;

                Point2 previous = points[(i - 1 + points.Count) % points.Count];
                Point2 current = points[i];
                Point2 next = points[(i + 1) % points.Count];

                double ax = (double)current.X - previous.X;
                double ay = (double)current.Y - previous.Y;
                double bx = (double)next.X - current.X;
                double by = (double)next.Y - current.Y;
                double cross = ax * by - ay * bx;
                double dot = ax * bx + ay * by;

                if (Math.Abs(cross) > 1e-12d || dot < 0d)
                    continue;

                points.RemoveAt(i);
                changed = true;
            }
        }
        while (changed);
    }

    private static Point2[] CopyPoints(IReadOnlyList<Point2> points)
    {
        var result = new Point2[points.Count];

        for (int i = 0; i < points.Count; i++)
            result[i] = points[i];

        return result;
    }
}

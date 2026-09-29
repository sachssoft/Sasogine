using Sachssoft.Engine;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Geometry.Internal;

/// <summary>
/// Extracts exact directed polygon contours from a binary raster mask.
/// </summary>
internal static class RasterBoundaryTracer
{
    private readonly struct GridEdge
    {
        public GridEdge(
            int startX,
            int startY,
            int endX,
            int endY,
            int direction)
        {
            StartX = startX;
            StartY = startY;
            EndX = endX;
            EndY = endY;
            Direction = direction;
        }

        public int StartX { get; }
        public int StartY { get; }
        public int EndX { get; }
        public int EndY { get; }
        public int Direction { get; }
    }

    private struct EdgeBucket
    {
        public int First;
        public int Second;
        public byte Count;

        public void Add(int index)
        {
            if (Count == 0)
            {
                First = index;
                Count = 1;
                return;
            }

            if (Count == 1)
            {
                Second = index;
                Count = 2;
                return;
            }

            throw new InvalidOperationException(
                "A raster boundary vertex contains more outgoing edges than expected.");
        }
    }

    /// <summary>
    /// Extracts closed contours from the specified row-major binary mask.
    /// </summary>
    public static IReadOnlyList<Point2[]> Trace(
        bool[] mask,
        int width,
        int height)
    {
        ArgumentNullException.ThrowIfNull(mask);

        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));

        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height));

        int pixelCount = checked(width * height);

        if (mask.Length != pixelCount)
        {
            throw new ArgumentException(
                "The mask length does not match the specified dimensions.",
                nameof(mask));
        }

        var edges = new List<GridEdge>();
        var outgoing = new Dictionary<long, EdgeBucket>();

        BuildBoundaryEdges(
            mask,
            width,
            height,
            edges,
            outgoing);

        if (edges.Count == 0)
            return Array.Empty<Point2[]>();

        bool[] visited = new bool[edges.Count];
        var contours = new List<Point2[]>();

        for (int i = 0; i < edges.Count; i++)
        {
            if (visited[i])
                continue;

            if (!TryTraceContour(
                    i,
                    edges,
                    outgoing,
                    visited,
                    out List<Point2> points))
            {
                continue;
            }

            RemoveCollinearPoints(points);

            if (points.Count >= 3)
                contours.Add(points.ToArray());
        }

        return contours;
    }

    private static void BuildBoundaryEdges(
        bool[] mask,
        int width,
        int height,
        List<GridEdge> edges,
        Dictionary<long, EdgeBucket> outgoing)
    {
        for (int y = 0; y < height; y++)
        {
            int row = y * width;

            for (int x = 0; x < width; x++)
            {
                if (!mask[row + x])
                    continue;

                if (y == 0 || !mask[(y - 1) * width + x])
                    AddEdge(edges, outgoing, x, y, x + 1, y, 0);

                if (x == width - 1 || !mask[row + x + 1])
                    AddEdge(edges, outgoing, x + 1, y, x + 1, y + 1, 1);

                if (y == height - 1 || !mask[(y + 1) * width + x])
                    AddEdge(edges, outgoing, x + 1, y + 1, x, y + 1, 2);

                if (x == 0 || !mask[row + x - 1])
                    AddEdge(edges, outgoing, x, y + 1, x, y, 3);
            }
        }
    }

    private static void AddEdge(
        List<GridEdge> edges,
        Dictionary<long, EdgeBucket> outgoing,
        int startX,
        int startY,
        int endX,
        int endY,
        int direction)
    {
        int index = edges.Count;

        edges.Add(
            new GridEdge(
                startX,
                startY,
                endX,
                endY,
                direction));

        long key = PackVertex(startX, startY);
        outgoing.TryGetValue(key, out EdgeBucket bucket);

        bucket.Add(index);
        outgoing[key] = bucket;
    }

    private static bool TryTraceContour(
        int startEdgeIndex,
        IReadOnlyList<GridEdge> edges,
        IReadOnlyDictionary<long, EdgeBucket> outgoing,
        bool[] visited,
        out List<Point2> points)
    {
        GridEdge startEdge = edges[startEdgeIndex];
        int startX = startEdge.StartX;
        int startY = startEdge.StartY;
        int currentEdgeIndex = startEdgeIndex;

        points = new List<Point2>
        {
            new Point2(startX, startY)
        };

        for (int step = 0; step <= edges.Count; step++)
        {
            if ((uint)currentEdgeIndex >= (uint)edges.Count ||
                visited[currentEdgeIndex])
            {
                return false;
            }

            GridEdge edge = edges[currentEdgeIndex];
            visited[currentEdgeIndex] = true;

            if (edge.EndX == startX && edge.EndY == startY)
                return points.Count >= 3;

            points.Add(
                new Point2(
                    edge.EndX,
                    edge.EndY));

            int nextEdgeIndex = FindNextEdge(
                edge,
                edges,
                outgoing,
                visited);

            if (nextEdgeIndex < 0)
                return false;

            currentEdgeIndex = nextEdgeIndex;
        }

        return false;
    }

    private static int FindNextEdge(
        GridEdge current,
        IReadOnlyList<GridEdge> edges,
        IReadOnlyDictionary<long, EdgeBucket> outgoing,
        bool[] visited)
    {
        long key = PackVertex(
            current.EndX,
            current.EndY);

        if (!outgoing.TryGetValue(key, out EdgeBucket bucket))
            return -1;

        int bestIndex = -1;
        int bestScore = int.MaxValue;

        if (bucket.Count >= 1)
        {
            EvaluateNextEdge(
                bucket.First,
                current.Direction,
                edges,
                visited,
                ref bestIndex,
                ref bestScore);
        }

        if (bucket.Count >= 2)
        {
            EvaluateNextEdge(
                bucket.Second,
                current.Direction,
                edges,
                visited,
                ref bestIndex,
                ref bestScore);
        }

        return bestIndex;
    }

    private static void EvaluateNextEdge(
        int candidateIndex,
        int currentDirection,
        IReadOnlyList<GridEdge> edges,
        bool[] visited,
        ref int bestIndex,
        ref int bestScore)
    {
        if ((uint)candidateIndex >= (uint)edges.Count ||
            visited[candidateIndex])
        {
            return;
        }

        int direction = edges[candidateIndex].Direction;
        int turn = (direction - currentDirection + 4) & 3;

        // Boundaries keep filled pixels on their right side. At a vertex where
        // two diagonally touching regions meet, preferring the right turn keeps
        // the regions as independent simple contours instead of a figure eight.
        int score = turn switch
        {
            1 => 0,
            0 => 1,
            3 => 2,
            2 => 3,
            _ => 4
        };

        if (score >= bestScore)
            return;

        bestScore = score;
        bestIndex = candidateIndex;
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

    private static long PackVertex(int x, int y)
    {
        return ((long)y << 32) | (uint)x;
    }
}

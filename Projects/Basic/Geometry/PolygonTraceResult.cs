using Microsoft.Xna.Framework;
using Sachssoft.Engine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Sachssoft.Engine.Geometry;

/// <summary>
/// Represents the result of a raster-to-vector polygon tracing operation.
/// </summary>
public sealed class PolygonTraceResult
{
    private readonly ReadOnlyCollection<PolygonTraceContour> _contours;

    /// <summary>
    /// Gets an empty tracing result.
    /// </summary>
    public static PolygonTraceResult Empty { get; } =
        new PolygonTraceResult(0, 0, Array.Empty<PolygonTraceContour>());

    /// <summary>
    /// Initializes a new instance of the <see cref="PolygonTraceResult"/> class.
    /// </summary>
    /// <param name="width">The source texture width.</param>
    /// <param name="height">The source texture height.</param>
    /// <param name="contours">The traced vector contours.</param>
    public PolygonTraceResult(
        int width,
        int height,
        IReadOnlyList<PolygonTraceContour> contours)
    {
        ArgumentNullException.ThrowIfNull(contours);

        if (width < 0)
            throw new ArgumentOutOfRangeException(nameof(width));

        if (height < 0)
            throw new ArgumentOutOfRangeException(nameof(height));

        var copy = new PolygonTraceContour[contours.Count];

        for (int i = 0; i < contours.Count; i++)
        {
            copy[i] = contours[i] ??
                throw new ArgumentException(
                    "Trace contours cannot contain null entries.",
                    nameof(contours));
        }

        Width = width;
        Height = height;
        _contours = Array.AsReadOnly(copy);
    }

    /// <summary>Gets the source texture width.</summary>
    public int Width { get; }

    /// <summary>Gets the source texture height.</summary>
    public int Height { get; }

    /// <summary>Gets the traced vector contours.</summary>
    public IReadOnlyList<PolygonTraceContour> Contours => _contours;

    /// <summary>Gets whether the result contains no contours.</summary>
    public bool IsEmpty => _contours.Count == 0;

    /// <summary>
    /// Samples the traced vector contours into polygon contours.
    /// </summary>
    /// <param name="sampleLength">
    /// The desired approximate distance between consecutive points when sampling
    /// cubic Bézier segments. Must be greater than zero.
    /// </param>
    /// <returns>The sampled polygon contours.</returns>
    public IReadOnlyList<IReadOnlyList<Vector2>> ToContours(float sampleLength = 1f)
    {
        if (!float.IsFinite(sampleLength) || sampleLength <= 0f)
            throw new ArgumentOutOfRangeException(nameof(sampleLength));

        if (IsEmpty)
            return Array.Empty<IReadOnlyList<Vector2>>();

        var contours = new List<IReadOnlyList<Vector2>>(_contours.Count);

        for (int i = 0; i < _contours.Count; i++)
        {
            Point2[] points = SampleContour(_contours[i], sampleLength);

            if (points.Length < 3)
                continue;

            var vectors = new Vector2[points.Length];

            for (int j = 0; j < points.Length; j++)
                vectors[j] = new Vector2(points[j].X, points[j].Y);

            contours.Add(vectors);
        }

        return contours;
    }

    /// <summary>
    /// Samples the tracing result into an immutable polygon <see cref="Path"/>.
    /// </summary>
    /// <param name="sampleLength">
    /// The desired approximate distance between consecutive points when sampling
    /// cubic Bézier segments. Must be greater than zero.
    /// </param>
    /// <returns>A path containing the sampled traced contours.</returns>
    public Path ToPath(float sampleLength = 1f)
    {
        if (!float.IsFinite(sampleLength) || sampleLength <= 0f)
            throw new ArgumentOutOfRangeException(nameof(sampleLength));

        if (IsEmpty)
            return Path.Empty;

        var polygons = new List<Point2[]>(_contours.Count);

        for (int i = 0; i < _contours.Count; i++)
        {
            Point2[] points = SampleContour(_contours[i], sampleLength);

            if (points.Length >= 3)
                polygons.Add(points);
        }

        return polygons.Count == 0
            ? Path.Empty
            : new Path(polygons);
    }

    private static Point2[] SampleContour(
        PolygonTraceContour contour,
        float sampleLength)
    {
        var points = new List<Point2>(
            Math.Max(4, contour.Segments.Count + 1));

        Point2 current = contour.Start;
        points.Add(current);

        for (int i = 0; i < contour.Segments.Count; i++)
        {
            PolygonTraceSegment segment = contour.Segments[i];

            if (segment.Type == PolygonTraceSegmentType.Line)
            {
                AppendPoint(points, segment.End, contour.Start);
                current = segment.End;
                continue;
            }

            double approximateLength =
                Distance(current, segment.Control1) +
                Distance(segment.Control1, segment.Control2) +
                Distance(segment.Control2, segment.End);

            int segmentCount = Math.Clamp(
                (int)Math.Ceiling(approximateLength / sampleLength),
                1,
                4096);

            for (int step = 1; step <= segmentCount; step++)
            {
                float t = step / (float)segmentCount;
                Point2 point = EvaluateCubic(
                    current,
                    segment.Control1,
                    segment.Control2,
                    segment.End,
                    t);

                AppendPoint(points, point, contour.Start);
            }

            current = segment.End;
        }

        if (points.Count > 1 && points[^1] == points[0])
            points.RemoveAt(points.Count - 1);

        RemoveConsecutiveDuplicates(points);

        return points.Count >= 3
            ? points.ToArray()
            : Array.Empty<Point2>();
    }

    private static void AppendPoint(
        List<Point2> points,
        Point2 point,
        Point2 start)
    {
        if (point == start)
            return;

        if (points.Count == 0 || points[^1] != point)
            points.Add(point);
    }

    private static void RemoveConsecutiveDuplicates(List<Point2> points)
    {
        for (int i = points.Count - 1; i > 0; i--)
        {
            if (points[i] == points[i - 1])
                points.RemoveAt(i);
        }
    }

    private static Point2 EvaluateCubic(
        Point2 p0,
        Point2 p1,
        Point2 p2,
        Point2 p3,
        float t)
    {
        float mt = 1f - t;
        float a = mt * mt * mt;
        float b = 3f * mt * mt * t;
        float c = 3f * mt * t * t;
        float d = t * t * t;

        return new Point2(
            a * p0.X + b * p1.X + c * p2.X + d * p3.X,
            a * p0.Y + b * p1.Y + c * p2.Y + d * p3.Y);
    }

    private static double Distance(Point2 a, Point2 b)
    {
        double x = (double)b.X - a.X;
        double y = (double)b.Y - a.Y;
        return Math.Sqrt(x * x + y * y);
    }
}

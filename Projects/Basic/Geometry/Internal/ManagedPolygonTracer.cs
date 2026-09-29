using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Geometry.Internal;

/// <summary>
/// Provides a fully managed raster-to-vector polygon tracing backend.
/// </summary>
/// <remarks>
/// The implementation uses exact pixel-boundary extraction, closed-polygon
/// simplification and optional cubic Bézier fitting. It does not depend on
/// native libraries and is suitable for AOT and cross-platform builds.
/// </remarks>
internal sealed class ManagedPolygonTracer : IPolygonTracer
{
    /// <summary>
    /// Gets the shared stateless tracing backend instance.
    /// </summary>
    public static ManagedPolygonTracer Instance { get; } = new();

    private ManagedPolygonTracer()
    {
    }

    /// <inheritdoc/>
    public PolygonTraceResult Trace(
        Texture2D texture,
        PolygonTraceOptions options)
    {
        ArgumentNullException.ThrowIfNull(texture);
        ArgumentNullException.ThrowIfNull(options);

        if (texture.IsDisposed)
            throw new ObjectDisposedException(nameof(texture));

        int width = texture.Width;
        int height = texture.Height;

        if (width <= 0 || height <= 0)
            return PolygonTraceResult.Empty;

        int pixelCount = checked(width * height);
        var pixels = new Color[pixelCount];

        try
        {
            texture.GetData(pixels);
        }
        catch (ArgumentException exception)
        {
            throw new NotSupportedException(
                $"Texture format '{texture.Format}' cannot be read as Color data for tracing.",
                exception);
        }

        return TracePixels(
            pixels,
            width,
            height,
            options);
    }

    internal static PolygonTraceResult TracePixels(
        Color[] pixels,
        int width,
        int height,
        PolygonTraceOptions options)
    {
        ArgumentNullException.ThrowIfNull(pixels);
        ArgumentNullException.ThrowIfNull(options);

        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));

        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height));

        int pixelCount = checked(width * height);

        if (pixels.Length != pixelCount)
        {
            throw new ArgumentException(
                "The pixel count does not match the specified dimensions.",
                nameof(pixels));
        }

        bool[] mask = BuildMask(pixels, options);
        IReadOnlyList<Point2[]> sourceContours =
            RasterBoundaryTracer.Trace(
                mask,
                width,
                height);

        if (sourceContours.Count == 0)
        {
            return new PolygonTraceResult(
                width,
                height,
                Array.Empty<PolygonTraceContour>());
        }

        var contours = new List<PolygonTraceContour>(sourceContours.Count);

        for (int i = 0; i < sourceContours.Count; i++)
        {
            Point2[] sourcePoints = sourceContours[i];
            double sourceArea = ComputeSignedArea(sourcePoints);

            if (!double.IsFinite(sourceArea) ||
                Math.Abs(sourceArea) < options.MinimumArea)
            {
                continue;
            }

            Point2[] points = options.Mode == PolygonTraceMode.Pixel ||
                options.SimplificationTolerance <= 0f
                    ? (Point2[])sourcePoints.Clone()
                    : ClosedPolygonSimplifier.Simplify(
                        sourcePoints,
                        options.SimplificationTolerance);

            if (points.Length < 3)
                continue;

            bool isHole = sourceArea < 0d;
            PolygonTraceContour contour = options.Mode == PolygonTraceMode.Spline
                ? CreateSplineContour(
                    points,
                    sourceArea,
                    isHole,
                    options)
                : CreateLineContour(
                    points,
                    sourceArea,
                    isHole);

            contours.Add(contour);
        }

        return new PolygonTraceResult(
            width,
            height,
            contours);
    }

    private static bool[] BuildMask(
        Color[] pixels,
        PolygonTraceOptions options)
    {
        var mask = new bool[pixels.Length];
        int threshold = Math.Clamp(
            (int)Math.Round(options.Threshold * 255d),
            0,
            255);

        for (int i = 0; i < pixels.Length; i++)
        {
            int value = GetChannelValue(
                pixels[i],
                options.Channel);

            bool inside = value >= threshold;
            mask[i] = options.Invert ? !inside : inside;
        }

        return mask;
    }

    private static int GetChannelValue(
        Color color,
        PolygonTraceChannel channel)
    {
        return channel switch
        {
            PolygonTraceChannel.Alpha => color.A,
            PolygonTraceChannel.Red => color.R,
            PolygonTraceChannel.Green => color.G,
            PolygonTraceChannel.Blue => color.B,
            PolygonTraceChannel.Luminance =>
                (54 * color.R +
                 183 * color.G +
                 19 * color.B +
                 128) >> 8,
            _ => throw new ArgumentOutOfRangeException(nameof(channel))
        };
    }

    private static PolygonTraceContour CreateLineContour(
        Point2[] points,
        double sourceArea,
        bool isHole)
    {
        var segments = new PolygonTraceSegment[points.Length];

        for (int i = 0; i < points.Length - 1; i++)
            segments[i] = PolygonTraceSegment.Line(points[i + 1]);

        segments[^1] = PolygonTraceSegment.Line(points[0]);

        return new PolygonTraceContour(
            points[0],
            segments,
            isHole,
            sourceArea);
    }

    private static PolygonTraceContour CreateSplineContour(
        Point2[] points,
        double sourceArea,
        bool isHole,
        PolygonTraceOptions options)
    {
        PolygonTraceSegment[] segments = CubicBezierContourFitter.Fit(
            points,
            options.CurveTolerance,
            options.CornerThreshold,
            out Point2 start);

        return new PolygonTraceContour(
            start,
            segments,
            isHole,
            sourceArea);
    }

    private static double ComputeSignedArea(IReadOnlyList<Point2> points)
    {
        double area = 0d;

        for (int i = 0; i < points.Count; i++)
        {
            Point2 a = points[i];
            Point2 b = points[(i + 1) % points.Count];

            area +=
                (double)a.X * b.Y -
                (double)b.X * a.Y;
        }

        return area * 0.5d;
    }
}

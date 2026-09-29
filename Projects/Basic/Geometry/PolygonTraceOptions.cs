using System;

namespace Sachssoft.Engine.Geometry;

/// <summary>
/// Defines options used when tracing raster texture data into vector geometry.
/// </summary>
public sealed class PolygonTraceOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PolygonTraceOptions"/> class.
    /// </summary>
    /// <param name="mode">The output representation mode.</param>
    /// <param name="channel">The texture channel used to create the tracing mask.</param>
    /// <param name="threshold">The normalized mask threshold in the range 0 to 1.</param>
    /// <param name="invert">Whether the generated mask is inverted.</param>
    /// <param name="minimumArea">The minimum absolute contour area, in source pixels.</param>
    /// <param name="simplificationTolerance">The polygon simplification tolerance, in source pixels.</param>
    /// <param name="curveTolerance">The maximum fitting error for spline generation, in source pixels.</param>
    /// <param name="cornerThreshold">The maximum interior angle, in degrees, that is preserved as a corner.</param>
    public PolygonTraceOptions(
        PolygonTraceMode mode = PolygonTraceMode.Spline,
        PolygonTraceChannel channel = PolygonTraceChannel.Alpha,
        float threshold = 0.5f,
        bool invert = false,
        float minimumArea = 1f,
        float simplificationTolerance = 0.75f,
        float curveTolerance = 1f,
        float cornerThreshold = 135f)
    {
        if (mode != PolygonTraceMode.Pixel &&
            mode != PolygonTraceMode.Polygon &&
            mode != PolygonTraceMode.Spline)
        {
            throw new ArgumentOutOfRangeException(nameof(mode));
        }

        if (channel != PolygonTraceChannel.Alpha &&
            channel != PolygonTraceChannel.Luminance &&
            channel != PolygonTraceChannel.Red &&
            channel != PolygonTraceChannel.Green &&
            channel != PolygonTraceChannel.Blue)
        {
            throw new ArgumentOutOfRangeException(nameof(channel));
        }

        if (!float.IsFinite(threshold) || threshold < 0f || threshold > 1f)
            throw new ArgumentOutOfRangeException(nameof(threshold));

        if (!float.IsFinite(minimumArea) || minimumArea < 0f)
            throw new ArgumentOutOfRangeException(nameof(minimumArea));

        if (!float.IsFinite(simplificationTolerance) || simplificationTolerance < 0f)
            throw new ArgumentOutOfRangeException(nameof(simplificationTolerance));

        if (!float.IsFinite(curveTolerance) || curveTolerance <= 0f)
            throw new ArgumentOutOfRangeException(nameof(curveTolerance));

        if (!float.IsFinite(cornerThreshold) || cornerThreshold <= 0f || cornerThreshold >= 180f)
            throw new ArgumentOutOfRangeException(nameof(cornerThreshold));

        Mode = mode;
        Channel = channel;
        Threshold = threshold;
        Invert = invert;
        MinimumArea = minimumArea;
        SimplificationTolerance = simplificationTolerance;
        CurveTolerance = curveTolerance;
        CornerThreshold = cornerThreshold;
    }

    /// <summary>Gets the output representation mode.</summary>
    public PolygonTraceMode Mode { get; }

    /// <summary>Gets the texture channel used to create the tracing mask.</summary>
    public PolygonTraceChannel Channel { get; }

    /// <summary>Gets the normalized mask threshold in the range 0 to 1.</summary>
    public float Threshold { get; }

    /// <summary>Gets whether the generated mask is inverted.</summary>
    public bool Invert { get; }

    /// <summary>Gets the minimum absolute contour area, in source pixels.</summary>
    public float MinimumArea { get; }

    /// <summary>Gets the polygon simplification tolerance, in source pixels.</summary>
    public float SimplificationTolerance { get; }

    /// <summary>Gets the maximum fitting error for spline generation, in source pixels.</summary>
    public float CurveTolerance { get; }

    /// <summary>Gets the maximum interior angle, in degrees, that is preserved as a corner.</summary>
    public float CornerThreshold { get; }
}

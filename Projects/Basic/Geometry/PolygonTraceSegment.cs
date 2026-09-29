using Sachssoft.Engine;
using System;

namespace Sachssoft.Engine.Geometry;

/// <summary>
/// Represents a segment of a traced vector contour.
/// </summary>
public readonly struct PolygonTraceSegment
{
    private PolygonTraceSegment(
        PolygonTraceSegmentType type,
        Point2 end,
        Point2 control1,
        Point2 control2)
    {
        Type = type;
        End = end;
        Control1 = control1;
        Control2 = control2;
    }

    /// <summary>Gets the segment type.</summary>
    public PolygonTraceSegmentType Type { get; }

    /// <summary>Gets the segment end point.</summary>
    public Point2 End { get; }

    /// <summary>Gets the first control point of a cubic Bézier segment.</summary>
    public Point2 Control1 { get; }

    /// <summary>Gets the second control point of a cubic Bézier segment.</summary>
    public Point2 Control2 { get; }

    /// <summary>
    /// Creates a line segment.
    /// </summary>
    /// <param name="end">The end point of the segment.</param>
    /// <returns>A new line segment.</returns>
    public static PolygonTraceSegment Line(Point2 end)
    {
        ValidatePoint(end, nameof(end));

        return new PolygonTraceSegment(
            PolygonTraceSegmentType.Line,
            end,
            default,
            default);
    }

    /// <summary>
    /// Creates a cubic Bézier segment.
    /// </summary>
    /// <param name="control1">The first control point.</param>
    /// <param name="control2">The second control point.</param>
    /// <param name="end">The end point.</param>
    /// <returns>A new cubic Bézier segment.</returns>
    public static PolygonTraceSegment CubicBezier(
        Point2 control1,
        Point2 control2,
        Point2 end)
    {
        ValidatePoint(control1, nameof(control1));
        ValidatePoint(control2, nameof(control2));
        ValidatePoint(end, nameof(end));

        return new PolygonTraceSegment(
            PolygonTraceSegmentType.CubicBezier,
            end,
            control1,
            control2);
    }

    private static void ValidatePoint(Point2 point, string paramName)
    {
        if (!float.IsFinite(point.X) ||
            !float.IsFinite(point.Y))
        {
            throw new ArgumentException(
                "Trace segment coordinates must be finite.",
                paramName);
        }
    }
}

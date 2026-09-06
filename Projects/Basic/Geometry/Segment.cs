using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
using System;
using System.Globalization;

namespace Sachssoft.Sasogine.Geometry;

/// <summary>
/// Represents a two-dimensional line segment defined by a start point and an end point.
/// </summary>
public struct Segment
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Segment"/> struct with default values.
    /// </summary>
    public Segment()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Segment"/> struct using the specified coordinates.
    /// </summary>
    /// <param name="x1">The X-coordinate of the start point.</param>
    /// <param name="y1">The Y-coordinate of the start point.</param>
    /// <param name="x2">The X-coordinate of the end point.</param>
    /// <param name="y2">The Y-coordinate of the end point.</param>
    public Segment(
        float x1,
        float y1,
        float x2,
        float y2)
    {
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Segment"/> struct
    /// using the specified start and end points.
    /// </summary>
    /// <param name="start">The start point of the segment.</param>
    /// <param name="end">The end point of the segment.</param>
    public Segment(
        Point2 start,
        Point2 end)
    {
        X1 = start.X;
        Y1 = start.Y;
        X2 = end.X;
        Y2 = end.Y;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Segment"/> struct
    /// using the specified start and end vectors.
    /// </summary>
    /// <param name="start">The start point represented as a vector.</param>
    /// <param name="end">The end point represented as a vector.</param>
    public Segment(
        Vector2 start,
        Vector2 end)
        : this(
            new Point2(start),
            new Point2(end))
    {
    }

    /// <summary>
    /// Gets or sets the X-coordinate of the start point.
    /// </summary>
    public float X1 { get; set; }

    /// <summary>
    /// Gets or sets the Y-coordinate of the start point.
    /// </summary>
    public float Y1 { get; set; }

    /// <summary>
    /// Gets or sets the X-coordinate of the end point.
    /// </summary>
    public float X2 { get; set; }

    /// <summary>
    /// Gets or sets the Y-coordinate of the end point.
    /// </summary>
    public float Y2 { get; set; }

    /// <summary>
    /// Gets or sets the start point of the segment.
    /// </summary>
    public Point2 Start
    {
        readonly get => new Point2(X1, Y1);
        set
        {
            X1 = value.X;
            Y1 = value.Y;
        }
    }

    /// <summary>
    /// Gets or sets the end point of the segment.
    /// </summary>
    public Point2 End
    {
        readonly get => new Point2(X2, Y2);
        set
        {
            X2 = value.X;
            Y2 = value.Y;
        }
    }

    /// <summary>
    /// Gets the displacement vector from the start point to the end point.
    /// </summary>
    public readonly Vector2 Delta => End - Start;

    /// <summary>
    /// Gets the length of the segment.
    /// </summary>
    public readonly float Length => Delta.Length();

    /// <summary>
    /// Gets the non-normalized vector perpendicular to the segment
    /// in the clockwise direction.
    /// </summary>
    public readonly Vector2 NormalClockwise
    {
        get
        {
            Vector2 delta = Delta;
            return new Vector2(-delta.Y, delta.X);
        }
    }

    /// <summary>
    /// Gets the non-normalized vector perpendicular to the segment
    /// in the anti-clockwise direction.
    /// </summary>
    public readonly Vector2 NormalAntiClockwise
    {
        get
        {
            Vector2 delta = Delta;
            return new Vector2(delta.Y, -delta.X);
        }
    }

    /// <summary>
    /// Gets the angle of the segment relative to the positive X-axis.
    /// </summary>
    /// <value>
    /// The angle in radians in the range [-π, π].
    /// </value>
    public readonly float Angle =>
        float.Atan2(
            Y2 - Y1,
            X2 - X1);

    /// <summary>
    /// Parses a string representation of a segment.
    /// </summary>
    /// <param name="value">
    /// The segment in the format <c>"x1 y1, x2 y2"</c>.
    /// </param>
    /// <returns>The parsed <see cref="Segment"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    /// <paramref name="value"/> does not have the expected format.
    /// </exception>
    public static Segment Parse(string? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var values = value.Split(',');

        if (values.Length != 2)
        {
            throw new FormatException(
                "Expected segment format: \"x1 y1, x2 y2\".");
        }

        var start = values[0].Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries);

        var end = values[1].Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries);

        if (start.Length != 2 ||
            end.Length != 2)
        {
            throw new FormatException(
                "Expected segment format: \"x1 y1, x2 y2\".");
        }

        float x1 = float.Parse(
            start[0],
            CultureInfo.InvariantCulture);

        float y1 = float.Parse(
            start[1],
            CultureInfo.InvariantCulture);

        float x2 = float.Parse(
            end[0],
            CultureInfo.InvariantCulture);

        float y2 = float.Parse(
            end[1],
            CultureInfo.InvariantCulture);

        return new Segment(
            x1,
            y1,
            x2,
            y2);
    }

    /// <summary>
    /// Returns a string representation of the segment.
    /// </summary>
    /// <returns>
    /// A string containing the start and end coordinates.
    /// </returns>
    public override readonly string ToString()
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "X1: {0}, Y1: {1}, X2: {2}, Y2: {3}",
            X1,
            Y1,
            X2,
            Y2);
    }
}
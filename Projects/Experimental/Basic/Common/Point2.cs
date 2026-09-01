using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Represents a two-dimensional point using single-precision
/// floating-point coordinates.
/// </summary>
public readonly struct Point2 : IEquatable<Point2>
{
    /// <summary>
    /// Represents a point with both coordinates set to zero.
    /// </summary>
    public static readonly Point2 Zero = new Point2(0f, 0f);

    /// <summary>
    /// Initializes a new instance of the <see cref="Point2"/> structure.
    /// </summary>
    /// <param name="x">The x-coordinate of the point.</param>
    /// <param name="y">The y-coordinate of the point.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point2(float x, float y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Point2"/> structure
    /// with both coordinates set to the specified value.
    /// </summary>
    /// <param name="value">The value assigned to both coordinates.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point2(float value)
    {
        X = value;
        Y = value;
    }

    /// <summary>
    /// Gets the x-coordinate of the point.
    /// </summary>
    public float X { get; }

    /// <summary>
    /// Gets the y-coordinate of the point.
    /// </summary>
    public float Y { get; }

    /// <summary>
    /// Converts this point to a <see cref="Vector2"/>.
    /// </summary>
    /// <returns>
    /// A vector whose components correspond to the coordinates of this point.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 ToVector2()
        => new Vector2(X, Y);

    /// <summary>
    /// Deconstructs this point into its individual coordinates.
    /// </summary>
    /// <param name="x">Receives the x-coordinate.</param>
    /// <param name="y">Receives the y-coordinate.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out float x, out float y)
    {
        x = X;
        y = Y;
    }

    /// <summary>
    /// Determines whether this instance is equal to another
    /// <see cref="Point2"/> instance.
    /// </summary>
    /// <param name="other">The point to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if both points have identical coordinates;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Point2 other)
        => X == other.X && Y == other.Y;

    /// <summary>
    /// Determines whether the specified object is equal to this instance.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="obj"/> is a
    /// <see cref="Point2"/> with identical coordinates;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
        => obj is Point2 point && Equals(point);

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>A hash code based on both coordinates.</returns>
    public override int GetHashCode()
        => HashCode.Combine(X, Y);

    /// <summary>
    /// Adds the coordinates of two points component-wise.
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <returns>
    /// A point whose coordinates are the sums of the corresponding
    /// coordinates of <paramref name="a"/> and <paramref name="b"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point2 operator +(Point2 a, Point2 b)
        => new Point2(a.X + b.X, a.Y + b.Y);

    /// <summary>
    /// Subtracts the coordinates of one point from another component-wise.
    /// </summary>
    /// <param name="a">The point from which to subtract.</param>
    /// <param name="b">The point to subtract.</param>
    /// <returns>
    /// A point whose coordinates are the differences between the corresponding
    /// coordinates of <paramref name="a"/> and <paramref name="b"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point2 operator -(Point2 a, Point2 b)
        => new Point2(a.X - b.X, a.Y - b.Y);

    /// <summary>
    /// Multiplies the coordinates of two points component-wise.
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <returns>
    /// A point whose coordinates are the products of the corresponding
    /// coordinates of <paramref name="a"/> and <paramref name="b"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point2 operator *(Point2 a, Point2 b)
        => new Point2(a.X * b.X, a.Y * b.Y);

    /// <summary>
    /// Multiplies both coordinates of a point by a scalar value.
    /// </summary>
    /// <param name="value">The point to multiply.</param>
    /// <param name="scaleFactor">The scalar multiplier.</param>
    /// <returns>
    /// A point whose coordinates are multiplied by
    /// <paramref name="scaleFactor"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point2 operator *(Point2 value, float scaleFactor)
        => new Point2(value.X * scaleFactor, value.Y * scaleFactor);

    /// <summary>
    /// Multiplies both coordinates of a point by a scalar value.
    /// </summary>
    /// <param name="scaleFactor">The scalar multiplier.</param>
    /// <param name="value">The point to multiply.</param>
    /// <returns>
    /// A point whose coordinates are multiplied by
    /// <paramref name="scaleFactor"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point2 operator *(float scaleFactor, Point2 value)
        => value * scaleFactor;

    /// <summary>
    /// Divides the coordinates of one point by another component-wise.
    /// </summary>
    /// <param name="source">The point containing the dividend coordinates.</param>
    /// <param name="divisor">The point containing the divisor coordinates.</param>
    /// <returns>
    /// A point whose coordinates are the quotients of the corresponding
    /// coordinates.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point2 operator /(Point2 source, Point2 divisor)
        => new Point2(
            source.X / divisor.X,
            source.Y / divisor.Y);

    /// <summary>
    /// Divides both coordinates of a point by a scalar value.
    /// </summary>
    /// <param name="value">The point to divide.</param>
    /// <param name="divisor">The scalar divisor.</param>
    /// <returns>
    /// A point whose coordinates are divided by <paramref name="divisor"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point2 operator /(Point2 value, float divisor)
        => new Point2(
            value.X / divisor,
            value.Y / divisor);

    /// <summary>
    /// Determines whether two <see cref="Point2"/> instances are equal.
    /// </summary>
    /// <param name="a">The first point to compare.</param>
    /// <param name="b">The second point to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both points have identical coordinates;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Point2 a, Point2 b)
        => a.Equals(b);

    /// <summary>
    /// Determines whether two <see cref="Point2"/> instances are not equal.
    /// </summary>
    /// <param name="a">The first point to compare.</param>
    /// <param name="b">The second point to compare.</param>
    /// <returns>
    /// <see langword="true"/> if the points have different coordinates;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Point2 a, Point2 b)
        => !a.Equals(b);

    /// <summary>
    /// Parses a string representation of a <see cref="Point2"/>.
    /// </summary>
    /// <param name="s">
    /// A string containing two numeric values representing the x- and
    /// y-coordinates.
    /// </param>
    /// <returns>The parsed <see cref="Point2"/>.</returns>
    /// <exception cref="FormatException">
    /// Thrown when the specified string does not contain exactly two valid
    /// numeric values.
    /// </exception>
    /// <remarks>
    /// Values may be separated by commas or spaces and are interpreted using
    /// <see cref="CultureInfo.InvariantCulture"/>.
    /// </remarks>
    public static Point2 Parse(string s)
    {
        if (TryParse(s, out var result))
            return result;

        throw new FormatException(
            $"Invalid Point2 format: '{s}'. Expected 2 numeric values separated by ',' or ' '.");
    }

    /// <summary>
    /// Attempts to parse a string representation of a <see cref="Point2"/>.
    /// </summary>
    /// <param name="s">
    /// A string containing two numeric values representing the x- and
    /// y-coordinates.
    /// </param>
    /// <param name="result">
    /// When this method returns <see langword="true"/>, contains the parsed
    /// point; otherwise, contains <see cref="Zero"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the string was successfully parsed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryParse(string? s, out Point2 result)
    {
        result = Zero;

        if (string.IsNullOrWhiteSpace(s))
            return false;

        var parts = s.Split(
            new[] { ',', ' ' },
            StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
            return false;

        if (!float.TryParse(
                parts[0],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float x) ||
            !float.TryParse(
                parts[1],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float y))
        {
            return false;
        }

        result = new Point2(x, y);
        return true;
    }

    /// <summary>
    /// Returns the string representation of this <see cref="Point2"/>.
    /// </summary>
    /// <returns>
    /// A string containing the x- and y-coordinates separated by a comma.
    /// </returns>
    /// <remarks>
    /// Numeric values are formatted using
    /// <see cref="CultureInfo.InvariantCulture"/>.
    /// </remarks>
    public override string ToString()
        => string.Format(
            CultureInfo.InvariantCulture,
            "{0}, {1}",
            X,
            Y);
}
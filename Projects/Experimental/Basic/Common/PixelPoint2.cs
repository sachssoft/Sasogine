using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Represents a two-dimensional point using integer pixel coordinates.
/// </summary>
public readonly struct PixelPoint2 : IEquatable<PixelPoint2>
{
    /// <summary>
    /// Represents a pixel point with both coordinates set to zero.
    /// </summary>
    public static readonly PixelPoint2 Zero = new PixelPoint2(0, 0);

    /// <summary>
    /// Initializes a new instance of the <see cref="PixelPoint2"/> structure.
    /// </summary>
    /// <param name="x">The x-coordinate of the point in pixels.</param>
    /// <param name="y">The y-coordinate of the point in pixels.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelPoint2(int x, int y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PixelPoint2"/> structure
    /// with both coordinates set to the specified value.
    /// </summary>
    /// <param name="value">The value assigned to both coordinates.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelPoint2(int value)
    {
        X = value;
        Y = value;
    }

    /// <summary>
    /// Gets the x-coordinate of the point in pixels.
    /// </summary>
    public int X { get; }

    /// <summary>
    /// Gets the y-coordinate of the point in pixels.
    /// </summary>
    public int Y { get; }

    /// <summary>
    /// Converts this pixel point to a <see cref="Vector2"/>.
    /// </summary>
    /// <returns>
    /// A vector whose components correspond to the pixel coordinates
    /// of this point.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 ToVector2()
        => new Vector2(X, Y);

    /// <summary>
    /// Deconstructs this pixel point into its individual coordinates.
    /// </summary>
    /// <param name="x">Receives the x-coordinate.</param>
    /// <param name="y">Receives the y-coordinate.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }

    /// <summary>
    /// Determines whether this instance is equal to another
    /// <see cref="PixelPoint2"/> instance.
    /// </summary>
    /// <param name="other">The pixel point to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if both pixel points have identical coordinates;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(PixelPoint2 other)
        => X == other.X && Y == other.Y;

    /// <summary>
    /// Determines whether the specified object is equal to this instance.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="obj"/> is a
    /// <see cref="PixelPoint2"/> with identical coordinates;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
        => obj is PixelPoint2 point && Equals(point);

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>A hash code based on both coordinates.</returns>
    public override int GetHashCode()
        => HashCode.Combine(X, Y);

    /// <summary>
    /// Adds the coordinates of two pixel points component-wise.
    /// </summary>
    /// <param name="a">The first pixel point.</param>
    /// <param name="b">The second pixel point.</param>
    /// <returns>A pixel point containing the component-wise sums.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PixelPoint2 operator +(PixelPoint2 a, PixelPoint2 b)
        => new PixelPoint2(a.X + b.X, a.Y + b.Y);

    /// <summary>
    /// Subtracts the coordinates of one pixel point from another component-wise.
    /// </summary>
    /// <param name="a">The pixel point from which to subtract.</param>
    /// <param name="b">The pixel point to subtract.</param>
    /// <returns>A pixel point containing the component-wise differences.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PixelPoint2 operator -(PixelPoint2 a, PixelPoint2 b)
        => new PixelPoint2(a.X - b.X, a.Y - b.Y);

    /// <summary>
    /// Multiplies the coordinates of two pixel points component-wise.
    /// </summary>
    /// <param name="a">The first pixel point.</param>
    /// <param name="b">The second pixel point.</param>
    /// <returns>A pixel point containing the component-wise products.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PixelPoint2 operator *(PixelPoint2 a, PixelPoint2 b)
        => new PixelPoint2(a.X * b.X, a.Y * b.Y);

    /// <summary>
    /// Multiplies both coordinates of a pixel point by an integer scalar.
    /// </summary>
    /// <param name="value">The pixel point to multiply.</param>
    /// <param name="scaleFactor">The integer scalar multiplier.</param>
    /// <returns>A pixel point containing the scaled coordinates.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PixelPoint2 operator *(PixelPoint2 value, int scaleFactor)
        => new PixelPoint2(
            value.X * scaleFactor,
            value.Y * scaleFactor);

    /// <summary>
    /// Multiplies both coordinates of a pixel point by an integer scalar.
    /// </summary>
    /// <param name="scaleFactor">The integer scalar multiplier.</param>
    /// <param name="value">The pixel point to multiply.</param>
    /// <returns>A pixel point containing the scaled coordinates.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PixelPoint2 operator *(int scaleFactor, PixelPoint2 value)
        => value * scaleFactor;

    /// <summary>
    /// Divides the coordinates of one pixel point by another component-wise
    /// using integer division.
    /// </summary>
    /// <param name="source">The pixel point containing the dividend coordinates.</param>
    /// <param name="divisor">The pixel point containing the divisor coordinates.</param>
    /// <returns>A pixel point containing the component-wise integer quotients.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PixelPoint2 operator /(PixelPoint2 source, PixelPoint2 divisor)
        => new PixelPoint2(
            source.X / divisor.X,
            source.Y / divisor.Y);

    /// <summary>
    /// Divides both coordinates of a pixel point by an integer scalar using
    /// integer division.
    /// </summary>
    /// <param name="value">The pixel point to divide.</param>
    /// <param name="divisor">The integer scalar divisor.</param>
    /// <returns>A pixel point containing the divided coordinates.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PixelPoint2 operator /(PixelPoint2 value, int divisor)
        => new PixelPoint2(
            value.X / divisor,
            value.Y / divisor);

    /// <summary>
    /// Determines whether two <see cref="PixelPoint2"/> instances are equal.
    /// </summary>
    /// <param name="a">The first pixel point to compare.</param>
    /// <param name="b">The second pixel point to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both pixel points have identical coordinates;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(PixelPoint2 a, PixelPoint2 b)
        => a.Equals(b);

    /// <summary>
    /// Determines whether two <see cref="PixelPoint2"/> instances are not equal.
    /// </summary>
    /// <param name="a">The first pixel point to compare.</param>
    /// <param name="b">The second pixel point to compare.</param>
    /// <returns>
    /// <see langword="true"/> if the pixel points have different coordinates;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(PixelPoint2 a, PixelPoint2 b)
        => !a.Equals(b);

    /// <summary>
    /// Parses a string representation of a <see cref="PixelPoint2"/>.
    /// </summary>
    /// <param name="s">
    /// A string containing two integer values representing the pixel coordinates.
    /// </param>
    /// <returns>The parsed <see cref="PixelPoint2"/>.</returns>
    /// <exception cref="FormatException">
    /// Thrown when the specified string does not contain exactly two valid
    /// integer values.
    /// </exception>
    public static PixelPoint2 Parse(string s)
    {
        if (TryParse(s, out var result))
            return result;

        throw new FormatException(
            $"Invalid PixelPoint2 format: '{s}'. Expected 2 integer values separated by ',' or ' '.");
    }

    /// <summary>
    /// Attempts to parse a string representation of a
    /// <see cref="PixelPoint2"/>.
    /// </summary>
    /// <param name="s">
    /// A string containing two integer values representing the pixel coordinates.
    /// </param>
    /// <param name="result">
    /// When this method returns <see langword="true"/>, contains the parsed
    /// pixel point; otherwise, contains <see cref="Zero"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the string was successfully parsed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryParse(string? s, out PixelPoint2 result)
    {
        result = Zero;

        if (string.IsNullOrWhiteSpace(s))
            return false;

        var parts = s.Split(
            new[] { ',', ' ' },
            StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
            return false;

        if (!int.TryParse(
                parts[0],
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int x) ||
            !int.TryParse(
                parts[1],
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int y))
        {
            return false;
        }

        result = new PixelPoint2(x, y);
        return true;
    }

    /// <summary>
    /// Returns the string representation of this
    /// <see cref="PixelPoint2"/>.
    /// </summary>
    /// <returns>
    /// A string containing the x- and y-coordinates separated by a comma.
    /// </returns>
    public override string ToString()
        => string.Format(
            CultureInfo.InvariantCulture,
            "{0}, {1}",
            X,
            Y);
}
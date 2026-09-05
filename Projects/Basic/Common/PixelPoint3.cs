using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Represents a three-dimensional point using integer pixel coordinates.
/// </summary>
public readonly struct PixelPoint3 : IEquatable<PixelPoint3>
{
    /// <summary>
    /// Represents a pixel point with all coordinates set to zero.
    /// </summary>
    public static readonly PixelPoint3 Zero = new PixelPoint3(0, 0, 0);

    /// <summary>
    /// Initializes a new instance of the <see cref="PixelPoint3"/> structure.
    /// </summary>
    /// <param name="x">The x-coordinate of the point.</param>
    /// <param name="y">The y-coordinate of the point.</param>
    /// <param name="z">The z-coordinate of the point.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelPoint3(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PixelPoint3"/> structure
    /// with all coordinates set to the specified value.
    /// </summary>
    /// <param name="value">The value assigned to all coordinates.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelPoint3(int value)
    {
        X = value;
        Y = value;
        Z = value;
    }

    /// <summary>
    /// Gets the x-coordinate of the point.
    /// </summary>
    public int X { get; }

    /// <summary>
    /// Gets the y-coordinate of the point.
    /// </summary>
    public int Y { get; }

    /// <summary>
    /// Gets the z-coordinate of the point.
    /// </summary>
    public int Z { get; }

    /// <summary>
    /// Converts this pixel point to a <see cref="Vector3"/>.
    /// </summary>
    /// <returns>
    /// A vector whose components correspond to the coordinates of this point.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 ToVector3()
        => new Vector3(X, Y, Z);

    /// <summary>
    /// Deconstructs this pixel point into its individual coordinates.
    /// </summary>
    /// <param name="x">Receives the x-coordinate.</param>
    /// <param name="y">Receives the y-coordinate.</param>
    /// <param name="z">Receives the z-coordinate.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out int x, out int y, out int z)
    {
        x = X;
        y = Y;
        z = Z;
    }

    /// <summary>
    /// Determines whether this instance is equal to another
    /// <see cref="PixelPoint3"/> instance.
    /// </summary>
    /// <param name="other">The pixel point to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if both pixel points have identical coordinates;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(PixelPoint3 other)
        => X == other.X && Y == other.Y && Z == other.Z;

    /// <summary>
    /// Determines whether the specified object is equal to this instance.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="obj"/> is a
    /// <see cref="PixelPoint3"/> with identical coordinates;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
        => obj is PixelPoint3 point && Equals(point);

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>A hash code based on all three coordinates.</returns>
    public override int GetHashCode()
        => HashCode.Combine(X, Y, Z);

    /// <summary>
    /// Adds the coordinates of two pixel points component-wise.
    /// </summary>
    /// <param name="a">The first pixel point.</param>
    /// <param name="b">The second pixel point.</param>
    /// <returns>A pixel point containing the component-wise sums.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PixelPoint3 operator +(PixelPoint3 a, PixelPoint3 b)
        => new PixelPoint3(
            a.X + b.X,
            a.Y + b.Y,
            a.Z + b.Z);

    /// <summary>
    /// Subtracts the coordinates of one pixel point from another component-wise.
    /// </summary>
    /// <param name="a">The pixel point from which to subtract.</param>
    /// <param name="b">The pixel point to subtract.</param>
    /// <returns>A pixel point containing the component-wise differences.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PixelPoint3 operator -(PixelPoint3 a, PixelPoint3 b)
        => new PixelPoint3(
            a.X - b.X,
            a.Y - b.Y,
            a.Z - b.Z);

    /// <summary>
    /// Multiplies the coordinates of two pixel points component-wise.
    /// </summary>
    /// <param name="a">The first pixel point.</param>
    /// <param name="b">The second pixel point.</param>
    /// <returns>A pixel point containing the component-wise products.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PixelPoint3 operator *(PixelPoint3 a, PixelPoint3 b)
        => new PixelPoint3(
            a.X * b.X,
            a.Y * b.Y,
            a.Z * b.Z);

    /// <summary>
    /// Multiplies all coordinates of a pixel point by an integer scalar.
    /// </summary>
    /// <param name="value">The pixel point to multiply.</param>
    /// <param name="scaleFactor">The integer scalar multiplier.</param>
    /// <returns>A pixel point containing the scaled coordinates.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PixelPoint3 operator *(PixelPoint3 value, int scaleFactor)
        => new PixelPoint3(
            value.X * scaleFactor,
            value.Y * scaleFactor,
            value.Z * scaleFactor);

    /// <summary>
    /// Multiplies all coordinates of a pixel point by an integer scalar.
    /// </summary>
    /// <param name="scaleFactor">The integer scalar multiplier.</param>
    /// <param name="value">The pixel point to multiply.</param>
    /// <returns>A pixel point containing the scaled coordinates.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PixelPoint3 operator *(int scaleFactor, PixelPoint3 value)
        => value * scaleFactor;

    /// <summary>
    /// Divides the coordinates of one pixel point by another component-wise
    /// using integer division.
    /// </summary>
    /// <param name="source">The pixel point containing the dividend coordinates.</param>
    /// <param name="divisor">The pixel point containing the divisor coordinates.</param>
    /// <returns>A pixel point containing the component-wise integer quotients.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PixelPoint3 operator /(PixelPoint3 source, PixelPoint3 divisor)
        => new PixelPoint3(
            source.X / divisor.X,
            source.Y / divisor.Y,
            source.Z / divisor.Z);

    /// <summary>
    /// Divides all coordinates of a pixel point by an integer scalar using
    /// integer division.
    /// </summary>
    /// <param name="value">The pixel point to divide.</param>
    /// <param name="divisor">The integer scalar divisor.</param>
    /// <returns>A pixel point containing the divided coordinates.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PixelPoint3 operator /(PixelPoint3 value, int divisor)
        => new PixelPoint3(
            value.X / divisor,
            value.Y / divisor,
            value.Z / divisor);

    /// <summary>
    /// Determines whether two <see cref="PixelPoint3"/> instances are equal.
    /// </summary>
    /// <param name="a">The first pixel point to compare.</param>
    /// <param name="b">The second pixel point to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both pixel points have identical coordinates;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(PixelPoint3 a, PixelPoint3 b)
        => a.Equals(b);

    /// <summary>
    /// Determines whether two <see cref="PixelPoint3"/> instances are not equal.
    /// </summary>
    /// <param name="a">The first pixel point to compare.</param>
    /// <param name="b">The second pixel point to compare.</param>
    /// <returns>
    /// <see langword="true"/> if the pixel points have different coordinates;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(PixelPoint3 a, PixelPoint3 b)
        => !a.Equals(b);

    /// <summary>
    /// Parses a string representation of a <see cref="PixelPoint3"/>.
    /// </summary>
    /// <param name="s">
    /// A string containing three integer values representing the coordinates.
    /// </param>
    /// <returns>The parsed <see cref="PixelPoint3"/>.</returns>
    /// <exception cref="FormatException">
    /// Thrown when the specified string does not contain exactly three valid
    /// integer values.
    /// </exception>
    public static PixelPoint3 Parse(string s)
    {
        if (TryParse(s, out var result))
            return result;

        throw new FormatException(
            $"Invalid PixelPoint3 format: '{s}'. Expected 3 integer values separated by ',' or ' '.");
    }

    /// <summary>
    /// Attempts to parse a string representation of a
    /// <see cref="PixelPoint3"/>.
    /// </summary>
    /// <param name="s">
    /// A string containing three integer values representing the coordinates.
    /// </param>
    /// <param name="result">
    /// When this method returns <see langword="true"/>, contains the parsed
    /// pixel point; otherwise, contains <see cref="Zero"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the string was successfully parsed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryParse(string? s, out PixelPoint3 result)
    {
        result = Zero;

        if (string.IsNullOrWhiteSpace(s))
            return false;

        var parts = s.Split(
            new[] { ',', ' ' },
            StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 3)
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
                out int y) ||
            !int.TryParse(
                parts[2],
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int z))
        {
            return false;
        }

        result = new PixelPoint3(x, y, z);
        return true;
    }

    /// <summary>
    /// Returns the string representation of this
    /// <see cref="PixelPoint3"/>.
    /// </summary>
    /// <returns>
    /// A string containing the x-, y-, and z-coordinates separated by commas.
    /// </returns>
    public override string ToString()
        => string.Format(
            CultureInfo.InvariantCulture,
            "{0}, {1}, {2}",
            X,
            Y,
            Z);
}
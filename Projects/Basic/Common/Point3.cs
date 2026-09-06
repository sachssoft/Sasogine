using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Represents a three-dimensional point using single-precision
/// floating-point coordinates.
/// </summary>
public readonly struct Point3 : IEquatable<Point3>
{
    /// <summary>
    /// Represents a point with all coordinates set to zero.
    /// </summary>
    public static readonly Point3 Zero = new Point3(0f, 0f, 0f);

    /// <summary>
    /// Initializes a new instance of the <see cref="Point3"/> structure.
    /// </summary>
    /// <param name="x">The x-coordinate of the point.</param>
    /// <param name="y">The y-coordinate of the point.</param>
    /// <param name="z">The z-coordinate of the point.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Point3"/> structure
    /// with all coordinates set to the specified value.
    /// </summary>
    /// <param name="value">The value assigned to all coordinates.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point3(float value)
    {
        X = value;
        Y = value;
        Z = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Point3"/> structure
    /// from the specified vector.
    /// </summary>
    /// <param name="value">The vector containing the point coordinates.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point3(Vector3 value)
    {
        X = value.X;
        Y = value.Y;
        Z = value.Z;
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
    /// Gets the z-coordinate of the point.
    /// </summary>
    public float Z { get; }

    /// <summary>
    /// Converts this point to a <see cref="Vector3"/>.
    /// </summary>
    /// <returns>
    /// A vector whose components correspond to the coordinates of this point.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 ToVector3()
        => new Vector3(X, Y, Z);

    /// <summary>
    /// Deconstructs this point into its individual coordinates.
    /// </summary>
    /// <param name="x">Receives the x-coordinate.</param>
    /// <param name="y">Receives the y-coordinate.</param>
    /// <param name="z">Receives the z-coordinate.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(
        out float x,
        out float y,
        out float z)
    {
        x = X;
        y = Y;
        z = Z;
    }

    /// <summary>
    /// Calculates the distance between two points.
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <returns>The distance between the two points.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Distance(Point3 a, Point3 b)
        => Vector3.Distance(a, b);

    /// <summary>
    /// Calculates the squared distance between two points.
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <returns>The squared distance between the two points.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DistanceSquared(Point3 a, Point3 b)
        => Vector3.DistanceSquared(a, b);

    /// <summary>
    /// Performs a linear interpolation between two points.
    /// </summary>
    /// <param name="a">The start point.</param>
    /// <param name="b">The end point.</param>
    /// <param name="amount">
    /// The interpolation amount between <paramref name="a"/> and
    /// <paramref name="b"/>.
    /// </param>
    /// <returns>The interpolated point.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point3 Lerp(
        Point3 a,
        Point3 b,
        float amount)
    {
        return new Point3(
            a.X + (b.X - a.X) * amount,
            a.Y + (b.Y - a.Y) * amount,
            a.Z + (b.Z - a.Z) * amount);
    }

    /// <summary>
    /// Adds the specified vector to the point.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="vector">The vector to add.</param>
    /// <returns>The translated point.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point3 operator +(
        Point3 point,
        Vector3 vector)
    {
        return new Point3(
            point.X + vector.X,
            point.Y + vector.Y,
            point.Z + vector.Z);
    }

    /// <summary>
    /// Adds the specified point to the vector.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <param name="point">The point to add.</param>
    /// <returns>The translated point.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point3 operator +(
        Vector3 vector,
        Point3 point)
    {
        return point + vector;
    }

    /// <summary>
    /// Subtracts the specified vector from the point.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="vector">The vector to subtract.</param>
    /// <returns>The translated point.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point3 operator -(
        Point3 point,
        Vector3 vector)
    {
        return new Point3(
            point.X - vector.X,
            point.Y - vector.Y,
            point.Z - vector.Z);
    }

    /// <summary>
    /// Subtracts one point from another and returns the displacement vector.
    /// </summary>
    /// <param name="a">The point from which to subtract.</param>
    /// <param name="b">The point to subtract.</param>
    /// <returns>
    /// The vector from <paramref name="b"/> to <paramref name="a"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 operator -(
        Point3 a,
        Point3 b)
    {
        return new Vector3(
            a.X - b.X,
            a.Y - b.Y,
            a.Z - b.Z);
    }

    /// <summary>
    /// Multiplies all coordinates of a point by a scalar value.
    /// </summary>
    /// <param name="value">The point to multiply.</param>
    /// <param name="scaleFactor">The scalar multiplier.</param>
    /// <returns>The scaled point.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point3 operator *(
        Point3 value,
        float scaleFactor)
    {
        return new Point3(
            value.X * scaleFactor,
            value.Y * scaleFactor,
            value.Z * scaleFactor);
    }

    /// <summary>
    /// Multiplies all coordinates of a point by a scalar value.
    /// </summary>
    /// <param name="scaleFactor">The scalar multiplier.</param>
    /// <param name="value">The point to multiply.</param>
    /// <returns>The scaled point.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point3 operator *(
        float scaleFactor,
        Point3 value)
    {
        return value * scaleFactor;
    }

    /// <summary>
    /// Divides all coordinates of a point by a scalar value.
    /// </summary>
    /// <param name="value">The point to divide.</param>
    /// <param name="divisor">The scalar divisor.</param>
    /// <returns>The divided point.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point3 operator /(
        Point3 value,
        float divisor)
    {
        return new Point3(
            value.X / divisor,
            value.Y / divisor,
            value.Z / divisor);
    }

    /// <summary>
    /// Converts a <see cref="Vector3"/> to a <see cref="Point3"/>.
    /// </summary>
    /// <param name="value">The vector to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Point3(Vector3 value)
        => new Point3(value);

    /// <summary>
    /// Converts a <see cref="Point3"/> to a <see cref="Vector3"/>.
    /// </summary>
    /// <param name="value">The point to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector3(Point3 value)
        => new Vector3(value.X, value.Y, value.Z);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Point3 other)
        => X == other.X &&
           Y == other.Y &&
           Z == other.Z;

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => obj is Point3 point &&
           Equals(point);

    /// <inheritdoc/>
    public override int GetHashCode()
        => HashCode.Combine(X, Y, Z);

    /// <summary>
    /// Determines whether two <see cref="Point3"/> instances are equal.
    /// </summary>
    /// <param name="a">The first point to compare.</param>
    /// <param name="b">The second point to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both points are equal;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(
        Point3 a,
        Point3 b)
    {
        return a.Equals(b);
    }

    /// <summary>
    /// Determines whether two <see cref="Point3"/> instances are not equal.
    /// </summary>
    /// <param name="a">The first point to compare.</param>
    /// <param name="b">The second point to compare.</param>
    /// <returns>
    /// <see langword="true"/> if the points are not equal;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(
        Point3 a,
        Point3 b)
    {
        return !a.Equals(b);
    }

    /// <summary>
    /// Parses a string representation of a <see cref="Point3"/>.
    /// </summary>
    /// <param name="s">
    /// A string containing three numeric values representing the x-, y-,
    /// and z-coordinates.
    /// </param>
    /// <returns>The parsed <see cref="Point3"/>.</returns>
    /// <exception cref="FormatException">
    /// Thrown when the specified string does not contain exactly three valid
    /// numeric values.
    /// </exception>
    public static Point3 Parse(string s)
    {
        if (TryParse(s, out var result))
            return result;

        throw new FormatException(
            $"Invalid Point3 format: '{s}'. Expected 3 numeric values separated by ',' or ' '.");
    }

    /// <summary>
    /// Attempts to parse a string representation of a <see cref="Point3"/>.
    /// </summary>
    /// <param name="s">
    /// A string containing three numeric values representing the coordinates.
    /// </param>
    /// <param name="result">
    /// When this method returns <see langword="true"/>, contains the parsed
    /// point; otherwise, contains <see cref="Zero"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the string was successfully parsed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryParse(
        string? s,
        out Point3 result)
    {
        result = Zero;

        if (string.IsNullOrWhiteSpace(s))
            return false;

        var parts = s.Split(
            new[] { ',', ' ' },
            StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 3)
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
                out float y) ||
            !float.TryParse(
                parts[2],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float z))
        {
            return false;
        }

        result = new Point3(x, y, z);
        return true;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0}, {1}, {2}",
            X,
            Y,
            Z);
    }
}
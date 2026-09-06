using Microsoft.Xna.Framework;
using System;
using System.Globalization;
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
    /// Initializes a new instance of the <see cref="Point2"/> structure
    /// from the specified vector.
    /// </summary>
    /// <param name="value">The vector containing the point coordinates.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point2(Vector2 value)
    {
        X = value.X;
        Y = value.Y;
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
    /// Calculates the distance between two points.
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <returns>The distance between the two points.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Distance(Point2 a, Point2 b)
        => Vector2.Distance(a, b);

    /// <summary>
    /// Calculates the squared distance between two points.
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <returns>The squared distance between the two points.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DistanceSquared(Point2 a, Point2 b)
        => Vector2.DistanceSquared(a, b);

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
    public static Point2 Lerp(
        Point2 a,
        Point2 b,
        float amount)
    {
        return new Point2(
            a.X + (b.X - a.X) * amount,
            a.Y + (b.Y - a.Y) * amount);
    }

    /// <summary>
    /// Adds the specified vector to the point.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="vector">The vector to add.</param>
    /// <returns>The translated point.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point2 operator +(
        Point2 point,
        Vector2 vector)
    {
        return new Point2(
            point.X + vector.X,
            point.Y + vector.Y);
    }

    /// <summary>
    /// Adds the specified point to the vector.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <param name="point">The point to add.</param>
    /// <returns>The translated point.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point2 operator +(
        Vector2 vector,
        Point2 point)
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
    public static Point2 operator -(
        Point2 point,
        Vector2 vector)
    {
        return new Point2(
            point.X - vector.X,
            point.Y - vector.Y);
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
    public static Vector2 operator -(
        Point2 a,
        Point2 b)
    {
        return new Vector2(
            a.X - b.X,
            a.Y - b.Y);
    }

    /// <summary>
    /// Multiplies both coordinates of a point by a scalar value.
    /// </summary>
    /// <param name="value">The point to multiply.</param>
    /// <param name="scaleFactor">The scalar multiplier.</param>
    /// <returns>The scaled point.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point2 operator *(
        Point2 value,
        float scaleFactor)
    {
        return new Point2(
            value.X * scaleFactor,
            value.Y * scaleFactor);
    }

    /// <summary>
    /// Multiplies both coordinates of a point by a scalar value.
    /// </summary>
    /// <param name="scaleFactor">The scalar multiplier.</param>
    /// <param name="value">The point to multiply.</param>
    /// <returns>The scaled point.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point2 operator *(
        float scaleFactor,
        Point2 value)
    {
        return value * scaleFactor;
    }

    /// <summary>
    /// Divides both coordinates of a point by a scalar value.
    /// </summary>
    /// <param name="value">The point to divide.</param>
    /// <param name="divisor">The scalar divisor.</param>
    /// <returns>The divided point.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point2 operator /(
        Point2 value,
        float divisor)
    {
        return new Point2(
            value.X / divisor,
            value.Y / divisor);
    }

    /// <summary>
    /// Converts a <see cref="Vector2"/> to a <see cref="Point2"/>.
    /// </summary>
    /// <param name="value">The vector to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Point2(Vector2 value)
        => new Point2(value);

    /// <summary>
    /// Converts a <see cref="Point2"/> to a <see cref="Vector2"/>.
    /// </summary>
    /// <param name="value">The point to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector2(Point2 value)
        => new Vector2(value.X, value.Y);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Point2 other)
        => X == other.X &&
           Y == other.Y;

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => obj is Point2 point &&
           Equals(point);

    /// <inheritdoc/>
    public override int GetHashCode()
        => HashCode.Combine(X, Y);

    /// <summary>
    /// Determines whether two <see cref="Point2"/> instances are equal.
    /// </summary>
    /// <param name="a">The first point to compare.</param>
    /// <param name="b">The second point to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both points are equal;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(
        Point2 a,
        Point2 b)
    {
        return a.Equals(b);
    }

    /// <summary>
    /// Determines whether two <see cref="Point2"/> instances are not equal.
    /// </summary>
    /// <param name="a">The first point to compare.</param>
    /// <param name="b">The second point to compare.</param>
    /// <returns>
    /// <see langword="true"/> if the points are not equal;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(
        Point2 a,
        Point2 b)
    {
        return !a.Equals(b);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0}, {1}",
            X,
            Y);
    }
}
using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Represents a three-dimensional size defined by a width, height, and depth.
/// </summary>
/// <remarks>
/// <see cref="Size3"/> stores all three dimensions as single-precision
/// floating-point values and is suitable for three-dimensional geometry,
/// layout, rendering, and other size-related operations.
/// </remarks>
public readonly struct Size3 : IEquatable<Size3>
{
    private readonly float _width;
    private readonly float _height;
    private readonly float _depth;

    /// <summary>
    /// Represents a size with a width, height, and depth of zero.
    /// </summary>
    public static readonly Size3 Zero = new Size3(0f, 0f, 0f);

    /// <summary>
    /// Represents a size with a width, height, and depth of one.
    /// </summary>
    public static readonly Size3 One = new Size3(1f, 1f, 1f);

    /// <summary>
    /// Initializes a new instance with the same value for width, height,
    /// and depth.
    /// </summary>
    /// <param name="uniform">
    /// The value used for width, height, and depth.
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Size3(float uniform)
    {
        _width = uniform;
        _height = uniform;
        _depth = uniform;
    }

    /// <summary>
    /// Initializes a new instance with the specified width, height,
    /// and depth.
    /// </summary>
    /// <param name="width">The width of the size.</param>
    /// <param name="height">The height of the size.</param>
    /// <param name="depth">The depth of the size.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Size3(float width, float height, float depth)
    {
        _width = width;
        _height = height;
        _depth = depth;
    }

    /// <summary>
    /// Initializes a new instance from a <see cref="Vector3"/>.
    /// </summary>
    /// <param name="vector">
    /// The vector containing the width, height, and depth.
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Size3(Vector3 vector)
    {
        _width = vector.X;
        _height = vector.Y;
        _depth = vector.Z;
    }

    /// <summary>
    /// Initializes a new instance from a <see cref="Point3"/>.
    /// </summary>
    /// <param name="point">
    /// The point containing the width, height, and depth values.
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Size3(Point3 point)
    {
        _width = point.X;
        _height = point.Y;
        _depth = point.Z;
    }

    /// <summary>
    /// Gets the width of the size.
    /// </summary>
    public float Width => _width;

    /// <summary>
    /// Gets the height of the size.
    /// </summary>
    public float Height => _height;

    /// <summary>
    /// Gets the depth of the size.
    /// </summary>
    public float Depth => _depth;

    /// <summary>
    /// Converts this size to a <see cref="Vector3"/>.
    /// </summary>
    /// <returns>
    /// A vector containing the width, height, and depth.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 ToVector3() => new(_width, _height, _depth);

    /// <summary>
    /// Converts this size to a <see cref="Vector2"/>.
    /// </summary>
    /// <returns>
    /// A vector containing the width and height. The depth is omitted.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 ToVector2() => new(_width, _height);

    /// <summary>
    /// Converts this size to a <see cref="Point3"/>.
    /// </summary>
    /// <returns>
    /// A point containing the width, height, and depth values.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point3 ToPoint3()
        => new(_width, _height, _depth);

    /// <summary>
    /// Converts this size to a <see cref="Point2"/>.
    /// </summary>
    /// <returns>
    /// A point containing the width and height values. The depth is omitted.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point2 ToPoint2()
        => new(_width, _height);

    /// <summary>
    /// Creates a new size with the specified width, height, and depth.
    /// </summary>
    /// <param name="width">The new width.</param>
    /// <param name="height">The new height.</param>
    /// <param name="depth">The new depth.</param>
    /// <returns>
    /// A new <see cref="Size3"/> with the specified dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Size3 With(float width, float height, float depth)
        => new(width, height, depth);

    /// <summary>
    /// Creates a new size with the specified width and the current
    /// height and depth.
    /// </summary>
    /// <param name="width">The new width.</param>
    /// <returns>
    /// A new <see cref="Size3"/> with the specified width.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Size3 WithWidth(float width)
        => new(width, _height, _depth);

    /// <summary>
    /// Creates a new size with the current width and depth and the
    /// specified height.
    /// </summary>
    /// <param name="height">The new height.</param>
    /// <returns>
    /// A new <see cref="Size3"/> with the specified height.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Size3 WithHeight(float height)
        => new(_width, height, _depth);

    /// <summary>
    /// Creates a new size with the current width and height and the
    /// specified depth.
    /// </summary>
    /// <param name="depth">The new depth.</param>
    /// <returns>
    /// A new <see cref="Size3"/> with the specified depth.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Size3 WithDepth(float depth)
        => new(_width, _height, depth);

    /// <summary>
    /// Creates a new size by adding the specified width, height,
    /// and depth.
    /// </summary>
    /// <param name="width">The value to add to the width.</param>
    /// <param name="height">The value to add to the height.</param>
    /// <param name="depth">The value to add to the depth.</param>
    /// <returns>
    /// A new <see cref="Size3"/> containing the resulting dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Size3 Add(float width, float height, float depth)
        => new(_width + width, _height + height, _depth + depth);

    /// <summary>
    /// Creates a new size by adding the specified value to the width.
    /// </summary>
    /// <param name="width">The value to add to the width.</param>
    /// <returns>
    /// A new <see cref="Size3"/> containing the resulting dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Size3 AddWidth(float width)
        => new(_width + width, _height, _depth);

    /// <summary>
    /// Creates a new size by adding the specified value to the height.
    /// </summary>
    /// <param name="height">The value to add to the height.</param>
    /// <returns>
    /// A new <see cref="Size3"/> containing the resulting dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Size3 AddHeight(float height)
        => new(_width, _height + height, _depth);

    /// <summary>
    /// Creates a new size by adding the specified value to the depth.
    /// </summary>
    /// <param name="depth">The value to add to the depth.</param>
    /// <returns>
    /// A new <see cref="Size3"/> containing the resulting dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Size3 AddDepth(float depth)
        => new(_width, _height, _depth + depth);

    /// <summary>
    /// Creates a new size by subtracting the specified width, height,
    /// and depth.
    /// </summary>
    /// <param name="width">The value to subtract from the width.</param>
    /// <param name="height">The value to subtract from the height.</param>
    /// <param name="depth">The value to subtract from the depth.</param>
    /// <returns>
    /// A new <see cref="Size3"/> containing the resulting dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Size3 Subtract(float width, float height, float depth)
        => new(_width - width, _height - height, _depth - depth);

    /// <summary>
    /// Subtracts a value from the width.
    /// </summary>
    /// <param name="width">The value to subtract from the width.</param>
    /// <returns>
    /// A new <see cref="Size3"/> containing the resulting dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Size3 SubtractWidth(float width)
        => new(_width - width, _height, _depth);

    /// <summary>
    /// Subtracts a value from the height.
    /// </summary>
    /// <param name="height">The value to subtract from the height.</param>
    /// <returns>
    /// A new <see cref="Size3"/> containing the resulting dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Size3 SubtractHeight(float height)
        => new(_width, _height - height, _depth);

    /// <summary>
    /// Subtracts a value from the depth.
    /// </summary>
    /// <param name="depth">The value to subtract from the depth.</param>
    /// <returns>
    /// A new <see cref="Size3"/> containing the resulting dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Size3 SubtractDepth(float depth)
        => new(_width, _height, _depth - depth);

    /// <summary>
    /// Adds two sizes component-wise.
    /// </summary>
    /// <param name="a">The first size.</param>
    /// <param name="b">The second size.</param>
    /// <returns>The resulting size.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size3 operator +(Size3 a, Size3 b)
        => new(
            a.Width + b.Width,
            a.Height + b.Height,
            a.Depth + b.Depth);

    /// <summary>
    /// Subtracts one size from another component-wise.
    /// </summary>
    /// <param name="a">The size to subtract from.</param>
    /// <param name="b">The size to subtract.</param>
    /// <returns>The resulting size.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size3 operator -(Size3 a, Size3 b)
        => new(
            a.Width - b.Width,
            a.Height - b.Height,
            a.Depth - b.Depth);

    /// <summary>
    /// Multiplies all dimensions by a scalar value.
    /// </summary>
    /// <param name="size">The size to multiply.</param>
    /// <param name="scalar">The scalar value.</param>
    /// <returns>The resulting size.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size3 operator *(Size3 size, float scalar)
        => new(
            size.Width * scalar,
            size.Height * scalar,
            size.Depth * scalar);

    /// <summary>
    /// Multiplies all dimensions by a scalar value.
    /// </summary>
    /// <param name="scalar">The scalar value.</param>
    /// <param name="size">The size to multiply.</param>
    /// <returns>The resulting size.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size3 operator *(float scalar, Size3 size)
        => size * scalar;

    /// <summary>
    /// Divides all dimensions by a scalar value.
    /// </summary>
    /// <param name="size">The size to divide.</param>
    /// <param name="scalar">The scalar value.</param>
    /// <returns>The resulting size.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size3 operator /(Size3 size, float scalar)
        => new(
            size.Width / scalar,
            size.Height / scalar,
            size.Depth / scalar);

    /// <summary>
    /// Determines whether this size is equal to another size.
    /// </summary>
    /// <param name="other">The size to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if all three dimensions are equal;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Size3 other)
        => _width == other._width
        && _height == other._height
        && _depth == other._depth;

    /// <summary>
    /// Determines whether this size is equal to the specified object.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="obj"/> is an equal
    /// <see cref="Size3"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
        => obj is Size3 s && Equals(s);

    /// <summary>
    /// Returns the hash code for this size.
    /// </summary>
    /// <returns>
    /// A hash code based on the width, height, and depth.
    /// </returns>
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + _width.GetHashCode();
            hash = hash * 31 + _height.GetHashCode();
            hash = hash * 31 + _depth.GetHashCode();
            return hash;
        }
    }

    /// <summary>
    /// Determines whether two sizes are equal.
    /// </summary>
    /// <param name="a">The first size.</param>
    /// <param name="b">The second size.</param>
    /// <returns>
    /// <see langword="true"/> if all three dimensions are equal;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in Size3 a, in Size3 b)
        => a.Equals(b);

    /// <summary>
    /// Determines whether two sizes are not equal.
    /// </summary>
    /// <param name="a">The first size.</param>
    /// <param name="b">The second size.</param>
    /// <returns>
    /// <see langword="true"/> if at least one dimension differs;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in Size3 a, in Size3 b)
        => !a.Equals(b);

    /// <summary>
    /// Parses a string representation of a size.
    /// </summary>
    /// <param name="s">
    /// The string containing the width, height, and depth.
    /// </param>
    /// <returns>The parsed <see cref="Size3"/>.</returns>
    /// <exception cref="FormatException">
    /// Thrown when the string does not contain exactly three valid numeric
    /// values.
    /// </exception>
    /// <remarks>
    /// The values may be separated by commas or spaces and are interpreted
    /// using <see cref="CultureInfo.InvariantCulture"/>.
    /// </remarks>
    public static Size3 Parse(string s)
    {
        if (TryParse(s, out var result))
            return result;

        throw new FormatException(
            $"Invalid Size3 format: '{s}'. Expected 3 numeric values separated by ',' or ' '.");
    }

    /// <summary>
    /// Attempts to parse a string representation of a size.
    /// </summary>
    /// <param name="s">
    /// The string containing the width, height, and depth.
    /// </param>
    /// <param name="result">
    /// When this method returns <see langword="true"/>, contains the parsed
    /// size; otherwise, contains <see cref="Zero"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if parsing was successful; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The string must contain exactly three valid numeric values separated
    /// by commas or spaces. Numeric values are interpreted using
    /// <see cref="CultureInfo.InvariantCulture"/>.
    /// </remarks>
    public static bool TryParse(string? s, out Size3 result)
    {
        result = Zero;

        if (string.IsNullOrWhiteSpace(s))
            return false;

        var parts = s.Split(
            new[] { ',', ' ' },
            StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 3)
            return false;

        try
        {
            float w = float.Parse(
                parts[0],
                CultureInfo.InvariantCulture);

            float h = float.Parse(
                parts[1],
                CultureInfo.InvariantCulture);

            float d = float.Parse(
                parts[2],
                CultureInfo.InvariantCulture);

            result = new Size3(w, h, d);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Returns a string representation of this size.
    /// </summary>
    /// <returns>
    /// A string containing the width, height, and depth separated by commas.
    /// </returns>
    /// <remarks>
    /// Numeric values are formatted using
    /// <see cref="CultureInfo.InvariantCulture"/>.
    /// </remarks>
    public override string ToString()
        => string.Format(
            CultureInfo.InvariantCulture,
            "{0}, {1}, {2}",
            Width,
            Height,
            Depth);
}
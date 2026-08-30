using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Represents a two-dimensional size defined by a width and height.
/// </summary>
/// <remarks>
/// <see cref="Size2"/> stores both dimensions as single-precision
/// floating-point values and is suitable for two-dimensional geometry,
/// layout, rendering, and other size-related operations.
/// </remarks>
public readonly struct Size2 : IEquatable<Size2>
{
    private readonly float _width;
    private readonly float _height;

    /// <summary>
    /// Represents a size with a width and height of zero.
    /// </summary>
    public static readonly Size2 Zero = new Size2(0f, 0f);

    /// <summary>
    /// Represents a size with a width and height of one.
    /// </summary>
    public static readonly Size2 One = new Size2(1f, 1f);

    /// <summary>
    /// Initializes a new instance with the same value for width and height.
    /// </summary>
    /// <param name="uniform">The value used for both width and height.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Size2(float uniform)
    {
        _width = uniform;
        _height = uniform;
    }

    /// <summary>
    /// Initializes a new instance with the specified width and height.
    /// </summary>
    /// <param name="width">The width of the size.</param>
    /// <param name="height">The height of the size.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Size2(float width, float height)
    {
        _width = width;
        _height = height;
    }

    /// <summary>
    /// Initializes a new instance from a <see cref="Vector2"/>.
    /// </summary>
    /// <param name="vector">The vector containing the width and height.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Size2(Vector2 vector)
    {
        _width = vector.X;
        _height = vector.Y;
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
    /// Converts this size to a <see cref="Vector2"/>.
    /// </summary>
    /// <returns>
    /// A vector containing the width and height.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 ToVector2() => new(_width, _height);

    /// <summary>
    /// Converts this size to a <see cref="Vector3"/>.
    /// </summary>
    /// <returns>
    /// A vector containing the width, height, and zero for the Z component.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 ToVector3() => new(_width, _height, 0f);

    /// <summary>
    /// Creates a new size with the specified width and height.
    /// </summary>
    /// <param name="width">The new width.</param>
    /// <param name="height">The new height.</param>
    /// <returns>
    /// A new <see cref="Size2"/> with the specified dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Size2 With(float width, float height) => new(width, height);

    /// <summary>
    /// Creates a new size with the specified width and the current height.
    /// </summary>
    /// <param name="width">The new width.</param>
    /// <returns>
    /// A new <see cref="Size2"/> with the specified width and current height.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Size2 WithWidth(float width) => new(width, _height);

    /// <summary>
    /// Creates a new size with the current width and specified height.
    /// </summary>
    /// <param name="height">The new height.</param>
    /// <returns>
    /// A new <see cref="Size2"/> with the current width and specified height.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Size2 WithHeight(float height) => new(_width, height);

    /// <summary>
    /// Creates a new size by adding the specified width and height.
    /// </summary>
    /// <param name="width">The value to add to the width.</param>
    /// <param name="height">The value to add to the height.</param>
    /// <returns>
    /// A new <see cref="Size2"/> containing the resulting dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Size2 Add(float width, float height)
        => new(_width + width, _height + height);

    /// <summary>
    /// Creates a new size by adding the specified value to the width.
    /// </summary>
    /// <param name="width">The value to add to the width.</param>
    /// <returns>
    /// A new <see cref="Size2"/> containing the resulting dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Size2 AddWidth(float width)
        => new(_width + width, _height);

    /// <summary>
    /// Creates a new size by adding the specified value to the height.
    /// </summary>
    /// <param name="height">The value to add to the height.</param>
    /// <returns>
    /// A new <see cref="Size2"/> containing the resulting dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Size2 AddHeight(float height)
        => new(_width, _height + height);

    /// <summary>
    /// Creates a new size by subtracting the specified width and height.
    /// </summary>
    /// <param name="width">The value to subtract from the width.</param>
    /// <param name="height">The value to subtract from the height.</param>
    /// <returns>
    /// A new <see cref="Size2"/> containing the resulting dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Size2 Subtract(float width, float height)
        => new(_width - width, _height - height);

    /// <summary>
    /// Adds two sizes component-wise.
    /// </summary>
    /// <param name="a">The first size.</param>
    /// <param name="b">The second size.</param>
    /// <returns>The resulting size.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size2 operator +(Size2 a, Size2 b)
        => new(a.Width + b.Width, a.Height + b.Height);

    /// <summary>
    /// Subtracts one size from another component-wise.
    /// </summary>
    /// <param name="a">The size to subtract from.</param>
    /// <param name="b">The size to subtract.</param>
    /// <returns>The resulting size.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size2 operator -(Size2 a, Size2 b)
        => new(a.Width - b.Width, a.Height - b.Height);

    /// <summary>
    /// Multiplies both dimensions by a scalar value.
    /// </summary>
    /// <param name="size">The size to multiply.</param>
    /// <param name="scalar">The scalar value.</param>
    /// <returns>The resulting size.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size2 operator *(Size2 size, float scalar)
        => new(size.Width * scalar, size.Height * scalar);

    /// <summary>
    /// Multiplies both dimensions by a scalar value.
    /// </summary>
    /// <param name="scalar">The scalar value.</param>
    /// <param name="size">The size to multiply.</param>
    /// <returns>The resulting size.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size2 operator *(float scalar, Size2 size)
        => size * scalar;

    /// <summary>
    /// Divides both dimensions by a scalar value.
    /// </summary>
    /// <param name="size">The size to divide.</param>
    /// <param name="scalar">The scalar value.</param>
    /// <returns>The resulting size.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size2 operator /(Size2 size, float scalar)
        => new(size.Width / scalar, size.Height / scalar);

    /// <summary>
    /// Determines whether this size is equal to another size.
    /// </summary>
    /// <param name="other">The size to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if both dimensions are equal;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Size2 other)
        => _width == other._width && _height == other._height;

    /// <summary>
    /// Determines whether this size is equal to the specified object.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="obj"/> is an equal
    /// <see cref="Size2"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
        => obj is Size2 s && Equals(s);

    /// <summary>
    /// Returns the hash code for this size.
    /// </summary>
    /// <returns>
    /// A hash code based on the width and height.
    /// </returns>
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + _width.GetHashCode();
            hash = hash * 31 + _height.GetHashCode();
            return hash;
        }
    }

    /// <summary>
    /// Determines whether two sizes are equal.
    /// </summary>
    /// <param name="a">The first size.</param>
    /// <param name="b">The second size.</param>
    /// <returns>
    /// <see langword="true"/> if both dimensions are equal;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in Size2 a, in Size2 b)
        => a.Equals(b);

    /// <summary>
    /// Determines whether two sizes are not equal.
    /// </summary>
    /// <param name="a">The first size.</param>
    /// <param name="b">The second size.</param>
    /// <returns>
    /// <see langword="true"/> if either dimension differs;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in Size2 a, in Size2 b)
        => !a.Equals(b);

    /// <summary>
    /// Parses a string representation of a size.
    /// </summary>
    /// <param name="s">
    /// The string containing the width and height.
    /// </param>
    /// <returns>The parsed <see cref="Size2"/>.</returns>
    /// <exception cref="FormatException">
    /// Thrown when the string does not contain exactly two valid numeric
    /// values.
    /// </exception>
    /// <remarks>
    /// The values may be separated by commas or spaces and are interpreted
    /// using <see cref="CultureInfo.InvariantCulture"/>.
    /// </remarks>
    public static Size2 Parse(string s)
    {
        if (TryParse(s, out var result))
            return result;

        throw new FormatException(
            $"Invalid Size format: '{s}'. Expected 2 numeric values separated by ',' or ' '.");
    }

    /// <summary>
    /// Attempts to parse a string representation of a size.
    /// </summary>
    /// <param name="s">
    /// The string containing the width and height.
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
    /// The string must contain exactly two valid numeric values separated
    /// by commas or spaces. Numeric values are interpreted using
    /// <see cref="CultureInfo.InvariantCulture"/>.
    /// </remarks>
    public static bool TryParse(string? s, out Size2 result)
    {
        result = Zero;

        if (string.IsNullOrWhiteSpace(s))
            return false;

        var parts = s.Split(
            new[] { ',', ' ' },
            StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
            return false;

        try
        {
            float w = float.Parse(
                parts[0],
                CultureInfo.InvariantCulture);

            float h = float.Parse(
                parts[1],
                CultureInfo.InvariantCulture);

            result = new Size2(w, h);
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
    /// A string containing the width and height separated by a comma.
    /// </returns>
    /// <remarks>
    /// Numeric values are formatted using
    /// <see cref="CultureInfo.InvariantCulture"/>.
    /// </remarks>
    public override string ToString()
        => string.Format(
            CultureInfo.InvariantCulture,
            "{0}, {1}",
            Width,
            Height);
}
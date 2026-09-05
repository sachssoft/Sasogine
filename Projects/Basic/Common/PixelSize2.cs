using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Represents a two-dimensional pixel size defined by an integer width and height.
/// </summary>
/// <remarks>
/// <see cref="PixelSize2"/> is intended for discrete dimensions measured in pixels,
/// such as texture sizes, render target dimensions, viewport sizes, and other
/// pixel-based resources.
/// </remarks>
public readonly struct PixelSize2 : IEquatable<PixelSize2>
{
    private readonly int _width;
    private readonly int _height;

    /// <summary>
    /// Represents a pixel size with a width and height of zero.
    /// </summary>
    public static readonly PixelSize2 Zero = new PixelSize2(0, 0);

    /// <summary>
    /// Represents a pixel size with a width and height of one.
    /// </summary>
    public static readonly PixelSize2 One = new PixelSize2(1, 1);

    /// <summary>
    /// Initializes a new instance with the same value for width and height.
    /// </summary>
    /// <param name="uniform">
    /// The value used for both width and height.
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelSize2(int uniform)
    {
        _width = uniform;
        _height = uniform;
    }

    /// <summary>
    /// Initializes a new instance with the specified width and height.
    /// </summary>
    /// <param name="width">
    /// The width in pixels.
    /// </param>
    /// <param name="height">
    /// The height in pixels.
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelSize2(int width, int height)
    {
        _width = width;
        _height = height;
    }

    /// <summary>
    /// Initializes a new instance from a <see cref="PixelPoint2"/>.
    /// </summary>
    /// <param name="point">
    /// The pixel point containing the width and height values.
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelSize2(PixelPoint2 point)
    {
        _width = point.X;
        _height = point.Y;
    }

    /// <summary>
    /// Gets the width in pixels.
    /// </summary>
    public int Width => _width;

    /// <summary>
    /// Gets the height in pixels.
    /// </summary>
    public int Height => _height;

    /// <summary>
    /// Converts this pixel size to a <see cref="PixelPoint2"/>.
    /// </summary>
    /// <returns>
    /// A pixel point containing the width and height values.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelPoint2 ToPixelPoint2() => new(_width, _height);

    /// <summary>
    /// Converts this pixel size to a <see cref="Vector2"/>.
    /// </summary>
    /// <returns>
    /// A vector containing the width and height.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 ToVector2() => new(_width, _height);

    /// <summary>
    /// Converts this pixel size to a <see cref="Vector3"/>.
    /// </summary>
    /// <returns>
    /// A vector containing the width, height, and zero for the Z component.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 ToVector3() => new(_width, _height, 0f);

    /// <summary>
    /// Creates a new pixel size with the specified width and height.
    /// </summary>
    /// <param name="width">
    /// The new width in pixels.
    /// </param>
    /// <param name="height">
    /// The new height in pixels.
    /// </param>
    /// <returns>
    /// A new <see cref="PixelSize2"/> with the specified dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelSize2 With(int width, int height) => new(width, height);

    /// <summary>
    /// Creates a new pixel size with the specified width and the current height.
    /// </summary>
    /// <param name="width">
    /// The new width in pixels.
    /// </param>
    /// <returns>
    /// A new <see cref="PixelSize2"/> with the specified width.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelSize2 WithWidth(int width) => new(width, _height);

    /// <summary>
    /// Creates a new pixel size with the current width and the specified height.
    /// </summary>
    /// <param name="height">
    /// The new height in pixels.
    /// </param>
    /// <returns>
    /// A new <see cref="PixelSize2"/> with the specified height.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelSize2 WithHeight(int height) => new(_width, height);

    /// <summary>
    /// Creates a new pixel size by adding the specified width and height.
    /// </summary>
    /// <param name="width">
    /// The value to add to the width in pixels.
    /// </param>
    /// <param name="height">
    /// The value to add to the height in pixels.
    /// </param>
    /// <returns>
    /// A new <see cref="PixelSize2"/> containing the resulting dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelSize2 Add(int width, int height)
        => new(_width + width, _height + height);

    /// <summary>
    /// Creates a new pixel size by adding the specified value to the width.
    /// </summary>
    /// <param name="width">
    /// The value to add to the width in pixels.
    /// </param>
    /// <returns>
    /// A new <see cref="PixelSize2"/> containing the resulting dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelSize2 AddWidth(int width)
        => new(_width + width, _height);

    /// <summary>
    /// Creates a new pixel size by adding the specified value to the height.
    /// </summary>
    /// <param name="height">
    /// The value to add to the height in pixels.
    /// </param>
    /// <returns>
    /// A new <see cref="PixelSize2"/> containing the resulting dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelSize2 AddHeight(int height)
        => new(_width, _height + height);

    /// <summary>
    /// Creates a new pixel size by subtracting the specified width and height.
    /// </summary>
    /// <param name="width">
    /// The value to subtract from the width in pixels.
    /// </param>
    /// <param name="height">
    /// The value to subtract from the height in pixels.
    /// </param>
    /// <returns>
    /// A new <see cref="PixelSize2"/> containing the resulting dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelSize2 Subtract(int width, int height)
        => new(_width - width, _height - height);

    /// <summary>
    /// Adds two pixel sizes component-wise.
    /// </summary>
    /// <param name="a">The first pixel size.</param>
    /// <param name="b">The second pixel size.</param>
    /// <returns>
    /// A new <see cref="PixelSize2"/> containing the summed dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PixelSize2 operator +(PixelSize2 a, PixelSize2 b)
        => new(a.Width + b.Width, a.Height + b.Height);

    /// <summary>
    /// Subtracts one pixel size from another component-wise.
    /// </summary>
    /// <param name="a">The pixel size to subtract from.</param>
    /// <param name="b">The pixel size to subtract.</param>
    /// <returns>
    /// A new <see cref="PixelSize2"/> containing the resulting dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PixelSize2 operator -(PixelSize2 a, PixelSize2 b)
        => new(a.Width - b.Width, a.Height - b.Height);

    /// <summary>
    /// Multiplies both dimensions by an integer scalar.
    /// </summary>
    /// <param name="size">The pixel size to multiply.</param>
    /// <param name="scalar">The scalar value.</param>
    /// <returns>
    /// A new <see cref="PixelSize2"/> containing the scaled dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PixelSize2 operator *(PixelSize2 size, int scalar)
        => new(size.Width * scalar, size.Height * scalar);

    /// <summary>
    /// Multiplies both dimensions by an integer scalar.
    /// </summary>
    /// <param name="scalar">The scalar value.</param>
    /// <param name="size">The pixel size to multiply.</param>
    /// <returns>
    /// A new <see cref="PixelSize2"/> containing the scaled dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PixelSize2 operator *(int scalar, PixelSize2 size)
        => size * scalar;

    /// <summary>
    /// Divides both dimensions by an integer scalar.
    /// </summary>
    /// <param name="size">The pixel size to divide.</param>
    /// <param name="scalar">The scalar value.</param>
    /// <returns>
    /// A new <see cref="PixelSize2"/> containing the divided dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PixelSize2 operator /(PixelSize2 size, int scalar)
        => new(size.Width / scalar, size.Height / scalar);

    /// <summary>
    /// Determines whether this pixel size is equal to another pixel size.
    /// </summary>
    /// <param name="other">
    /// The pixel size to compare with this instance.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if both dimensions are equal;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(PixelSize2 other)
        => _width == other._width && _height == other._height;

    /// <summary>
    /// Determines whether this pixel size is equal to the specified object.
    /// </summary>
    /// <param name="obj">
    /// The object to compare with this instance.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="obj"/> is an equal
    /// <see cref="PixelSize2"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
        => obj is PixelSize2 s && Equals(s);

    /// <summary>
    /// Returns the hash code for this pixel size.
    /// </summary>
    /// <returns>
    /// A hash code based on the width and height.
    /// </returns>
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + _width;
            hash = hash * 31 + _height;
            return hash;
        }
    }

    /// <summary>
    /// Determines whether two pixel sizes are equal.
    /// </summary>
    /// <param name="a">The first pixel size.</param>
    /// <param name="b">The second pixel size.</param>
    /// <returns>
    /// <see langword="true"/> if both dimensions are equal;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in PixelSize2 a, in PixelSize2 b)
        => a.Equals(b);

    /// <summary>
    /// Determines whether two pixel sizes are not equal.
    /// </summary>
    /// <param name="a">The first pixel size.</param>
    /// <param name="b">The second pixel size.</param>
    /// <returns>
    /// <see langword="true"/> if either dimension differs;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in PixelSize2 a, in PixelSize2 b)
        => !a.Equals(b);

    /// <summary>
    /// Parses a string representation of a pixel size.
    /// </summary>
    /// <param name="s">
    /// The string containing the width and height in pixels.
    /// </param>
    /// <returns>
    /// The parsed <see cref="PixelSize2"/>.
    /// </returns>
    /// <exception cref="FormatException">
    /// Thrown when the string does not contain exactly two valid integer
    /// values.
    /// </exception>
    /// <remarks>
    /// The values may be separated by commas or spaces and are interpreted
    /// using <see cref="CultureInfo.InvariantCulture"/>.
    /// </remarks>
    public static PixelSize2 Parse(string s)
    {
        if (TryParse(s, out var result))
            return result;

        throw new FormatException(
            $"Invalid PixelSize2 format: '{s}'. Expected 2 integer values separated by ',' or ' '.");
    }

    /// <summary>
    /// Attempts to parse a string representation of a pixel size.
    /// </summary>
    /// <param name="s">
    /// The string containing the width and height in pixels.
    /// </param>
    /// <param name="result">
    /// When this method returns <see langword="true"/>, contains the parsed
    /// pixel size; otherwise, contains <see cref="Zero"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if parsing was successful; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The string must contain exactly two valid integer values separated
    /// by commas or spaces. Numeric values are interpreted using
    /// <see cref="CultureInfo.InvariantCulture"/>.
    /// </remarks>
    public static bool TryParse(string? s, out PixelSize2 result)
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
            int w = int.Parse(
                parts[0],
                CultureInfo.InvariantCulture);

            int h = int.Parse(
                parts[1],
                CultureInfo.InvariantCulture);

            result = new PixelSize2(w, h);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Returns a string representation of this pixel size.
    /// </summary>
    /// <returns>
    /// A string containing the width and height separated by a comma.
    /// </returns>
    /// <remarks>
    /// The values represent integer pixel dimensions.
    /// </remarks>
    public override string ToString()
        => string.Format(
            CultureInfo.InvariantCulture,
            "{0}, {1}",
            Width,
            Height);
}
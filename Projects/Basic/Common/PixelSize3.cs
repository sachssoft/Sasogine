using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Represents a three-dimensional pixel size defined by an integer width,
/// height, and depth.
/// </summary>
/// <remarks>
/// <see cref="PixelSize3"/> is intended for discrete three-dimensional
/// dimensions measured in pixels or integer units, such as voxel dimensions,
/// texture volumes, render resources, and other pixel-based 3D data.
/// </remarks>
public readonly struct PixelSize3 : IEquatable<PixelSize3>
{
    private readonly int _width;
    private readonly int _height;
    private readonly int _depth;

    /// <summary>
    /// Represents a pixel size with a width, height, and depth of zero.
    /// </summary>
    public static readonly PixelSize3 Zero = new PixelSize3(0, 0, 0);

    /// <summary>
    /// Represents a pixel size with a width, height, and depth of one.
    /// </summary>
    public static readonly PixelSize3 One = new PixelSize3(1, 1, 1);

    /// <summary>
    /// Initializes a new instance with the same value for width, height,
    /// and depth.
    /// </summary>
    /// <param name="uniform">
    /// The value used for width, height, and depth.
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelSize3(int uniform)
    {
        _width = uniform;
        _height = uniform;
        _depth = uniform;
    }

    /// <summary>
    /// Initializes a new instance with the specified width, height,
    /// and depth.
    /// </summary>
    /// <param name="width">
    /// The width in pixels.
    /// </param>
    /// <param name="height">
    /// The height in pixels.
    /// </param>
    /// <param name="depth">
    /// The depth in pixels.
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelSize3(int width, int height, int depth)
    {
        _width = width;
        _height = height;
        _depth = depth;
    }

    /// <summary>
    /// Initializes a new instance from a <see cref="Vector3"/>.
    /// </summary>
    /// <param name="size">
    /// The vector containing the width, height, and depth.
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelSize3(Vector3 size)
    {
        _width = (int)size.X;
        _height = (int)size.Y;
        _depth = (int)size.Z;
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
    /// Gets the depth in pixels.
    /// </summary>
    public int Depth => _depth;

    /// <summary>
    /// Converts this pixel size to a <see cref="Vector3"/>.
    /// </summary>
    /// <returns>
    /// A vector containing the width, height, and depth.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 ToVector3()
        => new(_width, _height, _depth);

    /// <summary>
    /// Creates a new pixel size with the specified width, height,
    /// and depth.
    /// </summary>
    /// <param name="width">
    /// The new width in pixels.
    /// </param>
    /// <param name="height">
    /// The new height in pixels.
    /// </param>
    /// <param name="depth">
    /// The new depth in pixels.
    /// </param>
    /// <returns>
    /// A new <see cref="PixelSize3"/> with the specified dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelSize3 With(int width, int height, int depth)
        => new(width, height, depth);

    /// <summary>
    /// Creates a new pixel size with the specified width and the current
    /// height and depth.
    /// </summary>
    /// <param name="width">
    /// The new width in pixels.
    /// </param>
    /// <returns>
    /// A new <see cref="PixelSize3"/> with the specified width.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelSize3 WithWidth(int width)
        => new(width, _height, _depth);

    /// <summary>
    /// Creates a new pixel size with the current width and depth and the
    /// specified height.
    /// </summary>
    /// <param name="height">
    /// The new height in pixels.
    /// </param>
    /// <returns>
    /// A new <see cref="PixelSize3"/> with the specified height.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelSize3 WithHeight(int height)
        => new(_width, height, _depth);

    /// <summary>
    /// Creates a new pixel size with the current width and height and the
    /// specified depth.
    /// </summary>
    /// <param name="depth">
    /// The new depth in pixels.
    /// </param>
    /// <returns>
    /// A new <see cref="PixelSize3"/> with the specified depth.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelSize3 WithDepth(int depth)
        => new(_width, _height, depth);

    /// <summary>
    /// Creates a new pixel size by adding the specified width, height,
    /// and depth.
    /// </summary>
    /// <param name="width">
    /// The value to add to the width in pixels.
    /// </param>
    /// <param name="height">
    /// The value to add to the height in pixels.
    /// </param>
    /// <param name="depth">
    /// The value to add to the depth in pixels.
    /// </param>
    /// <returns>
    /// A new <see cref="PixelSize3"/> containing the resulting dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelSize3 Add(int width, int height, int depth)
        => new(_width + width, _height + height, _depth + depth);

    /// <summary>
    /// Creates a new pixel size by adding the specified value to the width.
    /// </summary>
    /// <param name="width">
    /// The value to add to the width in pixels.
    /// </param>
    /// <returns>
    /// A new <see cref="PixelSize3"/> containing the resulting dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelSize3 AddWidth(int width)
        => new(_width + width, _height, _depth);

    /// <summary>
    /// Creates a new pixel size by adding the specified value to the height.
    /// </summary>
    /// <param name="height">
    /// The value to add to the height in pixels.
    /// </param>
    /// <returns>
    /// A new <see cref="PixelSize3"/> containing the resulting dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelSize3 AddHeight(int height)
        => new(_width, _height + height, _depth);

    /// <summary>
    /// Creates a new pixel size by adding the specified value to the depth.
    /// </summary>
    /// <param name="depth">
    /// The value to add to the depth in pixels.
    /// </param>
    /// <returns>
    /// A new <see cref="PixelSize3"/> containing the resulting dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelSize3 AddDepth(int depth)
        => new(_width, _height, _depth + depth);

    /// <summary>
    /// Creates a new pixel size by subtracting the specified width, height,
    /// and depth.
    /// </summary>
    /// <param name="width">
    /// The value to subtract from the width in pixels.
    /// </param>
    /// <param name="height">
    /// The value to subtract from the height in pixels.
    /// </param>
    /// <param name="depth">
    /// The value to subtract from the depth in pixels.
    /// </param>
    /// <returns>
    /// A new <see cref="PixelSize3"/> containing the resulting dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelSize3 Subtract(int width, int height, int depth)
        => new(_width - width, _height - height, _depth - depth);

    /// <summary>
    /// Creates a new pixel size by subtracting the specified value from
    /// the width.
    /// </summary>
    /// <param name="width">
    /// The value to subtract from the width in pixels.
    /// </param>
    /// <returns>
    /// A new <see cref="PixelSize3"/> containing the resulting dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelSize3 SubtractWidth(int width)
        => new(_width - width, _height, _depth);

    /// <summary>
    /// Creates a new pixel size by subtracting the specified value from
    /// the height.
    /// </summary>
    /// <param name="height">
    /// The value to subtract from the height in pixels.
    /// </param>
    /// <returns>
    /// A new <see cref="PixelSize3"/> containing the resulting dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelSize3 SubtractHeight(int height)
        => new(_width, _height - height, _depth);

    /// <summary>
    /// Creates a new pixel size by subtracting the specified value from
    /// the depth.
    /// </summary>
    /// <param name="depth">
    /// The value to subtract from the depth in pixels.
    /// </param>
    /// <returns>
    /// A new <see cref="PixelSize3"/> containing the resulting dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelSize3 SubtractDepth(int depth)
        => new(_width, _height, _depth - depth);

    /// <summary>
    /// Adds two pixel sizes component-wise.
    /// </summary>
    /// <param name="a">The first pixel size.</param>
    /// <param name="b">The second pixel size.</param>
    /// <returns>
    /// A new <see cref="PixelSize3"/> containing the summed dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PixelSize3 operator +(PixelSize3 a, PixelSize3 b)
        => new(
            a.Width + b.Width,
            a.Height + b.Height,
            a.Depth + b.Depth);

    /// <summary>
    /// Subtracts one pixel size from another component-wise.
    /// </summary>
    /// <param name="a">The pixel size to subtract from.</param>
    /// <param name="b">The pixel size to subtract.</param>
    /// <returns>
    /// A new <see cref="PixelSize3"/> containing the resulting dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PixelSize3 operator -(PixelSize3 a, PixelSize3 b)
        => new(
            a.Width - b.Width,
            a.Height - b.Height,
            a.Depth - b.Depth);

    /// <summary>
    /// Multiplies all dimensions by an integer scalar.
    /// </summary>
    /// <param name="size">The pixel size to multiply.</param>
    /// <param name="scalar">The scalar value.</param>
    /// <returns>
    /// A new <see cref="PixelSize3"/> containing the scaled dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PixelSize3 operator *(PixelSize3 size, int scalar)
        => new(
            size.Width * scalar,
            size.Height * scalar,
            size.Depth * scalar);

    /// <summary>
    /// Multiplies all dimensions by an integer scalar.
    /// </summary>
    /// <param name="scalar">The scalar value.</param>
    /// <param name="size">The pixel size to multiply.</param>
    /// <returns>
    /// A new <see cref="PixelSize3"/> containing the scaled dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PixelSize3 operator *(int scalar, PixelSize3 size)
        => size * scalar;

    /// <summary>
    /// Divides all dimensions by an integer scalar.
    /// </summary>
    /// <param name="size">The pixel size to divide.</param>
    /// <param name="scalar">The scalar value.</param>
    /// <returns>
    /// A new <see cref="PixelSize3"/> containing the divided dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PixelSize3 operator /(PixelSize3 size, int scalar)
        => new(
            size.Width / scalar,
            size.Height / scalar,
            size.Depth / scalar);

    /// <summary>
    /// Determines whether this pixel size is equal to another pixel size.
    /// </summary>
    /// <param name="other">
    /// The pixel size to compare with this instance.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if all three dimensions are equal;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(PixelSize3 other)
        => _width == other._width
        && _height == other._height
        && _depth == other._depth;

    /// <summary>
    /// Determines whether this pixel size is equal to the specified object.
    /// </summary>
    /// <param name="obj">
    /// The object to compare with this instance.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="obj"/> is an equal
    /// <see cref="PixelSize3"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
        => obj is PixelSize3 s && Equals(s);

    /// <summary>
    /// Returns the hash code for this pixel size.
    /// </summary>
    /// <returns>
    /// A hash code based on the width, height, and depth.
    /// </returns>
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + _width;
            hash = hash * 31 + _height;
            hash = hash * 31 + _depth;
            return hash;
        }
    }

    /// <summary>
    /// Determines whether two pixel sizes are equal.
    /// </summary>
    /// <param name="a">The first pixel size.</param>
    /// <param name="b">The second pixel size.</param>
    /// <returns>
    /// <see langword="true"/> if all three dimensions are equal;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in PixelSize3 a, in PixelSize3 b)
        => a.Equals(b);

    /// <summary>
    /// Determines whether two pixel sizes are not equal.
    /// </summary>
    /// <param name="a">The first pixel size.</param>
    /// <param name="b">The second pixel size.</param>
    /// <returns>
    /// <see langword="true"/> if at least one dimension differs;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in PixelSize3 a, in PixelSize3 b)
        => !a.Equals(b);

    /// <summary>
    /// Parses a string representation of a three-dimensional pixel size.
    /// </summary>
    /// <param name="s">
    /// The string containing the width, height, and depth.
    /// </param>
    /// <returns>
    /// The parsed <see cref="PixelSize3"/>.
    /// </returns>
    /// <exception cref="FormatException">
    /// Thrown when the string does not contain exactly three valid integer
    /// values.
    /// </exception>
    /// <remarks>
    /// The values may be separated by commas or spaces.
    /// </remarks>
    public static PixelSize3 Parse(string s)
    {
        if (TryParse(s, out var result))
            return result;

        throw new FormatException(
            $"Invalid PixelSize3 format: '{s}'. Expected 3 integer values separated by ',' or ' '.");
    }

    /// <summary>
    /// Attempts to parse a string representation of a three-dimensional
    /// pixel size.
    /// </summary>
    /// <param name="s">
    /// The string containing the width, height, and depth.
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
    /// The string must contain exactly three valid integer values separated
    /// by commas or spaces. Numeric values are interpreted using
    /// <see cref="CultureInfo.InvariantCulture"/>.
    /// </remarks>
    public static bool TryParse(string? s, out PixelSize3 result)
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
            int w = int.Parse(parts[0], CultureInfo.InvariantCulture);
            int h = int.Parse(parts[1], CultureInfo.InvariantCulture);
            int d = int.Parse(parts[2], CultureInfo.InvariantCulture);

            result = new PixelSize3(w, h, d);
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
    /// A string containing the width, height, and depth separated by commas.
    /// </returns>
    public override string ToString()
        => string.Format(
            CultureInfo.InvariantCulture,
            "{0}, {1}, {2}",
            Width,
            Height,
            Depth);
}
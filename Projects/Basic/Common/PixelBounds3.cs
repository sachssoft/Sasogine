using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Represents a three-dimensional pixel-aligned bounding volume defined by
/// integer coordinates and dimensions.
/// </summary>
/// <remarks>
/// <see cref="PixelBounds3"/> stores its boundaries internally as minimum and
/// maximum coordinates. The width, height, and depth are derived from these
/// boundaries.
/// </remarks>
public readonly struct PixelBounds3 : IEquatable<PixelBounds3>
{
    private readonly int _left;
    private readonly int _top;
    private readonly int _front;
    private readonly int _right;
    private readonly int _bottom;
    private readonly int _back;

    /// <summary>
    /// Represents pixel bounds with all coordinates set to zero.
    /// </summary>
    public static readonly PixelBounds3 Zero =
        new PixelBounds3(0, 0, 0, 0, 0, 0);

    /// <summary>
    /// Initializes a new instance from a position and a pixel size.
    /// </summary>
    /// <param name="x">The X coordinate of the left edge.</param>
    /// <param name="y">The Y coordinate of the top edge.</param>
    /// <param name="z">The Z coordinate of the front edge.</param>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    /// <param name="depth">The depth in pixels.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelBounds3(
        int x,
        int y,
        int z,
        int width,
        int height,
        int depth)
    {
        _left = x;
        _top = y;
        _front = z;
        _right = x + width;
        _bottom = y + height;
        _back = z + depth;
    }

    /// <summary>
    /// Initializes a new instance from a pixel position and pixel size.
    /// </summary>
    /// <param name="position">The position of the left-top-front corner.</param>
    /// <param name="size">The width, height, and depth in pixels.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelBounds3(
        PixelPoint3 position,
        PixelSize3 size)
    {
        _left = position.X;
        _top = position.Y;
        _front = position.Z;
        _right = position.X + size.Width;
        _bottom = position.Y + size.Height;
        _back = position.Z + size.Depth;
    }


    /// <summary>
    /// Initializes a new instance from a three-dimensional position and size.
    /// </summary>
    /// <param name="position">The position of the left-top-front corner.</param>
    /// <param name="size">The width, height, and depth in pixels.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelBounds3(Vector3 position, Vector3 size)
    {
        _left = (int)position.X;
        _top = (int)position.Y;
        _front = (int)position.Z;
        _right = (int)(position.X + size.X);
        _bottom = (int)(position.Y + size.Y);
        _back = (int)(position.Z + size.Z);
    }

    /// <summary>
    /// Gets the X coordinate of the left edge.
    /// </summary>
    public int X => _left;

    /// <summary>
    /// Gets the Y coordinate of the top edge.
    /// </summary>
    public int Y => _top;

    /// <summary>
    /// Gets the Z coordinate of the front edge.
    /// </summary>
    public int Z => _front;

    /// <summary>
    /// Gets the width in pixels.
    /// </summary>
    public int Width => _right - _left;

    /// <summary>
    /// Gets the height in pixels.
    /// </summary>
    public int Height => _bottom - _top;

    /// <summary>
    /// Gets the depth in pixels.
    /// </summary>
    public int Depth => _back - _front;

    /// <summary>
    /// Gets the X coordinate of the left edge.
    /// </summary>
    public int Left => _left;

    /// <summary>
    /// Gets the Y coordinate of the top edge.
    /// </summary>
    public int Top => _top;

    /// <summary>
    /// Gets the Z coordinate of the front edge.
    /// </summary>
    public int Front => _front;

    /// <summary>
    /// Gets the X coordinate of the right edge.
    /// </summary>
    public int Right => _right;

    /// <summary>
    /// Gets the Y coordinate of the bottom edge.
    /// </summary>
    public int Bottom => _bottom;

    /// <summary>
    /// Gets the Z coordinate of the back edge.
    /// </summary>
    public int Back => _back;

    /// <summary>
    /// Gets the width, height, and depth in pixels.
    /// </summary>
    public PixelSize3 Size
        => new(Width, Height, Depth);

    /// <summary>
    /// Gets the left-top-front position of the bounds.
    /// </summary>
    public PixelPoint3 Location
        => new(_left, _top, _front);

    /// <summary>
    /// Gets the minimum coordinate of the bounds.
    /// </summary>
    public PixelPoint3 Min
        => new(_left, _top, _front);

    /// <summary>
    /// Gets the maximum coordinate of the bounds.
    /// </summary>
    public PixelPoint3 Max
        => new(_right, _bottom, _back);

    /// <summary>
    /// Converts this pixel bounds to a <see cref="PixelBox3"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="PixelBox3"/> containing the minimum and maximum
    /// coordinates of these bounds.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelBox3 ToBox()
        => new PixelBox3(
            _left,
            _top,
            _front,
            _right,
            _bottom,
            _back);

    /// <summary>
    /// Determines whether the specified pixel coordinate is contained within
    /// these bounds.
    /// </summary>
    /// <param name="px">The X coordinate in pixels.</param>
    /// <param name="py">The Y coordinate in pixels.</param>
    /// <param name="pz">The Z coordinate in pixels.</param>
    /// <returns>
    /// <see langword="true"/> if the coordinate is inside the bounds;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The left, top, and front edges are inclusive, while the right, bottom,
    /// and back edges are exclusive.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(int px, int py, int pz)
        => px >= _left && px < _right &&
           py >= _top && py < _bottom &&
           pz >= _front && pz < _back;

    /// <summary>
    /// Determines whether the specified pixel position is contained within
    /// these bounds.
    /// </summary>
    /// <param name="position">The three-dimensional pixel position to test.</param>
    /// <returns>
    /// <see langword="true"/> if the position is inside the bounds;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The minimum coordinates are inclusive, while the maximum coordinates
    /// are exclusive.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in PixelPoint3 position)
        => position.X >= _left && position.X < _right &&
           position.Y >= _top && position.Y < _bottom &&
           position.Z >= _front && position.Z < _back;

    /// <summary>
    /// Creates a new pixel bounds offset by the specified pixel delta.
    /// </summary>
    /// <param name="delta">
    /// The horizontal, vertical, and depth offset in pixels.
    /// </param>
    /// <returns>
    /// A new <see cref="PixelBounds3"/> with the same size and the specified
    /// offset.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelBounds3 Offset(in Vector3 delta)
        => new PixelBounds3(
            _left + (int)delta.X,
            _top + (int)delta.Y,
            _front + (int)delta.Z,
            Width,
            Height,
            Depth);

    /// <summary>
    /// Creates a new pixel bounds expanded by the specified amount on each
    /// side.
    /// </summary>
    /// <param name="dx">
    /// The horizontal amount in pixels to add to each side.
    /// </param>
    /// <param name="dy">
    /// The vertical amount in pixels to add to each side.
    /// </param>
    /// <param name="dz">
    /// The depth amount in pixels to add to each side.
    /// </param>
    /// <returns>
    /// A new <see cref="PixelBounds3"/> expanded by the specified amounts.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelBounds3 Inflate(int dx, int dy, int dz)
        => new PixelBounds3(
            _left - dx,
            _top - dy,
            _front - dz,
            Width + dx * 2,
            Height + dy * 2,
            Depth + dz * 2);

    /// <summary>
    /// Determines whether this pixel bounds is equal to another pixel bounds.
    /// </summary>
    /// <param name="other">
    /// The pixel bounds to compare with this instance.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if all six boundaries are equal;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(PixelBounds3 other)
        => _left == other._left &&
           _top == other._top &&
           _front == other._front &&
           _right == other._right &&
           _bottom == other._bottom &&
           _back == other._back;

    /// <summary>
    /// Determines whether this pixel bounds is equal to the specified object.
    /// </summary>
    /// <param name="obj">
    /// The object to compare with this instance.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="obj"/> is an equal
    /// <see cref="PixelBounds3"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
        => obj is PixelBounds3 b && Equals(b);

    /// <summary>
    /// Returns the hash code for this pixel bounds.
    /// </summary>
    /// <returns>
    /// A hash code based on all six boundaries.
    /// </returns>
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + _left;
            hash = hash * 31 + _top;
            hash = hash * 31 + _front;
            hash = hash * 31 + _right;
            hash = hash * 31 + _bottom;
            hash = hash * 31 + _back;
            return hash;
        }
    }

    /// <summary>
    /// Determines whether two pixel bounds are equal.
    /// </summary>
    /// <param name="a">The first pixel bounds.</param>
    /// <param name="b">The second pixel bounds.</param>
    /// <returns>
    /// <see langword="true"/> if all six boundaries are equal;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(
        in PixelBounds3 a,
        in PixelBounds3 b)
        => a.Equals(b);

    /// <summary>
    /// Determines whether two pixel bounds are not equal.
    /// </summary>
    /// <param name="a">The first pixel bounds.</param>
    /// <param name="b">The second pixel bounds.</param>
    /// <returns>
    /// <see langword="true"/> if at least one boundary differs;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(
        in PixelBounds3 a,
        in PixelBounds3 b)
        => !a.Equals(b);

    /// <summary>
    /// Parses a string representation of pixel bounds.
    /// </summary>
    /// <param name="s">
    /// The string containing X, Y, Z, width, height, and depth.
    /// </param>
    /// <returns>
    /// The parsed <see cref="PixelBounds3"/>.
    /// </returns>
    /// <exception cref="FormatException">
    /// Thrown when the string does not contain exactly six valid integer
    /// values.
    /// </exception>
    public static PixelBounds3 Parse(string s)
    {
        if (TryParse(s, out var result))
            return result;

        throw new FormatException(
            $"Invalid PixelBounds3 format: '{s}'. Expected 6 integer values separated by ',' or ' '.");
    }

    /// <summary>
    /// Attempts to parse a string representation of pixel bounds.
    /// </summary>
    /// <param name="s">
    /// The string containing X, Y, Z, width, height, and depth.
    /// </param>
    /// <param name="result">
    /// When this method returns <see langword="true"/>, contains the parsed
    /// pixel bounds; otherwise, contains <see cref="Zero"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if parsing was successful; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public static bool TryParse(
        string? s,
        out PixelBounds3 result)
    {
        result = Zero;

        if (string.IsNullOrWhiteSpace(s))
            return false;

        var parts = s.Split(
            new[] { ',', ' ' },
            StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 6)
            return false;

        try
        {
            int x = int.Parse(parts[0], CultureInfo.InvariantCulture);
            int y = int.Parse(parts[1], CultureInfo.InvariantCulture);
            int z = int.Parse(parts[2], CultureInfo.InvariantCulture);
            int w = int.Parse(parts[3], CultureInfo.InvariantCulture);
            int h = int.Parse(parts[4], CultureInfo.InvariantCulture);
            int d = int.Parse(parts[5], CultureInfo.InvariantCulture);

            result = new PixelBounds3(x, y, z, w, h, d);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Returns a string representation of this pixel bounds.
    /// </summary>
    /// <returns>
    /// A string containing X, Y, Z, width, height, and depth separated
    /// by commas.
    /// </returns>
    public override string ToString()
        => string.Format(
            CultureInfo.InvariantCulture,
            "{0}, {1}, {2}, {3}, {4}, {5}",
            X,
            Y,
            Z,
            Width,
            Height,
            Depth);
}
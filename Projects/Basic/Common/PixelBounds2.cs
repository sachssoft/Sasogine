using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Represents a two-dimensional pixel-aligned bounding rectangle defined by
/// integer coordinates and dimensions.
/// </summary>
/// <remarks>
/// <see cref="PixelBounds2"/> stores its boundaries internally as left, top,
/// right, and bottom coordinates. The width and height are derived from these
/// boundaries.
/// </remarks>
public readonly struct PixelBounds2 : IEquatable<PixelBounds2>
{
    private readonly int _left;
    private readonly int _top;
    private readonly int _right;
    private readonly int _bottom;

    /// <summary>
    /// Represents a pixel bounds with all coordinates set to zero.
    /// </summary>
    public static readonly PixelBounds2 Zero = new PixelBounds2(0, 0, 0, 0);

    /// <summary>
    /// Initializes a new instance from a position and a pixel size.
    /// </summary>
    /// <param name="x">
    /// The X coordinate of the left edge.
    /// </param>
    /// <param name="y">
    /// The Y coordinate of the top edge.
    /// </param>
    /// <param name="width">
    /// The width in pixels.
    /// </param>
    /// <param name="height">
    /// The height in pixels.
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelBounds2(int x, int y, int width, int height)
    {
        _left = x;
        _top = y;
        _right = x + width;
        _bottom = y + height;
    }

    /// <summary>
    /// Initializes a new instance from a position and a pixel size represented
    /// by <see cref="Point"/> values.
    /// </summary>
    /// <param name="position">
    /// The position of the top-left corner.
    /// </param>
    /// <param name="size">
    /// The width and height in pixels.
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelBounds2(Point position, Point size)
    {
        _left = position.X;
        _top = position.Y;
        _right = position.X + size.X;
        _bottom = position.Y + size.Y;
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
    /// Gets the width in pixels.
    /// </summary>
    public int Width => _right - _left;

    /// <summary>
    /// Gets the height in pixels.
    /// </summary>
    public int Height => _bottom - _top;

    /// <summary>
    /// Gets the X coordinate of the left edge.
    /// </summary>
    public int Left => _left;

    /// <summary>
    /// Gets the Y coordinate of the top edge.
    /// </summary>
    public int Top => _top;

    /// <summary>
    /// Gets the X coordinate of the right edge.
    /// </summary>
    public int Right => _right;

    /// <summary>
    /// Gets the Y coordinate of the bottom edge.
    /// </summary>
    public int Bottom => _bottom;

    /// <summary>
    /// Gets the size of the bounds in pixels.
    /// </summary>
    public Point Size => new Point(Width, Height);

    /// <summary>
    /// Gets the top-left position of the bounds.
    /// </summary>
    public Point Location => new Point(_left, _top);

    /// <summary>
    /// Converts this pixel bounds to a <see cref="PixelBox2"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="PixelBox2"/> containing the minimum and maximum coordinates
    /// of these bounds.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelBox2 ToBox()
        => new PixelBox2(_left, _top, _right, _bottom);

    /// <summary>
    /// Determines whether this pixel bounds is equal to another pixel bounds.
    /// </summary>
    /// <param name="other">
    /// The pixel bounds to compare with this instance.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if all four boundaries are equal;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(PixelBounds2 other)
        => _left == other._left &&
           _top == other._top &&
           _right == other._right &&
           _bottom == other._bottom;

    /// <summary>
    /// Determines whether this pixel bounds is equal to the specified object.
    /// </summary>
    /// <param name="obj">
    /// The object to compare with this instance.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="obj"/> is an equal
    /// <see cref="PixelBounds2"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
        => obj is PixelBounds2 b && Equals(b);

    /// <summary>
    /// Returns the hash code for this pixel bounds.
    /// </summary>
    /// <returns>
    /// A hash code based on the left, top, right, and bottom boundaries.
    /// </returns>
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + _left;
            hash = hash * 31 + _top;
            hash = hash * 31 + _right;
            hash = hash * 31 + _bottom;
            return hash;
        }
    }

    /// <summary>
    /// Determines whether the specified pixel coordinate is contained within
    /// these bounds.
    /// </summary>
    /// <param name="px">The X coordinate in pixels.</param>
    /// <param name="py">The Y coordinate in pixels.</param>
    /// <returns>
    /// <see langword="true"/> if the coordinate is inside the bounds;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The left and top edges are inclusive, while the right and bottom edges
    /// are exclusive.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(int px, int py)
        => px >= _left && px < _right &&
           py >= _top && py < _bottom;

    /// <summary>
    /// Determines whether the specified pixel position is contained within
    /// these bounds.
    /// </summary>
    /// <param name="pos">
    /// The pixel position to test.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the position is inside the bounds;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The left and top edges are inclusive, while the right and bottom edges
    /// are exclusive.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in Point pos)
        => pos.X >= _left && pos.X < _right &&
           pos.Y >= _top && pos.Y < _bottom;

    /// <summary>
    /// Creates a new pixel bounds offset by the specified pixel delta.
    /// </summary>
    /// <param name="delta">
    /// The horizontal and vertical pixel offset.
    /// </param>
    /// <returns>
    /// A new <see cref="PixelBounds2"/> with the same size and the specified
    /// offset.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelBounds2 Offset(in Point delta)
        => new PixelBounds2(
            _left + delta.X,
            _top + delta.Y,
            Width,
            Height);

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
    /// <returns>
    /// A new <see cref="PixelBounds2"/> expanded by the specified amounts.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelBounds2 Inflate(int dx, int dy)
        => new PixelBounds2(
            _left - dx,
            _top - dy,
            Width + dx * 2,
            Height + dy * 2);

    /// <summary>
    /// Converts the specified pixel bounds to a
    /// <see cref="Rectangle"/>.
    /// </summary>
    /// <param name="b">
    /// The pixel bounds to convert.
    /// </param>
    /// <returns>
    /// A <see cref="Rectangle"/> representing the same position and size.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Rectangle(PixelBounds2 b)
        => new Rectangle(
            b._left,
            b._top,
            b.Width,
            b.Height);

    /// <summary>
    /// Converts a <see cref="Rectangle"/> to pixel bounds.
    /// </summary>
    /// <param name="r">
    /// The rectangle to convert.
    /// </param>
    /// <returns>
    /// A <see cref="PixelBounds2"/> representing the same position and size.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator PixelBounds2(Rectangle r)
        => new PixelBounds2(
            r.X,
            r.Y,
            r.Width,
            r.Height);

    /// <summary>
    /// Determines whether two pixel bounds are equal.
    /// </summary>
    /// <param name="a">The first pixel bounds.</param>
    /// <param name="b">The second pixel bounds.</param>
    /// <returns>
    /// <see langword="true"/> if all four boundaries are equal;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(
        in PixelBounds2 a,
        in PixelBounds2 b)
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
    public static bool operator !=(
        in PixelBounds2 a,
        in PixelBounds2 b)
        => !a.Equals(b);

    /// <summary>
    /// Parses a string representation of pixel bounds.
    /// </summary>
    /// <param name="s">
    /// The string containing the X coordinate, Y coordinate, width,
    /// and height.
    /// </param>
    /// <returns>
    /// The parsed <see cref="PixelBounds2"/>.
    /// </returns>
    /// <exception cref="FormatException">
    /// Thrown when the string does not contain exactly four valid integer
    /// values.
    /// </exception>
    /// <remarks>
    /// The values may be separated by commas or spaces and are interpreted
    /// using <see cref="CultureInfo.InvariantCulture"/>.
    /// </remarks>
    public static PixelBounds2 Parse(string s)
    {
        if (TryParse(s, out var result))
            return result;

        throw new FormatException(
            $"Invalid PixelBounds2 format: '{s}'. Expected 4 integer values separated by ',' or ' '.");
    }

    /// <summary>
    /// Attempts to parse a string representation of pixel bounds.
    /// </summary>
    /// <param name="s">
    /// The string containing the X coordinate, Y coordinate, width,
    /// and height.
    /// </param>
    /// <param name="result">
    /// When this method returns <see langword="true"/>, contains the parsed
    /// pixel bounds; otherwise, contains <see cref="Zero"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if parsing was successful; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The string must contain exactly four valid integer values separated
    /// by commas or spaces. Numeric values are interpreted using
    /// <see cref="CultureInfo.InvariantCulture"/>.
    /// </remarks>
    public static bool TryParse(
        string? s,
        out PixelBounds2 result)
    {
        result = Zero;

        if (string.IsNullOrWhiteSpace(s))
            return false;

        var parts = s.Split(
            new[] { ',', ' ' },
            StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 4)
            return false;

        try
        {
            int x = int.Parse(
                parts[0],
                CultureInfo.InvariantCulture);

            int y = int.Parse(
                parts[1],
                CultureInfo.InvariantCulture);

            int w = int.Parse(
                parts[2],
                CultureInfo.InvariantCulture);

            int h = int.Parse(
                parts[3],
                CultureInfo.InvariantCulture);

            result = new PixelBounds2(x, y, w, h);
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
    /// A string containing the X coordinate, Y coordinate, width,
    /// and height separated by commas.
    /// </returns>
    /// <remarks>
    /// The X and Y values represent the top-left position, while the width
    /// and height represent the dimensions in pixels.
    /// </remarks>
    public override string ToString()
        => string.Format(
            CultureInfo.InvariantCulture,
            "{0}, {1}, {2}, {3}",
            X,
            Y,
            Width,
            Height);
}
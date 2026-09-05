using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Represents a three-dimensional axis-aligned bounding box using
/// single-precision floating-point coordinates.
/// </summary>
/// <remarks>
/// <para>
/// The bounds are internally represented by their left, top, front, right,
/// bottom, and back coordinates.
/// </para>
/// <para>
/// The <see cref="X"/>, <see cref="Y"/>, and <see cref="Z"/> properties
/// correspond to the left, top, and front boundaries. The
/// <see cref="Width"/>, <see cref="Height"/>, and <see cref="Depth"/>
/// properties represent the corresponding dimensions.
/// </para>
/// <para>
/// The right, bottom, and back boundaries are exclusive for containment
/// tests.
/// </para>
/// </remarks>
public readonly struct Bounds3 : IEquatable<Bounds3>
{
    private readonly float _left;
    private readonly float _top;
    private readonly float _front;
    private readonly float _right;
    private readonly float _bottom;
    private readonly float _back;

    /// <summary>
    /// Represents an empty bounds with all coordinates set to zero.
    /// </summary>
    public static readonly Bounds3 Zero = new Bounds3(0f, 0f, 0f, 0f, 0f, 0f);

    /// <summary>
    /// Initializes a new instance of the <see cref="Bounds3"/> structure
    /// from a position and size.
    /// </summary>
    /// <param name="x">The x-coordinate of the left boundary.</param>
    /// <param name="y">The y-coordinate of the top boundary.</param>
    /// <param name="z">The z-coordinate of the front boundary.</param>
    /// <param name="width">The width of the bounds.</param>
    /// <param name="height">The height of the bounds.</param>
    /// <param name="depth">The depth of the bounds.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bounds3(
        float x,
        float y,
        float z,
        float width,
        float height,
        float depth)
    {
        _left = x;
        _top = y;
        _front = z;
        _right = x + width;
        _bottom = y + height;
        _back = z + depth;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Bounds3"/> structure
    /// from a position and size.
    /// </summary>
    /// <param name="position">The position of the front-top-left corner.</param>
    /// <param name="size">The size of the bounds.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bounds3(Point3 position, Size3 size)
    {
        _left = position.X;
        _top = position.Y;
        _front = position.Z;
        _right = position.X + size.Width;
        _bottom = position.Y + size.Height;
        _back = position.Z + size.Depth;
    }

    /// <summary>
    /// Gets the x-coordinate of the left boundary.
    /// </summary>
    public float X => _left;

    /// <summary>
    /// Gets the y-coordinate of the top boundary.
    /// </summary>
    public float Y => _top;

    /// <summary>
    /// Gets the z-coordinate of the front boundary.
    /// </summary>
    public float Z => _front;

    /// <summary>
    /// Gets the width of the bounds.
    /// </summary>
    public float Width => _right - _left;

    /// <summary>
    /// Gets the height of the bounds.
    /// </summary>
    public float Height => _bottom - _top;

    /// <summary>
    /// Gets the depth of the bounds.
    /// </summary>
    public float Depth => _back - _front;

    /// <summary>
    /// Gets the x-coordinate of the left boundary.
    /// </summary>
    public float Left => _left;

    /// <summary>
    /// Gets the y-coordinate of the top boundary.
    /// </summary>
    public float Top => _top;

    /// <summary>
    /// Gets the z-coordinate of the front boundary.
    /// </summary>
    public float Front => _front;

    /// <summary>
    /// Gets the x-coordinate of the right boundary.
    /// </summary>
    public float Right => _right;

    /// <summary>
    /// Gets the y-coordinate of the bottom boundary.
    /// </summary>
    public float Bottom => _bottom;

    /// <summary>
    /// Gets the z-coordinate of the back boundary.
    /// </summary>
    public float Back => _back;

    /// <summary>
    /// Gets the size of the bounds.
    /// </summary>
    public Size3 Size => new Size3(Width, Height, Depth);

    /// <summary>
    /// Gets the position of the front-top-left corner.
    /// </summary>
    public Point3 Location => new Point3(_left, _top, _front);

    /// <summary>
    /// Determines whether this instance is equal to another
    /// <see cref="Bounds3"/> instance.
    /// </summary>
    /// <param name="other">The bounds to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if all six boundary coordinates are identical;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Bounds3 other)
        => _left == other._left
        && _top == other._top
        && _front == other._front
        && _right == other._right
        && _bottom == other._bottom
        && _back == other._back;

    /// <summary>
    /// Determines whether the specified object is equal to this instance.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="obj"/> is a
    /// <see cref="Bounds3"/> with identical boundary coordinates;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
        => obj is Bounds3 b && Equals(b);

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code based on all six boundary coordinates.
    /// </returns>
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + _left.GetHashCode();
            hash = hash * 31 + _top.GetHashCode();
            hash = hash * 31 + _front.GetHashCode();
            hash = hash * 31 + _right.GetHashCode();
            hash = hash * 31 + _bottom.GetHashCode();
            hash = hash * 31 + _back.GetHashCode();
            return hash;
        }
    }

    /// <summary>
    /// Determines whether the specified point is contained within these
    /// bounds.
    /// </summary>
    /// <param name="px">The x-coordinate of the point.</param>
    /// <param name="py">The y-coordinate of the point.</param>
    /// <param name="pz">The z-coordinate of the point.</param>
    /// <returns>
    /// <see langword="true"/> if the point is inside the bounds;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The left, top, and front boundaries are inclusive, while the right,
    /// bottom, and back boundaries are exclusive.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(float px, float py, float pz)
        => px >= _left && px < _right
        && py >= _top && py < _bottom
        && pz >= _front && pz < _back;

    /// <summary>
    /// Determines whether the specified point is contained within these
    /// bounds.
    /// </summary>
    /// <param name="pos">The point to test.</param>
    /// <returns>
    /// <see langword="true"/> if the point is inside the bounds;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The left, top, and front boundaries are inclusive, while the right,
    /// bottom, and back boundaries are exclusive.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in Point3 pos)
        => pos.X >= _left && pos.X < _right
        && pos.Y >= _top && pos.Y < _bottom
        && pos.Z >= _front && pos.Z < _back;

    /// <summary>
    /// Returns a new <see cref="Bounds3"/> translated by the specified
    /// offset.
    /// </summary>
    /// <param name="delta">The amount by which to offset the bounds.</param>
    /// <returns>A new bounds with the specified offset applied.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bounds3 Offset(in Vector3 delta)
        => new Bounds3(
            _left + delta.X,
            _top + delta.Y,
            _front + delta.Z,
            Width,
            Height,
            Depth);

    /// <summary>
    /// Returns a new <see cref="Bounds3"/> expanded or contracted by the
    /// specified amounts.
    /// </summary>
    /// <param name="dx">
    /// The horizontal amount to add to each side.
    /// </param>
    /// <param name="dy">
    /// The vertical amount to add to each side.
    /// </param>
    /// <param name="dz">
    /// The depth amount to add to each side.
    /// </param>
    /// <returns>
    /// A new bounds with the specified inflation applied.
    /// </returns>
    /// <remarks>
    /// Positive values expand the bounds, while negative values contract
    /// them.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bounds3 Inflate(float dx, float dy, float dz)
        => new Bounds3(
            _left - dx,
            _top - dy,
            _front - dz,
            Width + dx * 2f,
            Height + dy * 2f,
            Depth + dz * 2f);

    /// <summary>
    /// Determines whether two <see cref="Bounds3"/> instances are equal.
    /// </summary>
    /// <param name="a">The first bounds to compare.</param>
    /// <param name="b">The second bounds to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both bounds have identical boundary
    /// coordinates; otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in Bounds3 a, in Bounds3 b)
        => a.Equals(b);

    /// <summary>
    /// Determines whether two <see cref="Bounds3"/> instances are not equal.
    /// </summary>
    /// <param name="a">The first bounds to compare.</param>
    /// <param name="b">The second bounds to compare.</param>
    /// <returns>
    /// <see langword="true"/> if the bounds have different boundary
    /// coordinates; otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in Bounds3 a, in Bounds3 b)
        => !a.Equals(b);

    /// <summary>
    /// Parses a string representation of a <see cref="Bounds3"/>.
    /// </summary>
    /// <param name="s">
    /// A string containing six numeric values representing
    /// x, y, z, width, height, and depth.
    /// </param>
    /// <returns>The parsed <see cref="Bounds3"/>.</returns>
    /// <exception cref="FormatException">
    /// Thrown when the specified string does not contain exactly six
    /// valid numeric values.
    /// </exception>
    /// <remarks>
    /// The values may be separated by commas or spaces and are interpreted
    /// using <see cref="CultureInfo.InvariantCulture"/>.
    /// </remarks>
    public static Bounds3 Parse(string s)
    {
        if (TryParse(s, out var result))
            return result;

        throw new FormatException(
            $"Invalid Bounds3 format: '{s}'. Expected 6 numeric values separated by ',' or ' '.");
    }

    /// <summary>
    /// Attempts to parse a string representation of a
    /// <see cref="Bounds3"/>.
    /// </summary>
    /// <param name="s">
    /// A string containing six numeric values representing
    /// x, y, z, width, height, and depth.
    /// </param>
    /// <param name="result">
    /// When this method returns <see langword="true"/>, contains the parsed
    /// bounds; otherwise, contains <see cref="Zero"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the string was successfully parsed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The values may be separated by commas or spaces and are interpreted
    /// using <see cref="CultureInfo.InvariantCulture"/>.
    /// </remarks>
    public static bool TryParse(string? s, out Bounds3 result)
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
            float x = float.Parse(parts[0], CultureInfo.InvariantCulture);
            float y = float.Parse(parts[1], CultureInfo.InvariantCulture);
            float z = float.Parse(parts[2], CultureInfo.InvariantCulture);
            float width = float.Parse(parts[3], CultureInfo.InvariantCulture);
            float height = float.Parse(parts[4], CultureInfo.InvariantCulture);
            float depth = float.Parse(parts[5], CultureInfo.InvariantCulture);

            result = new Bounds3(x, y, z, width, height, depth);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Returns the string representation of this <see cref="Bounds3"/>.
    /// </summary>
    /// <returns>
    /// A string containing the x-coordinate, y-coordinate, z-coordinate,
    /// width, height, and depth separated by commas.
    /// </returns>
    /// <remarks>
    /// Numeric values are formatted using
    /// <see cref="CultureInfo.InvariantCulture"/>.
    /// </remarks>
    public override string ToString()
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0}, {1}, {2}, {3}, {4}, {5}",
            X,
            Y,
            Z,
            Width,
            Height,
            Depth);
    }
}
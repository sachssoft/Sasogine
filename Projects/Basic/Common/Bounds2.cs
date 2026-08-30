using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Represents a two-dimensional axis-aligned bounding rectangle using
/// single-precision floating-point coordinates.
/// </summary>
/// <remarks>
/// <para>
/// Unlike <see cref="Rectangle"/>, which uses integer coordinates,
/// <see cref="Bounds2"/> stores its boundaries as <see cref="float"/> values.
/// </para>
/// <para>
/// The bounds are internally represented by their left, top, right, and
/// bottom coordinates. The <see cref="X"/> and <see cref="Y"/> properties
/// correspond to the left and top boundaries, while <see cref="Width"/>
/// and <see cref="Height"/> are calculated from the boundaries.
/// </para>
/// <para>
/// The right and bottom boundaries are exclusive for containment tests.
/// </para>
/// </remarks>
public readonly struct Bounds2 : IEquatable<Bounds2>
{
    private readonly float _left;
    private readonly float _top;
    private readonly float _right;
    private readonly float _bottom;

    /// <summary>
    /// Represents an empty bounds with all coordinates set to zero.
    /// </summary>
    public static readonly Bounds2 Zero = new Bounds2(0f, 0f, 0f, 0f);

    /// <summary>
    /// Initializes a new instance of the <see cref="Bounds2"/> structure
    /// from a position and size.
    /// </summary>
    /// <param name="x">The x-coordinate of the left boundary.</param>
    /// <param name="y">The y-coordinate of the top boundary.</param>
    /// <param name="width">The width of the bounds.</param>
    /// <param name="height">The height of the bounds.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bounds2(float x, float y, float width, float height)
    {
        _left = x;
        _top = y;
        _right = x + width;
        _bottom = y + height;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Bounds2"/> structure
    /// from a position and size vector.
    /// </summary>
    /// <param name="position">The position of the top-left corner.</param>
    /// <param name="size">The size of the bounds.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bounds2(Vector2 position, Vector2 size)
    {
        _left = position.X;
        _top = position.Y;
        _right = position.X + size.X;
        _bottom = position.Y + size.Y;
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
    /// Gets the width of the bounds.
    /// </summary>
    public float Width => _right - _left;

    /// <summary>
    /// Gets the height of the bounds.
    /// </summary>
    public float Height => _bottom - _top;

    /// <summary>
    /// Gets the x-coordinate of the left boundary.
    /// </summary>
    public float Left => _left;

    /// <summary>
    /// Gets the y-coordinate of the top boundary.
    /// </summary>
    public float Top => _top;

    /// <summary>
    /// Gets the x-coordinate of the right boundary.
    /// </summary>
    public float Right => _right;

    /// <summary>
    /// Gets the y-coordinate of the bottom boundary.
    /// </summary>
    public float Bottom => _bottom;

    /// <summary>
    /// Gets the size of the bounds.
    /// </summary>
    public Vector2 Size => new Vector2(Width, Height);

    /// <summary>
    /// Gets the position of the top-left corner.
    /// </summary>
    public Vector2 Location => new Vector2(_left, _top);

    /// <summary>
    /// Converts this <see cref="Bounds2"/> instance to a <see cref="Box2"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="Box2"/> representing the same left, top, right, and
    /// bottom boundaries.
    /// </returns>
    public Box2 ToBox()
        => new Box2(_left, _top, _right, _bottom);

    /// <summary>
    /// Determines whether this instance is equal to another
    /// <see cref="Bounds2"/> instance.
    /// </summary>
    /// <param name="other">The bounds to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if both bounds have identical boundary
    /// coordinates; otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Bounds2 other)
        => _left == other._left && _top == other._top && _right == other._right && _bottom == other._bottom;

    /// <summary>
    /// Determines whether the specified object is equal to this instance.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="obj"/> is a
    /// <see cref="Bounds2"/> with identical boundary coordinates;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj) => obj is Bounds2 b && Equals(b);

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>A hash code based on all four boundary coordinates.</returns>
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + _left.GetHashCode();
            hash = hash * 31 + _top.GetHashCode();
            hash = hash * 31 + _right.GetHashCode();
            hash = hash * 31 + _bottom.GetHashCode();
            return hash;
        }
    }

    /// <summary>
    /// Determines whether the specified point is contained within these
    /// bounds.
    /// </summary>
    /// <param name="px">The x-coordinate of the point.</param>
    /// <param name="py">The y-coordinate of the point.</param>
    /// <returns>
    /// <see langword="true"/> if the point is inside the bounds;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The left and top boundaries are inclusive, while the right and
    /// bottom boundaries are exclusive.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(float px, float py) => px >= _left && px < _right && py >= _top && py < _bottom;

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
    /// The left and top boundaries are inclusive, while the right and
    /// bottom boundaries are exclusive.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in Vector2 pos) => pos.X >= _left && pos.X < _right && pos.Y >= _top && pos.Y < _bottom;

    /// <summary>
    /// Returns a new <see cref="Bounds2"/> translated by the specified
    /// offset.
    /// </summary>
    /// <param name="delta">The amount by which to offset the bounds.</param>
    /// <returns>A new bounds with the specified offset applied.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bounds2 Offset(in Vector2 delta)
        => new Bounds2(_left + delta.X, _top + delta.Y, Width, Height);

    /// <summary>
    /// Returns a new <see cref="Bounds2"/> expanded or contracted by the
    /// specified amounts.
    /// </summary>
    /// <param name="dx">
    /// The horizontal amount to add to each side. Positive values expand
    /// the bounds, while negative values contract them.
    /// </param>
    /// <param name="dy">
    /// The vertical amount to add to each side. Positive values expand
    /// the bounds, while negative values contract them.
    /// </param>
    /// <returns>A new bounds with the specified inflation applied.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bounds2 Inflate(float dx, float dy)
        => new Bounds2(_left - dx, _top - dy, Width + dx * 2, Height + dy * 2);

    /// <summary>
    /// Converts a <see cref="Bounds2"/> to a
    /// <see cref="Rectangle"/> using integer coordinates.
    /// </summary>
    /// <param name="b">The bounds to convert.</param>
    /// <returns>A <see cref="Rectangle"/> representing the bounds.</returns>
    /// <remarks>
    /// The conversion truncates the floating-point boundary and size
    /// values to integers.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Rectangle(Bounds2 b)
        => new Rectangle((int)b._left, (int)b._top, (int)b.Width, (int)b.Height);

    /// <summary>
    /// Converts a <see cref="Rectangle"/> to a <see cref="Bounds2"/>.
    /// </summary>
    /// <param name="r">The rectangle to convert.</param>
    /// <returns>A <see cref="Bounds2"/> representing the rectangle.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Bounds2(Rectangle r)
        => new Bounds2(r.X, r.Y, r.Width, r.Height);

    /// <summary>
    /// Determines whether two <see cref="Bounds2"/> instances are equal.
    /// </summary>
    /// <param name="a">The first bounds to compare.</param>
    /// <param name="b">The second bounds to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both bounds have identical boundary
    /// coordinates; otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in Bounds2 a, in Bounds2 b) => a.Equals(b);

    /// <summary>
    /// Determines whether two <see cref="Bounds2"/> instances are not equal.
    /// </summary>
    /// <param name="a">The first bounds to compare.</param>
    /// <param name="b">The second bounds to compare.</param>
    /// <returns>
    /// <see langword="true"/> if the bounds have different boundary
    /// coordinates; otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in Bounds2 a, in Bounds2 b) => !a.Equals(b);

    /// <summary>
    /// Parses a string representation of a <see cref="Bounds2"/>.
    /// </summary>
    /// <param name="s">
    /// A string containing four numeric values representing
    /// x, y, width, and height.
    /// </param>
    /// <returns>The parsed <see cref="Bounds2"/>.</returns>
    /// <exception cref="FormatException">
    /// Thrown when the specified string does not contain exactly four
    /// valid numeric values.
    /// </exception>
    /// <remarks>
    /// The values may be separated by commas or spaces.
    /// Numeric values are interpreted using
    /// <see cref="CultureInfo.InvariantCulture"/>.
    /// </remarks>
    public static Bounds2 Parse(string s)
    {
        if (TryParse(s, out var result))
            return result;

        throw new FormatException($"Invalid Bounds format: '{s}'. Expected 4 numeric values separated by ',' or ' '.");
    }

    /// <summary>
    /// Attempts to parse a string representation of a
    /// <see cref="Bounds2"/>.
    /// </summary>
    /// <param name="s">
    /// A string containing four numeric values representing
    /// x, y, width, and height.
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
    /// The values may be separated by commas or spaces.
    /// Numeric values are interpreted using
    /// <see cref="CultureInfo.InvariantCulture"/>.
    /// </remarks>
    public static bool TryParse(string? s, out Bounds2 result)
    {
        result = Zero;
        if (string.IsNullOrWhiteSpace(s))
            return false;

        var parts = s.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4)
            return false;

        try
        {
            float x = float.Parse(parts[0], CultureInfo.InvariantCulture);
            float y = float.Parse(parts[1], CultureInfo.InvariantCulture);
            float w = float.Parse(parts[2], CultureInfo.InvariantCulture);
            float h = float.Parse(parts[3], CultureInfo.InvariantCulture);

            result = new Bounds2(x, y, w, h);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Returns the string representation of this <see cref="Bounds2"/>.
    /// </summary>
    /// <returns>
    /// A string containing the x-coordinate, y-coordinate, width, and
    /// height separated by commas.
    /// </returns>
    /// <remarks>
    /// Numeric values are formatted using
    /// <see cref="CultureInfo.InvariantCulture"/>.
    /// </remarks>
    public override string ToString()
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0}, {1}, {2}, {3}",
            X,
            Y,
            Width,
            Height
        );
    }
}
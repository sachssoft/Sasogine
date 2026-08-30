using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Represents a two-dimensional axis-aligned bounding box using
/// single-precision floating-point minimum and maximum coordinates.
/// </summary>
/// <remarks>
/// <para>
/// Unlike <see cref="Bounds2"/>, which is represented by a position and
/// size, <see cref="Box2"/> is represented directly by its minimum and
/// maximum coordinates.
/// </para>
/// <para>
/// The minimum coordinates define the left and top boundaries, while the
/// maximum coordinates define the right and bottom boundaries.
/// </para>
/// <para>
/// The minimum boundaries are inclusive and the maximum boundaries are
/// exclusive for containment tests.
/// </para>
/// </remarks>
public readonly struct Box2 : IEquatable<Box2>
{
    private readonly float _minX;
    private readonly float _minY;
    private readonly float _maxX;
    private readonly float _maxY;

    /// <summary>
    /// Represents a box with all coordinates set to zero.
    /// </summary>
    public static readonly Box2 Zero = new Box2(0f, 0f, 0f, 0f);

    /// <summary>
    /// Initializes a new instance of the <see cref="Box2"/> structure
    /// from minimum and maximum coordinates.
    /// </summary>
    /// <param name="minX">The minimum x-coordinate.</param>
    /// <param name="minY">The minimum y-coordinate.</param>
    /// <param name="maxX">The maximum x-coordinate.</param>
    /// <param name="maxY">The maximum y-coordinate.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Box2(float minX, float minY, float maxX, float maxY)
    {
        _minX = minX;
        _minY = minY;
        _maxX = maxX;
        _maxY = maxY;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Box2"/> structure
    /// from minimum and maximum position vectors.
    /// </summary>
    /// <param name="min">The minimum coordinates of the box.</param>
    /// <param name="max">The maximum coordinates of the box.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Box2(Vector2 min, Vector2 max)
        : this(min.X, min.Y, max.X, max.Y) { }

    /// <summary>
    /// Gets the minimum x-coordinate of the box.
    /// </summary>
    public float MinX => _minX;

    /// <summary>
    /// Gets the minimum y-coordinate of the box.
    /// </summary>
    public float MinY => _minY;

    /// <summary>
    /// Gets the maximum x-coordinate of the box.
    /// </summary>
    public float MaxX => _maxX;

    /// <summary>
    /// Gets the maximum y-coordinate of the box.
    /// </summary>
    public float MaxY => _maxY;

    /// <summary>
    /// Gets the width of the box.
    /// </summary>
    public float Width => _maxX - _minX;

    /// <summary>
    /// Gets the height of the box.
    /// </summary>
    public float Height => _maxY - _minY;

    /// <summary>
    /// Gets the minimum coordinates of the box.
    /// </summary>
    public Vector2 Min => new Vector2(_minX, _minY);

    /// <summary>
    /// Gets the maximum coordinates of the box.
    /// </summary>
    public Vector2 Max => new Vector2(_maxX, _maxY);

    /// <summary>
    /// Converts this <see cref="Box2"/> to a <see cref="Bounds2"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="Bounds2"/> with the same minimum position and dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bounds2 ToBounds()
        => new Bounds2(_minX, _minY, _maxX - _minX, _maxY - _minY);

    /// <summary>
    /// Determines whether the specified point is contained within the box.
    /// </summary>
    /// <param name="x">The x-coordinate of the point.</param>
    /// <param name="y">The y-coordinate of the point.</param>
    /// <returns>
    /// <see langword="true"/> if the point is inside the box;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The minimum boundaries are inclusive, while the maximum boundaries
    /// are exclusive.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(float x, float y)
        => x >= _minX && x < _maxX && y >= _minY && y < _maxY;

    /// <summary>
    /// Determines whether the specified point is contained within the box.
    /// </summary>
    /// <param name="v">The point to test.</param>
    /// <returns>
    /// <see langword="true"/> if the point is inside the box;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The minimum boundaries are inclusive, while the maximum boundaries
    /// are exclusive.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in Vector2 v)
        => v.X >= _minX && v.X < _maxX && v.Y >= _minY && v.Y < _maxY;

    /// <summary>
    /// Determines whether this instance is equal to another
    /// <see cref="Box2"/> instance.
    /// </summary>
    /// <param name="other">The box to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if all minimum and maximum coordinates are
    /// identical; otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Box2 other)
        => _minX == other._minX && _minY == other._minY &&
           _maxX == other._maxX && _maxY == other._maxY;

    /// <summary>
    /// Determines whether the specified object is equal to this instance.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="obj"/> is a
    /// <see cref="Box2"/> with identical coordinates; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj) => obj is Box2 b && Equals(b);

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code based on the minimum and maximum coordinates.
    /// </returns>
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + _minX.GetHashCode();
            hash = hash * 31 + _minY.GetHashCode();
            hash = hash * 31 + _maxX.GetHashCode();
            hash = hash * 31 + _maxY.GetHashCode();
            return hash;
        }
    }

    /// <summary>
    /// Returns the string representation of this <see cref="Box2"/>.
    /// </summary>
    /// <returns>
    /// A string containing the minimum x-coordinate, minimum y-coordinate,
    /// maximum x-coordinate, and maximum y-coordinate separated by commas.
    /// </returns>
    /// <remarks>
    /// Numeric values are formatted using
    /// <see cref="CultureInfo.InvariantCulture"/>.
    /// </remarks>
    public override string ToString()
        => string.Format(
            CultureInfo.InvariantCulture,
            "{0}, {1}, {2}, {3}",
            _minX, _minY, _maxX, _maxY);
}
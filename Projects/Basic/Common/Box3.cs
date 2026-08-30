using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Represents a three-dimensional axis-aligned bounding box using
/// single-precision floating-point minimum and maximum coordinates.
/// </summary>
/// <remarks>
/// <para>
/// Unlike <see cref="Bounds3"/>, which is represented by a position and
/// size, <see cref="Box3"/> is represented directly by its minimum and
/// maximum coordinates.
/// </para>
/// <para>
/// The minimum coordinates define the left, top, and front boundaries,
/// while the maximum coordinates define the right, bottom, and back
/// boundaries.
/// </para>
/// <para>
/// The minimum boundaries are inclusive and the maximum boundaries are
/// exclusive for containment tests.
/// </para>
/// </remarks>
public readonly struct Box3 : IEquatable<Box3>
{
    private readonly float _minX;
    private readonly float _minY;
    private readonly float _minZ;
    private readonly float _maxX;
    private readonly float _maxY;
    private readonly float _maxZ;

    /// <summary>
    /// Represents a box with all coordinates set to zero.
    /// </summary>
    public static readonly Box3 Zero = new Box3(0f, 0f, 0f, 0f, 0f, 0f);

    /// <summary>
    /// Initializes a new instance of the <see cref="Box3"/> structure
    /// from minimum and maximum coordinates.
    /// </summary>
    /// <param name="minX">The minimum x-coordinate.</param>
    /// <param name="minY">The minimum y-coordinate.</param>
    /// <param name="minZ">The minimum z-coordinate.</param>
    /// <param name="maxX">The maximum x-coordinate.</param>
    /// <param name="maxY">The maximum y-coordinate.</param>
    /// <param name="maxZ">The maximum z-coordinate.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Box3(
        float minX,
        float minY,
        float minZ,
        float maxX,
        float maxY,
        float maxZ)
    {
        _minX = minX;
        _minY = minY;
        _minZ = minZ;
        _maxX = maxX;
        _maxY = maxY;
        _maxZ = maxZ;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Box3"/> structure
    /// from minimum and maximum position vectors.
    /// </summary>
    /// <param name="min">The minimum coordinates of the box.</param>
    /// <param name="max">The maximum coordinates of the box.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Box3(Vector3 min, Vector3 max)
        : this(min.X, min.Y, min.Z, max.X, max.Y, max.Z)
    {
    }

    /// <summary>
    /// Gets the minimum x-coordinate of the box.
    /// </summary>
    public float MinX => _minX;

    /// <summary>
    /// Gets the minimum y-coordinate of the box.
    /// </summary>
    public float MinY => _minY;

    /// <summary>
    /// Gets the minimum z-coordinate of the box.
    /// </summary>
    public float MinZ => _minZ;

    /// <summary>
    /// Gets the maximum x-coordinate of the box.
    /// </summary>
    public float MaxX => _maxX;

    /// <summary>
    /// Gets the maximum y-coordinate of the box.
    /// </summary>
    public float MaxY => _maxY;

    /// <summary>
    /// Gets the maximum z-coordinate of the box.
    /// </summary>
    public float MaxZ => _maxZ;

    /// <summary>
    /// Gets the width of the box.
    /// </summary>
    public float Width => _maxX - _minX;

    /// <summary>
    /// Gets the height of the box.
    /// </summary>
    public float Height => _maxY - _minY;

    /// <summary>
    /// Gets the depth of the box.
    /// </summary>
    public float Depth => _maxZ - _minZ;

    /// <summary>
    /// Gets the minimum coordinates of the box.
    /// </summary>
    public Vector3 Min => new Vector3(_minX, _minY, _minZ);

    /// <summary>
    /// Gets the maximum coordinates of the box.
    /// </summary>
    public Vector3 Max => new Vector3(_maxX, _maxY, _maxZ);

    /// <summary>
    /// Converts this <see cref="Box3"/> to a <see cref="Bounds3"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="Bounds3"/> with the same minimum position and dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bounds3 ToBounds()
        => new Bounds3(
            _minX,
            _minY,
            _minZ,
            _maxX - _minX,
            _maxY - _minY,
            _maxZ - _minZ);

    /// <summary>
    /// Determines whether the specified point is contained within the box.
    /// </summary>
    /// <param name="x">The x-coordinate of the point.</param>
    /// <param name="y">The y-coordinate of the point.</param>
    /// <param name="z">The z-coordinate of the point.</param>
    /// <returns>
    /// <see langword="true"/> if the point is inside the box;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The minimum boundaries are inclusive, while the maximum boundaries
    /// are exclusive.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(float x, float y, float z)
        => x >= _minX && x < _maxX
        && y >= _minY && y < _maxY
        && z >= _minZ && z < _maxZ;

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
    public bool Contains(in Vector3 v)
        => v.X >= _minX && v.X < _maxX
        && v.Y >= _minY && v.Y < _maxY
        && v.Z >= _minZ && v.Z < _maxZ;

    /// <summary>
    /// Determines whether this instance is equal to another
    /// <see cref="Box3"/> instance.
    /// </summary>
    /// <param name="other">The box to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if all minimum and maximum coordinates are
    /// identical; otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Box3 other)
        => _minX == other._minX
        && _minY == other._minY
        && _minZ == other._minZ
        && _maxX == other._maxX
        && _maxY == other._maxY
        && _maxZ == other._maxZ;

    /// <summary>
    /// Determines whether the specified object is equal to this instance.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="obj"/> is a
    /// <see cref="Box3"/> with identical coordinates; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
        => obj is Box3 b && Equals(b);

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
            hash = hash * 31 + _minZ.GetHashCode();
            hash = hash * 31 + _maxX.GetHashCode();
            hash = hash * 31 + _maxY.GetHashCode();
            hash = hash * 31 + _maxZ.GetHashCode();
            return hash;
        }
    }

    /// <summary>
    /// Returns the string representation of this <see cref="Box3"/>.
    /// </summary>
    /// <returns>
    /// A string containing the minimum x-coordinate, minimum y-coordinate,
    /// minimum z-coordinate, maximum x-coordinate, maximum y-coordinate,
    /// and maximum z-coordinate separated by commas.
    /// </returns>
    /// <remarks>
    /// Numeric values are formatted using
    /// <see cref="CultureInfo.InvariantCulture"/>.
    /// </remarks>
    public override string ToString()
        => string.Format(
            CultureInfo.InvariantCulture,
            "{0}, {1}, {2}, {3}, {4}, {5}",
            _minX,
            _minY,
            _minZ,
            _maxX,
            _maxY,
            _maxZ);
}
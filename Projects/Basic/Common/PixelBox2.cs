using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Represents a two-dimensional pixel-aligned bounding box defined by
/// minimum and maximum integer coordinates.
/// </summary>
/// <remarks>
/// Unlike <see cref="PixelBounds2"/>, which is defined by a position and
/// size, <see cref="PixelBox2"/> is defined directly by its minimum and
/// maximum coordinates.
/// </remarks>
public readonly struct PixelBox2 : IEquatable<PixelBox2>
{
    private readonly int _minX;
    private readonly int _minY;
    private readonly int _maxX;
    private readonly int _maxY;

    /// <summary>
    /// Represents a pixel box with all coordinates set to zero.
    /// </summary>
    public static readonly PixelBox2 Zero = new PixelBox2(0, 0, 0, 0);

    /// <summary>
    /// Initializes a new instance from the specified minimum and maximum
    /// coordinates.
    /// </summary>
    /// <param name="minX">The minimum X coordinate.</param>
    /// <param name="minY">The minimum Y coordinate.</param>
    /// <param name="maxX">The maximum X coordinate.</param>
    /// <param name="maxY">The maximum Y coordinate.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelBox2(int minX, int minY, int maxX, int maxY)
    {
        _minX = minX;
        _minY = minY;
        _maxX = maxX;
        _maxY = maxY;
    }

    /// <summary>
    /// Initializes a new instance from minimum and maximum pixel positions.
    /// </summary>
    /// <param name="min">The minimum pixel position.</param>
    /// <param name="max">The maximum pixel position.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelBox2(PixelPoint2 min, PixelPoint2 max)
        : this(min.X, min.Y, max.X, max.Y)
    {
    }

    /// <summary>
    /// Gets the minimum X coordinate.
    /// </summary>
    public int MinX => _minX;

    /// <summary>
    /// Gets the minimum Y coordinate.
    /// </summary>
    public int MinY => _minY;

    /// <summary>
    /// Gets the maximum X coordinate.
    /// </summary>
    public int MaxX => _maxX;

    /// <summary>
    /// Gets the maximum Y coordinate.
    /// </summary>
    public int MaxY => _maxY;

    /// <summary>
    /// Gets the width in pixels between the minimum and maximum X coordinates.
    /// </summary>
    public int Width => _maxX - _minX;

    /// <summary>
    /// Gets the height in pixels between the minimum and maximum Y coordinates.
    /// </summary>
    public int Height => _maxY - _minY;

    /// <summary>
    /// Gets the minimum pixel position of the box.
    /// </summary>
    public PixelPoint2 Min => new(_minX, _minY);

    /// <summary>
    /// Gets the maximum pixel position of the box.
    /// </summary>
    public PixelPoint2 Max => new(_maxX, _maxY);

    /// <summary>
    /// Converts this pixel box to pixel bounds.
    /// </summary>
    /// <returns>
    /// A <see cref="PixelBounds2"/> with the same minimum position and
    /// dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelBounds2 ToBounds()
        => new PixelBounds2(
            _minX,
            _minY,
            _maxX - _minX,
            _maxY - _minY);

    /// <summary>
    /// Determines whether the specified pixel coordinate is contained within
    /// this box.
    /// </summary>
    /// <param name="x">The X coordinate to test.</param>
    /// <param name="y">The Y coordinate to test.</param>
    /// <returns>
    /// <see langword="true"/> if the coordinate is contained within the box;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The minimum coordinates are inclusive, while the maximum coordinates
    /// are exclusive.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(int x, int y)
        => x >= _minX &&
           x < _maxX &&
           y >= _minY &&
           y < _maxY;

    /// <summary>
    /// Determines whether the specified pixel position is contained within
    /// this box.
    /// </summary>
    /// <param name="point">The pixel position to test.</param>
    /// <returns>
    /// <see langword="true"/> if the position is contained within the box;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The minimum coordinates are inclusive, while the maximum coordinates
    /// are exclusive.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in PixelPoint2 point)
        => point.X >= _minX &&
           point.X < _maxX &&
           point.Y >= _minY &&
           point.Y < _maxY;

    /// <summary>
    /// Determines whether this pixel box is equal to another pixel box.
    /// </summary>
    /// <param name="other">The pixel box to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if all minimum and maximum coordinates are equal;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(PixelBox2 other)
        => _minX == other._minX &&
           _minY == other._minY &&
           _maxX == other._maxX &&
           _maxY == other._maxY;

    /// <summary>
    /// Determines whether this pixel box is equal to the specified object.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="obj"/> is an equal
    /// <see cref="PixelBox2"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
        => obj is PixelBox2 b && Equals(b);

    /// <summary>
    /// Returns the hash code for this pixel box.
    /// </summary>
    /// <returns>
    /// A hash code based on the minimum and maximum coordinates.
    /// </returns>
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + _minX;
            hash = hash * 31 + _minY;
            hash = hash * 31 + _maxX;
            hash = hash * 31 + _maxY;
            return hash;
        }
    }

    /// <summary>
    /// Returns a string representation of this pixel box.
    /// </summary>
    /// <returns>
    /// A string containing the minimum X, minimum Y, maximum X,
    /// and maximum Y coordinates separated by commas.
    /// </returns>
    public override string ToString()
        => string.Format(
            CultureInfo.InvariantCulture,
            "{0}, {1}, {2}, {3}",
            _minX,
            _minY,
            _maxX,
            _maxY);
}
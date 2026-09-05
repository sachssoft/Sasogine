using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Represents a three-dimensional pixel-aligned bounding box defined by
/// minimum and maximum integer coordinates.
/// </summary>
/// <remarks>
/// Unlike <see cref="PixelBounds3"/>, which is defined by a position and size,
/// <see cref="PixelBox3"/> is defined directly by its minimum and maximum
/// coordinates.
/// </remarks>
public readonly struct PixelBox3 : IEquatable<PixelBox3>
{
    private readonly int _minX;
    private readonly int _minY;
    private readonly int _minZ;
    private readonly int _maxX;
    private readonly int _maxY;
    private readonly int _maxZ;

    /// <summary>
    /// Represents a pixel box with all coordinates set to zero.
    /// </summary>
    public static readonly PixelBox3 Zero =
        new PixelBox3(0, 0, 0, 0, 0, 0);

    /// <summary>
    /// Initializes a new instance from the specified minimum and maximum
    /// coordinates.
    /// </summary>
    /// <param name="minX">The minimum X coordinate.</param>
    /// <param name="minY">The minimum Y coordinate.</param>
    /// <param name="minZ">The minimum Z coordinate.</param>
    /// <param name="maxX">The maximum X coordinate.</param>
    /// <param name="maxY">The maximum Y coordinate.</param>
    /// <param name="maxZ">The maximum Z coordinate.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelBox3(
        int minX,
        int minY,
        int minZ,
        int maxX,
        int maxY,
        int maxZ)
    {
        _minX = minX;
        _minY = minY;
        _minZ = minZ;
        _maxX = maxX;
        _maxY = maxY;
        _maxZ = maxZ;
    }

    /// <summary>
    /// Initializes a new instance from minimum and maximum pixel positions.
    /// </summary>
    /// <param name="min">The minimum pixel position.</param>
    /// <param name="max">The maximum pixel position.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelBox3(PixelPoint3 min, PixelPoint3 max)
        : this(
            min.X,
            min.Y,
            min.Z,
            max.X,
            max.Y,
            max.Z)
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
    /// Gets the minimum Z coordinate.
    /// </summary>
    public int MinZ => _minZ;

    /// <summary>
    /// Gets the maximum X coordinate.
    /// </summary>
    public int MaxX => _maxX;

    /// <summary>
    /// Gets the maximum Y coordinate.
    /// </summary>
    public int MaxY => _maxY;

    /// <summary>
    /// Gets the maximum Z coordinate.
    /// </summary>
    public int MaxZ => _maxZ;

    /// <summary>
    /// Gets the width in pixels between the minimum and maximum X coordinates.
    /// </summary>
    public int Width => _maxX - _minX;

    /// <summary>
    /// Gets the height in pixels between the minimum and maximum Y coordinates.
    /// </summary>
    public int Height => _maxY - _minY;

    /// <summary>
    /// Gets the depth in pixels between the minimum and maximum Z coordinates.
    /// </summary>
    public int Depth => _maxZ - _minZ;

    /// <summary>
    /// Gets the minimum pixel position of the box.
    /// </summary>
    public PixelPoint3 Min => new(_minX, _minY, _minZ);

    /// <summary>
    /// Gets the maximum pixel position of the box.
    /// </summary>
    public PixelPoint3 Max => new(_maxX, _maxY, _maxZ);

    /// <summary>
    /// Converts this pixel box to pixel bounds.
    /// </summary>
    /// <returns>
    /// A <see cref="PixelBounds3"/> with the same minimum position
    /// and dimensions.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelBounds3 ToBounds()
        => new PixelBounds3(
            _minX,
            _minY,
            _minZ,
            _maxX - _minX,
            _maxY - _minY,
            _maxZ - _minZ);

    /// <summary>
    /// Determines whether the specified pixel coordinate is contained
    /// within this box.
    /// </summary>
    /// <param name="x">The X coordinate to test.</param>
    /// <param name="y">The Y coordinate to test.</param>
    /// <param name="z">The Z coordinate to test.</param>
    /// <returns>
    /// <see langword="true"/> if the coordinate is contained within the box;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The minimum coordinates are inclusive, while the maximum coordinates
    /// are exclusive.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(int x, int y, int z)
        => x >= _minX &&
           x < _maxX &&
           y >= _minY &&
           y < _maxY &&
           z >= _minZ &&
           z < _maxZ;

    /// <summary>
    /// Determines whether the specified pixel position is contained
    /// within this box.
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
    public bool Contains(in PixelPoint3 point)
        => point.X >= _minX &&
           point.X < _maxX &&
           point.Y >= _minY &&
           point.Y < _maxY &&
           point.Z >= _minZ &&
           point.Z < _maxZ;

    /// <summary>
    /// Determines whether this pixel box is equal to another pixel box.
    /// </summary>
    /// <param name="other">The pixel box to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if all minimum and maximum coordinates are equal;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(PixelBox3 other)
        => _minX == other._minX &&
           _minY == other._minY &&
           _minZ == other._minZ &&
           _maxX == other._maxX &&
           _maxY == other._maxY &&
           _maxZ == other._maxZ;

    /// <summary>
    /// Determines whether this pixel box is equal to the specified object.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="obj"/> is an equal
    /// <see cref="PixelBox3"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
        => obj is PixelBox3 b && Equals(b);

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
            hash = hash * 31 + _minZ;
            hash = hash * 31 + _maxX;
            hash = hash * 31 + _maxY;
            hash = hash * 31 + _maxZ;
            return hash;
        }
    }

    /// <summary>
    /// Determines whether two pixel boxes are equal.
    /// </summary>
    /// <param name="a">The first pixel box.</param>
    /// <param name="b">The second pixel box.</param>
    /// <returns>
    /// <see langword="true"/> if all minimum and maximum coordinates are equal;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(
        in PixelBox3 a,
        in PixelBox3 b)
        => a.Equals(b);

    /// <summary>
    /// Determines whether two pixel boxes are not equal.
    /// </summary>
    /// <param name="a">The first pixel box.</param>
    /// <param name="b">The second pixel box.</param>
    /// <returns>
    /// <see langword="true"/> if at least one coordinate differs;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(
        in PixelBox3 a,
        in PixelBox3 b)
        => !a.Equals(b);

    /// <summary>
    /// Parses a string representation of a pixel box.
    /// </summary>
    /// <param name="s">
    /// The string containing the minimum X, minimum Y, minimum Z,
    /// maximum X, maximum Y, and maximum Z coordinates.
    /// </param>
    /// <returns>
    /// The parsed <see cref="PixelBox3"/>.
    /// </returns>
    /// <exception cref="FormatException">
    /// Thrown when the string does not contain exactly six valid integer
    /// values.
    /// </exception>
    public static PixelBox3 Parse(string s)
    {
        if (TryParse(s, out var result))
            return result;

        throw new FormatException(
            $"Invalid PixelBox3 format: '{s}'. Expected 6 integer values separated by ',' or ' '.");
    }

    /// <summary>
    /// Attempts to parse a string representation of a pixel box.
    /// </summary>
    /// <param name="s">
    /// The string containing the minimum X, minimum Y, minimum Z,
    /// maximum X, maximum Y, and maximum Z coordinates.
    /// </param>
    /// <param name="result">
    /// When this method returns <see langword="true"/>, contains the parsed
    /// pixel box; otherwise, contains <see cref="Zero"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if parsing was successful; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public static bool TryParse(
        string? s,
        out PixelBox3 result)
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
            int minX = int.Parse(
                parts[0],
                CultureInfo.InvariantCulture);

            int minY = int.Parse(
                parts[1],
                CultureInfo.InvariantCulture);

            int minZ = int.Parse(
                parts[2],
                CultureInfo.InvariantCulture);

            int maxX = int.Parse(
                parts[3],
                CultureInfo.InvariantCulture);

            int maxY = int.Parse(
                parts[4],
                CultureInfo.InvariantCulture);

            int maxZ = int.Parse(
                parts[5],
                CultureInfo.InvariantCulture);

            result = new PixelBox3(
                minX,
                minY,
                minZ,
                maxX,
                maxY,
                maxZ);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Returns a string representation of this pixel box.
    /// </summary>
    /// <returns>
    /// A string containing the minimum X, minimum Y, minimum Z,
    /// maximum X, maximum Y, and maximum Z coordinates separated
    /// by commas.
    /// </returns>
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
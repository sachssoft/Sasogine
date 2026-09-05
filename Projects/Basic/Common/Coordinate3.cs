using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Represents an immutable three-dimensional grid coordinate (X, Y, Z).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Coordinate3"/> is intended for discrete grid-based systems such as:
/// voxel worlds, 3D tile maps, layer systems, chunk indexing, and pathfinding.
/// </para>
/// <para>
/// The structure uses integer coordinates exclusively and does not represent
/// continuous world-space positions.
/// </para>
/// <para>
/// Use <see cref="Point3"/> for continuous positions and
/// <see cref="PixelPoint3"/> for discrete pixel-based positions.
/// </para>
/// </remarks>
public readonly struct Coordinate3
{
    /// <summary>
    /// Gets the coordinate (0, 0, 0).
    /// </summary>
    public static Coordinate3 Zero => new(0, 0, 0);

    /// <summary>
    /// Gets the coordinate (1, 1, 1).
    /// </summary>
    public static Coordinate3 One => new(1, 1, 1);

    /// <summary>
    /// Gets the unit direction on the X axis (1, 0, 0).
    /// </summary>
    public static Coordinate3 UnitX => new(1, 0, 0);

    /// <summary>
    /// Gets the unit direction on the Y axis (0, 1, 0).
    /// </summary>
    public static Coordinate3 UnitY => new(0, 1, 0);

    /// <summary>
    /// Gets the unit direction on the Z axis (0, 0, 1).
    /// </summary>
    public static Coordinate3 UnitZ => new(0, 0, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="Coordinate3"/> structure.
    /// </summary>
    /// <param name="x">The X grid coordinate.</param>
    /// <param name="y">The Y grid coordinate.</param>
    /// <param name="z">The Z grid coordinate.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Coordinate3(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Coordinate3"/> structure
    /// from a pixel point.
    /// </summary>
    /// <param name="point">The pixel point.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Coordinate3(PixelPoint3 point)
    {
        X = point.X;
        Y = point.Y;
        Z = point.Z;
    }

    /// <summary>
    /// Gets the X grid coordinate.
    /// </summary>
    public int X { get; }

    /// <summary>
    /// Gets the Y grid coordinate.
    /// </summary>
    public int Y { get; }

    /// <summary>
    /// Gets the Z grid coordinate.
    /// </summary>
    public int Z { get; }

    /// <summary>
    /// Deconstructs the coordinate into its X, Y, and Z components.
    /// </summary>
    /// <param name="x">The X component.</param>
    /// <param name="y">The Y component.</param>
    /// <param name="z">The Z component.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out int x, out int y, out int z)
    {
        x = X;
        y = Y;
        z = Z;
    }

    /// <summary>
    /// Creates a copy of this coordinate with a new X component.
    /// </summary>
    /// <param name="x">The new X component.</param>
    /// <returns>The resulting coordinate.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Coordinate3 WithX(int x)
        => new(x, Y, Z);

    /// <summary>
    /// Creates a copy of this coordinate with a new Y component.
    /// </summary>
    /// <param name="y">The new Y component.</param>
    /// <returns>The resulting coordinate.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Coordinate3 WithY(int y)
        => new(X, y, Z);

    /// <summary>
    /// Creates a copy of this coordinate with a new Z component.
    /// </summary>
    /// <param name="z">The new Z component.</param>
    /// <returns>The resulting coordinate.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Coordinate3 WithZ(int z)
        => new(X, Y, z);

    /// <summary>
    /// Creates a new coordinate by adding the specified components.
    /// </summary>
    /// <param name="x">The X offset.</param>
    /// <param name="y">The Y offset.</param>
    /// <param name="z">The Z offset.</param>
    /// <returns>The resulting coordinate.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Coordinate3 Add(int x, int y, int z)
        => new(X + x, Y + y, Z + z);

    /// <summary>
    /// Creates a new coordinate by adding another coordinate.
    /// </summary>
    /// <param name="other">The coordinate to add.</param>
    /// <returns>The resulting coordinate.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Coordinate3 Add(Coordinate3 other)
        => new(
            X + other.X,
            Y + other.Y,
            Z + other.Z);

    /// <summary>
    /// Returns the coordinate with all components negated.
    /// </summary>
    /// <returns>The negated coordinate.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Coordinate3 Negative()
        => new(-X, -Y, -Z);

    /// <summary>
    /// Normalizes each component to -1, 0, or 1.
    /// </summary>
    /// <returns>The normalized direction coordinate.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Coordinate3 NormalizeDirection()
    {
        int nx = X == 0 ? 0 : X > 0 ? 1 : -1;
        int ny = Y == 0 ? 0 : Y > 0 ? 1 : -1;
        int nz = Z == 0 ? 0 : Z > 0 ? 1 : -1;

        return new Coordinate3(nx, ny, nz);
    }

    /// <summary>
    /// Determines whether this coordinate and another coordinate
    /// are on the same Z layer.
    /// </summary>
    /// <param name="other">The coordinate to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both coordinates have the same Z component;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool SameLayer(Coordinate3 other)
        => Z == other.Z;

    /// <summary>
    /// Projects this coordinate onto the XY plane.
    /// </summary>
    /// <returns>The resulting two-dimensional coordinate.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Coordinate2 ToXY()
        => new(X, Y);

    /// <summary>
    /// Projects this coordinate onto the XZ plane.
    /// </summary>
    /// <returns>The resulting two-dimensional coordinate.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Coordinate2 ToXZ()
        => new(X, Z);

    /// <summary>
    /// Projects this coordinate onto the YZ plane.
    /// </summary>
    /// <returns>The resulting two-dimensional coordinate.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Coordinate2 ToYZ()
        => new(Y, Z);

    /// <summary>
    /// Converts this coordinate into a <see cref="PixelPoint3"/>.
    /// </summary>
    /// <returns>The resulting pixel point.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelPoint3 ToPixelPoint3()
        => new(X, Y, Z);

    /// <summary>
    /// Converts this coordinate into a <see cref="PixelPoint3"/> position
    /// using a uniform pixel cell size.
    /// </summary>
    /// <param name="cellSize">The uniform cell size in pixels.</param>
    /// <returns>The resulting pixel position.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelPoint3 ToPixelPoint3(int cellSize)
        => new(
            X * cellSize,
            Y * cellSize,
            Z * cellSize);

    /// <summary>
    /// Converts this coordinate into a <see cref="PixelPoint3"/> position
    /// using the specified pixel cell size.
    /// </summary>
    /// <param name="cellSize">The cell size in pixels.</param>
    /// <returns>The resulting pixel position.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelPoint3 ToPixelPoint3(PixelSize3 cellSize)
        => new(
            X * cellSize.Width,
            Y * cellSize.Height,
            Z * cellSize.Depth);

    /// <summary>
    /// Converts this coordinate into a <see cref="Point3"/>.
    /// </summary>
    /// <returns>The resulting three-dimensional position.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point3 ToPoint3()
        => new(X, Y, Z);

    /// <summary>
    /// Converts this coordinate into a <see cref="Point3"/> position
    /// using a uniform cell size.
    /// </summary>
    /// <param name="cellSize">The uniform cell size.</param>
    /// <returns>The resulting three-dimensional position.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point3 ToPoint3(float cellSize)
        => new(
            X * cellSize,
            Y * cellSize,
            Z * cellSize);

    /// <summary>
    /// Converts this coordinate into a <see cref="Point3"/> position
    /// using the specified cell size.
    /// </summary>
    /// <param name="cellSize">The cell size.</param>
    /// <returns>The resulting three-dimensional position.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point3 ToPoint3(Size3 cellSize)
        => new(
            X * cellSize.Width,
            Y * cellSize.Height,
            Z * cellSize.Depth);

    /// <summary>
    /// Converts this coordinate into a linear index for a three-dimensional array.
    /// </summary>
    /// <param name="width">The width of the grid.</param>
    /// <param name="height">The height of the grid.</param>
    /// <returns>The corresponding linear array index.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ToIndex(int width, int height)
        => X + Y * width + Z * width * height;

    /// <summary>
    /// Calculates the Manhattan distance to another coordinate.
    /// </summary>
    /// <param name="other">The other coordinate.</param>
    /// <returns>The Manhattan distance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ManhattanDistance(Coordinate3 other)
        => int.Abs(X - other.X)
         + int.Abs(Y - other.Y)
         + int.Abs(Z - other.Z);

    /// <summary>
    /// Calculates the Euclidean distance to another coordinate.
    /// </summary>
    /// <param name="other">The other coordinate.</param>
    /// <returns>The Euclidean distance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float EuclideanDistance(Coordinate3 other)
    {
        int dx = X - other.X;
        int dy = Y - other.Y;
        int dz = Z - other.Z;

        return MathF.Sqrt(
            dx * dx +
            dy * dy +
            dz * dz);
    }

    /// <summary>
    /// Returns the six directly adjacent coordinates in the three-dimensional grid.
    /// </summary>
    /// <returns>The six directly adjacent coordinates.</returns>
    public IEnumerable<Coordinate3> GetNeighbors6()
    {
        yield return new Coordinate3(X + 1, Y, Z);
        yield return new Coordinate3(X - 1, Y, Z);
        yield return new Coordinate3(X, Y + 1, Z);
        yield return new Coordinate3(X, Y - 1, Z);
        yield return new Coordinate3(X, Y, Z + 1);
        yield return new Coordinate3(X, Y, Z - 1);
    }

    /// <summary>
    /// Returns all 26 neighboring coordinates in the three-dimensional grid,
    /// including diagonal neighbors.
    /// </summary>
    /// <returns>All 26 neighboring coordinates.</returns>
    public IEnumerable<Coordinate3> GetNeighbors26()
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    if (dx == 0 && dy == 0 && dz == 0)
                        continue;

                    yield return new Coordinate3(
                        X + dx,
                        Y + dy,
                        Z + dz);
                }
            }
        }
    }

    /// <summary>
    /// Determines whether two coordinates are equal.
    /// </summary>
    public static bool operator ==(
        Coordinate3 left,
        Coordinate3 right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two coordinates are different.
    /// </summary>
    public static bool operator !=(
        Coordinate3 left,
        Coordinate3 right)
        => !left.Equals(right);

    /// <summary>
    /// Adds two coordinates.
    /// </summary>
    public static Coordinate3 operator +(
        Coordinate3 left,
        Coordinate3 right)
        => new(
            left.X + right.X,
            left.Y + right.Y,
            left.Z + right.Z);

    /// <summary>
    /// Subtracts one coordinate from another.
    /// </summary>
    public static Coordinate3 operator -(
        Coordinate3 left,
        Coordinate3 right)
        => new(
            left.X - right.X,
            left.Y - right.Y,
            left.Z - right.Z);

    /// <summary>
    /// Negates a coordinate.
    /// </summary>
    public static Coordinate3 operator -(Coordinate3 value)
        => new(
            -value.X,
            -value.Y,
            -value.Z);

    /// <summary>
    /// Converts a <see cref="PixelPoint3"/> into a <see cref="Coordinate3"/>.
    /// </summary>
    public static implicit operator Coordinate3(PixelPoint3 point)
        => new(point.X, point.Y, point.Z);

    /// <summary>
    /// Converts a <see cref="Coordinate3"/> into a <see cref="PixelPoint3"/>.
    /// </summary>
    public static explicit operator PixelPoint3(Coordinate3 coordinate)
        => new(
            coordinate.X,
            coordinate.Y,
            coordinate.Z);

    /// <summary>
    /// Returns a string representation of this coordinate
    /// in the format "X,Y,Z".
    /// </summary>
    /// <returns>The string representation of this coordinate.</returns>
    public override string ToString()
        => $"{X},{Y},{Z}";

    /// <summary>
    /// Determines whether the specified object is equal to this coordinate.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns>
    /// <see langword="true"/> if the object represents the same coordinate;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
        => obj is Coordinate3 coordinate
        && coordinate.X == X
        && coordinate.Y == Y
        && coordinate.Z == Z;

    /// <summary>
    /// Returns the hash code for this coordinate.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode()
        => HashCode.Combine(X, Y, Z);
}
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Repräsentiert eine unveränderliche diskrete 2D-Koordinate (X, Y)
/// für rasterbasierte Systeme.
/// 
/// Coordinate2 wird verwendet, um Positionen innerhalb eines
/// zweidimensionalen Gitters zu beschreiben, beispielsweise für:
/// - Tilemaps
/// - Level-Editoren
/// - Chunk-Systeme
/// - Pathfinding
/// - Grid-basierte Spiele
/// - zellenbasierte Simulationen
/// 
/// Die Struktur arbeitet ausschließlich mit ganzzahligen Werten
/// und repräsentiert keine kontinuierlichen Weltpositionen.
/// Für Welt-, Bildschirm- oder Physikkoordinaten sollten Vector2
/// oder Point verwendet werden.
/// </summary>
/// <remarks>
/// Coordinate2 eignet sich besonders für diskrete Spiellogik,
/// bei der jede Position eine einzelne Zelle eines Rasters darstellt.
/// 
/// Die Struktur bietet Hilfsfunktionen für:
/// - Rasterkonvertierung zwischen Weltposition und Zelle
/// - Nachbarschaftsabfragen
/// - Richtungsberechnungen
/// - Rotation und Spiegelung
/// - Distanzberechnung
/// - Index-Konvertierung für lineare Speicherstrukturen
/// 
/// Durch die unveränderliche readonly-Struktur ist Coordinate2
/// sicher für deterministische Systeme und kann effizient als
/// Werttyp verwendet werden.
/// </remarks>
public readonly struct Coordinate2
{
    #region Constants

    /// <summary>
    /// Gets the coordinate (0,0).
    /// </summary>
    public static Coordinate2 Zero => new(0, 0);

    /// <summary>
    /// Gets the coordinate (1,1).
    /// </summary>
    public static Coordinate2 One => new(1, 1);

    /// <summary>
    /// Gets the unit direction on the X axis (1,0).
    /// </summary>
    public static Coordinate2 UnitX => new(1, 0);

    /// <summary>
    /// Gets the unit direction on the Y axis (0,1).
    /// </summary>
    public static Coordinate2 UnitY => new(0, 1);

    #endregion

    #region Constructors


    /// <summary>Erstellt eine neue 2D-Koordinate.</summary>
    public Coordinate2(int x, int y)
    {
        X = x;
        Y = y;
    }

    /// <summary>Erstellt eine Koordinate aus einem MonoGame Point.</summary>
    public Coordinate2(Point point)
    {
        X = point.X;
        Y = point.Y;
    }

    /// <summary>Zerlegt die Koordinate in X und Y.</summary>
    public void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }

    /// <summary>
    /// Creates a copy of this coordinate with a new X component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Coordinate2 WithX(int x) => new(x, Y);

    /// <summary>
    /// Creates a copy of this coordinate with a new Y component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Coordinate2 WithY(int y) => new(X, y);

    #endregion

    #region Properties

    /// <summary>
    /// Gets the horizontal grid coordinate.
    /// </summary>
    public int X { get; }

    /// <summary>
    /// Gets the vertical grid coordinate.
    /// </summary>
    public int Y { get; }

    /// <summary>
    /// Gets the X component as a column index.
    /// </summary>
    public int Column => X;

    /// <summary>
    /// Gets the Y component as a row index.
    /// </summary>
    public int Row => Y;

    /// <summary>
    /// Gets the coordinate of the neighboring cell to the left.
    /// </summary>
    public Coordinate2 Left => new(X - 1, Y);

    /// <summary>
    /// Gets the coordinate of the neighboring cell to the right.
    /// </summary>
    public Coordinate2 Right => new(X + 1, Y);

    /// <summary>
    /// Gets the coordinate of the neighboring cell above.
    /// </summary>
    public Coordinate2 Up => new(X, Y - 1);

    /// <summary>
    /// Gets the coordinate of the neighboring cell below.
    /// </summary>
    public Coordinate2 Down => new(X, Y + 1);

    #endregion

    #region Grid Operations

    /// <summary>
    /// Converts a world position into a grid coordinate using snap rules.
    /// Even tile sizes snap to the nearest tile center.
    /// Odd tile sizes snap to the containing tile.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Coordinate2 Snap(Vector2 position, Size tileSize)
    {
        int x = tileSize.Width % 2 == 0 ?
            (int)float.Round(position.X / tileSize.Width) :
            (int)float.Floor(position.X / tileSize.Width);

        int y = tileSize.Height % 2 == 0 ?
            (int)float.Round(position.Y / tileSize.Height) :
            (int)float.Floor(position.Y / tileSize.Height);

        return new Coordinate2(x, y);
    }

    /// <summary>
    /// Converts a world position into the containing grid cell.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Coordinate2 Floor(Vector2 position, Size tileSize)
    {
        int x = (int)float.Floor(position.X / tileSize.Width);
        int y = (int)float.Floor(position.Y / tileSize.Height);

        return new Coordinate2(x, y);
    }

    /// <summary>
    /// Converts a world position into the nearest grid cell.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Coordinate2 Round(Vector2 position, Size tileSize)
    {
        int x = (int)float.Round(position.X / tileSize.Width);
        int y = (int)float.Round(position.Y / tileSize.Height);

        return new Coordinate2(x, y);
    }

    /// <summary>
    /// Converts a world position into the next grid cell.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Coordinate2 Ceiling(Vector2 position, Size tileSize)
    {
        int x = (int)float.Ceiling(position.X / tileSize.Width);
        int y = (int)float.Ceiling(position.Y / tileSize.Height);

        return new Coordinate2(x, y);
    }

    /// <summary>
    /// Rotates the coordinate 90 degrees clockwise.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 Center(Size tileSize)
    {
        return new Vector2(
            (X + 0.5f) * tileSize.Width,
            (Y + 0.5f) * tileSize.Height);
    }

    /// <summary>
    /// Converts this coordinate into a MonoGame Point.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point ToPoint() => new(X, Y);

    /// <summary>
    /// Converts this coordinate into a Vector2 in grid space.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 ToVector2() => new(X, Y);

    /// <summary>
    /// Converts this coordinate into a Vector2 position using a uniform tile size.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 ToVector2(int tileSize)
        => new(X * tileSize, Y * tileSize);

    /// <summary>
    /// Converts this coordinate into a Vector2 position using a uniform tile size.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 ToVector2(float tileSize)
        => new(X * tileSize, Y * tileSize);

    /// <summary>
    /// Converts this coordinate into a Vector2 position using the specified tile size.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 ToVector2(Size tileSize)
        => new(X * tileSize.Width, Y * tileSize.Height);

    /// <summary>
    /// Converts this coordinate into a Vector2 position using the specified pixel size.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 ToVector2(PixelSize tileSize)
        => new(X * tileSize.Width, Y * tileSize.Height);

    /// <summary>
    /// Converts this coordinate into a Vector2 position using the specified tile size.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 ToVector2(Vector2 tileSize)
        => new(X * tileSize.X, Y * tileSize.Y);

    /// <summary>
    /// Converts this coordinate into a Vector3 in grid space with the specified layer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 ToVector3(float layer)
        => new(X, Y, layer);

    /// <summary>
    /// Converts this coordinate into a Vector3 position using a uniform tile size and layer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 ToVector3(int tileSize, float layer)
        => new(X * tileSize, Y * tileSize, layer);

    /// <summary>
    /// Converts this coordinate into a Vector3 position using a uniform tile size and layer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 ToVector3(float tileSize, float layer)
        => new(X * tileSize, Y * tileSize, layer);

    /// <summary>
    /// Converts this coordinate into a Vector3 position using the specified pixel size and layer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 ToVector3(PixelSize tileSize, float layer)
        => new(X * tileSize.Width, Y * tileSize.Height, layer);

    /// <summary>
    /// Converts this coordinate into a Vector3 position using the specified tile size and layer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 ToVector3(Size tileSize, float layer)
        => new(X * tileSize.Width, Y * tileSize.Height, layer);

    /// <summary>
    /// Converts this coordinate into a linear array index.
    /// </summary>
    /// <param name="width">The width of the grid.</param>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ToIndex(int width)
        // z.b. tiles[position.ToIndex(mapWidth)]
        => Y * width + X;

    /// <summary>
    /// Creates a coordinate from a linear array index.
    /// </summary>
    /// <param name="index">The array index.</param>
    /// <param name="width">The width of the grid.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Coordinate2 FromIndex(
        int index,
        int width)
    {
        return new(
            index % width,
            index / width);
    }

    #endregion

    #region Operations

    /// <summary>
    /// Determines whether this coordinate is outside the specified grid bounds.
    /// </summary>
    public bool IsOutOfBorder(int columns, int rows)
        => X < 0 || Y < 0 || X >= columns || Y >= rows;

    /// <summary>
    /// Determines whether this coordinate is inside the specified grid size.
    /// </summary>
    public bool IsInside(
    Coordinate2 size)
    {
        return X >= 0 &&
               Y >= 0 &&
               X < size.X &&
               Y < size.Y;
    }

    /// <summary>
    /// Determines whether this coordinate is equal to (0,0).
    /// </summary>
    public bool IsZero() => X == 0 && Y == 0;

    /// <summary>
    /// Calculates the Manhattan distance to another coordinate.
    /// </summary>
    public int ManhattanDistance(Coordinate2 other)
        => int.Abs(X - other.X) + int.Abs(Y - other.Y);

    /// <summary>
    /// Calculates the Euclidean distance to another coordinate.
    /// </summary>
    public float EuclideanDistance(Coordinate2 other)
    {
        int dx = X - other.X;
        int dy = Y - other.Y;

        return MathF.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// Creates a new coordinate by adding an offset.
    /// </summary>
    public Coordinate2 Add(int x, int y) => new(X + x, Y + y);

    /// <summary>
    /// Creates a new coordinate by adding another coordinate.
    /// </summary>
    public Coordinate2 Add(Coordinate2 other) => new(X + other.X, Y + other.Y);

    /// <summary>
    /// Moves this coordinate by the specified direction.
    /// </summary>
    public Coordinate2 Move(Coordinate2 direction)
    => new(X + direction.X, Y + direction.Y);

    /// <summary>
    /// Calculates the direction from one coordinate to another.
    /// </summary>
    public static Coordinate2 DirectionTo(
    Coordinate2 from,
    Coordinate2 to)
    {
        return new(
            Math.Sign(to.X - from.X),
            Math.Sign(to.Y - from.Y));
    }

    /// <summary>
    /// Normalizes the coordinate components to -1, 0 or 1.
    /// </summary>
    public Coordinate2 NormalizeDirection()
    {
        int nx = X == 0 ? 0 : (X > 0 ? 1 : -1);
        int ny = Y == 0 ? 0 : (Y > 0 ? 1 : -1);
        return new(nx, ny);
    }
    /// <summary>
    /// Gets the neighboring coordinates.
    /// </summary>
    /// <param name="includeDiagonals">
    /// Indicates whether diagonal neighbors should be included.
    /// </param>
    public IEnumerable<Coordinate2> GetNeighbors(bool includeDiagonals = false)
    {
        yield return this + UnitY.Negative();
        yield return this + UnitY;
        yield return this + UnitX.Negative();
        yield return this + UnitX;

        if (includeDiagonals)
        {
            yield return new Coordinate2(-1, -1) + this;
            yield return new Coordinate2(1, -1) + this;
            yield return new Coordinate2(-1, 1) + this;
            yield return new Coordinate2(1, 1) + this;
        }
    }

    #endregion

    #region Transformations

    /// <summary>
    /// Clamps a coordinate between minimum and maximum values.
    /// </summary>
    public static Coordinate2 Clamp(Coordinate2 value, Coordinate2 min, Coordinate2 max)
        => new(
            int.Clamp(value.X, min.X, max.X),
            int.Clamp(value.Y, min.Y, max.Y)
        );

    /// <summary>
    /// Returns a coordinate with the smallest components of two coordinates.
    /// </summary>
    public static Coordinate2 Min(Coordinate2 a, Coordinate2 b)
        => new(int.Min(a.X, b.X), int.Min(a.Y, b.Y));

    /// <summary>
    /// Returns a coordinate with the largest components of two coordinates.
    /// </summary>
    public static Coordinate2 Max(Coordinate2 a, Coordinate2 b)
        => new(int.Max(a.X, b.X), int.Max(a.Y, b.Y));

    /// <summary>
    /// Returns the coordinate with inverted components.
    /// </summary>
    public Coordinate2 Negative() => new(-X, -Y);

    /// <summary>
    /// Mirrors the coordinate horizontally.
    /// </summary>
    public Coordinate2 FlipHorizontal(int width) => new(width - 1 - X, Y);

    /// <summary>
    /// Mirrors the coordinate vertically.
    /// </summary>
    public Coordinate2 FlipVertical(int height) => new(X, height - 1 - Y);

    /// <summary>
    /// Mirrors the coordinate on both axes.
    /// </summary>
    public Coordinate2 FlipBoth(int width, int height) => new(width - 1 - X, height - 1 - Y);

    /// <summary>
    /// Rotates the coordinate 90 degrees clockwise.
    /// </summary>
    public Coordinate2 Rotate90()
    => new(-Y, X);

    /// <summary>
    /// Rotates the coordinate 180 degrees.
    /// </summary>
    public Coordinate2 Rotate180()
        => new(-X, -Y);

    /// <summary>
    /// Rotates the coordinate 270 degrees clockwise.
    /// </summary>
    public Coordinate2 Rotate270()
        => new(Y, -X);

    #endregion

    #region Operators

    /// <summary>
    /// Determines whether two coordinates are equal.
    /// </summary>
    public static bool operator ==(Coordinate2 a, Coordinate2 b) => a.Equals(b);

    /// <summary>
    /// Determines whether two coordinates are different.
    /// </summary>
    public static bool operator !=(Coordinate2 a, Coordinate2 b) => !a.Equals(b);

    /// <summary>
    /// Adds two coordinates.
    /// </summary>
    public static Coordinate2 operator +(Coordinate2 a, Coordinate2 b) => new(a.X + b.X, a.Y + b.Y);

    /// <summary>
    /// Subtracts one coordinate from another.
    /// </summary>
    public static Coordinate2 operator -(Coordinate2 a, Coordinate2 b) => new(a.X - b.X, a.Y - b.Y);

    /// <summary>
    /// Negates a coordinate.
    /// </summary>
    public static Coordinate2 operator -(Coordinate2 v) => new(-v.X, -v.Y);

    /// <summary>
    /// Multiplies a coordinate by a scalar value.
    /// </summary>
    public static Coordinate2 operator *(Coordinate2 v, int s) => new(v.X * s, v.Y * s);

    /// <summary>
    /// Divides a coordinate by a scalar value.
    /// </summary>
    public static Coordinate2 operator /(Coordinate2 v, int s) => new(v.X / s, v.Y / s);

    /// <summary>
    /// Converts a Point into a Coordinate2.
    /// </summary>
    public static implicit operator Coordinate2(Point p) => new(p.X, p.Y);

    /// <summary>
    /// Converts a Coordinate2 into a Point.
    /// </summary>
    public static explicit operator Point(Coordinate2 c) => new(c.X, c.Y);

    /// <summary>
    /// Converts a Vector2 into a Coordinate2.
    /// </summary>
    public static implicit operator Coordinate2(Vector2 v) => new((int)v.X, (int)v.Y);

    /// <summary>
    /// Converts a Coordinate2 into a Vector2.
    /// </summary>
    public static explicit operator Vector2(Coordinate2 c) => new(c.X, c.Y);

    #endregion

    #region #region Overrides

    /// <summary>
    /// Returns a string representation in the format "X,Y".
    /// </summary>
    public override string ToString() => $"{X},{Y}";

    /// <summary>
    /// Determines whether this coordinate equals another object.
    /// </summary>
    public override bool Equals(object? obj)
        => obj is Coordinate2 c && c.X == X && c.Y == Y;

    /// <summary>
    /// Returns the hash code for this coordinate.
    /// </summary>
    public override int GetHashCode() => HashCode.Combine(X, Y);

    #endregion
}
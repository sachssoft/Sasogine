using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Repräsentiert eine unveränderliche 2D-Gitterkoordinate (X, Y).
/// Wird für Tilemaps, Grid-Systeme, Pathfinding und Chunk-basierte Welten verwendet.
/// </summary>
/// <remarks>
/// Diese Struktur ist für diskrete Rasterlogik gedacht und nicht für kontinuierliche Geometrie.
/// Sie arbeitet rein integer-basiert und ist damit stabil für deterministische Spielsysteme.
/// </remarks>
public readonly struct Coordinate2
{
    /// <summary>Koordinate (0,0).</summary>
    public static Coordinate2 Zero => new(0, 0);

    /// <summary>Koordinate (1,1).</summary>
    public static Coordinate2 One => new(1, 1);

    /// <summary>Einheitsvektor X (1,0).</summary>
    public static Coordinate2 UnitX => new(1, 0);

    /// <summary>Einheitsvektor Y (0,1).</summary>
    public static Coordinate2 UnitY => new(0, 1);

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

    /// <summary>X-Komponente (Spalte im Grid).</summary>
    public int X { get; }

    /// <summary>Y-Komponente (Zeile im Grid).</summary>
    public int Y { get; }

    /// <summary>Alias für X (Spalte).</summary>
    public int Column => X;

    /// <summary>Alias für Y (Zeile).</summary>
    public int Row => Y;

    /// <summary>Zerlegt die Koordinate in X und Y.</summary>
    public void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }

    /// <summary>Linker Nachbar.</summary>
    public Coordinate2 Left => new(X - 1, Y);

    /// <summary>Rechter Nachbar.</summary>
    public Coordinate2 Right => new(X + 1, Y);

    /// <summary>Oberer Nachbar.</summary>
    public Coordinate2 Up => new(X, Y - 1);

    /// <summary>Unterer Nachbar.</summary>
    public Coordinate2 Down => new(X, Y + 1);

    /// <summary>Konvertiert in MonoGame Point.</summary>
    public Point ToPoint() => new(X, Y);

    /// <summary>Konvertiert in Vector2 im Grid-Space.</summary>
    public Vector2 ToVector2() => new(X, Y);

    /// <summary>Konvertiert in Weltposition basierend auf Tilegröße.</summary>
    public Vector2 ToVector2(Vector2 tileSize)
        => new(X * tileSize.X, Y * tileSize.Y);

    /// <summary>Ändert nur X.</summary>
    public Coordinate2 WithX(int x) => new(x, Y);

    /// <summary>Ändert nur Y.</summary>
    public Coordinate2 WithY(int y) => new(X, y);

    /// <summary>Prüft, ob außerhalb eines Gitters liegt.</summary>
    public bool IsOutOfBorder(int columns, int rows)
        => X < 0 || Y < 0 || X >= columns || Y >= rows;

    /// <summary>Prüft, ob (0,0).</summary>
    public bool IsZero() => X == 0 && Y == 0;

    /// <summary>Manhattan-Distanz.</summary>
    public int ManhattanDistance(Coordinate2 other)
        => int.Abs(X - other.X) + int.Abs(Y - other.Y);

    /// <summary>Euklidische Distanz.</summary>
    public float EuclideanDistance(Coordinate2 other)
        => float.Sqrt(float.Pow(X - other.X, 2) + float.Pow(Y - other.Y, 2));

    /// <summary>Verschiebt um Offset.</summary>
    public Coordinate2 Add(int x, int y) => new(X + x, Y + y);

    /// <summary>Verschiebt um andere Koordinate.</summary>
    public Coordinate2 Add(Coordinate2 other) => new(X + other.X, Y + other.Y);

    /// <summary>Begrenzt auf min/max Bereich.</summary>
    public static Coordinate2 Clamp(Coordinate2 value, Coordinate2 min, Coordinate2 max)
        => new(
            int.Clamp(value.X, min.X, max.X),
            int.Clamp(value.Y, min.Y, max.Y)
        );

    /// <summary>Kleinste Komponente.</summary>
    public static Coordinate2 Min(Coordinate2 a, Coordinate2 b)
        => new(int.Min(a.X, b.X), int.Min(a.Y, b.Y));

    /// <summary>Größte Komponente.</summary>
    public static Coordinate2 Max(Coordinate2 a, Coordinate2 b)
        => new(int.Max(a.X, b.X), int.Max(a.Y, b.Y));

    /// <summary>Negiert die Koordinate.</summary>
    public Coordinate2 Negative() => new(-X, -Y);

    /// <summary>Spiegelt horizontal.</summary>
    public Coordinate2 FlipHorizontal(int width) => new(width - 1 - X, Y);

    /// <summary>Spiegelt vertikal.</summary>
    public Coordinate2 FlipVertical(int height) => new(X, height - 1 - Y);

    /// <summary>Spiegelt beide Achsen.</summary>
    public Coordinate2 FlipBoth(int width, int height) => new(width - 1 - X, height - 1 - Y);

    /// <summary>Normalisiert Richtung auf -1,0,1.</summary>
    public Coordinate2 NormalizeDirection()
    {
        int nx = X == 0 ? 0 : (X > 0 ? 1 : -1);
        int ny = Y == 0 ? 0 : (Y > 0 ? 1 : -1);
        return new(nx, ny);
    }

    /// <summary>Gibt Nachbarn zurück (4 oder 8 Richtungen).</summary>
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

    /// <summary>String Darstellung "X,Y".</summary>
    public override string ToString() => $"{X},{Y}";

    /// <summary>Gleichheitsvergleich.</summary>
    public override bool Equals(object? obj)
        => obj is Coordinate2 c && c.X == X && c.Y == Y;

    /// <summary>Hashcode.</summary>
    public override int GetHashCode() => HashCode.Combine(X, Y);

    public static bool operator ==(Coordinate2 a, Coordinate2 b) => a.Equals(b);
    public static bool operator !=(Coordinate2 a, Coordinate2 b) => !a.Equals(b);

    public static Coordinate2 operator +(Coordinate2 a, Coordinate2 b) => new(a.X + b.X, a.Y + b.Y);
    public static Coordinate2 operator -(Coordinate2 a, Coordinate2 b) => new(a.X - b.X, a.Y - b.Y);
    public static Coordinate2 operator -(Coordinate2 v) => new(-v.X, -v.Y);
    public static Coordinate2 operator *(Coordinate2 v, int s) => new(v.X * s, v.Y * s);
    public static Coordinate2 operator /(Coordinate2 v, int s) => new(v.X / s, v.Y / s);

    public static implicit operator Coordinate2(Point p) => new(p.X, p.Y);
    public static explicit operator Point(Coordinate2 c) => new(c.X, c.Y);

    public static implicit operator Coordinate2(Vector2 v) => new((int)v.X, (int)v.Y);
    public static explicit operator Vector2(Coordinate2 c) => new(c.X, c.Y);
}
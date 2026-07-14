using System.Collections.Generic;

using System;
using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Tiling;

/// <summary>
/// Stellt eine unveränderliche 2D-Koordinate mit ganzzahligen X- und Y-Werten dar,
/// die speziell für Tile-basierte Spiele oder Raster verwendet wird.
/// Enthält zahlreiche Hilfsmethoden zur Richtungsbestimmung, Spiegelung,
/// Distanzberechnung, Nachbarschaftsermittlung und Konvertierung.
/// </summary>
/// <remarks>
/// Anders als <see cref="Microsoft.Xna.Framework.Point"/> bietet <c>Coordinate</c>
/// semantisch sinnvolle Eigenschaften wie <c>Row</c>, <c>Column</c> sowie
/// Methoden zur Rasterlogik.
/// </remarks>
public readonly struct Coordinate
{
    /// <summary>
    /// Gibt eine Koordinate mit den Werten (0, 0) zurück.
    /// </summary>
    public static Coordinate Zero => new Coordinate(0, 0);

    /// <summary>
    /// Gibt eine Koordinate mit den Werten (1, 1) zurück.
    /// </summary>
    public static Coordinate One => new Coordinate(1, 1);

    /// <summary>
    /// Gibt eine Koordinate mit den Werten (1, 0) zurück.
    /// </summary>
    public static Coordinate UnitX => new Coordinate(1, 0);

    /// <summary>
    /// Gibt eine Koordinate mit den Werten (0, 1) zurück.
    /// </summary>
    public static Coordinate UnitY => new Coordinate(0, 1);

    /// <summary>
    /// Erstellt eine neue Koordinate mit den angegebenen X- und Y-Werten.
    /// </summary>
    public Coordinate(int x, int y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Erstellt eine Koordinate aus einem <see cref="Point"/>.
    /// </summary>
    public Coordinate(Point pt)
    {
        X = pt.X;
        Y = pt.Y;
    }

    /// <summary>
    /// X-Wert der Koordinate (entspricht der Spalte).
    /// </summary>
    public int X { get; }

    /// <summary>
    /// Y-Wert der Koordinate (entspricht der Zeile).
    /// </summary>
    public int Y { get; }

    /// <summary>
    /// Spaltenindex (entspricht X).
    /// </summary>
    public int Column => X;

    /// <summary>
    /// Zeilenindex (entspricht Y).
    /// </summary>
    public int Row => Y;

    /// <summary>
    /// Zerlegt die Koordinate in ihre X- und Y-Komponenten.
    /// </summary>
    public void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }

    /// <summary>
    /// Gibt die Nachbarkoordinate links von dieser Koordinate zurück.
    /// </summary>
    public Coordinate Left => new Coordinate(X - 1, Y);

    /// <summary>
    /// Gibt die Nachbarkoordinate rechts von dieser Koordinate zurück.
    /// </summary>
    public Coordinate Right => new Coordinate(X + 1, Y);

    /// <summary>
    /// Gibt die Nachbarkoordinate oberhalb dieser Koordinate zurück.
    /// </summary>
    public Coordinate Up => new Coordinate(X, Y - 1);

    /// <summary>
    /// Gibt die Nachbarkoordinate unterhalb dieser Koordinate zurück.
    /// </summary>
    public Coordinate Down => new Coordinate(X, Y + 1);

    /// <summary>
    /// Wandelt die Koordinate in einen <see cref="Point"/> um.
    /// </summary>
    public Point ToPoint() => new Point(X, Y);

    /// <summary>
    /// Wandelt die Koordinate in einen <see cref="Vector2"/> um.
    /// </summary>
    public Vector2 ToVector2() => new Vector2(X, Y);

    /// <summary>
    /// Wandelt die Koordinate unter Berücksichtigung der Kachelgröße in einen <see cref="Vector2"/> um.
    /// </summary>
    public Vector2 ToVector2(Vector2 tile_size) => new Vector2(X * tile_size.X, Y * tile_size.Y);

    /// <summary>
    /// Gibt eine neue Koordinate mit geändertem X-Wert zurück.
    /// </summary>
    public Coordinate WithX(int new_x) => new Coordinate(new_x, Y);

    /// <summary>
    /// Gibt eine neue Koordinate mit geändertem Y-Wert zurück.
    /// </summary>
    public Coordinate WithY(int new_y) => new Coordinate(X, new_y);

    /// <summary>
    /// Prüft, ob die Koordinate außerhalb eines Rasters mit gegebener Spalten- und Zeilenzahl liegt.
    /// </summary>
    public bool IsOutOfBorder(int columns, int rows)
    {
        return X < 0 || Y < 0 || X >= columns || Y >= rows;
    }

    /// <summary>
    /// Gibt zurück, ob die Koordinate gleich (0, 0) ist.
    /// </summary>
    public bool IsZero() => X == 0 && Y == 0;

    /// <summary>
    /// Berechnet die Manhattan-Distanz (Taxi-Distanz) zu einer anderen Koordinate.
    /// </summary>
    public int ManhattanDistance(Coordinate other)
        => Math.Abs(X - other.X) + Math.Abs(Y - other.Y);

    /// <summary>
    /// Berechnet die euklidische Distanz zu einer anderen Koordinate.
    /// </summary>
    public double EuclideanDistance(Coordinate other)
        => Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));

    /// <summary>
    /// Gibt eine neue Koordinate zurück, die um (x, y) verschoben ist.
    /// </summary>
    public Coordinate Add(int x, int y) => new Coordinate(X + x, Y + y);

    /// <summary>
    /// Gibt eine neue Koordinate zurück, die um eine andere Koordinate verschoben ist.
    /// </summary>
    public Coordinate Add(Coordinate coord) => new Coordinate(X + coord.X, Y + coord.Y);

    /// <summary>
    /// Parst eine Zeichenkette zu einer Koordinate. Format: "x y".
    /// </summary>
    public static Coordinate Parse(string? str)
    {
        if (string.IsNullOrEmpty(str))
            return Zero;

        var s = str.Split(' ');
        int x = s.Length > 0 && int.TryParse(s[0], out var px) ? px : 0;
        int y = s.Length > 1 && int.TryParse(s[1], out var py) ? py : 0;
        return new Coordinate(x, y);
    }

    /// <summary>
    /// Parst eine Zeichenkette mit mehreren Ganzzahlen zu einem Koordinaten-Array. Format: "x1 y1 x2 y2 …".
    /// </summary>
    public static Coordinate[] ParseArray(string? str)
    {
        if (string.IsNullOrEmpty(str))
            return new Coordinate[] { Zero };

        var s = str.Split(' ');
        var coords = new List<Coordinate>();

        for (int i = 0; i < s.Length; i += 2)
        {
            int x = int.TryParse(s[i], out var px) ? px : 0;
            int y = (i + 1) < s.Length && int.TryParse(s[i + 1], out var py) ? py : 0;
            coords.Add(new Coordinate(x, y));
        }

        return coords.ToArray();
    }

    /// <summary>
    /// Beschränkt eine Koordinate auf einen minimalen und maximalen Bereich.
    /// </summary>
    public static Coordinate Clamp(Coordinate value, Coordinate min, Coordinate max)
    {
        int x = int.Clamp(value.X, min.X, max.X);
        int y = int.Clamp(value.Y, min.Y, max.Y);
        return new Coordinate(x, y);
    }

    /// <summary>
    /// Gibt die minimale Koordinate zurück (kleinster X- und Y-Wert).
    /// </summary>
    public static Coordinate Min(Coordinate a, Coordinate b)
    {
        int x = int.Min(a.X, b.X);
        int y = int.Min(a.Y, b.Y);
        return new Coordinate(x, y);
    }

    /// <summary>
    /// Gibt die maximale Koordinate zurück (größter X- und Y-Wert).
    /// </summary>
    public static Coordinate Max(Coordinate a, Coordinate b)
    {
        int x = int.Max(a.X, b.X);
        int y = int.Max(a.Y, b.Y);
        return new Coordinate(x, y);
    }

    /// <summary>
    /// Vergleicht die Gleichheit zweier Koordinaten.
    /// </summary>
    public bool Equals(Coordinate other) => X == other.X && Y == other.Y;

    public override bool Equals(object? obj) => obj is Coordinate other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(X, Y);

    /// <summary>
    /// Vergleicht diese Koordinate mit einer anderen (erst X, dann Y).
    /// </summary>
    public int CompareTo(Coordinate other)
    {
        int cmp = X.CompareTo(other.X);
        return cmp != 0 ? cmp : Y.CompareTo(other.Y);
    }

    /// <summary>
    /// Gibt alle benachbarten Koordinaten (4 oder 8 Richtungen) zurück.
    /// </summary>
    public IEnumerable<Coordinate> GetNeighbors(bool include_diagonals = false)
    {
        var directions = new List<Coordinate>
        {
            new Coordinate(0, -1),  // Oben
            new Coordinate(0, 1),   // Unten
            new Coordinate(-1, 0),  // Links
            new Coordinate(1, 0)    // Rechts
        };

        if (include_diagonals)
        {
            directions.AddRange(new[]
            {
                new Coordinate(-1, -1),
                new Coordinate(1, -1),
                new Coordinate(-1, 1),
                new Coordinate(1, 1)
            });
        }

        foreach (var dir in directions)
            yield return this + dir;
    }

    /// <summary>
    /// Gibt die negierte Koordinate zurück (-X, -Y).
    /// </summary>
    public Coordinate Negative() => new Coordinate(-X, -Y);

    /// <summary>
    /// Spiegelt die Koordinate horizontal innerhalb einer gegebenen Breite.
    /// </summary>
    public Coordinate FlipHorizontal(int width) => new Coordinate(width - 1 - X, Y);

    /// <summary>
    /// Spiegelt die Koordinate vertikal innerhalb einer gegebenen Höhe.
    /// </summary>
    public Coordinate FlipVertical(int height) => new Coordinate(X, height - 1 - Y);

    /// <summary>
    /// Spiegelt die Koordinate sowohl horizontal als auch vertikal.
    /// </summary>
    public Coordinate FlipBoth(int width, int height) => new Coordinate(width - 1 - X, height - 1 - Y);

    /// <summary>
    /// Normalisiert eine Richtung (X und Y werden auf -1, 0 oder 1 reduziert).
    /// </summary>
    public Coordinate NormalizeDirection()
    {
        int norm_x = X == 0 ? 0 : (X > 0 ? 1 : -1);
        int norm_y = Y == 0 ? 0 : (Y > 0 ? 1 : -1);
        return new Coordinate(norm_x, norm_y);
    }

    public static Coordinate operator +(Coordinate a, Coordinate b)
        => new Coordinate(a.X + b.X, a.Y + b.Y);

    public static Coordinate operator -(Coordinate a, Coordinate b)
        => new Coordinate(a.X - b.X, a.Y - b.Y);

    public static Coordinate operator -(Coordinate value)
        => new Coordinate(-value.X, -value.Y);

    public static Coordinate operator *(Coordinate coord, int scalar)
        => new Coordinate(coord.X * scalar, coord.Y * scalar);

    public static Coordinate operator /(Coordinate coord, int scalar)
        => new Coordinate(coord.X / scalar, coord.Y / scalar);

    public static bool operator ==(Coordinate left, Coordinate right) => left.Equals(right);
    public static bool operator !=(Coordinate left, Coordinate right) => !(left == right);

    public static implicit operator Coordinate(Point pt) => new Coordinate(pt.X, pt.Y);
    public static explicit operator Point(Coordinate coord) => new Point(coord.X, coord.Y);

    public static implicit operator Coordinate(Vector2 vec) => new Coordinate((int)vec.X, (int)vec.Y);
    public static explicit operator Vector2(Coordinate coord) => new Vector2(coord.X, coord.Y);

    /// <summary>
    /// Gibt eine Zeichenkette im Format "X,Y" zurück.
    /// </summary>
    public override string ToString() => $"{X},{Y}";
}

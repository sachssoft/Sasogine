using Microsoft.Xna.Framework;
using sachssoft.Sasogine.Document;
using System;

namespace sachssoft.Sasogine.Tiling;

/// <summary>
/// Definiert einen rechteckigen Koordinatenbereich (Scope) mit unterem und oberem Eckpunkt.
/// Wird z. B. in Editoren, Tilemaps oder Auswahloperationen verwendet.
/// </summary>
public struct Scope
{
    public static readonly Scope Zero = new Scope(0, 0, 0, 0);

    public Coordinate Lower { get; set; }
    public Coordinate Upper { get; set; }

    public Scope(int x, int y) : this(new Coordinate(x, y)) { }

    public Scope(Coordinate coordinate) : this(coordinate, coordinate) { }

    public Scope(Coordinate lower, Coordinate upper)
    {
        Lower = lower;
        Upper = upper;
    }

    public Scope(Point lower, Point upper)
        : this(new Coordinate(lower), new Coordinate(upper)) { }

    public Scope(int lowerX, int lowerY, int upperX, int upperY)
        : this(new Coordinate(lowerX, lowerY), new Coordinate(upperX, upperY)) { }

    public Scope(ushort lowerX, ushort lowerY, ushort upperX, ushort upperY)
        : this(new Coordinate(lowerX, lowerY), new Coordinate(upperX, upperY)) { }

    public Scope FromRectangle(int x, int y, int width, int height)
    {
        return new Scope(new Coordinate(x, y), new Coordinate(x + width, y + height));
    }

    public Scope FromRectangle(Rectangle rectangle)
        => FromRectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);

    /// <summary>
    /// Gibt die Breite und Höhe des Bereichs als <see cref="Point"/> zurück.
    /// Negative Werte werden vermieden.
    /// </summary>
    public Point ToSize() =>
        new Point(Math.Max(0, Upper.X - Lower.X), Math.Max(0, Upper.Y - Lower.Y));

    /// <summary>
    /// Wandelt den Scope in ein <see cref="Rectangle"/> um.
    /// </summary>
    public Rectangle ToRectangle() =>
        new Rectangle(new Point(Lower.X, Lower.Y), ToSize());

    public static Scope FromInclusive(Coordinate a, Coordinate b)
    {
        var xmin = int.Min(a.X, b.X);
        var xmax = int.Max(a.X, b.X);
        var ymin = int.Min(a.Y, b.Y);
        var ymax = int.Max(a.Y, b.Y);

        return new Scope(xmin, ymin, xmax, ymax);
    }

    public bool Contains(Coordinate point) =>
        point.X >= Lower.X && point.X <= Upper.X &&
        point.Y >= Lower.Y && point.Y <= Upper.Y;

    public bool Contains(int x, int y) =>
        x >= Lower.X && x <= Upper.X &&
        y >= Lower.Y && y <= Upper.Y;

    public bool IsZero()
        => Lower.X == 0 && Lower.Y == 0 &&
           Upper.X == 0 && Upper.Y == 0;

    public static Scope Parse(string? str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return Zero;

        var coords = Coordinate.ParseArray(str);
        return coords.Length >= 2 ? new Scope(coords[0], coords[1]) : Zero;
    }

    public override string ToString() =>
        $"Lower: {Lower}, Upper: {Upper}";
}

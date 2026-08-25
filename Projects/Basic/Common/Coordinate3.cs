using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Repräsentiert eine unveränderliche 3D-Gitterkoordinate (X, Y, Z).
/// Wird für voxelbasierte Systeme, 3D-Tilemaps, Layer-Systeme und Chunk-Indexierung verwendet.
/// </summary>
/// <remarks>
/// Diese Struktur ist für diskrete Rasterlogik gedacht und ersetzt keinen Vector3.
/// Sie arbeitet ausschließlich mit Integer-Koordinaten und ist damit deterministisch
/// für Simulationen, World-Streaming und Grid-basierte Logik.
/// </remarks>
public readonly struct Coordinate3
{
    /// <summary>Koordinate (0,0,0).</summary>
    public static Coordinate3 Zero => new(0, 0, 0);

    /// <summary>Koordinate (1,1,1).</summary>
    public static Coordinate3 One => new(1, 1, 1);

    /// <summary>Einheitsvektor X (1,0,0).</summary>
    public static Coordinate3 UnitX => new(1, 0, 0);

    /// <summary>Einheitsvektor Y (0,1,0).</summary>
    public static Coordinate3 UnitY => new(0, 1, 0);

    /// <summary>Einheitsvektor Z (0,0,1).</summary>
    public static Coordinate3 UnitZ => new(0, 0, 1);

    /// <summary>Erstellt eine neue 3D-Koordinate.</summary>
    public Coordinate3(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>X-Komponente (Breite im Grid).</summary>
    public int X { get; }

    /// <summary>Y-Komponente (Höhe im Grid).</summary>
    public int Y { get; }

    /// <summary>Z-Komponente (Tiefe / Layer / Chunk-Ebene).</summary>
    public int Z { get; }

    /// <summary>Zerlegt die Koordinate in X, Y und Z.</summary>
    public void Deconstruct(out int x, out int y, out int z)
    {
        x = X;
        y = Y;
        z = Z;
    }

    /// <summary>Erstellt eine Kopie mit geändertem X.</summary>
    public Coordinate3 WithX(int x) => new(x, Y, Z);

    /// <summary>Erstellt eine Kopie mit geändertem Y.</summary>
    public Coordinate3 WithY(int y) => new(X, y, Z);

    /// <summary>Erstellt eine Kopie mit geändertem Z.</summary>
    public Coordinate3 WithZ(int z) => new(X, Y, z);

    /// <summary>Verschiebt die Koordinate um einen Offset.</summary>
    public Coordinate3 Add(int x, int y, int z) => new(X + x, Y + y, Z + z);

    /// <summary>Verschiebt die Koordinate um eine andere Koordinate.</summary>
    public Coordinate3 Add(Coordinate3 other) => new(X + other.X, Y + other.Y, Z + other.Z);

    /// <summary>Negiert die Koordinate (-X,-Y,-Z).</summary>
    public Coordinate3 Negative() => new(-X, -Y, -Z);

    /// <summary>Normalisiert jede Achse auf -1, 0 oder 1.</summary>
    public Coordinate3 NormalizeDirection()
    {
        int nx = X == 0 ? 0 : (X > 0 ? 1 : -1);
        int ny = Y == 0 ? 0 : (Y > 0 ? 1 : -1);
        int nz = Z == 0 ? 0 : (Z > 0 ? 1 : -1);
        return new(nx, ny, nz);
    }

    /// <summary>Prüft, ob zwei Koordinaten auf derselben Z-Ebene liegen.</summary>
    public bool SameLayer(Coordinate3 other) => Z == other.Z;

    /// <summary>Projektion auf XY-Ebene (Z wird ignoriert).</summary>
    public Coordinate2 ToXY() => new(X, Y);

    /// <summary>Projektion auf XZ-Ebene (Y wird ignoriert).</summary>
    public Coordinate2 ToXZ() => new(X, Z);

    /// <summary>Projektion auf YZ-Ebene (X wird ignoriert).</summary>
    public Coordinate2 ToYZ() => new(Y, Z);

    /// <summary>
    /// Konvertiert die Koordinate in einen linearen Index für 3D Arrays.
    /// </summary>
    public int ToIndex(int width, int height)
        => X + Y * width + Z * width * height;

    /// <summary>
    /// Berechnet die Manhattan-Distanz im 3D-Gitter.
    /// </summary>
    public int ManhattanDistance(Coordinate3 other)
        => int.Abs(X - other.X)
         + int.Abs(Y - other.Y)
         + int.Abs(Z - other.Z);

    /// <summary>
    /// Berechnet die euklidische Distanz im 3D-Raum.
    /// </summary>
    public float EuclideanDistance(Coordinate3 other)
        => MathF.Sqrt(
            MathF.Pow(X - other.X, 2) +
            MathF.Pow(Y - other.Y, 2) +
            MathF.Pow(Z - other.Z, 2)
        );

    /// <summary>
    /// Gibt die 6 direkten Nachbarn im 3D-Gitter zurück (Voxel-Bewegung).
    /// </summary>
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
    /// Gibt alle 26 Nachbarn im 3D-Raum zurück (inkl. Diagonalen).
    /// </summary>
    public IEnumerable<Coordinate3> GetNeighbors26()
    {
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
                for (int dz = -1; dz <= 1; dz++)
                {
                    if (dx == 0 && dy == 0 && dz == 0)
                        continue;

                    yield return new Coordinate3(X + dx, Y + dy, Z + dz);
                }
    }

    /// <summary>String-Darstellung im Format "X,Y,Z".</summary>
    public override string ToString() => $"{X},{Y},{Z}";

    /// <summary>Vergleicht zwei 3D-Koordinaten.</summary>
    public override bool Equals(object? obj)
        => obj is Coordinate3 c && c.X == X && c.Y == Y && c.Z == Z;

    /// <summary>Hashwert der Koordinate.</summary>
    public override int GetHashCode() => HashCode.Combine(X, Y, Z);

    public static bool operator ==(Coordinate3 a, Coordinate3 b) => a.Equals(b);
    public static bool operator !=(Coordinate3 a, Coordinate3 b) => !a.Equals(b);

    public static Coordinate3 operator +(Coordinate3 a, Coordinate3 b)
        => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public static Coordinate3 operator -(Coordinate3 a, Coordinate3 b)
        => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public static Coordinate3 operator -(Coordinate3 v)
        => new(-v.X, -v.Y, -v.Z);
}
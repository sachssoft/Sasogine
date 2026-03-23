using Microsoft.Xna.Framework;
using System;
using System.Globalization;

namespace Sachssoft.Sasogine.Surface.Visuals;

public struct Thickness
{
    public static readonly Thickness Zero = new Thickness();

    public int Left { get; set; }
    public int Right { get; set; }
    public int Top { get; set; }
    public int Bottom { get; set; }

    public int Width => Left + Right;
    public int Height => Top + Bottom;
    public int Horizontal => Width;
    public int Vertical => Height;
    public bool SameSize => (Left == Top && Top == Right && Right == Bottom);

    public Thickness(int left, int top, int right, int bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public Thickness(int horizontalValue, int verticalValue)
        : this(horizontalValue, verticalValue, horizontalValue, verticalValue) { }

    public Thickness(int value)
        : this(value, value, value, value) { }

    public override string ToString()
    {
        if (SameSize) return Left.ToString();
        if (Left == Right && Top == Bottom) return $"{Left}, {Top}";
        return $"{Left}, {Top}, {Right}, {Bottom}";
    }

    public static Thickness Parse(string? s)
    {
        if (!TryParse(s, out Thickness result))
            throw new FormatException($"Could not parse '{s}' to a valid Thickness.");

        return result;
    }

    public static bool TryParse(string? s, out Thickness result)
    {
        result = Zero;
        if (string.IsNullOrWhiteSpace(s))
            return true;

        // Trennzeichen: Komma oder Leerzeichen, beliebige Kombination
        var parts = s.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 1 && parts.Length != 2 && parts.Length != 4)
            return false;

        int[] values = new int[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            if (!int.TryParse(parts[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out values[i]))
                return false;
        }

        switch (values.Length)
        {
            case 1:
                result = new Thickness(values[0]);
                break;
            case 2:
                result = new Thickness(values[0], values[1]);
                break;
            case 4:
                result = new Thickness(values[0], values[1], values[2], values[3]);
                break;
            default:
                return false;
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is Thickness t &&
               Left == t.Left &&
               Right == t.Right &&
               Top == t.Top &&
               Bottom == t.Bottom;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + Left;
            hash = hash * 31 + Top;
            hash = hash * 31 + Right;
            hash = hash * 31 + Bottom;
            return hash;
        }
    }

    public static bool operator ==(Thickness a, Thickness b) => a.Equals(b);
    public static bool operator !=(Thickness a, Thickness b) => !(a == b);

    public static Rectangle operator -(Rectangle rect, Thickness t)
    {
        int x = rect.X + t.Left;
        int y = rect.Y + t.Top;
        int width = int.Max(0, rect.Width - t.Width);
        int height = int.Max(0, rect.Height - t.Height);
        return new Rectangle(x, y, width, height);
    }
}

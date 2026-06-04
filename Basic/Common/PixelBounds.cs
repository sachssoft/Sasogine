using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

public readonly struct PixelBounds : IEquatable<PixelBounds>
{
    private readonly int _left;
    private readonly int _top;
    private readonly int _right;
    private readonly int _bottom;

    public static readonly PixelBounds Zero = new PixelBounds(0, 0, 0, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelBounds(int x, int y, int width, int height)
    {
        _left = x;
        _top = y;
        _right = x + width;
        _bottom = y + height;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelBounds(Point position, Point size)
    {
        _left = position.X;
        _top = position.Y;
        _right = position.X + size.X;
        _bottom = position.Y + size.Y;
    }

    public int X => _left;
    public int Y => _top;
    public int Width => _right - _left;
    public int Height => _bottom - _top;

    public int Left => _left;
    public int Top => _top;
    public int Right => _right;
    public int Bottom => _bottom;

    public Point Size => new Point(Width, Height);
    public Point Location => new Point(_left, _top);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(PixelBounds other)
        => _left == other._left &&
           _top == other._top &&
           _right == other._right &&
           _bottom == other._bottom;

    public override bool Equals(object? obj) => obj is PixelBounds b && Equals(b);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + _left;
            hash = hash * 31 + _top;
            hash = hash * 31 + _right;
            hash = hash * 31 + _bottom;
            return hash;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(int px, int py)
        => px >= _left && px < _right && py >= _top && py < _bottom;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in Point pos)
        => pos.X >= _left && pos.X < _right &&
           pos.Y >= _top && pos.Y < _bottom;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelBounds Offset(in Point delta)
        => new PixelBounds(_left + delta.X, _top + delta.Y, Width, Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelBounds Inflate(int dx, int dy)
        => new PixelBounds(_left - dx, _top - dy, Width + dx * 2, Height + dy * 2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Rectangle(PixelBounds b)
        => new Rectangle(b._left, b._top, b.Width, b.Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator PixelBounds(Rectangle r)
        => new PixelBounds(r.X, r.Y, r.Width, r.Height);

    public static bool operator ==(in PixelBounds a, in PixelBounds b) => a.Equals(b);
    public static bool operator !=(in PixelBounds a, in PixelBounds b) => !a.Equals(b);

    public static PixelBounds Parse(string s)
    {
        if (TryParse(s, out var result))
            return result;

        throw new FormatException(
            $"Invalid PixelBounds format: '{s}'. Expected 4 integer values separated by ',' or ' '.");
    }

    public static bool TryParse(string? s, out PixelBounds result)
    {
        result = Zero;

        if (string.IsNullOrWhiteSpace(s))
            return false;

        var parts = s.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 4)
            return false;

        try
        {
            int x = int.Parse(parts[0], CultureInfo.InvariantCulture);
            int y = int.Parse(parts[1], CultureInfo.InvariantCulture);
            int w = int.Parse(parts[2], CultureInfo.InvariantCulture);
            int h = int.Parse(parts[3], CultureInfo.InvariantCulture);

            result = new PixelBounds(x, y, w, h);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public override string ToString()
        => string.Format(
            CultureInfo.InvariantCulture,
            "{0}, {1}, {2}, {3}",
            X, Y, Width, Height
        );
}
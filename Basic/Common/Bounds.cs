using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

public readonly struct Bounds : IEquatable<Bounds>
{
    private readonly float _left;
    private readonly float _top;
    private readonly float _right;
    private readonly float _bottom;

    public static readonly Bounds Zero = new Bounds(0f, 0f, 0f, 0f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bounds(float x, float y, float width, float height)
    {
        _left = x;
        _top = y;
        _right = x + width;
        _bottom = y + height;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bounds(Vector2 position, Vector2 size)
    {
        _left = position.X;
        _top = position.Y;
        _right = position.X + size.X;
        _bottom = position.Y + size.Y;
    }

    public float X => _left;
    public float Y => _top;
    public float Width => _right - _left;
    public float Height => _bottom - _top;
    public float Left => _left;
    public float Top => _top;
    public float Right => _right;
    public float Bottom => _bottom;
    public Vector2 Size => new Vector2(Width, Height);
    public Vector2 Location => new Vector2(_left, _top);

    // Performance-optimierte Equals
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Bounds other)
        => _left == other._left && _top == other._top && _right == other._right && _bottom == other._bottom;

    public override bool Equals(object? obj) => obj is Bounds b && Equals(b);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + _left.GetHashCode();
            hash = hash * 31 + _top.GetHashCode();
            hash = hash * 31 + _right.GetHashCode();
            hash = hash * 31 + _bottom.GetHashCode();
            return hash;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(float px, float py) => px >= _left && px < _right && py >= _top && py < _bottom;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in Vector2 pos) => pos.X >= _left && pos.X < _right && pos.Y >= _top && pos.Y < _bottom;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bounds Offset(in Vector2 delta)
        => new Bounds(_left + delta.X, _top + delta.Y, Width, Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bounds Inflate(float dx, float dy)
        => new Bounds(_left - dx, _top - dy, Width + dx * 2, Height + dy * 2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Rectangle(Bounds b)
        => new Rectangle((int)b._left, (int)b._top, (int)b.Width, (int)b.Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Bounds(Rectangle r)
        => new Bounds(r.X, r.Y, r.Width, r.Height);

    // Vergleichsoperatoren ohne Kopien (in Bounds)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in Bounds a, in Bounds b) => a.Equals(b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in Bounds a, in Bounds b) => !a.Equals(b);

    public static Bounds Parse(string s)
    {
        if (TryParse(s, out var result))
            return result;

        throw new FormatException($"Invalid Bounds format: '{s}'. Expected 4 numeric values separated by ',' or ' '.");
    }

    public static bool TryParse(string? s, out Bounds result)
    {
        result = Zero;
        if (string.IsNullOrWhiteSpace(s))
            return false;

        var parts = s.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4)
            return false;

        try
        {
            float x = float.Parse(parts[0], CultureInfo.InvariantCulture);
            float y = float.Parse(parts[1], CultureInfo.InvariantCulture);
            float w = float.Parse(parts[2], CultureInfo.InvariantCulture);
            float h = float.Parse(parts[3], CultureInfo.InvariantCulture);

            result = new Bounds(x, y, w, h);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public override string ToString()
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0}, {1}, {2}, {3}",
            X,
            Y,
            Width,
            Height
        );
    }
}
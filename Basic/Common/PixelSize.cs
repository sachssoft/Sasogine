using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

public readonly struct PixelSize : IEquatable<PixelSize>
{
    private readonly int _width;
    private readonly int _height;

    public static readonly PixelSize Zero = new PixelSize(0, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelSize(int width, int height)
    {
        _width = width;
        _height = height;
    }

    public int Width => _width;
    public int Height => _height;

    public Point Point => new(_width, _height);
    public Vector2 Vector => new(_width, _height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(PixelSize other)
        => _width == other._width && _height == other._height;

    public override bool Equals(object? obj)
        => obj is PixelSize s && Equals(s);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + _width;
            hash = hash * 31 + _height;
            return hash;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in PixelSize a, in PixelSize b) => a.Equals(b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in PixelSize a, in PixelSize b) => !a.Equals(b);

    public static PixelSize Parse(string s)
    {
        if (TryParse(s, out var result))
            return result;

        throw new FormatException(
            $"Invalid PixelSize format: '{s}'. Expected 2 integer values separated by ',' or ' '.");
    }

    public static bool TryParse(string? s, out PixelSize result)
    {
        result = Zero;

        if (string.IsNullOrWhiteSpace(s))
            return false;

        var parts = s.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
            return false;

        try
        {
            int w = int.Parse(parts[0], CultureInfo.InvariantCulture);
            int h = int.Parse(parts[1], CultureInfo.InvariantCulture);

            result = new PixelSize(w, h);
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
            "{0}, {1}",
            Width, Height
        );
}
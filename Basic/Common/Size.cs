using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

public readonly struct Size : IEquatable<Size>
{
    private readonly float _width;
    private readonly float _height;

    public static readonly Size Zero = new Size(0f, 0f);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Size(float uniform)
    {
        _width = uniform;
        _height = uniform;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Size(float width, float height)
    {
        _width = width;
        _height = height;
    }

    public float Width => _width;
    public float Height => _height;

    public Vector2 ToVector2() => new(_width, _height);
    public Vector3 ToVector3() => new(_width, _height, 0f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Size other)
        => _width == other._width && _height == other._height;

    public override bool Equals(object? obj)
        => obj is Size s && Equals(s);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + _width.GetHashCode();
            hash = hash * 31 + _height.GetHashCode();
            return hash;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in Size a, in Size b) => a.Equals(b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in Size a, in Size b) => !a.Equals(b);

    public static Size Parse(string s)
    {
        if (TryParse(s, out var result))
            return result;

        throw new FormatException(
            $"Invalid Size format: '{s}'. Expected 2 numeric values separated by ',' or ' '.");
    }

    public static bool TryParse(string? s, out Size result)
    {
        result = Zero;

        if (string.IsNullOrWhiteSpace(s))
            return false;

        var parts = s.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
            return false;

        try
        {
            float w = float.Parse(parts[0], CultureInfo.InvariantCulture);
            float h = float.Parse(parts[1], CultureInfo.InvariantCulture);

            result = new Size(w, h);
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
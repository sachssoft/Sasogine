using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Common;

public readonly struct PixelInsets
{
    private readonly int _left;
    private readonly int _top;
    private readonly int _right;
    private readonly int _bottom;

    public static readonly PixelInsets None = new PixelInsets(0);

    public PixelInsets(int uniform)
    {
        _left = uniform;
        _top = uniform;
        _right = uniform;
        _bottom = uniform;
    }

    public PixelInsets(int horizontal, int vertical)
    {
        _left = horizontal;
        _right = horizontal;
        _top = vertical;
        _bottom = vertical;
    }

    public PixelInsets(int left, int top, int right, int bottom)
    {
        _left = left;
        _top = top;
        _right = right;
        _bottom = bottom;
    }

    public int Left => _left;
    public int Top => _top;
    public int Right => _right;
    public int Bottom => _bottom;

    public int Horizontal => _left + _right;
    public int Vertical => _top + _bottom;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelBounds Apply(in PixelBounds bounds)
        => new PixelBounds(
            bounds.Left + _left,
            bounds.Top + _top,
            bounds.Width - Horizontal,
            bounds.Height - Vertical
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelBounds Expand(in PixelBounds bounds)
        => new PixelBounds(
            bounds.Left - _left,
            bounds.Top - _top,
            bounds.Width + Horizontal,
            bounds.Height + Vertical
        );

    public static PixelInsets Parse(string s)
    {
        if (TryParse(s, out var result))
            return result;

        throw new FormatException(
            $"Invalid PixelInsets format: '{s}'. Expected 1, 2, or 4 integer values separated by ',' or ' '.");
    }

    public static bool TryParse(string? s, out PixelInsets result)
    {
        result = None;

        if (string.IsNullOrWhiteSpace(s))
            return false;

        var parts = s.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

        int[] values = new int[parts.Length];

        try
        {
            for (int i = 0; i < parts.Length; i++)
                values[i] = int.Parse(parts[i], CultureInfo.InvariantCulture);

            result = values.Length switch
            {
                1 => new PixelInsets(values[0]),
                2 => new PixelInsets(values[0], values[1]),
                4 => new PixelInsets(values[0], values[1], values[2], values[3]),
                _ => None
            };

            return values.Length is 1 or 2 or 4;
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
            Left, Top, Right, Bottom
        );
}
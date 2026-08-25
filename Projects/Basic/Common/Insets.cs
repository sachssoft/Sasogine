using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

public readonly struct Insets
{
    private readonly float _left;
    private readonly float _top;
    private readonly float _right;
    private readonly float _bottom;

    public static readonly Insets Zero = new Insets(0.0f);

    public Insets(float uniform)
    {
        _left = uniform;
        _top = uniform;
        _right = uniform;
        _bottom = uniform;
    }

    public Insets(float horizontal, float vertical)
    {
        _left = horizontal;
        _right = horizontal;
        _top = vertical;
        _bottom = vertical;
    }

    public Insets(float left, float top, float right, float bottom)
    {
        _left = left;
        _top = top;
        _right = right;
        _bottom = bottom;
    }

    public float Left => _left;
    public float Top => _top;
    public float Right => _right;
    public float Bottom => _bottom;

    public float Horizontal => _left + _right;
    public float Vertical => _top + _bottom;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bounds Apply(in Bounds bounds)
        => new Bounds(
            bounds.Left + _left,
            bounds.Top + _top,
            bounds.Width - Horizontal,
            bounds.Height - Vertical
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bounds Expand(in Bounds bounds)
        => new Bounds(
            bounds.Left - _left,
            bounds.Top - _top,
            bounds.Width + Horizontal,
            bounds.Height + Vertical
        );

    public static Insets Parse(string s)
    {
        if (TryParse(s, out var result))
            return result;

        throw new FormatException($"Invalid Insets format: '{s}'. Expected 1, 2, or 4 numeric values separated by ',' or ' '.");
    }

    public static bool TryParse(string? s, out Insets result)
    {
        result = Zero;

        if (string.IsNullOrWhiteSpace(s))
            return false;

        var parts = s.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        float[] values = new float[parts.Length];

        try
        {
            for (int i = 0; i < parts.Length; i++)
                values[i] = float.Parse(parts[i], CultureInfo.InvariantCulture);

            result = values.Length switch
            {
                1 => new Insets(values[0]),
                2 => new Insets(values[0], values[1]),
                4 => new Insets(values[0], values[1], values[2], values[3]),
                _ => Zero
            };

            return values.Length == 1 || values.Length == 2 || values.Length == 4;
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
            Left,
            Top,
            Right,
            Bottom
        );
    }
}
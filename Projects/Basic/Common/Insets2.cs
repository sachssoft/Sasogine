using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Represents the spacing applied to the left, top, right, and bottom
/// sides of a two-dimensional area.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Insets2"/> can be used to reduce or expand a
/// <see cref="Bounds2"/> by applying independent horizontal and vertical
/// insets.
/// </para>
/// <para>
/// The values can be specified uniformly, as separate horizontal and
/// vertical values, or individually for all four sides.
/// </para>
/// </remarks>
public readonly struct Insets2
{
    private readonly float _left;
    private readonly float _top;
    private readonly float _right;
    private readonly float _bottom;

    /// <summary>
    /// Represents zero insets on all four sides.
    /// </summary>
    public static readonly Insets2 Zero = new Insets2(0.0f);

    /// <summary>
    /// Initializes a new instance of the <see cref="Insets2"/> structure
    /// with the same value applied to all four sides.
    /// </summary>
    /// <param name="uniform">The inset value for all four sides.</param>
    public Insets2(float uniform)
    {
        _left = uniform;
        _top = uniform;
        _right = uniform;
        _bottom = uniform;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Insets2"/> structure
    /// with separate horizontal and vertical values.
    /// </summary>
    /// <param name="horizontal">
    /// The inset value applied to the left and right sides.
    /// </param>
    /// <param name="vertical">
    /// The inset value applied to the top and bottom sides.
    /// </param>
    public Insets2(float horizontal, float vertical)
    {
        _left = horizontal;
        _right = horizontal;
        _top = vertical;
        _bottom = vertical;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Insets2"/> structure
    /// with an individual value for each side.
    /// </summary>
    /// <param name="left">The inset value for the left side.</param>
    /// <param name="top">The inset value for the top side.</param>
    /// <param name="right">The inset value for the right side.</param>
    /// <param name="bottom">The inset value for the bottom side.</param>
    public Insets2(float left, float top, float right, float bottom)
    {
        _left = left;
        _top = top;
        _right = right;
        _bottom = bottom;
    }

    /// <summary>
    /// Gets the inset value for the left side.
    /// </summary>
    public float Left => _left;

    /// <summary>
    /// Gets the inset value for the top side.
    /// </summary>
    public float Top => _top;

    /// <summary>
    /// Gets the inset value for the right side.
    /// </summary>
    public float Right => _right;

    /// <summary>
    /// Gets the inset value for the bottom side.
    /// </summary>
    public float Bottom => _bottom;

    /// <summary>
    /// Gets the combined horizontal inset.
    /// </summary>
    /// <remarks>
    /// This value is the sum of the <see cref="Left"/> and
    /// <see cref="Right"/> insets.
    /// </remarks>
    public float Horizontal => _left + _right;

    /// <summary>
    /// Gets the combined vertical inset.
    /// </summary>
    /// <remarks>
    /// This value is the sum of the <see cref="Top"/> and
    /// <see cref="Bottom"/> insets.
    /// </remarks>
    public float Vertical => _top + _bottom;

    /// <summary>
    /// Applies the insets to the specified bounds.
    /// </summary>
    /// <param name="bounds">The bounds to which the insets are applied.</param>
    /// <returns>
    /// A new <see cref="Bounds2"/> reduced by the specified insets.
    /// </returns>
    /// <remarks>
    /// The left and top position are moved inward by their respective
    /// inset values, while the width and height are reduced by the
    /// combined horizontal and vertical insets.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bounds2 Apply(in Bounds2 bounds)
        => new Bounds2(
            bounds.Left + _left,
            bounds.Top + _top,
            bounds.Width - Horizontal,
            bounds.Height - Vertical
        );

    /// <summary>
    /// Expands the specified bounds by the insets.
    /// </summary>
    /// <param name="bounds">The bounds to expand.</param>
    /// <returns>
    /// A new <see cref="Bounds2"/> expanded by the specified insets.
    /// </returns>
    /// <remarks>
    /// The left and top position are moved outward by their respective
    /// inset values, while the width and height are increased by the
    /// combined horizontal and vertical insets.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bounds2 Expand(in Bounds2 bounds)
        => new Bounds2(
            bounds.Left - _left,
            bounds.Top - _top,
            bounds.Width + Horizontal,
            bounds.Height + Vertical
        );

    /// <summary>
    /// Parses a string representation of <see cref="Insets2"/>.
    /// </summary>
    /// <param name="s">
    /// A string containing one, two, or four numeric values.
    /// </param>
    /// <returns>The parsed <see cref="Insets2"/>.</returns>
    /// <exception cref="FormatException">
    /// Thrown when the specified string does not contain one, two, or four
    /// valid numeric values.
    /// </exception>
    /// <remarks>
    /// Values may be separated by commas or spaces and are interpreted
    /// using <see cref="CultureInfo.InvariantCulture"/>.
    /// </remarks>
    public static Insets2 Parse(string s)
    {
        if (TryParse(s, out var result))
            return result;

        throw new FormatException(
            $"Invalid Insets2 format: '{s}'. Expected 1, 2, or 4 numeric values separated by ',' or ' '.");
    }

    /// <summary>
    /// Attempts to parse a string representation of <see cref="Insets2"/>.
    /// </summary>
    /// <param name="s">
    /// A string containing one, two, or four numeric values.
    /// </param>
    /// <param name="result">
    /// When this method returns <see langword="true"/>, contains the parsed
    /// insets; otherwise, contains <see cref="Zero"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the string contains exactly one, two, or
    /// four valid numeric values; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Values may be separated by commas or spaces and are interpreted
    /// using <see cref="CultureInfo.InvariantCulture"/>.
    /// A single value is applied uniformly to all sides, while two values
    /// represent horizontal and vertical insets.
    /// </remarks>
    public static bool TryParse(string? s, out Insets2 result)
    {
        result = Zero;

        if (string.IsNullOrWhiteSpace(s))
            return false;

        var parts = s.Split(
            new[] { ',', ' ' },
            StringSplitOptions.RemoveEmptyEntries);

        float[] values = new float[parts.Length];

        try
        {
            for (int i = 0; i < parts.Length; i++)
                values[i] = float.Parse(
                    parts[i],
                    CultureInfo.InvariantCulture);

            result = values.Length switch
            {
                1 => new Insets2(values[0]),
                2 => new Insets2(values[0], values[1]),
                4 => new Insets2(
                    values[0],
                    values[1],
                    values[2],
                    values[3]),
                _ => Zero
            };

            return values.Length == 1
                || values.Length == 2
                || values.Length == 4;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Returns the string representation of this <see cref="Insets2"/>.
    /// </summary>
    /// <returns>
    /// A string containing the left, top, right, and bottom inset values
    /// separated by commas.
    /// </returns>
    /// <remarks>
    /// Numeric values are formatted using
    /// <see cref="CultureInfo.InvariantCulture"/>.
    /// </remarks>
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
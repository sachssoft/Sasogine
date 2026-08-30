using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Represents the spacing applied to the left, top, front, right, bottom,
/// and back sides of a three-dimensional area.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Insets3"/> can be used to reduce or expand a
/// <see cref="Bounds3"/> by applying independent horizontal, vertical,
/// and depth insets.
/// </para>
/// <para>
/// The values can be specified uniformly, as separate horizontal and
/// vertical values, individually for all four two-dimensional sides,
/// or individually for all six three-dimensional sides.
/// </para>
/// </remarks>
public readonly struct Insets3
{
    private readonly float _left;
    private readonly float _top;
    private readonly float _front;
    private readonly float _right;
    private readonly float _bottom;
    private readonly float _back;

    /// <summary>
    /// Represents zero insets on all six sides.
    /// </summary>
    public static readonly Insets3 Zero = new Insets3(0.0f);

    /// <summary>
    /// Initializes a new instance of the <see cref="Insets3"/> structure
    /// with the same value applied to all six sides.
    /// </summary>
    /// <param name="uniform">The inset value for all six sides.</param>
    public Insets3(float uniform)
    {
        _left = uniform;
        _top = uniform;
        _front = uniform;
        _right = uniform;
        _bottom = uniform;
        _back = uniform;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Insets3"/> structure
    /// with separate horizontal and vertical values.
    /// </summary>
    /// <param name="horizontal">
    /// The inset value applied to the left and right sides.
    /// </param>
    /// <param name="vertical">
    /// The inset value applied to the top and bottom sides.
    /// </param>
    public Insets3(float horizontal, float vertical)
    {
        _left = horizontal;
        _right = horizontal;
        _top = vertical;
        _bottom = vertical;
        _front = 0.0f;
        _back = 0.0f;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Insets3"/> structure
    /// with separate horizontal, vertical, and depth values.
    /// </summary>
    /// <param name="horizontal">
    /// The inset value applied to the left and right sides.
    /// </param>
    /// <param name="vertical">
    /// The inset value applied to the top and bottom sides.
    /// </param>
    /// <param name="depth">
    /// The inset value applied to the front and back sides.
    /// </param>
    public Insets3(float horizontal, float vertical, float depth)
    {
        _left = horizontal;
        _right = horizontal;
        _top = vertical;
        _bottom = vertical;
        _front = depth;
        _back = depth;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Insets3"/> structure
    /// with an individual value for each side.
    /// </summary>
    /// <param name="left">The inset value for the left side.</param>
    /// <param name="top">The inset value for the top side.</param>
    /// <param name="front">The inset value for the front side.</param>
    /// <param name="right">The inset value for the right side.</param>
    /// <param name="bottom">The inset value for the bottom side.</param>
    /// <param name="back">The inset value for the back side.</param>
    public Insets3(
        float left,
        float top,
        float front,
        float right,
        float bottom,
        float back)
    {
        _left = left;
        _top = top;
        _front = front;
        _right = right;
        _bottom = bottom;
        _back = back;
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
    /// Gets the inset value for the front side.
    /// </summary>
    public float Front => _front;

    /// <summary>
    /// Gets the inset value for the right side.
    /// </summary>
    public float Right => _right;

    /// <summary>
    /// Gets the inset value for the bottom side.
    /// </summary>
    public float Bottom => _bottom;

    /// <summary>
    /// Gets the inset value for the back side.
    /// </summary>
    public float Back => _back;

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
    /// Gets the combined depth inset.
    /// </summary>
    /// <remarks>
    /// This value is the sum of the <see cref="Front"/> and
    /// <see cref="Back"/> insets.
    /// </remarks>
    public float Depth => _front + _back;

    /// <summary>
    /// Applies the insets to the specified bounds.
    /// </summary>
    /// <param name="bounds">The bounds to which the insets are applied.</param>
    /// <returns>
    /// A new <see cref="Bounds3"/> reduced by the specified insets.
    /// </returns>
    /// <remarks>
    /// The left, top, and front positions are moved inward by their
    /// respective inset values, while the width, height, and depth are
    /// reduced by the combined insets.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bounds3 Apply(in Bounds3 bounds)
        => new Bounds3(
            bounds.Left + _left,
            bounds.Top + _top,
            bounds.Front + _front,
            bounds.Width - Horizontal,
            bounds.Height - Vertical,
            bounds.Depth - Depth
        );

    /// <summary>
    /// Expands the specified bounds by the insets.
    /// </summary>
    /// <param name="bounds">The bounds to expand.</param>
    /// <returns>
    /// A new <see cref="Bounds3"/> expanded by the specified insets.
    /// </returns>
    /// <remarks>
    /// The left, top, and front positions are moved outward by their
    /// respective inset values, while the width, height, and depth are
    /// increased by the combined insets.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bounds3 Expand(in Bounds3 bounds)
        => new Bounds3(
            bounds.Left - _left,
            bounds.Top - _top,
            bounds.Front - _front,
            bounds.Width + Horizontal,
            bounds.Height + Vertical,
            bounds.Depth + Depth
        );

    /// <summary>
    /// Parses a string representation of <see cref="Insets3"/>.
    /// </summary>
    /// <param name="s">
    /// A string containing one, two, three, four, or six numeric values.
    /// </param>
    /// <returns>The parsed <see cref="Insets3"/>.</returns>
    /// <exception cref="FormatException">
    /// Thrown when the specified string does not contain one, two, three,
    /// four, or six valid numeric values.
    /// </exception>
    /// <remarks>
    /// Values may be separated by commas or spaces and are interpreted
    /// using <see cref="CultureInfo.InvariantCulture"/>.
    /// </remarks>
    public static Insets3 Parse(string s)
    {
        if (TryParse(s, out var result))
            return result;

        throw new FormatException(
            $"Invalid Insets3 format: '{s}'. Expected 1, 2, 3, 4, or 6 numeric values separated by ',' or ' '.");
    }

    /// <summary>
    /// Attempts to parse a string representation of <see cref="Insets3"/>.
    /// </summary>
    /// <param name="s">
    /// A string containing one, two, three, four, or six numeric values.
    /// </param>
    /// <param name="result">
    /// When this method returns <see langword="true"/>, contains the parsed
    /// insets; otherwise, contains <see cref="Zero"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the string contains exactly one, two,
    /// three, four, or six valid numeric values; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// A single value is applied uniformly to all six sides.
    /// </para>
    /// <para>
    /// Two values represent horizontal and vertical insets; the front and
    /// back insets are set to zero.
    /// </para>
    /// <para>
    /// Three values represent horizontal, vertical, and depth insets.
    /// </para>
    /// <para>
    /// Four values represent left, top, right, and bottom insets; the front
    /// and back insets are set to zero.
    /// </para>
    /// <para>
    /// Six values represent left, top, front, right, bottom, and back insets.
    /// </para>
    /// </remarks>
    public static bool TryParse(string? s, out Insets3 result)
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
                1 => new Insets3(values[0]),

                2 => new Insets3(
                    values[0],
                    values[1]),

                3 => new Insets3(
                    values[0],
                    values[1],
                    values[2]),

                4 => new Insets3(
                    values[0],
                    values[1],
                    0.0f,
                    values[2],
                    values[3],
                    0.0f),

                6 => new Insets3(
                    values[0],
                    values[1],
                    values[2],
                    values[3],
                    values[4],
                    values[5]),

                _ => Zero
            };

            return values.Length == 1
                || values.Length == 2
                || values.Length == 3
                || values.Length == 4
                || values.Length == 6;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Returns the string representation of this <see cref="Insets3"/>.
    /// </summary>
    /// <returns>
    /// A string containing the left, top, front, right, bottom, and back
    /// inset values separated by commas.
    /// </returns>
    /// <remarks>
    /// Numeric values are formatted using
    /// <see cref="CultureInfo.InvariantCulture"/>.
    /// </remarks>
    public override string ToString()
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0}, {1}, {2}, {3}, {4}, {5}",
            Left,
            Top,
            Front,
            Right,
            Bottom,
            Back
        );
    }
}
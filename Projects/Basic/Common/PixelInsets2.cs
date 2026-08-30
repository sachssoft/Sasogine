using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Represents two-dimensional pixel insets defined by integer values for
/// the left, top, right, and bottom edges.
/// </summary>
public readonly struct PixelInsets2
{
    private readonly int _left;
    private readonly int _top;
    private readonly int _right;
    private readonly int _bottom;

    /// <summary>
    /// Represents pixel insets with all values set to zero.
    /// </summary>
    public static readonly PixelInsets2 Zero = new PixelInsets2(0);

    /// <summary>
    /// Initializes a new instance with the same inset value for all four edges.
    /// </summary>
    /// <param name="uniform">
    /// The inset value in pixels applied to the left, top, right, and bottom edges.
    /// </param>
    public PixelInsets2(int uniform)
    {
        _left = uniform;
        _top = uniform;
        _right = uniform;
        _bottom = uniform;
    }

    /// <summary>
    /// Initializes a new instance with separate horizontal and vertical
    /// inset values.
    /// </summary>
    /// <param name="horizontal">
    /// The inset value in pixels applied to the left and right edges.
    /// </param>
    /// <param name="vertical">
    /// The inset value in pixels applied to the top and bottom edges.
    /// </param>
    public PixelInsets2(int horizontal, int vertical)
    {
        _left = horizontal;
        _right = horizontal;
        _top = vertical;
        _bottom = vertical;
    }

    /// <summary>
    /// Initializes a new instance with individual inset values for each edge.
    /// </summary>
    /// <param name="left">The left inset in pixels.</param>
    /// <param name="top">The top inset in pixels.</param>
    /// <param name="right">The right inset in pixels.</param>
    /// <param name="bottom">The bottom inset in pixels.</param>
    public PixelInsets2(int left, int top, int right, int bottom)
    {
        _left = left;
        _top = top;
        _right = right;
        _bottom = bottom;
    }

    /// <summary>
    /// Gets the left inset in pixels.
    /// </summary>
    public int Left => _left;

    /// <summary>
    /// Gets the top inset in pixels.
    /// </summary>
    public int Top => _top;

    /// <summary>
    /// Gets the right inset in pixels.
    /// </summary>
    public int Right => _right;

    /// <summary>
    /// Gets the bottom inset in pixels.
    /// </summary>
    public int Bottom => _bottom;

    /// <summary>
    /// Gets the combined horizontal inset in pixels.
    /// </summary>
    /// <remarks>
    /// This value is the sum of the left and right insets.
    /// </remarks>
    public int Horizontal => _left + _right;

    /// <summary>
    /// Gets the combined vertical inset in pixels.
    /// </summary>
    /// <remarks>
    /// This value is the sum of the top and bottom insets.
    /// </remarks>
    public int Vertical => _top + _bottom;

    /// <summary>
    /// Applies these insets to the specified pixel bounds.
    /// </summary>
    /// <param name="bounds">
    /// The pixel bounds to which the insets are applied.
    /// </param>
    /// <returns>
    /// New pixel bounds reduced by the corresponding inset values.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelBounds2 Apply(in PixelBounds2 bounds)
        => new PixelBounds2(
            bounds.Left + _left,
            bounds.Top + _top,
            bounds.Width - Horizontal,
            bounds.Height - Vertical
        );

    /// <summary>
    /// Expands the specified pixel bounds by these insets.
    /// </summary>
    /// <param name="bounds">
    /// The pixel bounds to expand.
    /// </param>
    /// <returns>
    /// New pixel bounds expanded by the corresponding inset values.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelBounds2 Expand(in PixelBounds2 bounds)
        => new PixelBounds2(
            bounds.Left - _left,
            bounds.Top - _top,
            bounds.Width + Horizontal,
            bounds.Height + Vertical
        );

    /// <summary>
    /// Parses a string representation of pixel insets.
    /// </summary>
    /// <param name="s">
    /// The string containing one, two, or four integer inset values.
    /// </param>
    /// <returns>
    /// The parsed <see cref="PixelInsets2"/>.
    /// </returns>
    /// <exception cref="FormatException">
    /// Thrown when the string does not contain one, two, or four valid
    /// integer values.
    /// </exception>
    public static PixelInsets2 Parse(string s)
    {
        if (TryParse(s, out var result))
            return result;

        throw new FormatException(
            $"Invalid PixelInsets2 format: '{s}'. Expected 1, 2, or 4 integer values separated by ',' or ' '.");
    }

    /// <summary>
    /// Attempts to parse a string representation of pixel insets.
    /// </summary>
    /// <param name="s">
    /// The string containing one, two, or four integer inset values.
    /// </param>
    /// <param name="result">
    /// When this method returns <see langword="true"/>, contains the parsed
    /// pixel insets; otherwise, contains <see cref="Zero"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the string contains exactly one, two,
    /// or four valid integer values; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryParse(string? s, out PixelInsets2 result)
    {
        result = Zero;

        if (string.IsNullOrWhiteSpace(s))
            return false;

        var parts = s.Split(
            new[] { ',', ' ' },
            StringSplitOptions.RemoveEmptyEntries);

        int[] values = new int[parts.Length];

        try
        {
            for (int i = 0; i < parts.Length; i++)
                values[i] = int.Parse(
                    parts[i],
                    CultureInfo.InvariantCulture);

            result = values.Length switch
            {
                1 => new PixelInsets2(values[0]),
                2 => new PixelInsets2(values[0], values[1]),
                4 => new PixelInsets2(
                    values[0],
                    values[1],
                    values[2],
                    values[3]),
                _ => Zero
            };

            return values.Length is 1 or 2 or 4;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Returns a string representation of these pixel insets.
    /// </summary>
    /// <returns>
    /// A string containing the left, top, right, and bottom inset values
    /// separated by commas.
    /// </returns>
    public override string ToString()
        => string.Format(
            CultureInfo.InvariantCulture,
            "{0}, {1}, {2}, {3}",
            Left,
            Top,
            Right,
            Bottom
        );
}
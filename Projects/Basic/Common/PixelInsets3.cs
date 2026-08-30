using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Represents three-dimensional pixel insets defined by integer values for
/// the left, top, front, right, bottom, and back edges.
/// </summary>
public readonly struct PixelInsets3
{
    private readonly int _left;
    private readonly int _top;
    private readonly int _front;
    private readonly int _right;
    private readonly int _bottom;
    private readonly int _back;

    /// <summary>
    /// Represents pixel insets with all values set to zero.
    /// </summary>
    public static readonly PixelInsets3 Zero = new PixelInsets3(0);

    /// <summary>
    /// Initializes a new instance with the same inset value for all six edges.
    /// </summary>
    /// <param name="uniform">
    /// The inset value in pixels applied to all edges.
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelInsets3(int uniform)
    {
        _left = uniform;
        _top = uniform;
        _front = uniform;
        _right = uniform;
        _bottom = uniform;
        _back = uniform;
    }

    /// <summary>
    /// Initializes a new instance with separate horizontal, vertical,
    /// and depth inset values.
    /// </summary>
    /// <param name="horizontal">
    /// The inset value in pixels applied to the left and right edges.
    /// </param>
    /// <param name="vertical">
    /// The inset value in pixels applied to the top and bottom edges.
    /// </param>
    /// <param name="depth">
    /// The inset value in pixels applied to the front and back edges.
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelInsets3(
        int horizontal,
        int vertical,
        int depth)
    {
        _left = horizontal;
        _right = horizontal;
        _top = vertical;
        _bottom = vertical;
        _front = depth;
        _back = depth;
    }

    /// <summary>
    /// Initializes a new instance with individual inset values for each edge.
    /// </summary>
    /// <param name="left">The left inset in pixels.</param>
    /// <param name="top">The top inset in pixels.</param>
    /// <param name="front">The front inset in pixels.</param>
    /// <param name="right">The right inset in pixels.</param>
    /// <param name="bottom">The bottom inset in pixels.</param>
    /// <param name="back">The back inset in pixels.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelInsets3(
        int left,
        int top,
        int front,
        int right,
        int bottom,
        int back)
    {
        _left = left;
        _top = top;
        _front = front;
        _right = right;
        _bottom = bottom;
        _back = back;
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
    /// Gets the front inset in pixels.
    /// </summary>
    public int Front => _front;

    /// <summary>
    /// Gets the right inset in pixels.
    /// </summary>
    public int Right => _right;

    /// <summary>
    /// Gets the bottom inset in pixels.
    /// </summary>
    public int Bottom => _bottom;

    /// <summary>
    /// Gets the back inset in pixels.
    /// </summary>
    public int Back => _back;

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
    /// Gets the combined depth inset in pixels.
    /// </summary>
    /// <remarks>
    /// This value is the sum of the front and back insets.
    /// </remarks>
    public int Depth => _front + _back;

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
    public PixelBounds3 Apply(in PixelBounds3 bounds)
        => new PixelBounds3(
            bounds.Left + _left,
            bounds.Top + _top,
            bounds.Front + _front,
            bounds.Width - Horizontal,
            bounds.Height - Vertical,
            bounds.Depth - Depth
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
    public PixelBounds3 Expand(in PixelBounds3 bounds)
        => new PixelBounds3(
            bounds.Left - _left,
            bounds.Top - _top,
            bounds.Front - _front,
            bounds.Width + Horizontal,
            bounds.Height + Vertical,
            bounds.Depth + Depth
        );

    /// <summary>
    /// Parses a string representation of pixel insets.
    /// </summary>
    /// <param name="s">
    /// The string containing one, three, or six integer inset values.
    /// </param>
    /// <returns>
    /// The parsed <see cref="PixelInsets3"/>.
    /// </returns>
    /// <exception cref="FormatException">
    /// Thrown when the string does not contain one, three, or six valid
    /// integer values.
    /// </exception>
    public static PixelInsets3 Parse(string s)
    {
        if (TryParse(s, out var result))
            return result;

        throw new FormatException(
            $"Invalid PixelInsets3 format: '{s}'. Expected 1, 3, or 6 integer values separated by ',' or ' '.");
    }

    /// <summary>
    /// Attempts to parse a string representation of pixel insets.
    /// </summary>
    /// <param name="s">
    /// The string containing one, three, or six integer inset values.
    /// </param>
    /// <param name="result">
    /// When this method returns <see langword="true"/>, contains the parsed
    /// pixel insets; otherwise, contains <see cref="Zero"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the string contains exactly one, three,
    /// or six valid integer values; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryParse(
        string? s,
        out PixelInsets3 result)
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
                1 => new PixelInsets3(values[0]),

                3 => new PixelInsets3(
                    values[0],
                    values[1],
                    values[2]),

                6 => new PixelInsets3(
                    values[0],
                    values[1],
                    values[2],
                    values[3],
                    values[4],
                    values[5]),

                _ => Zero
            };

            return values.Length is 1 or 3 or 6;
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
    /// A string containing the left, top, front, right, bottom, and back
    /// inset values separated by commas.
    /// </returns>
    public override string ToString()
        => string.Format(
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
using Microsoft.Xna.Framework;
using System;
using System.Globalization;

namespace Sachssoft.Engine.Graphics;

/// <summary>
/// Provides utility methods for converting, parsing, and adjusting colors.
/// </summary>
public static class ColorUtils
{
    /// <summary>
    /// Converts the specified color to a hexadecimal color string.
    /// </summary>
    /// <param name="color">
    /// The color to convert.
    /// </param>
    /// <param name="alpha">
    /// <see langword="true"/> to include the alpha channel using the
    /// <c>#AARRGGBB</c> format; otherwise, <see langword="false"/> to use
    /// the <c>#RRGGBB</c> format.
    /// </param>
    /// <returns>
    /// The hexadecimal representation of the specified color.
    /// </returns>
    public static string ToHexString(
        Color color,
        bool alpha = true)
    {
        var r = color.R;
        var g = color.G;
        var b = color.B;
        var a = color.A;

        if (alpha)
            return $"#{a:X2}{r:X2}{g:X2}{b:X2}";

        return $"#{r:X2}{g:X2}{b:X2}";
    }

    /// <summary>
    /// Creates a color from a hexadecimal color string.
    /// </summary>
    /// <param name="hex">
    /// The hexadecimal color string in <c>RRGGBB</c>,
    /// <c>#RRGGBB</c>, <c>AARRGGBB</c>, or <c>#AARRGGBB</c> format.
    /// </param>
    /// <returns>
    /// The color represented by the hexadecimal string.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="hex"/> is null, empty, or consists
    /// only of white-space characters.
    /// </exception>
    /// <exception cref="FormatException">
    /// Thrown when <paramref name="hex"/> does not contain a valid
    /// hexadecimal color value.
    /// </exception>
    public static Color FromHexString(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            throw new ArgumentException(
                "Hex string cannot be null or empty.",
                nameof(hex));
        }

        hex = hex.TrimStart('#');

        byte r;
        byte g;
        byte b;
        byte a = 255;

        if (hex.Length == 6)
        {
            r = byte.Parse(
                hex.Substring(0, 2),
                NumberStyles.HexNumber);

            g = byte.Parse(
                hex.Substring(2, 2),
                NumberStyles.HexNumber);

            b = byte.Parse(
                hex.Substring(4, 2),
                NumberStyles.HexNumber);
        }
        else if (hex.Length == 8)
        {
            a = byte.Parse(
                hex.Substring(0, 2),
                NumberStyles.HexNumber);

            r = byte.Parse(
                hex.Substring(2, 2),
                NumberStyles.HexNumber);

            g = byte.Parse(
                hex.Substring(4, 2),
                NumberStyles.HexNumber);

            b = byte.Parse(
                hex.Substring(6, 2),
                NumberStyles.HexNumber);
        }
        else
        {
            throw new FormatException(
                "Hex string must be RRGGBB or AARRGGBB.");
        }

        return new Color(r, g, b, a);
    }

    /// <summary>
    /// Parses a hexadecimal color string and returns a fallback color
    /// if the value cannot be parsed.
    /// </summary>
    /// <param name="s">
    /// The hexadecimal color string to parse.
    /// </param>
    /// <param name="alpha">
    /// <see langword="true"/> to expect the <c>#AARRGGBB</c> format;
    /// otherwise, <see langword="false"/> to expect <c>#RRGGBB</c>.
    /// </param>
    /// <param name="fallback">
    /// The color returned when <paramref name="s"/> cannot be parsed.
    /// </param>
    /// <returns>
    /// The parsed color, or <paramref name="fallback"/> if parsing fails.
    /// </returns>
    public static Color Parse(
        string? s,
        bool alpha = false,
        Color fallback = default)
    {
        if (TryParse(s, alpha, out var result))
            return result;

        return fallback;
    }

    /// <summary>
    /// Attempts to parse a hexadecimal color string.
    /// </summary>
    /// <param name="s">
    /// The hexadecimal color string to parse.
    /// </param>
    /// <param name="alpha">
    /// <see langword="true"/> to expect the <c>#AARRGGBB</c> format;
    /// otherwise, <see langword="false"/> to expect <c>#RRGGBB</c>.
    /// </param>
    /// <param name="result">
    /// When this method returns <see langword="true"/>, contains the
    /// parsed color; otherwise, contains the default color value.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the color was parsed successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryParse(
        string? s,
        bool alpha,
        out Color result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(s) ||
            !s.StartsWith("#"))
        {
            return false;
        }

        s = s.Substring(1);

        int expectedLength =
            alpha ? 8 : 6;

        if (s.Length != expectedLength)
            return false;

        try
        {
            byte r = byte.Parse(
                s.Substring(alpha ? 2 : 0, 2),
                NumberStyles.HexNumber);

            byte g = byte.Parse(
                s.Substring(alpha ? 4 : 2, 2),
                NumberStyles.HexNumber);

            byte b = byte.Parse(
                s.Substring(alpha ? 6 : 4, 2),
                NumberStyles.HexNumber);

            byte a = alpha
                ? byte.Parse(
                    s.Substring(0, 2),
                    NumberStyles.HexNumber)
                : (byte)255;

            result = new Color(r, g, b, a);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Creates a color with the specified normalized alpha value while
    /// preserving the red, green, and blue channels.
    /// </summary>
    /// <param name="color">
    /// The source color.
    /// </param>
    /// <param name="alpha">
    /// The alpha value in the range from <c>0</c> for fully transparent
    /// to <c>1</c> for fully opaque. Values outside this range are clamped.
    /// </param>
    /// <returns>
    /// A color with the adjusted alpha channel.
    /// </returns>
    public static Color AdjustAlpha(
        Color color,
        float alpha)
    {
        return new Color(
            color.R,
            color.G,
            color.B,
            (byte)(float.Clamp(alpha, 0f, 1f) * 255));
    }

    /// <summary>
    /// Creates a color with the specified normalized red value while
    /// preserving the green, blue, and alpha channels.
    /// </summary>
    /// <param name="color">
    /// The source color.
    /// </param>
    /// <param name="red">
    /// The red value in the range from <c>0</c> to <c>1</c>.
    /// Values outside this range are clamped.
    /// </param>
    /// <returns>
    /// A color with the adjusted red channel.
    /// </returns>
    public static Color AdjustRed(
        Color color,
        float red)
    {
        return new Color(
            (byte)(float.Clamp(red, 0f, 1f) * 255),
            color.G,
            color.B,
            color.A);
    }

    /// <summary>
    /// Creates a color with the specified normalized green value while
    /// preserving the red, blue, and alpha channels.
    /// </summary>
    /// <param name="color">
    /// The source color.
    /// </param>
    /// <param name="green">
    /// The green value in the range from <c>0</c> to <c>1</c>.
    /// Values outside this range are clamped.
    /// </param>
    /// <returns>
    /// A color with the adjusted green channel.
    /// </returns>
    public static Color AdjustGreen(
        Color color,
        float green)
    {
        return new Color(
            color.R,
            (byte)(float.Clamp(green, 0f, 1f) * 255),
            color.B,
            color.A);
    }

    /// <summary>
    /// Creates a color with the specified normalized blue value while
    /// preserving the red, green, and alpha channels.
    /// </summary>
    /// <param name="color">
    /// The source color.
    /// </param>
    /// <param name="blue">
    /// The blue value in the range from <c>0</c> to <c>1</c>.
    /// Values outside this range are clamped.
    /// </param>
    /// <returns>
    /// A color with the adjusted blue channel.
    /// </returns>
    public static Color AdjustBlue(
        Color color,
        float blue)
    {
        return new Color(
            color.R,
            color.G,
            (byte)(float.Clamp(blue, 0f, 1f) * 255),
            color.A);
    }
}
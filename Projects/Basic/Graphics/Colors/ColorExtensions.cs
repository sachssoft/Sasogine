using Microsoft.Xna.Framework;
using System;
namespace Sachssoft.Engine.Graphics.Colors;

/// <summary>
/// Provides conversion and manipulation operations for MonoGame <see cref="Color"/> values.
/// </summary>
public static class ColorExtensions
{
    /// <summary>Converts a color to the normalized RGB color model.</summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The converted RGB color.</returns>
    public static RgbColor ToRgb(this Color color) => RgbColor.FromColor(color);

    /// <summary>Converts a color to the HSL color model.</summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The converted HSL color.</returns>
    public static HslColor ToHsl(this Color color) => HslColor.FromColor(color);

    /// <summary>Converts a color to the HSV color model.</summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The converted HSV color.</returns>
    public static HsvColor ToHsv(this Color color) => HsvColor.FromColor(color);

    /// <summary>Converts an sRGB color to linear RGB.</summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The converted linear RGB color.</returns>
    public static LinearRgbColor ToLinearRgb(this Color color) => LinearRgbColor.FromColor(color);

    /// <summary>Converts a color to the OKLab color model.</summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The converted OKLab color.</returns>
    public static OklabColor ToOklab(this Color color) => OklabColor.FromColor(color);

    /// <summary>Darkens a color by reducing its HSL lightness.</summary>
    /// <param name="color">The color to darken.</param>
    /// <param name="amount">The darkening amount in the range <c>0.0</c> through <c>1.0</c>.</param>
    /// <returns>The darkened color.</returns>
    public static Color Darken(this Color color, float amount)
    {
        var hsl = color.ToHsl();
        amount = MathHelper.Clamp(amount, 0f, 1f);
        return new HslColor(hsl.Hue, hsl.Saturation, hsl.Lightness * (1f - amount), hsl.Alpha).ToColor();
    }

    /// <summary>Lightens a color by increasing its HSL lightness.</summary>
    /// <param name="color">The color to lighten.</param>
    /// <param name="amount">The lightening amount in the range <c>0.0</c> through <c>1.0</c>.</param>
    /// <returns>The lightened color.</returns>
    public static Color Lighten(this Color color, float amount)
    {
        var hsl = color.ToHsl();
        amount = MathHelper.Clamp(amount, 0f, 1f);
        return new HslColor(
            hsl.Hue, hsl.Saturation, hsl.Lightness + (1f - hsl.Lightness) * amount, hsl.Alpha).ToColor();
    }

    /// <summary>Increases the HSL saturation of a color.</summary>
    /// <param name="color">The color to saturate.</param>
    /// <param name="amount">The saturation amount in the range <c>0.0</c> through <c>1.0</c>.</param>
    /// <returns>The saturated color.</returns>
    public static Color Saturate(this Color color, float amount)
    {
        var hsl = color.ToHsl();
        amount = MathHelper.Clamp(amount, 0f, 1f);
        return new HslColor(
            hsl.Hue, hsl.Saturation + (1f - hsl.Saturation) * amount, hsl.Lightness, hsl.Alpha).ToColor();
    }

    /// <summary>Reduces the HSL saturation of a color.</summary>
    /// <param name="color">The color to desaturate.</param>
    /// <param name="amount">The desaturation amount in the range <c>0.0</c> through <c>1.0</c>.</param>
    /// <returns>The desaturated color.</returns>
    public static Color Desaturate(this Color color, float amount)
    {
        var hsl = color.ToHsl();
        amount = MathHelper.Clamp(amount, 0f, 1f);
        return new HslColor(hsl.Hue, hsl.Saturation * (1f - amount), hsl.Lightness, hsl.Alpha).ToColor();
    }

    /// <summary>Rotates the hue of a color by the specified number of degrees.</summary>
    /// <param name="color">The color whose hue to rotate.</param>
    /// <param name="degrees">The hue rotation in degrees. Positive and negative values are supported.</param>
    /// <returns>The color with the rotated hue.</returns>
    public static Color RotateHue(this Color color, float degrees)
    {
        var hsl = color.ToHsl();
        return new HslColor(hsl.Hue + degrees, hsl.Saturation, hsl.Lightness, hsl.Alpha).ToColor();
    }

    /// <summary>Applies a gamma adjustment to the RGB channels of a color while preserving alpha.</summary>
    /// <param name="color">The color to adjust.</param>
    /// <param name="gamma">The positive gamma value to apply.</param>
    /// <returns>The gamma-adjusted color.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="gamma"/> is less than or equal to zero.</exception>
    public static Color AdjustGamma(this Color color, float gamma)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(gamma);
        var inverse = 1f / gamma;
        return new Color(
            MathF.Pow(color.R / 255f, inverse), MathF.Pow(color.G / 255f, inverse),
            MathF.Pow(color.B / 255f, inverse), color.A / 255f);
    }
}

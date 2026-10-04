using Microsoft.Xna.Framework;
using Sachssoft.Engine.Graphics;
using System;

namespace Sachssoft.Engine.Graphics.Colors;

/// <summary>
/// Represents a color in the HSL color model.
/// </summary>
public readonly struct HslColor : IColorConvertible<HslColor>, IEquatable<HslColor>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HslColor"/> structure.
    /// </summary>
    /// <param name="hue">The hue in degrees. Values are normalized to the range <c>0</c> through <c>360</c>.</param>
    /// <param name="saturation">The saturation in the range <c>0.0</c> through <c>1.0</c>.</param>
    /// <param name="lightness">The lightness component.</param>
    /// <param name="alpha">The alpha component in the range <c>0.0</c> through <c>1.0</c>.</param>
    public HslColor(float hue, float saturation, float lightness, float alpha = 1f)
    {
        Hue = NormalizeHue(hue);
        Saturation = MathHelper.Clamp(saturation, 0f, 1f);
        Lightness = MathHelper.Clamp(lightness, 0f, 1f);
        Alpha = MathHelper.Clamp(alpha, 0f, 1f);
    }

    /// <summary>
    /// Gets the hue in degrees.
    /// </summary>
    public float Hue { get; }

    /// <summary>
    /// Gets the saturation component.
    /// </summary>
    public float Saturation { get; }

    /// <summary>
    /// Gets the lightness component.
    /// </summary>
    public float Lightness { get; }

    /// <summary>
    /// Gets the alpha component.
    /// </summary>
    public float Alpha { get; }

    /// <summary>
    /// Creates a <see cref="HslColor"/> from a standard <see cref="Color"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The converted HSL color.</returns>
    public static HslColor FromColor(Color color)
    {
        var r = color.R / 255f;
        var g = color.G / 255f;
        var b = color.B / 255f;
        var max = MathF.Max(r, MathF.Max(g, b));
        var min = MathF.Min(r, MathF.Min(g, b));
        var delta = max - min;
        var lightness = (max + min) * 0.5f;
        var hue = 0f;
        var saturation = 0f;

        if (delta != 0f)
        {
            saturation = delta / (1f - MathF.Abs(2f * lightness - 1f));
            if (max == r) hue = 60f * (((g - b) / delta) % 6f);
            else if (max == g) hue = 60f * (((b - r) / delta) + 2f);
            else hue = 60f * (((r - g) / delta) + 4f);
            if (hue < 0f) hue += 360f;
        }

        return new HslColor(hue, saturation, lightness, color.A / 255f);
    }

    /// <summary>
    /// Converts this color to the standard <see cref="Color"/> representation.
    /// </summary>
    /// <returns>The converted color.</returns>
    public Color ToColor()
    {
        var chroma = (1f - MathF.Abs(2f * Lightness - 1f)) * Saturation;
        var h = Hue / 60f;
        var x = chroma * (1f - MathF.Abs(h % 2f - 1f));
        float r, g, b;

        if (h < 1f) (r, g, b) = (chroma, x, 0f);
        else if (h < 2f) (r, g, b) = (x, chroma, 0f);
        else if (h < 3f) (r, g, b) = (0f, chroma, x);
        else if (h < 4f) (r, g, b) = (0f, x, chroma);
        else if (h < 5f) (r, g, b) = (x, 0f, chroma);
        else (r, g, b) = (chroma, 0f, x);

        var m = Lightness - chroma * 0.5f;
        return new Color(r + m, g + m, b + m, Alpha);
    }

    /// <summary>
    /// Converts this color to a normalized RGB vector.
    /// </summary>
    /// <returns>A vector containing normalized red, green, and blue components.</returns>
    public Vector3 ToVector3() => ToColor().ToVector3();

    /// <summary>
    /// Converts this color to a normalized RGBA vector.
    /// </summary>
    /// <returns>A vector containing normalized red, green, blue, and alpha components.</returns>
    public Vector4 ToVector4() => ToColor().ToVector4();

    /// <summary>
    /// Determines whether this color equals another <see cref="HslColor"/>.
    /// </summary>
    /// <param name="other">The color to compare with this instance.</param>
    /// <returns><see langword="true"/> if the colors are equal; otherwise, <see langword="false"/>.</returns>
    public bool Equals(HslColor other) =>
        Hue.Equals(other.Hue) &&
        Saturation.Equals(other.Saturation) &&
        Lightness.Equals(other.Lightness) &&
        Alpha.Equals(other.Alpha);

    /// <summary>
    /// Determines whether this color equals the specified object.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns><see langword="true"/> if <paramref name="obj"/> is an equal <see cref="HslColor"/>; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is HslColor other && Equals(other);

    /// <summary>
    /// Returns a hash code for this color.
    /// </summary>
    /// <returns>A hash code for this instance.</returns>
    public override int GetHashCode() => HashCode.Combine(Hue, Saturation, Lightness, Alpha);

    /// <summary>
    /// Determines whether two <see cref="HslColor"/> values are equal.
    /// </summary>
    /// <param name="left">The first color.</param>
    /// <param name="right">The second color.</param>
    /// <returns><see langword="true"/> if both colors are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(HslColor left, HslColor right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="HslColor"/> values are not equal.
    /// </summary>
    /// <param name="left">The first color.</param>
    /// <param name="right">The second color.</param>
    /// <returns><see langword="true"/> if both colors differ; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(HslColor left, HslColor right) => !left.Equals(right);
    private static float NormalizeHue(float hue)
    {
        hue %= 360f;
        return hue < 0f ? hue + 360f : hue;
    }
}

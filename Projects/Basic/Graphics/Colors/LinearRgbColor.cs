using Microsoft.Xna.Framework;
using Sachssoft.Engine.Graphics;
using System;

namespace Sachssoft.Engine.Graphics.Colors;

/// <summary>
/// Represents a color in the linear RGB color model.
/// </summary>
public readonly struct LinearRgbColor : IColorConvertible<LinearRgbColor>, IEquatable<LinearRgbColor>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LinearRgbColor"/> structure.
    /// </summary>
    /// <param name="red">The red component in the range <c>0.0</c> through <c>1.0</c>.</param>
    /// <param name="green">The green component in the range <c>0.0</c> through <c>1.0</c>.</param>
    /// <param name="blue">The blue component in the range <c>0.0</c> through <c>1.0</c>.</param>
    /// <param name="alpha">The alpha component in the range <c>0.0</c> through <c>1.0</c>.</param>
    public LinearRgbColor(float red, float green, float blue, float alpha = 1f)
    {
        Red = MathHelper.Clamp(red, 0f, 1f);
        Green = MathHelper.Clamp(green, 0f, 1f);
        Blue = MathHelper.Clamp(blue, 0f, 1f);
        Alpha = MathHelper.Clamp(alpha, 0f, 1f);
    }

    /// <summary>
    /// Gets the red component.
    /// </summary>
    public float Red { get; }

    /// <summary>
    /// Gets the green component.
    /// </summary>
    public float Green { get; }

    /// <summary>
    /// Gets the blue component.
    /// </summary>
    public float Blue { get; }

    /// <summary>
    /// Gets the alpha component.
    /// </summary>
    public float Alpha { get; }

    /// <summary>
    /// Creates a <see cref="LinearRgbColor"/> from a standard <see cref="Color"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The converted linear RGB color.</returns>
    public static LinearRgbColor FromColor(Color color) => new(ToLinear(color.R / 255f), ToLinear(color.G / 255f), ToLinear(color.B / 255f), color.A / 255f);

    /// <summary>
    /// Converts this color to the standard <see cref="Color"/> representation.
    /// </summary>
    /// <returns>The converted color.</returns>
    public Color ToColor() => new(ToSrgb(Red), ToSrgb(Green), ToSrgb(Blue), Alpha);

    /// <summary>
    /// Converts this color to a normalized RGB vector.
    /// </summary>
    /// <returns>A vector containing normalized red, green, and blue components.</returns>
    public Vector3 ToVector3()
    {
        var color = ToColor();
        return color.ToVector3();
    }

    /// <summary>
    /// Converts this color to a normalized RGBA vector.
    /// </summary>
    /// <returns>A vector containing normalized red, green, blue, and alpha components.</returns>
    public Vector4 ToVector4()
    {
        var color = ToColor();
        return color.ToVector4();
    }

    /// <summary>
    /// Determines whether this color equals another <see cref="LinearRgbColor"/>.
    /// </summary>
    /// <param name="other">The color to compare with this instance.</param>
    /// <returns><see langword="true"/> if the colors are equal; otherwise, <see langword="false"/>.</returns>
    public bool Equals(LinearRgbColor other) =>
        Red.Equals(other.Red) &&
        Green.Equals(other.Green) &&
        Blue.Equals(other.Blue) &&
        Alpha.Equals(other.Alpha);

    /// <summary>
    /// Determines whether this color equals the specified object.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns><see langword="true"/> if <paramref name="obj"/> is an equal <see cref="LinearRgbColor"/>; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is LinearRgbColor other && Equals(other);

    /// <summary>
    /// Returns a hash code for this color.
    /// </summary>
    /// <returns>A hash code for this instance.</returns>
    public override int GetHashCode() => HashCode.Combine(Red, Green, Blue, Alpha);

    /// <summary>
    /// Determines whether two <see cref="LinearRgbColor"/> values are equal.
    /// </summary>
    /// <param name="left">The first color.</param>
    /// <param name="right">The second color.</param>
    /// <returns><see langword="true"/> if both colors are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(LinearRgbColor left, LinearRgbColor right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="LinearRgbColor"/> values are not equal.
    /// </summary>
    /// <param name="left">The first color.</param>
    /// <param name="right">The second color.</param>
    /// <returns><see langword="true"/> if both colors differ; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(LinearRgbColor left, LinearRgbColor right) => !left.Equals(right);
    internal static float ToLinear(float value) => value <= 0.04045f ? value / 12.92f : MathF.Pow((value + 0.055f) / 1.055f, 2.4f);
    internal static float ToSrgb(float value) => value <= 0.0031308f ? value * 12.92f : 1.055f * MathF.Pow(value, 1f / 2.4f) - 0.055f;
}

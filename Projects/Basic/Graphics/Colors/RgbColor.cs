using Microsoft.Xna.Framework;
using Sachssoft.Engine.Graphics;
using System;

namespace Sachssoft.Engine.Graphics.Colors;

/// <summary>
/// Represents a color in the RGB color model.
/// </summary>
public readonly struct RgbColor : IColorConvertible<RgbColor>, IEquatable<RgbColor>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RgbColor"/> structure.
    /// </summary>
    /// <param name="red">The red component in the range <c>0.0</c> through <c>1.0</c>.</param>
    /// <param name="green">The green component in the range <c>0.0</c> through <c>1.0</c>.</param>
    /// <param name="blue">The blue component in the range <c>0.0</c> through <c>1.0</c>.</param>
    /// <param name="alpha">The alpha component in the range <c>0.0</c> through <c>1.0</c>.</param>
    public RgbColor(float red, float green, float blue, float alpha = 1f)
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
    /// Creates a <see cref="RgbColor"/> from a standard <see cref="Color"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The converted RGB color.</returns>
    public static RgbColor FromColor(Color color) => new(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);

    /// <summary>
    /// Converts this color to the standard <see cref="Color"/> representation.
    /// </summary>
    /// <returns>The converted color.</returns>
    public Color ToColor() => new(Red, Green, Blue, Alpha);

    /// <summary>
    /// Converts this color to a normalized RGB vector.
    /// </summary>
    /// <returns>A vector containing normalized red, green, and blue components.</returns>
    public Vector3 ToVector3() => new(Red, Green, Blue);

    /// <summary>
    /// Converts this color to a normalized RGBA vector.
    /// </summary>
    /// <returns>A vector containing normalized red, green, blue, and alpha components.</returns>
    public Vector4 ToVector4() => new(Red, Green, Blue, Alpha);

    /// <summary>
    /// Determines whether this color equals another <see cref="RgbColor"/>.
    /// </summary>
    /// <param name="other">The color to compare with this instance.</param>
    /// <returns><see langword="true"/> if the colors are equal; otherwise, <see langword="false"/>.</returns>
    public bool Equals(RgbColor other) =>
        Red.Equals(other.Red) &&
        Green.Equals(other.Green) &&
        Blue.Equals(other.Blue) &&
        Alpha.Equals(other.Alpha);

    /// <summary>
    /// Determines whether this color equals the specified object.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns><see langword="true"/> if <paramref name="obj"/> is an equal <see cref="RgbColor"/>; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is RgbColor other && Equals(other);

    /// <summary>
    /// Returns a hash code for this color.
    /// </summary>
    /// <returns>A hash code for this instance.</returns>
    public override int GetHashCode() => HashCode.Combine(Red, Green, Blue, Alpha);

    /// <summary>
    /// Determines whether two <see cref="RgbColor"/> values are equal.
    /// </summary>
    /// <param name="left">The first color.</param>
    /// <param name="right">The second color.</param>
    /// <returns><see langword="true"/> if both colors are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(RgbColor left, RgbColor right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="RgbColor"/> values are not equal.
    /// </summary>
    /// <param name="left">The first color.</param>
    /// <param name="right">The second color.</param>
    /// <returns><see langword="true"/> if both colors differ; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(RgbColor left, RgbColor right) => !left.Equals(right);
}

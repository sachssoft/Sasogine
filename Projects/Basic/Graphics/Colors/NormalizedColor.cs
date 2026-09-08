using System;
using System.Numerics;

namespace Sachssoft.Sasogine.Graphics;

/// <summary>
/// Represents a color using normalized floating-point components.
/// </summary>
/// <remarks>
/// Each color component is represented in the range <c>0.0</c> through <c>1.0</c>.
/// </remarks>
public readonly struct NormalizedColor : IColorConvertible<NormalizedColor>
{
    /// <summary>
    /// Gets the red component.
    /// </summary>
    public float R { get; }

    /// <summary>
    /// Gets the green component.
    /// </summary>
    public float G { get; }

    /// <summary>
    /// Gets the blue component.
    /// </summary>
    public float B { get; }

    /// <summary>
    /// Gets the alpha component.
    /// </summary>
    public float A { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NormalizedColor"/> struct.
    /// </summary>
    /// <param name="r">The red component.</param>
    /// <param name="g">The green component.</param>
    /// <param name="b">The blue component.</param>
    /// <param name="a">The alpha component.</param>
    public NormalizedColor(
        float r,
        float g,
        float b,
        float a = 1f)
    {
        R = Math.Clamp(r, 0f, 1f);
        G = Math.Clamp(g, 0f, 1f);
        B = Math.Clamp(b, 0f, 1f);
        A = Math.Clamp(a, 0f, 1f);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NormalizedColor"/> struct
    /// from a normalized RGBA vector.
    /// </summary>
    /// <param name="value">
    /// The normalized RGBA vector.
    /// </param>
    public NormalizedColor(Vector4 value)
        : this(value.X, value.Y, value.Z, value.W)
    {
    }

    /// <summary>
    /// Creates a normalized color from the specified standard color.
    /// </summary>
    /// <param name="color">
    /// The color to convert.
    /// </param>
    /// <returns>
    /// The normalized color.
    /// </returns>
    public static NormalizedColor FromColor(Color color)
    {
        const float scale = 1f / 255f;

        return new NormalizedColor(
            color.R * scale,
            color.G * scale,
            color.B * scale,
            color.A * scale);
    }

    /// <summary>
    /// Converts this color to the standard <see cref="Color"/> representation.
    /// </summary>
    /// <returns>
    /// The converted color.
    /// </returns>
    public Color ToColor()
    {
        return new Color(
            (byte)MathF.Round(R * 255f),
            (byte)MathF.Round(G * 255f),
            (byte)MathF.Round(B * 255f),
            (byte)MathF.Round(A * 255f));
    }

    /// <summary>
    /// Converts this color to a normalized RGB vector.
    /// </summary>
    /// <returns>
    /// A vector containing the red, green, and blue components.
    /// </returns>
    public Vector3 ToVector3()
    {
        return new Vector3(R, G, B);
    }

    /// <summary>
    /// Converts this color to a normalized RGBA vector.
    /// </summary>
    /// <returns>
    /// A vector containing the red, green, blue, and alpha components.
    /// </returns>
    public Vector4 ToVector4()
    {
        return new Vector4(R, G, B, A);
    }

    /// <summary>
/// Creates a copy of this color with the specified red component.
/// </summary>
/// <param name="red">
/// The red component in the range <c>0.0</c> through <c>1.0</c>.
/// </param>
/// <returns>
/// A new color with the specified red component.
/// </returns>
public NormalizedColor WithRed(float red)
{
    return new NormalizedColor(red, G, B, A);
}

/// <summary>
/// Creates a copy of this color with the specified green component.
/// </summary>
/// <param name="green">
/// The green component in the range <c>0.0</c> through <c>1.0</c>.
/// </param>
/// <returns>
/// A new color with the specified green component.
/// </returns>
public NormalizedColor WithGreen(float green)
{
    return new NormalizedColor(R, green, B, A);
}

/// <summary>
/// Creates a copy of this color with the specified blue component.
/// </summary>
/// <param name="blue">
/// The blue component in the range <c>0.0</c> through <c>1.0</c>.
/// </param>
/// <returns>
/// A new color with the specified blue component.
/// </returns>
public NormalizedColor WithBlue(float blue)
{
    return new NormalizedColor(R, G, blue, A);
}

/// <summary>
/// Creates a copy of this color with the specified alpha component.
/// </summary>
/// <param name="alpha">
/// The alpha component in the range <c>0.0</c> through <c>1.0</c>.
/// </param>
/// <returns>
/// A new color with the specified alpha component.
/// </returns>
public NormalizedColor WithAlpha(float alpha)
{
    return new NormalizedColor(R, G, B, alpha);
}

/// <summary>
/// Creates a copy of this color with the specified RGB components.
/// </summary>
/// <param name="red">The red component.</param>
/// <param name="green">The green component.</param>
/// <param name="blue">The blue component.</param>
/// <returns>
/// A new color with the specified RGB components and the current alpha component.
/// </returns>
public NormalizedColor With(
    float red,
    float green,
    float blue)
{
    return new NormalizedColor(red, green, blue, A);
}

/// <summary>
/// Creates a copy of this color with the specified RGB components.
/// </summary>
/// <param name="rgb">
/// The vector containing the red, green, and blue components.
/// </param>
/// <returns>
/// A new color with the specified RGB components and the current alpha component.
/// </returns>
public NormalizedColor With(Vector3 rgb)
{
    return new NormalizedColor(rgb.X, rgb.Y, rgb.Z, A);
}

    /// <summary>
    /// Converts a standard color to a normalized color.
    /// </summary>
    /// <param name="color">
    /// The color to convert.
    /// </param>
    public static implicit operator NormalizedColor(Color color)
    {
        return FromColor(color);
    }

    /// <summary>
    /// Converts a normalized color to the standard color representation.
    /// </summary>
    /// <param name="color">
    /// The color to convert.
    /// </param>
    public static explicit operator Color(NormalizedColor color)
    {
        return color.ToColor();
    }

    /// <summary>
    /// Converts a normalized color to an RGB vector.
    /// </summary>
    /// <param name="color">
    /// The color to convert.
    /// </param>
    public static implicit operator Vector3(NormalizedColor color)
    {
        return color.ToVector3();
    }

    /// <summary>
    /// Converts a normalized color to an RGBA vector.
    /// </summary>
    /// <param name="color">
    /// The color to convert.
    /// </param>
    public static implicit operator Vector4(NormalizedColor color)
    {
        return color.ToVector4();
    }

    /// <summary>
    /// Converts an RGBA vector to a normalized color.
    /// </summary>
    /// <param name="value">
    /// The vector to convert.
    /// </param>
    public static implicit operator NormalizedColor(Vector4 value)
    {
        return new NormalizedColor(value);
    }
}
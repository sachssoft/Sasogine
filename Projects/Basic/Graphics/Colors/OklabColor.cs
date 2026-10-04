using Microsoft.Xna.Framework;
using Sachssoft.Engine.Graphics;
using System;

namespace Sachssoft.Engine.Graphics.Colors;

/// <summary>
/// Represents a color in the OKLab color model.
/// </summary>
public readonly struct OklabColor : IColorConvertible<OklabColor>, IEquatable<OklabColor>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OklabColor"/> structure.
    /// </summary>
    /// <param name="lightness">The lightness component.</param>
    /// <param name="a">The green-red opponent component.</param>
    /// <param name="b">The blue-yellow opponent component.</param>
    /// <param name="alpha">The alpha component in the range <c>0.0</c> through <c>1.0</c>.</param>
    public OklabColor(float lightness, float a, float b, float alpha = 1f)
    {
        Lightness = lightness;
        A = a;
        B = b;
        Alpha = MathHelper.Clamp(alpha, 0f, 1f);
    }

    /// <summary>
    /// Gets the lightness component.
    /// </summary>
    public float Lightness { get; }

    /// <summary>
    /// Gets the green-red opponent component.
    /// </summary>
    public float A { get; }

    /// <summary>
    /// Gets the blue-yellow opponent component.
    /// </summary>
    public float B { get; }

    /// <summary>
    /// Gets the alpha component.
    /// </summary>
    public float Alpha { get; }

    /// <summary>
    /// Creates a <see cref="OklabColor"/> from a standard <see cref="Color"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The converted OKLab color.</returns>
    public static OklabColor FromColor(Color color)
    {
        var linear = LinearRgbColor.FromColor(color);
        var l = 0.4122214708f * linear.Red + 0.5363325363f * linear.Green + 0.0514459929f * linear.Blue;
        var m = 0.2119034982f * linear.Red + 0.6806995451f * linear.Green + 0.1073969566f * linear.Blue;
        var s = 0.0883024619f * linear.Red + 0.2817188376f * linear.Green + 0.6299787005f * linear.Blue;
        var lRoot = MathF.Cbrt(l);
        var mRoot = MathF.Cbrt(m);
        var sRoot = MathF.Cbrt(s);
        return new OklabColor(0.2104542553f * lRoot + 0.793617785f * mRoot - 0.0040720468f * sRoot, 1.9779984951f * lRoot - 2.428592205f * mRoot + 0.4505937099f * sRoot, 0.0259040371f * lRoot + 0.7827717662f * mRoot - 0.808675766f * sRoot, linear.Alpha);
    }

    /// <summary>
    /// Converts this color to the standard <see cref="Color"/> representation.
    /// </summary>
    /// <returns>The converted color.</returns>
    public Color ToColor()
    {
        var lRoot = Lightness + 0.3963377774f * A + 0.2158037573f * B;
        var mRoot = Lightness - 0.1055613458f * A - 0.0638541728f * B;
        var sRoot = Lightness - 0.0894841775f * A - 1.291485548f * B;
        var l = lRoot * lRoot * lRoot;
        var m = mRoot * mRoot * mRoot;
        var s = sRoot * sRoot * sRoot;
        var r = +4.0767416621f * l - 3.3077115913f * m + 0.2309699292f * s;
        var g = -1.2684380046f * l + 2.6097574011f * m - 0.3413193965f * s;
        var b = -0.0041960863f * l - 0.7034186147f * m + 1.707614701f * s;
        return new LinearRgbColor(MathHelper.Clamp(r, 0f, 1f), MathHelper.Clamp(g, 0f, 1f), MathHelper.Clamp(b, 0f, 1f), Alpha).ToColor();
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
    /// Determines whether this color equals another <see cref="OklabColor"/>.
    /// </summary>
    /// <param name="other">The color to compare with this instance.</param>
    /// <returns><see langword="true"/> if the colors are equal; otherwise, <see langword="false"/>.</returns>
    public bool Equals(OklabColor other) =>
        Lightness.Equals(other.Lightness) &&
        A.Equals(other.A) &&
        B.Equals(other.B) &&
        Alpha.Equals(other.Alpha);

    /// <summary>
    /// Determines whether this color equals the specified object.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns><see langword="true"/> if <paramref name="obj"/> is an equal <see cref="OklabColor"/>; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is OklabColor other && Equals(other);

    /// <summary>
    /// Returns a hash code for this color.
    /// </summary>
    /// <returns>A hash code for this instance.</returns>
    public override int GetHashCode() => HashCode.Combine(Lightness, A, B, Alpha);

    /// <summary>
    /// Determines whether two <see cref="OklabColor"/> values are equal.
    /// </summary>
    /// <param name="left">The first color.</param>
    /// <param name="right">The second color.</param>
    /// <returns><see langword="true"/> if both colors are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(OklabColor left, OklabColor right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="OklabColor"/> values are not equal.
    /// </summary>
    /// <param name="left">The first color.</param>
    /// <param name="right">The second color.</param>
    /// <returns><see langword="true"/> if both colors differ; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(OklabColor left, OklabColor right) => !left.Equals(right);
}

using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Graphics;

/// <summary>
/// Defines common conversion operations for color representations.
/// </summary>
/// <typeparam name="TSelf">
/// The implementing color type.
/// </typeparam>
/// <remarks>
/// Vector representations use normalized color components in the range
/// <c>0.0</c> through <c>1.0</c>.
/// </remarks>
public interface IColorConvertible<TSelf>
    where TSelf : IColorConvertible<TSelf>
{
    /// <summary>
    /// Creates a color value from the standard <see cref="Color"/> representation.
    /// </summary>
    /// <param name="color">
    /// The color to convert.
    /// </param>
    /// <returns>
    /// The converted color value.
    /// </returns>
    static abstract TSelf FromColor(Color color);

    /// <summary>
    /// Converts the color to the standard <see cref="Color"/> representation.
    /// </summary>
    /// <returns>
    /// The converted color.
    /// </returns>
    Color ToColor();

    /// <summary>
    /// Converts the color to a normalized RGB vector.
    /// </summary>
    /// <returns>
    /// A vector containing the red, green, and blue components in the range
    /// <c>0.0</c> through <c>1.0</c>.
    /// </returns>
    Vector3 ToVector3();

    /// <summary>
    /// Converts the color to a normalized RGBA vector.
    /// </summary>
    /// <returns>
    /// A vector containing the red, green, blue, and alpha components in the range
    /// <c>0.0</c> through <c>1.0</c>.
    /// </returns>
    Vector4 ToVector4();
}
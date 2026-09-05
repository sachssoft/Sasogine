using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Graphics.Rendering;

/// <summary>
/// Defines a 2D transform definition that can be skewed.
/// </summary>
public interface ITransformSkewable2Definition
{
    /// <summary>
    /// Gets or sets the 2D skew.
    /// </summary>
    Vector2 Skew { get; set; }
}
using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Graphics.Rendering;

/// <summary>
/// Defines a 2D transform definition that can be scaled.
/// </summary>
public interface ITransformScalable2Definition
{
    /// <summary>
    /// Gets or sets the 2D scale.
    /// </summary>
    Vector2 Scale { get; set; }
}
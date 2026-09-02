using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Experimental.Graphics.Rendering;

/// <summary>
/// Defines a 2D transform definition that can be moved.
/// </summary>
public interface ITransformMovable2Definition
{
    /// <summary>
    /// Gets or sets the 2D position.
    /// </summary>
    Vector2 Position { get; set; }
}
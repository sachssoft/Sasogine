using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Graphics.Rendering;

/// <summary>
/// Defines a 2D transform definition that can be moved.
/// </summary>
public interface ITransformMovable2Definition
{
    /// <summary>
    /// Gets or sets the 2D position.
    /// </summary>
    Point2 Position { get; set; }
}
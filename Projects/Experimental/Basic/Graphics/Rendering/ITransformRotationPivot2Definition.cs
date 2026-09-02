using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Experimental.Graphics.Rendering;

/// <summary>
/// Defines a 2D transform definition that provides a rotation pivot.
/// </summary>
public interface ITransformRotationPivot2Definition
{
    /// <summary>
    /// Gets or sets the local pivot point used for rotation.
    /// </summary>
    Point2 RotationPivot { get; set; }
}
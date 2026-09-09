using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Graphics.Rendering;

/// <summary>
/// Defines a read-only 2D transform that provides a rotation pivot.
/// </summary>
public interface IReadOnlyRotationPivot2
{
    /// <summary>
    /// Gets the local pivot point used for rotation.
    /// </summary>
    Point2 RotationPivot { get; }

    /// <summary>
    /// Gets a value indicating whether modifying the rotation pivot is allowed.
    /// </summary>
    bool AllowRotationPivot { get; }
}
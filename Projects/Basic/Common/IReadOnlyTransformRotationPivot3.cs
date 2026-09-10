namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Defines read-only access to a three-dimensional rotation pivot.
/// </summary>
public interface IReadOnlyTransformRotationPivot3 : ITransform3
{
    /// <summary>
    /// Gets the position around which rotation is applied.
    /// </summary>
    Point3 RotationPivot { get; }
}

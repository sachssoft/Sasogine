namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Defines read-only access to a two-dimensional rotation pivot.
/// </summary>
public interface IReadOnlyTransformRotationPivot2 : ITransform2
{
    /// <summary>
    /// Gets the position around which rotation is applied.
    /// </summary>
    Point2 RotationPivot { get; }
}

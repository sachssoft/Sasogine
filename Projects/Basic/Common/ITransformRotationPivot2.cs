namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Defines read-write access to a two-dimensional rotation pivot.
/// </summary>
public interface ITransformRotationPivot2 : IReadOnlyTransformRotationPivot2
{
    /// <summary>
    /// Gets or sets the position around which rotation is applied.
    /// </summary>
    new Point2 RotationPivot { get; set; }
}

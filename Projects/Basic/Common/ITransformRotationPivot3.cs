namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Defines read-write access to a three-dimensional rotation pivot.
/// </summary>
public interface ITransformRotationPivot3 : IReadOnlyTransformRotationPivot3
{
    /// <summary>
    /// Gets or sets the position around which rotation is applied.
    /// </summary>
    new Point3 RotationPivot { get; set; }
}

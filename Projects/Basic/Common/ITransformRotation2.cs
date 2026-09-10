namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Defines read-write access to a two-dimensional transform rotation.
/// </summary>
public interface ITransformRotation2 : IReadOnlyTransformRotation2
{
    /// <summary>
    /// Gets or sets the rotation angle, in radians.
    /// </summary>
    new float Rotation { get; set; }
}

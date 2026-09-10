namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Defines read-only access to a two-dimensional transform rotation.
/// </summary>
public interface IReadOnlyTransformRotation2 : ITransform2
{
    /// <summary>
    /// Gets the rotation angle, in radians.
    /// </summary>
    float Rotation { get; }
}

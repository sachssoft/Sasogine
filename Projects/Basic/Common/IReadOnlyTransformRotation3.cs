using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Defines read-only access to a three-dimensional transform rotation.
/// </summary>
public interface IReadOnlyTransformRotation3 : ITransform3
{
    /// <summary>
    /// Gets the three-dimensional rotation represented as a quaternion.
    /// </summary>
    Quaternion Rotation { get; }
}

using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Defines read-write access to a three-dimensional transform rotation.
/// </summary>
public interface ITransformRotation3 : IReadOnlyTransformRotation3
{
    /// <summary>
    /// Gets or sets the three-dimensional rotation represented as a quaternion.
    /// </summary>
    new Quaternion Rotation { get; set; }
}

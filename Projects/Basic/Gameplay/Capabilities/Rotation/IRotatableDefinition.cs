using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Gameplay.Capabilities
{
    /// <summary>
    /// Defines the rotation state of a rotatable object.
    /// </summary>
    public interface IRotatableDefinition : IDefinition
    {
        /// <summary>
        /// Gets or sets the rotation of the object.
        /// </summary>
        float Rotation { get; set; }

        /// <summary>
        /// Gets or sets the pivot point used for rotation.
        /// </summary>
        Vector2 RotationPivot { get; set; }
    }
}
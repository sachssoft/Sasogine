using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Gameplay.Capabilities
{
    /// <summary>
    /// Represents an object whose rotation state can be changed.
    /// </summary>
    public interface IRotatable : IReadOnlyRotatable
    {
        /// <summary>
        /// Gets or sets the rotation of the object.
        /// </summary>
        new float Rotation { get; set; }

        /// <summary>
        /// Gets or sets the pivot point used for rotation.
        /// </summary>
        new Vector2 RotationPivot { get; set; }
    }
}
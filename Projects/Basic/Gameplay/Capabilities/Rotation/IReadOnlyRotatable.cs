using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Gameplay.Capabilities
{
    /// <summary>
    /// Represents an object that can provide its rotatable state.
    /// </summary>
    public interface IReadOnlyRotatable
    {
        /// <summary>
        /// Gets the current rotation of the object.
        /// </summary>
        float Rotation { get; }

        /// <summary>
        /// Gets the pivot point used for rotation.
        /// </summary>
        Vector2 RotationPivot { get; }

        /// <summary>
        /// Gets a value indicating whether the object can be rotated.
        /// </summary>
        bool AllowRotate { get; }
    }
}
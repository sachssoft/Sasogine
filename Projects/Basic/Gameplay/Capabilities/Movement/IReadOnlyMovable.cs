using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Gameplay.Capabilities
{
    /// <summary>
    /// Represents an object that can provide its movable state.
    /// </summary>
    public interface IReadOnlyMovable
    {
        /// <summary>
        /// Gets the current position of the object.
        /// </summary>
        Vector2 Position { get; }

        /// <summary>
        /// Gets a value indicating whether the object can be moved.
        /// </summary>
        bool AllowMove { get; }
    }
}
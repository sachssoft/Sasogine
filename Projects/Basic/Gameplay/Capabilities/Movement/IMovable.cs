using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Gameplay.Capabilities
{
    /// <summary>
    /// Represents an object whose position can be changed.
    /// </summary>
    public interface IMovable : IReadOnlyMovable
    {
        /// <summary>
        /// Gets or sets the position of the object.
        /// </summary>
        new Vector2 Position { get; set; }
    }
}
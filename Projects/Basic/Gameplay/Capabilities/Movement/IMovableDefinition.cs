using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Gameplay.Capabilities
{
    /// <summary>
    /// Defines the position of a movable object.
    /// </summary>
    public interface IMovableDefinition : IDefinition
    {
        /// <summary>
        /// Gets or sets the position of the object.
        /// </summary>
        Vector2 Position { get; set; }
    }
}
using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.World
{
    /// <summary>
    /// Defines an entity that participates in the scene update cycle.
    /// </summary>
    /// <remarks>
    /// Entities implementing this interface can perform runtime logic during
    /// each update cycle.
    /// </remarks>
    public interface IUpdatableEntity
    {
        /// <summary>
        /// Updates the entity using the specified scene update context.
        /// </summary>
        /// <param name="context">
        /// The context containing information for the current update cycle.
        /// </param>
        void Update(SceneUpdateContext context);
    }
}
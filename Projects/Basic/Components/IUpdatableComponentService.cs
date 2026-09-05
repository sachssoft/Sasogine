using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Components
{
    /// <summary>
    /// Defines a component service that participates in the update cycle.
    /// </summary>
    public interface IUpdatableComponentService : IComponentService
    {
        /// <summary>
        /// Updates the service using the specified scene update context.
        /// </summary>
        /// <param name="context">
        /// Provides information about the current scene update.
        /// </param>
        void Update(SceneUpdateContext context);
    }
}
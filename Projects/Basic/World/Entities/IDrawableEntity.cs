using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.World
{
    /// <summary>
    /// Defines an entity that participates in the scene drawing cycle.
    /// </summary>
    /// <remarks>
    /// Entities implementing this interface can perform rendering operations
    /// during each drawing cycle.
    /// </remarks>
    public interface IDrawableEntity
    {
        /// <summary>
        /// Draws the entity using the specified scene draw context.
        /// </summary>
        /// <param name="context">
        /// The context containing information for the current drawing cycle.
        /// </param>
        void Draw(SceneDrawContext context);
    }
}
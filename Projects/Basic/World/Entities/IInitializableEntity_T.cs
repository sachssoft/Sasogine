namespace Sachssoft.Sasogine.World
{
    /// <summary>
    /// Represents an entity that supports initialization and deinitialization
    /// using a specific entity context type.
    /// </summary>
    /// <typeparam name="TEntityContext">
    /// The type of context used to initialize the entity.
    /// </typeparam>
    public interface IInitializableEntity<TEntityContext>
        where TEntityContext : IEntityContext
    {
        /// <summary>
        /// Initializes the entity using the specified context.
        /// </summary>
        /// <param name="context">
        /// The context used to initialize the entity.
        /// </param>
        void Initialize(TEntityContext context);

        /// <summary>
        /// Deinitializes the entity from its current context.
        /// </summary>
        void Deinitialize();
    }
}
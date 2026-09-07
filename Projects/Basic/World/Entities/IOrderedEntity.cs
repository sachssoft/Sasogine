namespace Sachssoft.Sasogine.World
{
    /// <summary>
    /// Defines an entity that participates in ordered processing.
    /// </summary>
    /// <remarks>
    /// The order value can be used to determine the relative processing,
    /// updating, or drawing order of entities within an entity collection.
    /// </remarks>
    public interface IOrderedEntity
    {
        /// <summary>
        /// Gets the order of the entity.
        /// </summary>
        /// <value>
        /// The value used to determine the relative order of the entity.
        /// Lower values are processed before higher values.
        /// </value>
        int Order { get; }
    }
}
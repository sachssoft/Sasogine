namespace Sachssoft.Engine.World;

/// <summary>
/// Defines the mutable ordering information of an entity definition.
/// </summary>
/// <remarks>
/// The configured order determines the relative processing, updating,
/// or drawing order of the associated entity within an entity collection.
/// </remarks>
public interface IOrderedEntityDefinition : IEntityDefinition
{
    /// <summary>
    /// Gets or sets the order of the entity.
    /// </summary>
    /// <value>
    /// The value used to determine the relative order of the entity.
    /// Lower values are processed before higher values.
    /// </value>
    int Order { get; set; }
}
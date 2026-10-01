using Sachssoft.Engine.Components.Definitions;

namespace Sachssoft.Engine.World;

/// <summary>
/// Provides a base implementation for entity definitions.
/// </summary>
/// <remarks>
/// The base definition provides common engine object metadata shared by entity
/// definitions, including an identifier and an optional class.
/// </remarks>
public abstract class EntityDefinitionBase : IEntityDefinition
{
    /// <summary>
    /// Gets or sets the identifier of the entity.
    /// </summary>
    /// <value>
    /// The entity identifier, or <see langword="null"/> if no identifier has
    /// been assigned.
    /// </value>
    [DefinitionMember(
        Title = "Id",
        Category = Categories.Common,
        Description = "Specifies the identifier of the entity.")]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the class associated with the entity.
    /// </summary>
    /// <value>
    /// The entity class, or <see langword="null"/> if no class has been assigned.
    /// </value>
    [DefinitionMember(
        Title = "Class",
        Category = Categories.Common,
        Description = "Specifies the class associated with the entity.")]
    public string? Class { get; set; }
}
using Sachssoft.Sasogine.Components.Models;
using System.ComponentModel;

namespace Sachssoft.Sasogine.World;

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
    [Category(Categories.Common)]
    [DisplayName("Id")]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the class associated with the entity.
    /// </summary>
    /// <value>
    /// The entity class, or <see langword="null"/> if no class has been assigned.
    /// </value>
    [Category(Categories.Common)]
    [DisplayName("Class")]
    public string? Class { get; set; }
}
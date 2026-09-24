using Sachssoft.Engine.Common;
using System.Collections.Generic;

namespace Sachssoft.Engine.Components.Models;

/// <summary>
/// Defines a definition that provides static metadata describing its properties.
/// </summary>
/// <remarks>
/// Implementations expose property metadata without requiring runtime reflection,
/// making them suitable for trimming and AOT scenarios.
/// </remarks>
public interface IDefinitionMetadata : IDefinition
{
    /// <summary>
    /// Gets the metadata for the property with the specified name.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <returns>The metadata associated with the specified property.</returns>
    /// <exception cref="KeyNotFoundException">
    /// No property with the specified <paramref name="name"/> exists.
    /// </exception>
    abstract static IDefinitionPropertyMetadata GetProperty(string name);

    /// <summary>
    /// Gets the metadata for all properties of the definition.
    /// </summary>
    /// <returns>
    /// A read-only list containing the metadata for all properties of the definition.
    /// </returns>
    abstract static IReadOnlyList<IDefinitionPropertyMetadata> GetProperties();
}
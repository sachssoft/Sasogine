using Sachssoft.Sasogine.Common;
using System;

namespace Sachssoft.Sasogine.Experimental;

/// <summary>
/// Represents an entry in a game registry.
/// </summary>
public interface IGameRegistryEntry
{
    /// <summary>
    /// Gets the key associated with the registry entry.
    /// </summary>
    IGameRegistryKey Key { get; }

    /// <summary>
    /// Gets the type of the registry key.
    /// </summary>
    Type KeyType { get; }

    /// <summary>
    /// Gets the definition type associated with the registry entry.
    /// </summary>
    Type DefinitionType { get; }

    /// <summary>
    /// Gets the engine object type associated with the registry entry.
    /// </summary>
    Type ObjectType { get; }

    /// <summary>
    /// Creates a new definition.
    /// </summary>
    /// <returns>
    /// The created definition.
    /// </returns>
    IDefinition CreateDefinition();

    /// <summary>
    /// Creates an engine object from the specified definition.
    /// </summary>
    /// <param name="definition">
    /// The definition used to create the engine object.
    /// </param>
    /// <returns>
    /// The created engine object.
    /// </returns>
    IEngineObject CreateObject(IDefinition definition);
}
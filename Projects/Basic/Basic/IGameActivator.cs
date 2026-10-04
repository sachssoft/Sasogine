using Sachssoft.Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Engine;

/// <summary>
/// Defines an activator for creating engine object instances from registered
/// definitions.
/// </summary>
public interface IGameActivator
{
    /// <summary>
    /// Gets the registry keys supported by this activator.
    /// </summary>
    /// <returns>
    /// A snapshot containing the supported registry keys.
    /// </returns>
    IReadOnlyList<IGameRegistryKey> GetKeys();

    /// <summary>
    /// Gets the registry key associated with the specified definition.
    /// </summary>
    /// <param name="definition">
    /// The definition used to locate the corresponding registry key.
    /// </param>
    /// <returns>The registry key associated with the definition.</returns>
    IGameRegistryKey GetKey(IDefinition definition);

    /// <summary>
    /// Attempts to get the registry key associated with the specified definition.
    /// </summary>
    /// <param name="definition">
    /// The definition used to locate the corresponding registry key.
    /// </param>
    /// <param name="key">
    /// When this method returns <see langword="true"/>, contains the associated
    /// registry key; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a registry key was found;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool TryGetKey(
        IDefinition definition,
        [NotNullWhen(true)] out IGameRegistryKey? key);

    /// <summary>
    /// Determines whether the specified registry key is supported.
    /// </summary>
    /// <param name="key">The registry key to check.</param>
    /// <returns>
    /// <see langword="true"/> if the registry key is supported;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsSupported(IGameRegistryKey key);

    /// <summary>
    /// Determines whether the specified definition type is supported.
    /// </summary>
    /// <param name="definitionType">The definition type to check.</param>
    /// <returns>
    /// <see langword="true"/> if the definition type is supported;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsDefinitionSupported(Type definitionType);

    /// <summary>
    /// Determines whether the specified engine object type is supported.
    /// </summary>
    /// <param name="objectType">The engine object type to check.</param>
    /// <returns>
    /// <see langword="true"/> if the object type is supported;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsObjectSupported(Type objectType);

    /// <summary>
    /// Creates an engine object associated with the specified registry key
    /// using the provided definition.
    /// </summary>
    /// <param name="key">The registry key used to locate the corresponding object factory.</param>
    /// <param name="definition">The definition used to create the engine object.</param>
    /// <returns>The created engine object.</returns>
    IEngineObject Create(IGameRegistryKey key, IDefinition definition);

    /// <summary>
    /// Creates an engine object compatible with the specified object type
    /// using the provided definition.
    /// </summary>
    /// <param name="objectType">The requested engine object type.</param>
    /// <param name="definition">The definition used to create the engine object.</param>
    /// <returns>The created engine object.</returns>
    IEngineObject Create(Type objectType, IDefinition definition);

    /// <summary>
    /// Creates an engine object using the registry entry associated with the
    /// specified definition.
    /// </summary>
    /// <param name="definition">The definition used to locate the corresponding object factory.</param>
    /// <returns>The created engine object.</returns>
    IEngineObject CreateFromDefinition(IDefinition definition);

    /// <summary>
    /// Creates a definition associated with the specified registry key.
    /// </summary>
    /// <param name="key">The registry key used to locate the corresponding definition factory.</param>
    /// <returns>The created definition.</returns>
    IDefinition CreateDefinition(IGameRegistryKey key);

    /// <summary>
    /// Attempts to create a definition associated with the specified registry key.
    /// </summary>
    /// <param name="key">The registry key used to locate the corresponding definition factory.</param>
    /// <param name="definition">
    /// When this method returns <see langword="true"/>, contains the created
    /// definition; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the definition could be created;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool TryCreateDefinition(
        IGameRegistryKey key,
        [NotNullWhen(true)] out IDefinition? definition);

    /// <summary>
    /// Attempts to create an engine object associated with the specified
    /// registry key using the provided definition.
    /// </summary>
    /// <param name="key">The registry key used to locate the corresponding object factory.</param>
    /// <param name="definition">The definition used to create the engine object.</param>
    /// <param name="instance">
    /// When this method returns <see langword="true"/>, contains the created
    /// engine object; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the engine object could be created;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool TryCreate(
        IGameRegistryKey key,
        IDefinition definition,
        [NotNullWhen(true)] out IEngineObject? instance);

    /// <summary>
    /// Attempts to create an engine object compatible with the specified
    /// object type using the provided definition.
    /// </summary>
    /// <param name="objectType">The requested engine object type.</param>
    /// <param name="definition">The definition used to create the engine object.</param>
    /// <param name="instance">
    /// When this method returns <see langword="true"/>, contains the created
    /// engine object; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the engine object could be created;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool TryCreate(
        Type objectType,
        IDefinition definition,
        [NotNullWhen(true)] out IEngineObject? instance);

    /// <summary>
    /// Attempts to create an engine object using the registry entry associated
    /// with the specified definition.
    /// </summary>
    /// <param name="definition">The definition used to locate the corresponding object factory.</param>
    /// <param name="instance">
    /// When this method returns <see langword="true"/>, contains the created
    /// engine object; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the engine object could be created;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool TryCreateFromDefinition(
        IDefinition definition,
        [NotNullWhen(true)] out IEngineObject? instance);
}
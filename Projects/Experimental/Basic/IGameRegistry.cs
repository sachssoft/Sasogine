using Sachssoft.Sasogine.Common;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Experimental;

/// <summary>
/// Defines a registry for registering and creating definitions and engine
/// object instances.
/// </summary>
public interface IGameRegistry
{
    /// <summary>
    /// Registers a definition factory and its corresponding engine object factory
    /// using the specified registry key.
    /// </summary>
    /// <param name="key">The key used to identify the registry entry.</param>
    /// <param name="definitionFactory">The factory used to create definitions.</param>
    /// <param name="objectFactory">The factory used to create engine objects from definitions.</param>
    void Register(
        IGameRegistryKey key,
        Func<IDefinition> definitionFactory,
        Func<IDefinition, IEngineObject> objectFactory);

    /// <summary>
    /// Determines whether the specified definition type is registered.
    /// </summary>
    /// <param name="definitionType">The definition type to check.</param>
    /// <returns>
    /// <see langword="true"/> if the definition type is registered;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsDefinitionRegistered(Type definitionType);

    /// <summary>
    /// Determines whether the specified engine object type is registered.
    /// </summary>
    /// <param name="objectType">The engine object type to check.</param>
    /// <returns>
    /// <see langword="true"/> if the object type is registered;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsObjectRegistered(Type objectType);

    /// <summary>
    /// Creates a definition for the specified definition type.
    /// </summary>
    /// <param name="definitionType">The definition type used to locate the registry entry.</param>
    /// <returns>The created definition.</returns>
    IDefinition CreateDefinition(Type definitionType);

    /// <summary>
    /// Creates a definition associated with the specified registry key.
    /// </summary>
    /// <param name="key">The registry key used to locate the registry entry.</param>
    /// <returns>The created definition.</returns>
    IDefinition CreateDefinition(IGameRegistryKey key);

    /// <summary>
    /// Attempts to create a definition for the specified definition type.
    /// </summary>
    /// <param name="definitionType">The definition type used to locate the registry entry.</param>
    /// <param name="definition">
    /// When this method returns <see langword="true"/>, contains the created
    /// definition; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the definition could be created;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool TryCreateDefinition(
        Type definitionType,
        [NotNullWhen(true)] out IDefinition? definition);

    /// <summary>
    /// Attempts to create a definition associated with the specified registry key.
    /// </summary>
    /// <param name="key">The registry key used to locate the registry entry.</param>
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
    /// Creates an engine object compatible with the specified object type
    /// using the provided definition.
    /// </summary>
    /// <param name="objectType">The requested engine object type.</param>
    /// <param name="definition">The definition used to create the engine object.</param>
    /// <returns>The created engine object.</returns>
    IEngineObject Create(Type objectType, IDefinition definition);

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
    /// Creates an engine object using the registry entry associated with the
    /// specified definition.
    /// </summary>
    /// <param name="definition">
    /// The definition used to locate the registry entry and create the engine object.
    /// </param>
    /// <returns>The created engine object.</returns>
    IEngineObject CreateFromDefinition(IDefinition definition);

    /// <summary>
    /// Attempts to create an engine object using the registry entry associated
    /// with the specified definition.
    /// </summary>
    /// <param name="definition">
    /// The definition used to locate the registry entry and create the engine object.
    /// </param>
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
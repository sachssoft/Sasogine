using Sachssoft.Sasogine.Common;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine;

/// <summary>
/// Defines an activator for creating engine object instances from registered
/// definitions.
/// </summary>
public interface IGameActivator
{
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
using Sachssoft.Sasogine.Common;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Experimental;

/// <summary>
/// Provides functionality for resolving registered engine object factories
/// and creating engine objects from definitions.
/// </summary>
public interface IGameRegistry
{
    /// <summary>
    /// Creates an engine object using the specified identifier.
    /// </summary>
    IEngineObject Create<TKey>(
        TKey key,
        IEngineObjectDefinition definition)
        where TKey : notnull;

    /// <summary>
    /// Creates a strongly typed engine object using the specified identifier.
    /// </summary>
    TObject Create<TKey, TObject>(
        TKey key,
        IEngineObjectDefinition definition)
        where TKey : notnull
        where TObject : class, IEngineObject;

    /// <summary>
    /// Creates an engine object using its registered engine object type.
    /// </summary>
    IEngineObject Create(
        Type type,
        IEngineObjectDefinition definition);

    /// <summary>
    /// Creates a strongly typed engine object using its registered type.
    /// </summary>
    TObject Create<TObject>(
        IEngineObjectDefinition definition)
        where TObject : class, IEngineObject;

    /// <summary>
    /// Attempts to create an engine object using the specified identifier.
    /// </summary>
    bool TryCreate<TKey>(
        TKey key,
        IEngineObjectDefinition definition,
        [NotNullWhen(true)] out IEngineObject? instance)
        where TKey : notnull;

    /// <summary>
    /// Attempts to create a strongly typed engine object using the
    /// specified identifier.
    /// </summary>
    bool TryCreate<TKey, TObject>(
        TKey key,
        IEngineObjectDefinition definition,
        [NotNullWhen(true)] out TObject? instance)
        where TKey : notnull
        where TObject : class, IEngineObject;

    /// <summary>
    /// Determines whether a factory is registered for the specified identifier.
    /// </summary>
    bool IsRegistered<TKey>(
        TKey key)
        where TKey : notnull;

    /// <summary>
    /// Determines whether a factory is registered for the specified
    /// engine object type.
    /// </summary>
    bool IsRegistered(
        Type type);

    /// <summary>
    /// Determines whether a factory is registered for the specified
    /// engine object type.
    /// </summary>
    bool IsRegistered<TObject>()
        where TObject : class, IEngineObject;
}
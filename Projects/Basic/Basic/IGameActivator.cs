using Sachssoft.Sasogine.Common;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Experimental;

/// <summary>
/// Provides functionality for creating engine objects from registered
/// factories and engine object definitions.
/// </summary>
public interface IGameActivator
{
    /// <summary>
    /// Creates an engine object using the specified key and definition.
    /// </summary>
    /// <typeparam name="TKey">
    /// The type of key used to identify the registered factory.
    /// </typeparam>
    /// <param name="key">
    /// The key identifying the factory.
    /// </param>
    /// <param name="definition">
    /// The definition used to create the engine object.
    /// </param>
    /// <returns>
    /// The created engine object.
    /// </returns>
    IEngineObject Create<TKey>(
        TKey key,
        IEngineObjectDefinition definition)
        where TKey : notnull;

    /// <summary>
    /// Creates a strongly typed engine object using the specified key
    /// and definition.
    /// </summary>
    /// <typeparam name="TKey">
    /// The type of key used to identify the registered factory.
    /// </typeparam>
    /// <typeparam name="TObject">
    /// The expected engine object type.
    /// </typeparam>
    /// <param name="key">
    /// The key identifying the factory.
    /// </param>
    /// <param name="definition">
    /// The definition used to create the engine object.
    /// </param>
    /// <returns>
    /// The created engine object.
    /// </returns>
    TObject Create<TKey, TObject>(
        TKey key,
        IEngineObjectDefinition definition)
        where TKey : notnull
        where TObject : class, IEngineObject;

    /// <summary>
    /// Creates an engine object using the specified engine object type
    /// and definition.
    /// </summary>
    /// <param name="type">
    /// The engine object type identifying the registered factory.
    /// </param>
    /// <param name="definition">
    /// The definition used to create the engine object.
    /// </param>
    /// <returns>
    /// The created engine object.
    /// </returns>
    IEngineObject Create(
        Type type,
        IEngineObjectDefinition definition);

    /// <summary>
    /// Creates a strongly typed engine object using its type and
    /// the specified definition.
    /// </summary>
    /// <typeparam name="TObject">
    /// The engine object type.
    /// </typeparam>
    /// <param name="definition">
    /// The definition used to create the engine object.
    /// </param>
    /// <returns>
    /// The created engine object.
    /// </returns>
    TObject Create<TObject>(
        IEngineObjectDefinition definition)
        where TObject : class, IEngineObject;

    /// <summary>
    /// Attempts to create an engine object using the specified key
    /// and definition.
    /// </summary>
    /// <typeparam name="TKey">
    /// The type of key used to identify the registered factory.
    /// </typeparam>
    /// <param name="key">
    /// The key identifying the factory.
    /// </param>
    /// <param name="definition">
    /// The definition used to create the engine object.
    /// </param>
    /// <param name="instance">
    /// When this method returns <see langword="true"/>, contains the
    /// created engine object; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the engine object could be created;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool TryCreate<TKey>(
        TKey key,
        IEngineObjectDefinition definition,
        [NotNullWhen(true)] out IEngineObject? instance)
        where TKey : notnull;

    /// <summary>
    /// Attempts to create a strongly typed engine object using the
    /// specified key and definition.
    /// </summary>
    /// <typeparam name="TKey">
    /// The type of key used to identify the registered factory.
    /// </typeparam>
    /// <typeparam name="TObject">
    /// The expected engine object type.
    /// </typeparam>
    /// <param name="key">
    /// The key identifying the factory.
    /// </param>
    /// <param name="definition">
    /// The definition used to create the engine object.
    /// </param>
    /// <param name="instance">
    /// When this method returns <see langword="true"/>, contains the
    /// created engine object; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a compatible engine object could be
    /// created; otherwise, <see langword="false"/>.
    /// </returns>
    bool TryCreate<TKey, TObject>(
        TKey key,
        IEngineObjectDefinition definition,
        [NotNullWhen(true)] out TObject? instance)
        where TKey : notnull
        where TObject : class, IEngineObject;

    /// <summary>
    /// Determines whether the specified key is supported.
    /// </summary>
    /// <typeparam name="TKey">
    /// The type of key.
    /// </typeparam>
    /// <param name="key">
    /// The key to test.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the key is supported; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    bool IsSupported<TKey>(
        TKey key)
        where TKey : notnull;

    /// <summary>
    /// Determines whether the specified engine object type is supported.
    /// </summary>
    /// <typeparam name="TObject">
    /// The engine object type to test.
    /// </typeparam>
    /// <returns>
    /// <see langword="true"/> if the engine object type is supported;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsSupported<TObject>()
        where TObject : class, IEngineObject;
}
using Sachssoft.Sasogine.Common;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Experimental;

/// <summary>
/// Provides a base implementation for a game activator
/// with a preconfigured game registry.
/// </summary>
public abstract class PreconfiguredGameActivator : IGameActivator
{
    private readonly GameRegistry _gameRegistry;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="PreconfiguredGameActivator"/> class.
    /// </summary>
    protected PreconfiguredGameActivator()
    {
        _gameRegistry = CreateRegistry() ??
            throw new InvalidOperationException(
                "Registry creation returned null.");
    }

    /// <summary>
    /// Creates the game registry used by this activator.
    /// </summary>
    /// <returns>
    /// The preconfigured game registry.
    /// </returns>
    protected abstract GameRegistry CreateRegistry();

    /// <inheritdoc/>
    public IEngineObject Create<TKey>(
        TKey key,
        IEngineObjectDefinition definition)
        where TKey : notnull
        => _gameRegistry.Create(key, definition);

    /// <inheritdoc/>
    public TObject Create<TKey, TObject>(
        TKey key,
        IEngineObjectDefinition definition)
        where TKey : notnull
        where TObject : class, IEngineObject
        => _gameRegistry.Create<TKey, TObject>(key, definition);

    /// <inheritdoc/>
    public IEngineObject Create(
        Type type,
        IEngineObjectDefinition definition)
        => _gameRegistry.Create(type, definition);

    /// <inheritdoc/>
    public TObject Create<TObject>(
        IEngineObjectDefinition definition)
        where TObject : class, IEngineObject
        => _gameRegistry.Create<TObject>(definition);

    /// <inheritdoc/>
    public bool TryCreate<TKey>(
        TKey key,
        IEngineObjectDefinition definition,
        [NotNullWhen(true)] out IEngineObject? instance)
        where TKey : notnull
        => _gameRegistry.TryCreate(key, definition, out instance);

    /// <inheritdoc/>
    public bool TryCreate<TKey, TObject>(
        TKey key,
        IEngineObjectDefinition definition,
        [NotNullWhen(true)] out TObject? instance)
        where TKey : notnull
        where TObject : class, IEngineObject
        => _gameRegistry.TryCreate<TKey, TObject>(
            key,
            definition,
            out instance);

    /// <inheritdoc/>
    public bool IsSupported<TKey>(TKey key)
        where TKey : notnull
        => _gameRegistry.IsRegistered(key);

    /// <inheritdoc/>
    public bool IsSupported<TObject>()
        where TObject : class, IEngineObject
        => _gameRegistry.IsRegistered<TObject>();
}
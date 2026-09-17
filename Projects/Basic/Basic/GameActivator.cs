using Sachssoft.Sasogine.Common;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Experimental;

/// <summary>
/// Provides a restricted activation interface for a
/// <see cref="GameRegistry"/>.
/// </summary>
/// <remarks>
/// This class wraps a game registry and exposes only object creation
/// and support-query functionality. Registry modification operations
/// are not exposed through this type.
/// </remarks>
public sealed class GameActivator : IGameActivator
{
    private readonly GameRegistry _registry;

    /// <summary>
    /// Initializes a new game activator using the specified registry.
    /// </summary>
    /// <param name="registry">
    /// The game registry used to resolve and create engine objects.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="registry"/> is <see langword="null"/>.
    /// </exception>
    public GameActivator(GameRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        _registry = registry;
    }

    /// <inheritdoc/>
    public IEngineObject Create<TKey>(
        TKey key,
        IEngineObjectDefinition definition)
        where TKey : notnull
    {
        return _registry.Create(
            key,
            definition);
    }

    /// <inheritdoc/>
    public TObject Create<TKey, TObject>(
        TKey key,
        IEngineObjectDefinition definition)
        where TKey : notnull
        where TObject : class, IEngineObject
    {
        return _registry.Create<TKey, TObject>(
            key,
            definition);
    }

    /// <inheritdoc/>
    public IEngineObject Create(
        Type type,
        IEngineObjectDefinition definition)
    {
        return _registry.Create(
            type,
            definition);
    }

    /// <inheritdoc/>
    public TObject Create<TObject>(
        IEngineObjectDefinition definition)
        where TObject : class, IEngineObject
    {
        return _registry.Create<TObject>(
            definition);
    }

    /// <inheritdoc/>
    public bool TryCreate<TKey>(
        TKey key,
        IEngineObjectDefinition definition,
        [NotNullWhen(true)] out IEngineObject? instance)
        where TKey : notnull
    {
        return _registry.TryCreate(
            key,
            definition,
            out instance);
    }

    /// <inheritdoc/>
    public bool TryCreate<TKey, TObject>(
        TKey key,
        IEngineObjectDefinition definition,
        [NotNullWhen(true)] out TObject? instance)
        where TKey : notnull
        where TObject : class, IEngineObject
    {
        return _registry.TryCreate<TKey, TObject>(
            key,
            definition,
            out instance);
    }

    /// <inheritdoc/>
    public bool IsSupported<TKey>(
        TKey key)
        where TKey : notnull
    {
        return _registry.IsRegistered(key);
    }

    /// <inheritdoc/>
    public bool IsSupported<TObject>()
        where TObject : class, IEngineObject
    {
        return _registry.IsRegistered<TObject>();
    }
}
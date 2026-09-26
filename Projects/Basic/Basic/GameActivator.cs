using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Engine;

/// <summary>
/// Provides a restricted activation interface for an <see cref="IGameRegistry"/>.
/// </summary>
/// <remarks>
/// This class wraps a game registry and exposes only object creation
/// and support-query functionality. Registry modification operations
/// are not exposed through this type.
/// </remarks>
public sealed class GameActivator : IGameActivator
{
    private readonly IGameRegistry _registry;

    /// <summary>
    /// Gets the shared empty game activator instance.
    /// </summary>
    public static IGameActivator Instance => EmptyGameActivator.Instance;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameActivator"/> class.
    /// </summary>
    /// <param name="registry">The game registry used to resolve and create engine objects.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="registry"/> is <see langword="null"/>.
    /// </exception>
    public GameActivator(IGameRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        _registry = registry;
    }

    /// <inheritdoc/>
    public IReadOnlyList<IGameRegistryKey> GetKeys()
    {
        return _registry.GetKeys();
    }

    /// <inheritdoc/>
    public bool IsSupported(IGameRegistryKey key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _registry.IsRegistered(key);
    }

    /// <inheritdoc/>
    public bool IsDefinitionSupported(Type definitionType)
    {
        ArgumentNullException.ThrowIfNull(definitionType);
        return _registry.IsDefinitionRegistered(definitionType);
    }

    /// <inheritdoc/>
    public bool IsObjectSupported(Type objectType)
    {
        ArgumentNullException.ThrowIfNull(objectType);
        return _registry.IsObjectRegistered(objectType);
    }

    /// <inheritdoc/>
    public IEngineObject Create(IGameRegistryKey key, IDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(definition);

        IDefinition registeredDefinition = _registry.CreateDefinition(key);

        if (registeredDefinition.GetType() != definition.GetType())
        {
            throw new ArgumentException(
                $"Definition type '{definition.GetType().FullName}' does not match the definition type " +
                $"'{registeredDefinition.GetType().FullName}' registered for key '{key}'.",
                nameof(definition));
        }

        return _registry.CreateFromDefinition(definition);
    }

    /// <inheritdoc/>
    public IEngineObject Create(Type objectType, IDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(objectType);
        ArgumentNullException.ThrowIfNull(definition);
        return _registry.Create(objectType, definition);
    }

    /// <inheritdoc/>
    public IEngineObject CreateFromDefinition(IDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return _registry.CreateFromDefinition(definition);
    }

    /// <inheritdoc/>
    public IDefinition CreateDefinition(IGameRegistryKey key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _registry.CreateDefinition(key);
    }

    /// <inheritdoc/>
    public bool TryCreateDefinition(
        IGameRegistryKey key,
        [NotNullWhen(true)] out IDefinition? definition)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _registry.TryCreateDefinition(key, out definition);
    }

    /// <inheritdoc/>
    public bool TryCreate(
        IGameRegistryKey key,
        IDefinition definition,
        [NotNullWhen(true)] out IEngineObject? instance)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(definition);

        if (!_registry.TryCreateDefinition(key, out IDefinition? registeredDefinition) ||
            registeredDefinition.GetType() != definition.GetType())
        {
            instance = null;
            return false;
        }

        return _registry.TryCreateFromDefinition(definition, out instance);
    }

    /// <inheritdoc/>
    public bool TryCreate(
        Type objectType,
        IDefinition definition,
        [NotNullWhen(true)] out IEngineObject? instance)
    {
        ArgumentNullException.ThrowIfNull(objectType);
        ArgumentNullException.ThrowIfNull(definition);
        return _registry.TryCreate(objectType, definition, out instance);
    }

    /// <inheritdoc/>
    public bool TryCreateFromDefinition(
        IDefinition definition,
        [NotNullWhen(true)] out IEngineObject? instance)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return _registry.TryCreateFromDefinition(definition, out instance);
    }
}
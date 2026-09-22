using Sachssoft.Engine.Common;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Engine;

/// <summary>
/// Provides a base implementation for a game activator
/// with a preconfigured game registry.
/// </summary>
public abstract class PreconfiguredGameActivator : IGameActivator
{
    private readonly IGameRegistry _gameRegistry;

    /// <summary>
    /// Initializes a new instance of the <see cref="PreconfiguredGameActivator"/> class.
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
    /// <returns>The preconfigured game registry.</returns>
    protected abstract IGameRegistry CreateRegistry();

    /// <inheritdoc/>
    public bool IsDefinitionSupported(Type definitionType)
    {
        ArgumentNullException.ThrowIfNull(definitionType);
        return _gameRegistry.IsDefinitionRegistered(definitionType);
    }

    /// <inheritdoc/>
    public bool IsObjectSupported(Type objectType)
    {
        ArgumentNullException.ThrowIfNull(objectType);
        return _gameRegistry.IsObjectRegistered(objectType);
    }

    /// <inheritdoc/>
    public IEngineObject Create(IGameRegistryKey key, IDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(definition);

        IDefinition registeredDefinition = _gameRegistry.CreateDefinition(key);

        if (registeredDefinition.GetType() != definition.GetType())
        {
            throw new ArgumentException(
                $"Definition type '{definition.GetType().FullName}' does not match the definition type " +
                $"'{registeredDefinition.GetType().FullName}' registered for key '{key}'.",
                nameof(definition));
        }

        return _gameRegistry.CreateFromDefinition(definition);
    }

    /// <inheritdoc/>
    public IEngineObject Create(Type objectType, IDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(objectType);
        ArgumentNullException.ThrowIfNull(definition);
        return _gameRegistry.Create(objectType, definition);
    }

    /// <inheritdoc/>
    public IEngineObject CreateFromDefinition(IDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return _gameRegistry.CreateFromDefinition(definition);
    }

    /// <inheritdoc/>
    public bool TryCreate(
        IGameRegistryKey key,
        IDefinition definition,
        [NotNullWhen(true)] out IEngineObject? instance)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(definition);

        if (!_gameRegistry.TryCreateDefinition(key, out IDefinition? registeredDefinition) ||
            registeredDefinition.GetType() != definition.GetType())
        {
            instance = null;
            return false;
        }

        return _gameRegistry.TryCreateFromDefinition(definition, out instance);
    }

    /// <inheritdoc/>
    public bool TryCreate(
        Type objectType,
        IDefinition definition,
        [NotNullWhen(true)] out IEngineObject? instance)
    {
        ArgumentNullException.ThrowIfNull(objectType);
        ArgumentNullException.ThrowIfNull(definition);
        return _gameRegistry.TryCreate(objectType, definition, out instance);
    }

    /// <inheritdoc/>
    public bool TryCreateFromDefinition(
        IDefinition definition,
        [NotNullWhen(true)] out IEngineObject? instance)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return _gameRegistry.TryCreateFromDefinition(definition, out instance);
    }
}
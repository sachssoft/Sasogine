using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Engine;

/// <summary>
/// Represents an empty game activator that does not support
/// definition or engine object creation.
/// </summary>
internal sealed class EmptyGameActivator : IGameActivator
{
    /// <summary>
    /// Gets the shared empty game activator instance.
    /// </summary>
    public static EmptyGameActivator Instance { get; } = new();

    private EmptyGameActivator()
    {
    }

    IReadOnlyList<IGameRegistryKey> IGameActivator.GetKeys()
    {
        return Array.Empty<IGameRegistryKey>();
    }

    IGameRegistryKey IGameActivator.GetKey(IDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        throw new NotSupportedException(
            "The empty game activator does not support registry key lookup.");
    }

    bool IGameActivator.TryGetKey(
        IDefinition definition,
        [NotNullWhen(true)] out IGameRegistryKey? key)
    {
        ArgumentNullException.ThrowIfNull(definition);

        key = null;
        return false;
    }

    bool IGameActivator.IsSupported(IGameRegistryKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        return false;
    }

    bool IGameActivator.IsDefinitionSupported(Type definitionType)
    {
        ArgumentNullException.ThrowIfNull(definitionType);

        return false;
    }

    bool IGameActivator.IsObjectSupported(Type objectType)
    {
        ArgumentNullException.ThrowIfNull(objectType);

        return false;
    }

    IEngineObject IGameActivator.Create(
        IGameRegistryKey key,
        IDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(definition);

        throw new NotSupportedException(
            "The empty game activator does not support engine object creation.");
    }

    IEngineObject IGameActivator.Create(
        Type objectType,
        IDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(objectType);
        ArgumentNullException.ThrowIfNull(definition);

        throw new NotSupportedException(
            "The empty game activator does not support engine object creation.");
    }

    IEngineObject IGameActivator.CreateFromDefinition(
        IDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        throw new NotSupportedException(
            "The empty game activator does not support engine object creation.");
    }

    IDefinition IGameActivator.CreateDefinition(
        IGameRegistryKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        throw new NotSupportedException(
            "The empty game activator does not support definition creation.");
    }

    bool IGameActivator.TryCreateDefinition(
        IGameRegistryKey key,
        [NotNullWhen(true)] out IDefinition? definition)
    {
        ArgumentNullException.ThrowIfNull(key);

        definition = null;
        return false;
    }

    bool IGameActivator.TryCreate(
        IGameRegistryKey key,
        IDefinition definition,
        [NotNullWhen(true)] out IEngineObject? instance)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(definition);

        instance = null;
        return false;
    }

    bool IGameActivator.TryCreate(
        Type objectType,
        IDefinition definition,
        [NotNullWhen(true)] out IEngineObject? instance)
    {
        ArgumentNullException.ThrowIfNull(objectType);
        ArgumentNullException.ThrowIfNull(definition);

        instance = null;
        return false;
    }

    bool IGameActivator.TryCreateFromDefinition(
        IDefinition definition,
        [NotNullWhen(true)] out IEngineObject? instance)
    {
        ArgumentNullException.ThrowIfNull(definition);

        instance = null;
        return false;
    }
}
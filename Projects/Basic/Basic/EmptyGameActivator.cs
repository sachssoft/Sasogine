using Sachssoft.Engine.Common;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Engine;

/// <summary>
/// Represents an empty game activator that does not support
/// engine object creation.
/// </summary>
internal sealed class EmptyGameActivator : IGameActivator
{
    /// <summary>
    /// Gets the shared empty game activator instance.
    /// </summary>
    public static EmptyGameActivator Instance { get; } = new();

    internal EmptyGameActivator()
    {
    }

    /// <inheritdoc/>
    public bool IsDefinitionSupported(Type definitionType)
    {
        ArgumentNullException.ThrowIfNull(definitionType);

        return false;
    }

    /// <inheritdoc/>
    public bool IsObjectSupported(Type objectType)
    {
        ArgumentNullException.ThrowIfNull(objectType);

        return false;
    }

    /// <inheritdoc/>
    public IEngineObject Create(
        IGameRegistryKey key,
        IDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(definition);

        throw new NotSupportedException(
            "The empty game activator does not support engine object creation.");
    }

    /// <inheritdoc/>
    public IEngineObject Create(
        Type objectType,
        IDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(objectType);
        ArgumentNullException.ThrowIfNull(definition);

        throw new NotSupportedException(
            "The empty game activator does not support engine object creation.");
    }

    /// <inheritdoc/>
    public IEngineObject CreateFromDefinition(
        IDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        throw new NotSupportedException(
            "The empty game activator does not support engine object creation.");
    }

    /// <inheritdoc/>
    public bool TryCreate(
        IGameRegistryKey key,
        IDefinition definition,
        [NotNullWhen(true)] out IEngineObject? instance)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(definition);

        instance = null;
        return false;
    }

    /// <inheritdoc/>
    public bool TryCreate(
        Type objectType,
        IDefinition definition,
        [NotNullWhen(true)] out IEngineObject? instance)
    {
        ArgumentNullException.ThrowIfNull(objectType);
        ArgumentNullException.ThrowIfNull(definition);

        instance = null;
        return false;
    }

    /// <inheritdoc/>
    public bool TryCreateFromDefinition(
        IDefinition definition,
        [NotNullWhen(true)] out IEngineObject? instance)
    {
        ArgumentNullException.ThrowIfNull(definition);

        instance = null;
        return false;
    }
}
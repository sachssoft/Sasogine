using Sachssoft.Sasogine.Common;
using System;

namespace Sachssoft.Sasogine;

/// <summary>
/// Represents an entry in a game registry that associates a registry key
/// with a concrete definition type and a concrete engine object type.
/// </summary>
public class GameRegistryEntry : IGameRegistryEntry
{
    private readonly Func<IDefinition> _definitionFactory;
    private readonly Func<IDefinition, IEngineObject> _objectFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameRegistryEntry"/> class.
    /// </summary>
    /// <param name="key">
    /// The key used to identify the registry entry.
    /// </param>
    /// <param name="definitionType">
    /// The concrete definition type associated with the entry.
    /// </param>
    /// <param name="objectType">
    /// The concrete engine object type associated with the entry.
    /// </param>
    /// <param name="definitionFactory">
    /// The factory used to create definition instances.
    /// </param>
    /// <param name="objectFactory">
    /// The factory used to create engine object instances from definitions.
    /// </param>
    public GameRegistryEntry(
        IGameRegistryKey key,
        Type definitionType,
        Type objectType,
        Func<IDefinition> definitionFactory,
        Func<IDefinition, IEngineObject> objectFactory)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(definitionType);
        ArgumentNullException.ThrowIfNull(objectType);
        ArgumentNullException.ThrowIfNull(definitionFactory);
        ArgumentNullException.ThrowIfNull(objectFactory);

        if (!typeof(IDefinition).IsAssignableFrom(definitionType))
        {
            throw new ArgumentException(
                $"Definition type '{definitionType.FullName}' does not implement '{typeof(IDefinition).FullName}'.",
                nameof(definitionType));
        }

        if (!typeof(IEngineObject).IsAssignableFrom(objectType))
        {
            throw new ArgumentException(
                $"Object type '{objectType.FullName}' does not implement '{typeof(IEngineObject).FullName}'.",
                nameof(objectType));
        }

        Key = key;
        DefinitionType = definitionType;
        ObjectType = objectType;

        _definitionFactory = definitionFactory;
        _objectFactory = objectFactory;
    }

    /// <summary>
    /// Gets the key used to identify the registry entry.
    /// </summary>
    public IGameRegistryKey Key { get; }

    /// <summary>
    /// Gets the runtime type of the registry key.
    /// </summary>
    public Type KeyType => Key.GetType();

    /// <summary>
    /// Gets the concrete definition type associated with the entry.
    /// </summary>
    public Type DefinitionType { get; }

    /// <summary>
    /// Gets the concrete engine object type associated with the entry.
    /// </summary>
    public Type ObjectType { get; }

    /// <summary>
    /// Creates a new definition instance using the registered definition factory.
    /// </summary>
    /// <returns>
    /// The created definition instance.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the definition factory returns <see langword="null"/> or
    /// an instance that is not compatible with <see cref="DefinitionType"/>.
    /// </exception>
    public IDefinition CreateDefinition()
    {
        IDefinition definition = _definitionFactory()
            ?? throw new InvalidOperationException(
                "The definition factory returned null.");

        if (!DefinitionType.IsInstanceOfType(definition))
        {
            throw new InvalidOperationException(
                $"The definition factory returned an instance of type " +
                $"'{definition.GetType().FullName}', but the registered " +
                $"definition type is '{DefinitionType.FullName}'.");
        }

        return definition;
    }

    /// <summary>
    /// Creates a new engine object instance using the registered object factory.
    /// </summary>
    /// <param name="definition">
    /// The definition used to create the engine object.
    /// </param>
    /// <returns>
    /// The created engine object instance.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="definition"/> is not compatible with
    /// <see cref="DefinitionType"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the object factory returns <see langword="null"/> or
    /// an instance that is not compatible with <see cref="ObjectType"/>.
    /// </exception>
    public IEngineObject CreateObject(IDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (!DefinitionType.IsInstanceOfType(definition))
        {
            throw new ArgumentException(
                $"Definition type '{definition.GetType().FullName}' is not " +
                $"compatible with registered definition type " +
                $"'{DefinitionType.FullName}'.",
                nameof(definition));
        }

        IEngineObject instance = _objectFactory(definition)
            ?? throw new InvalidOperationException(
                "The object factory returned null.");

        if (!ObjectType.IsInstanceOfType(instance))
        {
            throw new InvalidOperationException(
                $"The object factory returned an instance of type " +
                $"'{instance.GetType().FullName}', but the registered " +
                $"object type is '{ObjectType.FullName}'.");
        }

        return instance;
    }
}
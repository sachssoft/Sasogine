using Sachssoft.Sasodoc;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Experimental;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Markup.Serialization;

/// <summary>
/// Provides a game registry with serialization support.
/// </summary>
/// <typeparam name="TKey">The type of the registry key.</typeparam>
/// <typeparam name="TDefinition">The base type of registered definitions.</typeparam>
/// <typeparam name="TObject">The base type of registered engine objects.</typeparam>
public class SerializationRegistry<TKey, TDefinition, TObject> :
    GameRegistry<TKey, TDefinition, TObject>,
    ISerializationRegistry
    where TKey : notnull, IGameRegistryKey
    where TDefinition : class, IDefinition
    where TObject : class, IEngineObject
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="SerializationRegistry{TKey, TDefinition, TObject}"/> class.
    /// </summary>
    public SerializationRegistry()
    {
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="SerializationRegistry{TKey, TDefinition, TObject}"/> class.
    /// </summary>
    /// <param name="definitionMatchMode">The definition matching mode.</param>
    public SerializationRegistry(DefinitionMatchMode definitionMatchMode)
        : base(definitionMatchMode)
    {
    }

    /// <summary>
    /// Registers a concrete definition type, concrete engine object type,
    /// and serialization handler using the specified registry key.
    /// </summary>
    /// <typeparam name="TConcreteDefinition">
    /// The concrete definition type to register.
    /// </typeparam>
    /// <typeparam name="TConcreteObject">
    /// The concrete engine object type to register.
    /// </typeparam>
    /// <param name="key">
    /// The key used to identify the registry entry.
    /// </param>
    /// <param name="serialization">
    /// The serialization handler associated with the registry entry.
    /// </param>
    /// <param name="definitionFactory">
    /// The factory used to create concrete definition instances.
    /// </param>
    /// <param name="objectFactory">
    /// The factory used to create concrete engine object instances from definitions.
    /// </param>
    public void Register<TConcreteDefinition, TConcreteObject>(
        TKey key,
        ISerialization serialization,
        Func<TConcreteDefinition> definitionFactory,
        Func<TConcreteDefinition, TConcreteObject> objectFactory)
        where TConcreteDefinition : class, TDefinition
        where TConcreteObject : class, TObject
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(serialization);
        ArgumentNullException.ThrowIfNull(definitionFactory);
        ArgumentNullException.ThrowIfNull(objectFactory);

        RegisterEntry(
            new SerializationRegistryEntry(
                key,
                typeof(TConcreteDefinition),
                typeof(TConcreteObject),
                serialization,
                () => definitionFactory(),
                definition => objectFactory(
                    (TConcreteDefinition)definition)));
    }

    /// <summary>
    /// Determines whether serialization is registered for the specified key.
    /// </summary>
    /// <param name="key">The registry key.</param>
    /// <returns>
    /// <see langword="true"/> if serialization is registered;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsSerializationRegistered(TKey key)
    {
        return TryGetSerialization(key, out _);
    }

    /// <summary>
    /// Gets the serialization handler registered for the specified key.
    /// </summary>
    /// <param name="key">The registry key.</param>
    /// <returns>The registered serialization handler.</returns>
    public ISerialization GetSerialization(TKey key)
    {
        if (!TryGetSerialization(key, out ISerialization? serialization))
            throw new KeyNotFoundException(
                $"No serialization is registered for key '{key}'.");

        return serialization;
    }

    /// <summary>
    /// Attempts to get the serialization handler registered for the specified key.
    /// </summary>
    /// <param name="key">The registry key.</param>
    /// <param name="serialization">The registered serialization handler.</param>
    /// <returns>
    /// <see langword="true"/> if serialization was found;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGetSerialization(
        TKey key,
        [NotNullWhen(true)] out ISerialization? serialization)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (TryGetSerializationEntry(key, out ISerializationRegistryEntry? entry))
        {
            serialization = entry.Serialization;
            return true;
        }

        serialization = null;
        return false;
    }

    /// <summary>
    /// Gets the serialization handler registered for the specified name.
    /// </summary>
    /// <param name="name">The registry key name.</param>
    /// <returns>The registered serialization handler.</returns>
    public ISerialization GetSerialization(string name)
    {
        if (!TryGetSerialization(name, out ISerialization? serialization))
            throw new KeyNotFoundException(
                $"No serialization is registered with name '{name}'.");

        return serialization;
    }

    /// <summary>
    /// Attempts to get the serialization handler registered for the specified name.
    /// </summary>
    /// <param name="name">The registry key name.</param>
    /// <param name="serialization">The registered serialization handler.</param>
    /// <returns>
    /// <see langword="true"/> if serialization was found;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGetSerialization(
        string name,
        [NotNullWhen(true)] out ISerialization? serialization)
    {
        if (TryGetSerializationEntry(name, out ISerializationRegistryEntry? entry))
        {
            serialization = entry.Serialization;
            return true;
        }

        serialization = null;
        return false;
    }

    /// <summary>
    /// Serializes the specified definition.
    /// </summary>
    /// <param name="definition">The definition to serialize.</param>
    /// <param name="writer">The writer that receives the serialized values.</param>
    public void Serialize(TDefinition definition, FormatWriterBase writer)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(writer);

        if (!TryGetSerializationEntry(
            definition.GetType(),
            out ISerializationRegistryEntry? entry))
        {
            throw new KeyNotFoundException(
                $"No serialization is registered for definition type " +
                $"'{definition.GetType().FullName}'.");
        }

        entry.Serialization.Serialize(definition, writer);
    }

    /// <summary>
    /// Attempts to serialize the specified definition.
    /// </summary>
    /// <param name="definition">The definition to serialize.</param>
    /// <param name="writer">The writer that receives the serialized values.</param>
    /// <returns>
    /// <see langword="true"/> if serialization was found and executed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TrySerialize(TDefinition definition, FormatWriterBase writer)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(writer);

        if (!TryGetSerializationEntry(
            definition.GetType(),
            out ISerializationRegistryEntry? entry))
        {
            return false;
        }

        entry.Serialization.Serialize(definition, writer);
        return true;
    }

    /// <summary>
    /// Deserializes a definition registered with the specified name.
    /// </summary>
    /// <param name="name">The registry key name.</param>
    /// <param name="reader">The reader that provides the serialized values.</param>
    /// <returns>The deserialized definition.</returns>
    public TDefinition Deserialize(string name, FormatReaderBase reader)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(reader);

        if (!TryGetSerializationEntry(
            name,
            out ISerializationRegistryEntry? entry))
        {
            throw new KeyNotFoundException(
                $"No serialization is registered with name '{name}'.");
        }

        TDefinition definition = (TDefinition)entry.CreateDefinition();
        entry.Serialization.Deserialize(definition, reader);

        return definition;
    }

    /// <summary>
    /// Attempts to deserialize a definition registered with the specified name.
    /// </summary>
    /// <param name="name">The registry key name.</param>
    /// <param name="reader">The reader that provides the serialized values.</param>
    /// <param name="definition">The deserialized definition.</param>
    /// <returns>
    /// <see langword="true"/> if deserialization succeeded;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryDeserialize(
        string name,
        FormatReaderBase reader,
        [NotNullWhen(true)] out TDefinition? definition)
    {
        ArgumentNullException.ThrowIfNull(reader);

        if (!TryGetSerializationEntry(
            name,
            out ISerializationRegistryEntry? entry))
        {
            definition = null;
            return false;
        }

        definition = (TDefinition)entry.CreateDefinition();
        entry.Serialization.Deserialize(definition, reader);
        return true;
    }

    /// <summary>
    /// Attempts to deserialize a concrete definition registered with the specified name.
    /// </summary>
    /// <typeparam name="TConcreteDefinition">The concrete definition type.</typeparam>
    /// <param name="name">The registry key name.</param>
    /// <param name="reader">The reader that provides the serialized values.</param>
    /// <param name="definition">The deserialized definition.</param>
    /// <returns>
    /// <see langword="true"/> if deserialization succeeded and the definition
    /// is compatible with <typeparamref name="TConcreteDefinition"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryDeserialize<TConcreteDefinition>(
        string name,
        FormatReaderBase reader,
        [NotNullWhen(true)] out TConcreteDefinition? definition)
        where TConcreteDefinition : class, TDefinition
    {
        ArgumentNullException.ThrowIfNull(reader);

        if (!TryGetSerializationEntry(
            name,
            out ISerializationRegistryEntry? entry))
        {
            definition = null;
            return false;
        }

        if (entry.CreateDefinition() is not TConcreteDefinition concreteDefinition)
        {
            definition = null;
            return false;
        }

        entry.Serialization.Deserialize(concreteDefinition, reader);
        definition = concreteDefinition;
        return true;
    }

    /// <summary>
    /// Deserializes a definition registered with the specified key.
    /// </summary>
    /// <param name="key">The registry key.</param>
    /// <param name="reader">The reader that provides the serialized values.</param>
    /// <returns>The deserialized definition.</returns>
    public TDefinition Deserialize(TKey key, FormatReaderBase reader)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(reader);

        if (!TryGetSerializationEntry(
            key,
            out ISerializationRegistryEntry? entry))
        {
            throw new KeyNotFoundException(
                $"No serialization is registered for key '{key}'.");
        }

        TDefinition definition = (TDefinition)entry.CreateDefinition();
        entry.Serialization.Deserialize(definition, reader);

        return definition;
    }

    /// <summary>
    /// Deserializes a concrete definition registered with the specified key.
    /// </summary>
    /// <typeparam name="TConcreteDefinition">The concrete definition type.</typeparam>
    /// <param name="key">The registry key.</param>
    /// <param name="reader">The reader that provides the serialized values.</param>
    /// <returns>The deserialized concrete definition.</returns>
    public TConcreteDefinition Deserialize<TConcreteDefinition>(
        TKey key,
        FormatReaderBase reader)
        where TConcreteDefinition : class, TDefinition
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(reader);

        if (!TryGetSerializationEntry(
            key,
            out ISerializationRegistryEntry? entry))
        {
            throw new KeyNotFoundException(
                $"No serialization is registered for key '{key}'.");
        }

        if (entry.CreateDefinition() is not TConcreteDefinition definition)
        {
            throw new InvalidCastException(
                $"Definition type '{entry.DefinitionType.FullName}' is not compatible with " +
                $"'{typeof(TConcreteDefinition).FullName}'.");
        }

        entry.Serialization.Deserialize(definition, reader);
        return definition;
    }

    /// <summary>
    /// Attempts to deserialize a definition registered with the specified key.
    /// </summary>
    /// <param name="key">The registry key.</param>
    /// <param name="reader">The reader that provides the serialized values.</param>
    /// <param name="definition">The deserialized definition.</param>
    /// <returns>
    /// <see langword="true"/> if deserialization succeeded;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryDeserialize(
        TKey key,
        FormatReaderBase reader,
        [NotNullWhen(true)] out TDefinition? definition)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(reader);

        if (!TryGetSerializationEntry(
            key,
            out ISerializationRegistryEntry? entry))
        {
            definition = null;
            return false;
        }

        definition = (TDefinition)entry.CreateDefinition();
        entry.Serialization.Deserialize(definition, reader);
        return true;
    }

    /// <summary>
    /// Attempts to deserialize a concrete definition registered with the specified key.
    /// </summary>
    /// <typeparam name="TConcreteDefinition">The concrete definition type.</typeparam>
    /// <param name="key">The registry key.</param>
    /// <param name="reader">The reader that provides the serialized values.</param>
    /// <param name="definition">The deserialized concrete definition.</param>
    /// <returns>
    /// <see langword="true"/> if deserialization succeeded and the definition
    /// is compatible with <typeparamref name="TConcreteDefinition"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryDeserialize<TConcreteDefinition>(
        TKey key,
        FormatReaderBase reader,
        [NotNullWhen(true)] out TConcreteDefinition? definition)
        where TConcreteDefinition : class, TDefinition
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(reader);

        if (!TryGetSerializationEntry(
            key,
            out ISerializationRegistryEntry? entry))
        {
            definition = null;
            return false;
        }

        if (entry.CreateDefinition() is not TConcreteDefinition concreteDefinition)
        {
            definition = null;
            return false;
        }

        entry.Serialization.Deserialize(concreteDefinition, reader);
        definition = concreteDefinition;
        return true;
    }

    private bool TryGetSerializationEntry(
        TKey key,
        [NotNullWhen(true)] out ISerializationRegistryEntry? entry)
    {
        foreach (IGameRegistryEntry candidate in GetEntries())
        {
            if (candidate is ISerializationRegistryEntry serializationEntry &&
                serializationEntry.Key is TKey entryKey &&
                EqualityComparer<TKey>.Default.Equals(entryKey, key))
            {
                entry = serializationEntry;
                return true;
            }
        }

        entry = null;
        return false;
    }

    private bool TryGetSerializationEntry(
        string name,
        [NotNullWhen(true)] out ISerializationRegistryEntry? entry)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            foreach (IGameRegistryEntry candidate in GetEntries())
            {
                if (candidate is ISerializationRegistryEntry serializationEntry &&
                    StringComparer.OrdinalIgnoreCase.Equals(
                        serializationEntry.Key.Name,
                        name))
                {
                    entry = serializationEntry;
                    return true;
                }
            }
        }

        entry = null;
        return false;
    }

    private bool TryGetSerializationEntry(
        Type definitionType,
        [NotNullWhen(true)] out ISerializationRegistryEntry? entry)
    {
        ArgumentNullException.ThrowIfNull(definitionType);

        ISerializationRegistryEntry? best = null;

        foreach (IGameRegistryEntry candidate in GetEntries())
        {
            if (candidate is not ISerializationRegistryEntry serializationEntry)
                continue;

            if (serializationEntry.DefinitionType == definitionType)
            {
                entry = serializationEntry;
                return true;
            }

            if (DefinitionMatchMode == DefinitionMatchMode.Assignable &&
                serializationEntry.DefinitionType.IsAssignableFrom(definitionType) &&
                (best is null ||
                 best.DefinitionType.IsAssignableFrom(
                     serializationEntry.DefinitionType)))
            {
                best = serializationEntry;
            }
        }

        entry = best;
        return entry is not null;
    }

    bool ISerializationRegistry.IsSerializationRegistered(IGameRegistryKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        return key is TKey typedKey &&
               IsSerializationRegistered(typedKey);
    }

    ISerialization ISerializationRegistry.GetSerialization(IGameRegistryKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (key is not TKey typedKey)
            throw UnsupportedKey(key);

        return GetSerialization(typedKey);
    }

    bool ISerializationRegistry.TryGetSerialization(
        IGameRegistryKey key,
        [NotNullWhen(true)] out ISerialization? serialization)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (key is not TKey typedKey)
        {
            serialization = null;
            return false;
        }

        return TryGetSerialization(typedKey, out serialization);
    }

    ISerialization ISerializationRegistry.GetSerialization(string name)
    {
        return GetSerialization(name);
    }

    bool ISerializationRegistry.TryGetSerialization(
        string name,
        [NotNullWhen(true)] out ISerialization? serialization)
    {
        return TryGetSerialization(name, out serialization);
    }

    void ISerializationRegistry.Serialize(
        IDefinition definition,
        FormatWriterBase writer)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(writer);

        if (definition is not TDefinition typedDefinition)
        {
            throw new ArgumentException(
                $"Definition type '{definition.GetType().FullName}' is not compatible with " +
                $"'{typeof(TDefinition).FullName}'.",
                nameof(definition));
        }

        Serialize(typedDefinition, writer);
    }

    bool ISerializationRegistry.TrySerialize(
        IDefinition definition,
        FormatWriterBase writer)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(writer);

        return definition is TDefinition typedDefinition &&
               TrySerialize(typedDefinition, writer);
    }

    IDefinition ISerializationRegistry.Deserialize(
        string name,
        FormatReaderBase reader)
    {
        return Deserialize(name, reader);
    }

    bool ISerializationRegistry.TryDeserialize(
        string name,
        FormatReaderBase reader,
        [NotNullWhen(true)] out IDefinition? definition)
    {
        if (TryDeserialize(name, reader, out TDefinition? result))
        {
            definition = result;
            return true;
        }

        definition = null;
        return false;
    }

    IDefinition ISerializationRegistry.Deserialize(
        IGameRegistryKey key,
        FormatReaderBase reader)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (key is not TKey typedKey)
            throw UnsupportedKey(key);

        return Deserialize(typedKey, reader);
    }

    bool ISerializationRegistry.TryDeserialize(
        IGameRegistryKey key,
        FormatReaderBase reader,
        [NotNullWhen(true)] out IDefinition? definition)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (key is not TKey typedKey)
        {
            definition = null;
            return false;
        }

        if (TryDeserialize(typedKey, reader, out TDefinition? result))
        {
            definition = result;
            return true;
        }

        definition = null;
        return false;
    }

    private static ArgumentException UnsupportedKey(IGameRegistryKey key)
    {
        return new ArgumentException(
            $"Registry key type '{key.GetType().FullName}' is not compatible with " +
            $"'{typeof(TKey).FullName}'.",
            nameof(key));
    }
}
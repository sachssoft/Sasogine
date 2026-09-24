using Sachssoft.Engine;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Documents.Serialization;

/// <summary>
/// Provides a preconfigured game activator with serialization support.
/// </summary>
/// <typeparam name="TKey">
/// The registry key type.
/// </typeparam>
/// <typeparam name="TDefinition">
/// The base definition type.
/// </typeparam>
/// <typeparam name="TObject">
/// The base engine object type.
/// </typeparam>
public abstract class PreconfiguredSerializationGameActivator<TKey, TDefinition, TObject> :
    ISerializationGameActivator
    where TKey : notnull, IGameRegistryKey
    where TDefinition : class, IDefinition
    where TObject : class, IEngineObject
{
    private readonly SerializationRegistry<TKey, TDefinition, TObject> _registry;
    private readonly IGameActivator _activator;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="PreconfiguredSerializationGameActivator{TKey, TDefinition, TObject}"/>
    /// class.
    /// </summary>
    protected PreconfiguredSerializationGameActivator()
    {
        _registry = CreateSerializationRegistry()
            ?? throw new InvalidOperationException(
                "The serialization registry cannot be null.");

        _activator = new GameActivator(_registry);
    }

    /// <summary>
    /// Creates and configures the serialization registry used by this
    /// activator.
    /// </summary>
    /// <returns>
    /// The configured serialization registry.
    /// </returns>
    protected abstract SerializationRegistry<TKey, TDefinition, TObject>
        CreateSerializationRegistry();

    #region Public API

    /// <summary>
    /// Determines whether the specified definition type is supported.
    /// </summary>
    /// <param name="definitionType">
    /// The definition type to check.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the definition type is supported;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsDefinitionSupported(Type definitionType)
    {
        return _activator.IsDefinitionSupported(definitionType);
    }

    /// <summary>
    /// Determines whether the specified engine object type is supported.
    /// </summary>
    /// <param name="objectType">
    /// The engine object type to check.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the engine object type is supported;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsObjectSupported(Type objectType)
    {
        return _activator.IsObjectSupported(objectType);
    }

    /// <summary>
    /// Determines whether serialization is supported for the specified key.
    /// </summary>
    /// <param name="key">
    /// The registry key to check.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if serialization is supported;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsSerializationSupported(TKey key)
    {
        return _registry.IsSerializationRegistered(key);
    }

    /// <summary>
    /// Creates an engine object associated with the specified registry key
    /// using the provided definition.
    /// </summary>
    /// <param name="key">
    /// The registry key used to locate the corresponding object factory.
    /// </param>
    /// <param name="definition">
    /// The definition used to create the engine object.
    /// </param>
    /// <returns>
    /// The created engine object.
    /// </returns>
    public TObject Create(
        TKey key,
        TDefinition definition)
    {
        return (TObject)_activator.Create(
            key,
            definition);
    }

    /// <summary>
    /// Creates an engine object compatible with the specified object type
    /// using the provided definition.
    /// </summary>
    /// <param name="objectType">
    /// The requested engine object type.
    /// </param>
    /// <param name="definition">
    /// The definition used to create the engine object.
    /// </param>
    /// <returns>
    /// The created engine object.
    /// </returns>
    public TObject Create(
        Type objectType,
        TDefinition definition)
    {
        return (TObject)_activator.Create(
            objectType,
            definition);
    }

    /// <summary>
    /// Creates an engine object using the registry entry associated with the
    /// specified definition.
    /// </summary>
    /// <param name="definition">
    /// The definition used to locate the corresponding object factory.
    /// </param>
    /// <returns>
    /// The created engine object.
    /// </returns>
    public TObject CreateFromDefinition(
        TDefinition definition)
    {
        return (TObject)_activator.CreateFromDefinition(
            definition);
    }

    /// <summary>
    /// Attempts to create an engine object associated with the specified
    /// registry key using the provided definition.
    /// </summary>
    /// <param name="key">
    /// The registry key used to locate the corresponding object factory.
    /// </param>
    /// <param name="definition">
    /// The definition used to create the engine object.
    /// </param>
    /// <param name="instance">
    /// When this method returns <see langword="true"/>, contains the created
    /// engine object; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the engine object could be created;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryCreate(
        TKey key,
        TDefinition definition,
        [NotNullWhen(true)] out TObject? instance)
    {
        if (_activator.TryCreate(
                key,
                definition,
                out IEngineObject? result) &&
            result is TObject typedResult)
        {
            instance = typedResult;
            return true;
        }

        instance = null;
        return false;
    }

    /// <summary>
    /// Attempts to create an engine object compatible with the specified
    /// object type using the provided definition.
    /// </summary>
    /// <param name="objectType">
    /// The requested engine object type.
    /// </param>
    /// <param name="definition">
    /// The definition used to create the engine object.
    /// </param>
    /// <param name="instance">
    /// When this method returns <see langword="true"/>, contains the created
    /// engine object; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the engine object could be created;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryCreate(
        Type objectType,
        TDefinition definition,
        [NotNullWhen(true)] out TObject? instance)
    {
        if (_activator.TryCreate(
                objectType,
                definition,
                out IEngineObject? result) &&
            result is TObject typedResult)
        {
            instance = typedResult;
            return true;
        }

        instance = null;
        return false;
    }

    /// <summary>
    /// Attempts to create an engine object using the registry entry associated
    /// with the specified definition.
    /// </summary>
    /// <param name="definition">
    /// The definition used to locate the corresponding object factory.
    /// </param>
    /// <param name="instance">
    /// When this method returns <see langword="true"/>, contains the created
    /// engine object; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the engine object could be created;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryCreateFromDefinition(
        TDefinition definition,
        [NotNullWhen(true)] out TObject? instance)
    {
        if (_activator.TryCreateFromDefinition(
                definition,
                out IEngineObject? result) &&
            result is TObject typedResult)
        {
            instance = typedResult;
            return true;
        }

        instance = null;
        return false;
    }

    /// <summary>
    /// Gets the serialization handler associated with the specified key.
    /// </summary>
    /// <param name="key">
    /// The registry key.
    /// </param>
    /// <returns>
    /// The registered serialization handler.
    /// </returns>
    public ISerialization GetSerialization(TKey key)
    {
        return _registry.GetSerialization(key);
    }

    /// <summary>
    /// Attempts to get the serialization handler associated with the
    /// specified key.
    /// </summary>
    /// <param name="key">
    /// The registry key.
    /// </param>
    /// <param name="serialization">
    /// When this method returns <see langword="true"/>, contains the
    /// registered serialization handler; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a serialization handler was found;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGetSerialization(
        TKey key,
        [NotNullWhen(true)] out ISerialization? serialization)
    {
        return _registry.TryGetSerialization(
            key,
            out serialization);
    }

    /// <summary>
    /// Serializes the specified definition.
    /// </summary>
    /// <param name="definition">
    /// The definition to serialize.
    /// </param>
    /// <param name="writer">
    /// The writer that receives the serialized values.
    /// </param>
    public void Serialize(
        TDefinition definition,
        FormatWriterBase writer)
    {
        _registry.Serialize(
            definition,
            writer);
    }

    /// <summary>
    /// Attempts to serialize the specified definition.
    /// </summary>
    /// <param name="definition">
    /// The definition to serialize.
    /// </param>
    /// <param name="writer">
    /// The writer that receives the serialized values.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if serialization succeeded;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TrySerialize(
        TDefinition definition,
        FormatWriterBase writer)
    {
        return _registry.TrySerialize(
            definition,
            writer);
    }

    /// <summary>
    /// Deserializes a definition using the specified registered name.
    /// </summary>
    /// <param name="name">
    /// The registered serialization name.
    /// </param>
    /// <param name="reader">
    /// The reader that provides the serialized values.
    /// </param>
    /// <returns>
    /// The deserialized definition.
    /// </returns>
    public TDefinition Deserialize(
        string name,
        FormatReaderBase reader)
    {
        return _registry.Deserialize(
            name,
            reader);
    }

    /// <summary>
    /// Deserializes a definition using the specified registry key.
    /// </summary>
    /// <param name="key">
    /// The registry key identifying the serialization.
    /// </param>
    /// <param name="reader">
    /// The reader that provides the serialized values.
    /// </param>
    /// <returns>
    /// The deserialized definition.
    /// </returns>
    public TDefinition Deserialize(
        TKey key,
        FormatReaderBase reader)
    {
        return _registry.Deserialize(
            key,
            reader);
    }

    /// <summary>
    /// Attempts to deserialize a definition using the specified registered
    /// name.
    /// </summary>
    /// <param name="name">
    /// The registered serialization name.
    /// </param>
    /// <param name="reader">
    /// The reader that provides the serialized values.
    /// </param>
    /// <param name="definition">
    /// When this method returns <see langword="true"/>, contains the
    /// deserialized definition; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if deserialization succeeded;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryDeserialize(
        string name,
        FormatReaderBase reader,
        [NotNullWhen(true)] out TDefinition? definition)
    {
        return _registry.TryDeserialize(
            name,
            reader,
            out definition);
    }

    /// <summary>
    /// Attempts to deserialize a definition using the specified registry key.
    /// </summary>
    /// <param name="key">
    /// The registry key identifying the serialization.
    /// </param>
    /// <param name="reader">
    /// The reader that provides the serialized values.
    /// </param>
    /// <param name="definition">
    /// When this method returns <see langword="true"/>, contains the
    /// deserialized definition; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if deserialization succeeded;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryDeserialize(
        TKey key,
        FormatReaderBase reader,
        [NotNullWhen(true)] out TDefinition? definition)
    {
        return _registry.TryDeserialize(
            key,
            reader,
            out definition);
    }

    #endregion

    #region IGameActivator

    IEngineObject IGameActivator.Create(
        IGameRegistryKey key,
        IDefinition definition)
    {
        return _activator.Create(
            key,
            definition);
    }

    IEngineObject IGameActivator.Create(
        Type objectType,
        IDefinition definition)
    {
        return _activator.Create(
            objectType,
            definition);
    }

    IEngineObject IGameActivator.CreateFromDefinition(
        IDefinition definition)
    {
        return _activator.CreateFromDefinition(
            definition);
    }

    bool IGameActivator.IsDefinitionSupported(
        Type definitionType)
    {
        return IsDefinitionSupported(definitionType);
    }

    bool IGameActivator.IsObjectSupported(
        Type objectType)
    {
        return IsObjectSupported(objectType);
    }

    bool IGameActivator.TryCreate(
        IGameRegistryKey key,
        IDefinition definition,
        [NotNullWhen(true)] out IEngineObject? instance)
    {
        return _activator.TryCreate(
            key,
            definition,
            out instance);
    }

    bool IGameActivator.TryCreate(
        Type objectType,
        IDefinition definition,
        [NotNullWhen(true)] out IEngineObject? instance)
    {
        return _activator.TryCreate(
            objectType,
            definition,
            out instance);
    }

    bool IGameActivator.TryCreateFromDefinition(
        IDefinition definition,
        [NotNullWhen(true)] out IEngineObject? instance)
    {
        return _activator.TryCreateFromDefinition(
            definition,
            out instance);
    }

    #endregion

    #region ISerializationGameActivator

    bool ISerializationGameActivator.IsSerializationSupported(
        IGameRegistryKey key)
    {
        return key is TKey typedKey &&
               IsSerializationSupported(typedKey);
    }

    ISerialization ISerializationGameActivator.GetSerialization(
        IGameRegistryKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (key is not TKey typedKey)
        {
            throw new ArgumentException(
                $"Registry key type '{key.GetType().FullName}' is not " +
                $"compatible with '{typeof(TKey).FullName}'.",
                nameof(key));
        }

        return GetSerialization(typedKey);
    }

    bool ISerializationGameActivator.TryGetSerialization(
        IGameRegistryKey key,
        [NotNullWhen(true)] out ISerialization? serialization)
    {
        if (key is TKey typedKey)
        {
            return TryGetSerialization(
                typedKey,
                out serialization);
        }

        serialization = null;
        return false;
    }

    void ISerializationGameActivator.Serialize(
        IDefinition definition,
        FormatWriterBase writer)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (definition is not TDefinition typedDefinition)
        {
            throw new ArgumentException(
                $"Definition type '{definition.GetType().FullName}' is not " +
                $"compatible with '{typeof(TDefinition).FullName}'.",
                nameof(definition));
        }

        Serialize(
            typedDefinition,
            writer);
    }

    bool ISerializationGameActivator.TrySerialize(
        IDefinition definition,
        FormatWriterBase writer)
    {
        if (definition is not TDefinition typedDefinition)
            return false;

        return TrySerialize(
            typedDefinition,
            writer);
    }

    IDefinition ISerializationGameActivator.Deserialize(
        string name,
        FormatReaderBase reader)
    {
        return Deserialize(
            name,
            reader);
    }

    bool ISerializationGameActivator.TryDeserialize(
        string name,
        FormatReaderBase reader,
        [NotNullWhen(true)] out IDefinition? definition)
    {
        if (TryDeserialize(
                name,
                reader,
                out TDefinition? typedDefinition))
        {
            definition = typedDefinition;
            return true;
        }

        definition = null;
        return false;
    }

    IDefinition ISerializationGameActivator.Deserialize(
        IGameRegistryKey key,
        FormatReaderBase reader)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (key is not TKey typedKey)
        {
            throw new ArgumentException(
                $"Registry key type '{key.GetType().FullName}' is not " +
                $"compatible with '{typeof(TKey).FullName}'.",
                nameof(key));
        }

        return Deserialize(
            typedKey,
            reader);
    }

    bool ISerializationGameActivator.TryDeserialize(
        IGameRegistryKey key,
        FormatReaderBase reader,
        [NotNullWhen(true)] out IDefinition? definition)
    {
        if (key is TKey typedKey &&
            TryDeserialize(
                typedKey,
                reader,
                out TDefinition? typedDefinition))
        {
            definition = typedDefinition;
            return true;
        }

        definition = null;
        return false;
    }

    #endregion
}
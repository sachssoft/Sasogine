using Sachssoft.Sasogine.Common;
using System;

namespace Sachssoft.Sasogine.Documents.Serialization;

/// <summary>
/// Represents a game registry entry with serialization support.
/// </summary>
public class SerializationRegistryEntry :
    GameRegistryEntry,
    ISerializationRegistryEntry
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="SerializationRegistryEntry"/> class.
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
    /// <param name="serialization">
    /// The serialization handler associated with the entry.
    /// </param>
    /// <param name="definitionFactory">
    /// The factory used to create definition instances.
    /// </param>
    /// <param name="objectFactory">
    /// The factory used to create engine object instances from definitions.
    /// </param>
    public SerializationRegistryEntry(
        IGameRegistryKey key,
        Type definitionType,
        Type objectType,
        ISerialization serialization,
        Func<IDefinition> definitionFactory,
        Func<IDefinition, IEngineObject> objectFactory)
        : base(
            key,
            definitionType,
            objectType,
            definitionFactory,
            objectFactory)
    {
        ArgumentNullException.ThrowIfNull(serialization);

        Serialization = serialization;
    }

    /// <summary>
    /// Gets the serialization handler associated with the registry entry.
    /// </summary>
    public ISerialization Serialization { get; }
}
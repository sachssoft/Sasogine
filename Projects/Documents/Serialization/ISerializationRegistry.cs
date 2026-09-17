using Sachssoft.Sasodoc;
using Sachssoft.Sasogine.Common;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Documents.Serialization;

/// <summary>
/// Defines a game registry with serialization support.
/// </summary>
public interface ISerializationRegistry : IGameRegistry
{
    /// <summary>
    /// Determines whether serialization is registered for the specified key.
    /// </summary>
    bool IsSerializationRegistered(IGameRegistryKey key);

    /// <summary>
    /// Gets the serialization handler registered for the specified key.
    /// </summary>
    ISerialization GetSerialization(IGameRegistryKey key);

    /// <summary>
    /// Attempts to get the serialization handler registered for the specified key.
    /// </summary>
    bool TryGetSerialization(
        IGameRegistryKey key,
        [NotNullWhen(true)] out ISerialization? serialization);

    /// <summary>
    /// Gets the serialization handler registered for the specified name.
    /// </summary>
    ISerialization GetSerialization(string name);

    /// <summary>
    /// Attempts to get the serialization handler registered for the specified name.
    /// </summary>
    bool TryGetSerialization(
        string name,
        [NotNullWhen(true)] out ISerialization? serialization);

    /// <summary>
    /// Serializes the specified definition.
    /// </summary>
    void Serialize(IDefinition definition, FormatWriterBase writer);

    /// <summary>
    /// Attempts to serialize the specified definition.
    /// </summary>
    bool TrySerialize(IDefinition definition, FormatWriterBase writer);

    /// <summary>
    /// Deserializes a definition registered with the specified name.
    /// </summary>
    IDefinition Deserialize(string name, FormatReaderBase reader);

    /// <summary>
    /// Attempts to deserialize a definition registered with the specified name.
    /// </summary>
    bool TryDeserialize(
        string name,
        FormatReaderBase reader,
        [NotNullWhen(true)] out IDefinition? definition);

    /// <summary>
    /// Deserializes a definition registered with the specified key.
    /// </summary>
    IDefinition Deserialize(IGameRegistryKey key, FormatReaderBase reader);

    /// <summary>
    /// Attempts to deserialize a definition registered with the specified key.
    /// </summary>
    bool TryDeserialize(
        IGameRegistryKey key,
        FormatReaderBase reader,
        [NotNullWhen(true)] out IDefinition? definition);
}
using Sachssoft.Engine;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Documents.Serialization;

/// <summary>
/// Defines an activator that provides serialization and deserialization
/// functionality for registered game definitions.
/// </summary>
public interface ISerializationGameActivator : IGameActivator
{
    /// <summary>
    /// Determines whether serialization is supported for the specified key.
    /// </summary>
    /// <param name="key">The registry key to check.</param>
    /// <returns>
    /// <see langword="true"/> if serialization is supported;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsSerializationSupported(IGameRegistryKey key);

    /// <summary>
    /// Gets the serialization handler associated with the specified key.
    /// </summary>
    /// <param name="key">The registry key.</param>
    /// <returns>The registered serialization handler.</returns>
    ISerialization GetSerialization(IGameRegistryKey key);

    /// <summary>
    /// Attempts to get the serialization handler associated with the
    /// specified key.
    /// </summary>
    /// <param name="key">The registry key.</param>
    /// <param name="serialization">
    /// When this method returns <see langword="true"/>, contains the
    /// registered serialization handler; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a serialization handler was found;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool TryGetSerialization(
        IGameRegistryKey key,
        [NotNullWhen(true)] out ISerialization? serialization);

    /// <summary>
    /// Serializes the specified definition.
    /// </summary>
    /// <param name="definition">The definition to serialize.</param>
    /// <param name="writer">The writer that receives the serialized values.</param>
    void Serialize(
        IDefinition definition,
        FormatWriterBase writer);

    /// <summary>
    /// Attempts to serialize the specified definition.
    /// </summary>
    /// <param name="definition">The definition to serialize.</param>
    /// <param name="writer">The writer that receives the serialized values.</param>
    /// <returns>
    /// <see langword="true"/> if serialization succeeded;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool TrySerialize(
        IDefinition definition,
        FormatWriterBase writer);

    /// <summary>
    /// Deserializes a definition using the specified name.
    /// </summary>
    /// <param name="name">The registered serialization name.</param>
    /// <param name="reader">The reader that provides the serialized values.</param>
    /// <returns>The deserialized definition.</returns>
    IDefinition Deserialize(
        string name,
        FormatReaderBase reader);

    /// <summary>
    /// Attempts to deserialize a definition using the specified name.
    /// </summary>
    /// <param name="name">The registered serialization name.</param>
    /// <param name="reader">The reader that provides the serialized values.</param>
    /// <param name="definition">
    /// When this method returns <see langword="true"/>, contains the
    /// deserialized definition; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if deserialization succeeded;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool TryDeserialize(
        string name,
        FormatReaderBase reader,
        [NotNullWhen(true)] out IDefinition? definition);

    /// <summary>
    /// Deserializes a definition using the specified key.
    /// </summary>
    /// <param name="key">The registry key.</param>
    /// <param name="reader">The reader that provides the serialized values.</param>
    /// <returns>The deserialized definition.</returns>
    IDefinition Deserialize(
        IGameRegistryKey key,
        FormatReaderBase reader);

    /// <summary>
    /// Attempts to deserialize a definition using the specified key.
    /// </summary>
    /// <param name="key">The registry key.</param>
    /// <param name="reader">The reader that provides the serialized values.</param>
    /// <param name="definition">
    /// When this method returns <see langword="true"/>, contains the
    /// deserialized definition; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if deserialization succeeded;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool TryDeserialize(
        IGameRegistryKey key,
        FormatReaderBase reader,
        [NotNullWhen(true)] out IDefinition? definition);
}
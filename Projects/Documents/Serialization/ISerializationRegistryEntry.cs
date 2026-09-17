namespace Sachssoft.Sasogine.Documents.Serialization;

/// <summary>
/// Represents a game registry entry with serialization support.
/// </summary>
public interface ISerializationRegistryEntry : IGameRegistryEntry
{
    /// <summary>
    /// Gets the serialization handler.
    /// </summary>
    ISerialization Serialization { get; }
}
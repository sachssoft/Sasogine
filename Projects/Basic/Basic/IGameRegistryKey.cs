namespace Sachssoft.Sasogine.Experimental;

/// <summary>
/// Represents a registry key that provides a serialization name.
/// </summary>
public interface IGameRegistryKey
{
    /// <summary>
    /// Gets the serialization name.
    /// </summary>
    string Name { get; }
}
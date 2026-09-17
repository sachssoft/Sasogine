namespace Sachssoft.Sasogine.Experimental;

/// <summary>
/// Represents a registry key that provides a string identifier.
/// </summary>
public interface IGameRegistryKey
{
    /// <summary>
    /// Gets the string identifier of the registry key.
    /// </summary>
    string Name { get; }
}
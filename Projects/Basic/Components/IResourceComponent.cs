namespace Sachssoft.Sasogine.Components;

/// <summary>
/// Defines a component that manages loadable resources.
/// </summary>
public interface IResourceComponent : IComponent
{
    /// <summary>
    /// Gets a value indicating whether the component resources are currently loaded.
    /// </summary>
    bool IsLoaded { get; }

    /// <summary>
    /// Loads the resources required by the component.
    /// </summary>
    void Load();

    /// <summary>
    /// Unloads the resources owned by the component.
    /// </summary>
    void Unload();
}
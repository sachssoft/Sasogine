namespace Sachssoft.Sasogine.Components;

/// <summary>
/// Provides a base implementation for components that manage loadable resources.
/// </summary>
public abstract class ResourceComponentBase : IResourceComponent
{
    /// <summary>
    /// Gets a value indicating whether the component resources are currently loaded.
    /// </summary>
    public bool IsLoaded { get; private set; }

    /// <summary>
    /// Loads the resources required by the component.
    /// </summary>
    public void Load()
    {
        if (IsLoaded)
            return;

        OnLoad();
        IsLoaded = true;
    }

    /// <summary>
    /// Unloads the resources owned by the component.
    /// </summary>
    public void Unload()
    {
        if (!IsLoaded)
            return;

        OnUnload();
        IsLoaded = false;
    }

    /// <summary>
    /// Called when the component resources should be loaded.
    /// </summary>
    protected virtual void OnLoad()
    {
    }

    /// <summary>
    /// Called when the component resources should be unloaded.
    /// </summary>
    protected virtual void OnUnload()
    {
    }
}
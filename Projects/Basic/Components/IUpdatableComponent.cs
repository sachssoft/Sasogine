using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Components;

/// <summary>
/// Defines a component that participates in the scene update cycle.
/// </summary>
public interface IUpdatableComponent : IComponent
{
    /// <summary>
    /// Updates the component using the specified scene update context.
    /// </summary>
    /// <param name="context">
    /// Provides information required for the current scene update cycle.
    /// </param>
    void Update(SceneUpdateContext context);
}
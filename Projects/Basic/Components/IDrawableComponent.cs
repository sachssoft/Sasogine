using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Components;

/// <summary>
/// Defines a component that participates in the scene drawing cycle.
/// </summary>
public interface IDrawableComponent : IComponent
{
    /// <summary>
    /// Draws the component using the specified scene drawing context.
    /// </summary>
    /// <param name="context">
    /// Provides information required for the current scene drawing cycle.
    /// </param>
    void Draw(SceneDrawContext context);
}
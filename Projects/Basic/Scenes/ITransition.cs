using Microsoft.Xna.Framework;
using System.Threading.Tasks;

namespace Sachssoft.Engine.Scenes;

/// <summary>
/// Defines an updateable and drawable operation that controls when a scene change can proceed.
/// </summary>
public interface ITransition
{
    /// <summary>
    /// Gets the task that represents the completion or failure of the transition.
    /// </summary>
    Task Completion { get; }

    /// <summary>
    /// Gets a value indicating whether the transition has completed successfully.
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// Advances the transition state by one frame.
    /// </summary>
    /// <param name="gameTime">The current frame timing information.</param>
    void Update(GameTime gameTime);

    /// <summary>
    /// Draws the transition for the specified scene view.
    /// </summary>
    /// <param name="context">The active scene rendering context.</param>
    void Draw(SceneDrawContext context);
}

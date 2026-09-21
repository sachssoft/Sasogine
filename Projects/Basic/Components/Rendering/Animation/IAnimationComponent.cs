using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Timing.Easing;

namespace Sachssoft.Sasogine.Components.Rendering;

/// <summary>
/// Defines a component that provides animation behavior.
/// </summary>
public interface IAnimationComponent : IResourceComponent
{
    /// <summary>
    /// Gets or sets the animation speed.
    /// </summary>
    float Speed { get; set; }

    /// <summary>
    /// Gets or sets the animation duration in milliseconds.
    /// </summary>
    int Duration { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the animation runs indefinitely.
    /// </summary>
    bool Infinite { get; set; }

    /// <summary>
    /// Gets or sets the delay in milliseconds before the animation starts.
    /// </summary>
    int Delay { get; set; }

    /// <summary>
    /// Gets or sets the optional easing function applied to the normalized
    /// animation progress.
    /// </summary>
    EasingBase? Easing { get; set; }

    /// <summary>
    /// Starts the animation at the specified position and rotation.
    /// </summary>
    /// <param name="position">The initial position of the animation.</param>
    /// <param name="rotation">The initial rotation in degrees.</param>
    void Start(Point2 position, float rotation);

    /// <summary>
    /// Resets the animation.
    /// </summary>
    void Reset();

    /// <summary>
    /// Toggles the paused state of the animation.
    /// </summary>
    void Pause();

    /// <summary>
    /// Calculates the current position contribution of the animation.
    /// </summary>
    /// <param name="elapsedTime">The elapsed time since the previous update.</param>
    /// <returns>The current position contribution.</returns>
    Vector2 AddPosition(float elapsedTime);

    /// <summary>
    /// Calculates the current rotation contribution of the animation.
    /// </summary>
    /// <param name="elapsedTime">The elapsed time since the previous update.</param>
    /// <returns>The current rotation contribution.</returns>
    float AddRotation(float elapsedTime);
}
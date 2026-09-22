using Microsoft.Xna.Framework;
using Sachssoft.Engine.Common;

namespace Sachssoft.Engine.Components.Rendering;

/// <summary>
/// Provides a position animation that moves an object back and forth
/// along a defined distance over time.
/// </summary>
public class PositionAnimation : AnimationComponent
{
    private float _progress;
    private float _direction = 1f;

    /// <summary>
    /// Gets or sets the distance applied by the animation.
    /// </summary>
    public Vector2 Distance { get; set; }

    /// <summary>
    /// Calculates the current position offset based on animation progress.
    /// </summary>
    /// <param name="elapsedTime">The elapsed time since the previous update.</param>
    /// <returns>The calculated position offset.</returns>
    protected override Vector2 AddPositionOverride(float elapsedTime)
    {
        float progressIncrement = (elapsedTime * Speed) / 100f;

        if (_progress >= 1f)
            _direction = -1f;
        else if (_progress <= 0f)
            _direction = 1f;

        _progress += progressIncrement * _direction;
        return Distance * _progress;
    }
    /// <summary>
    /// Resets the position animation progress and direction.
    /// </summary>
    protected override void OnReset()
    {
        _progress = 0f;
        _direction = 1f;
        base.OnReset();
    }

}

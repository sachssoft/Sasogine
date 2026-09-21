
namespace Sachssoft.Sasogine.Components.Rendering;

/// <summary>
/// Provides a rotation animation that continuously rotates an object over time.
/// </summary>
public class RotationAnimation : AnimationComponent
{
    private float _currentRotation;

    /// <summary>
    /// Calculates the current rotation angle based on elapsed time and animation speed.
    /// </summary>
    /// <param name="elapsedTime">The elapsed time since the previous update.</param>
    /// <returns>The current rotation angle in degrees.</returns>
    protected override float AddRotationOverride(float elapsedTime)
    {
        _currentRotation += elapsedTime * -Speed;

        if (_currentRotation >= 360f)
            _currentRotation -= 360f;
        else if (_currentRotation < 0f)
            _currentRotation += 360f;

        return _currentRotation;
    }
    /// <summary>
    /// Resets the accumulated rotation.
    /// </summary>
    protected override void OnReset()
    {
        _currentRotation = 0f;
        base.OnReset();
    }

}

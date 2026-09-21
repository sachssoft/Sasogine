namespace Sachssoft.Sasogine.Timing.Easing;

/// <summary>
/// Applies a back easing function that accelerates from the start with an initial overshoot in the opposite direction.
/// </summary>
public sealed class EaseInBack : EasingBase
{
    /// <summary>
    /// Calculates the eased value for the specified normalized progress.
    /// </summary>
    /// <param name="percent">
    /// The normalized progress, typically between 0.0 and 1.0.
    /// </param>
    /// <returns>The value produced by the easing function.</returns>
    public override float GetValue(float percent)
    {
        const float s = 1.70158f;
        return percent * percent * ((s + 1) * percent - s);
    }
}
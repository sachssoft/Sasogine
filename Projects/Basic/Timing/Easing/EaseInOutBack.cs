namespace Sachssoft.Engine.Timing.Easing;

/// <summary>
/// Applies a back easing function with overshoot behavior at both the beginning and end.
/// </summary>
public sealed class EaseInOutBack : EasingBase
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
        const float s = 1.70158f * 1.525f;
        percent *= 2;
        if (percent < 1)
            return 0.5f * (percent * percent * ((s + 1) * percent - s));
        percent -= 2;
        return 0.5f * (percent * percent * ((s + 1) * percent + s) + 2);
    }
}
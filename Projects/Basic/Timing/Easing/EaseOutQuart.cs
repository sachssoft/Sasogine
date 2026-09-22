namespace Sachssoft.Engine.Timing.Easing;

/// <summary>
/// Applies a quartic easing function that decelerates toward the end.
/// </summary>
public sealed class EaseOutQuart : EasingBase
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
        percent--;
        return 1 - (percent * percent * percent * percent);
    }
}
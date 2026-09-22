namespace Sachssoft.Engine.Timing.Easing;

/// <summary>
/// Applies a quintic easing function that accelerates at the beginning and decelerates toward the end.
/// </summary>
public sealed class EaseInOutQuint : EasingBase
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
        if (percent < 0.5f)
            return 16 * percent * percent * percent * percent * percent;
        percent--;
        return 1 + 16 * percent * percent * percent * percent * percent;
    }
}
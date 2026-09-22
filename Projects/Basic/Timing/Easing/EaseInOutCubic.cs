namespace Sachssoft.Engine.Timing.Easing;

/// <summary>
/// Applies a cubic easing function that accelerates at the beginning and decelerates toward the end.
/// </summary>
public sealed class EaseInOutCubic : EasingBase
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
        return percent < 0.5f
            ? 4 * percent * percent * percent
            : (percent - 1) * (2 * percent - 2) * (2 * percent - 2) + 1;
    }
}
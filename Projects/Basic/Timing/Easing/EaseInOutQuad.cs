namespace Sachssoft.Engine.Timing.Easing;

/// <summary>
/// Applies a quadratic easing function that accelerates at the beginning and decelerates toward the end.
/// </summary>
public sealed class EaseInOutQuad : EasingBase
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
            ? 2 * percent * percent
            : -1 + (4 - 2 * percent) * percent;
    }
}

namespace Sachssoft.Engine.Timing.Easing;

/// <summary>
/// Applies an exponential easing function that accelerates at the beginning and decelerates toward the end.
/// </summary>
public sealed class EaseInOutExpo : EasingBase
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
        if (percent == 0) return 0;
        if (percent == 1) return 1;
        if (percent < 0.5f)
            return 0.5f * float.Pow(2, 20 * percent - 10);
        return 1 - 0.5f * float.Pow(2, -20 * percent + 10);
    }
}
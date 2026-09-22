namespace Sachssoft.Engine.Timing.Easing;

/// <summary>
/// Applies a bounce easing function that produces bouncing behavior at the end.
/// </summary>
public sealed class EaseOutBounce : EasingBase
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
        if (percent < 1 / 2.75f)
        {
            return 7.5625f * percent * percent;
        }
        else if (percent < 2 / 2.75f)
        {
            percent -= 1.5f / 2.75f;
            return 7.5625f * percent * percent + 0.75f;
        }
        else if (percent < 2.5 / 2.75)
        {
            percent -= 2.25f / 2.75f;
            return 7.5625f * percent * percent + 0.9375f;
        }
        else
        {
            percent -= 2.625f / 2.75f;
            return 7.5625f * percent * percent + 0.984375f;
        }
    }
}
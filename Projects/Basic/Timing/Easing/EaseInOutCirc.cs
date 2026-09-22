namespace Sachssoft.Engine.Timing.Easing;

/// <summary>
/// Applies a circular easing function that accelerates at the beginning and decelerates toward the end.
/// </summary>
public sealed class EaseInOutCirc : EasingBase
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
            return 0.5f * (1 - float.Sqrt(1 - 4 * percent * percent));
        percent = percent * 2 - 1;
        return 0.5f * (float.Sqrt(1 - percent * percent) + 1);
    }
}
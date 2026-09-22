using System;

namespace Sachssoft.Engine.Timing.Easing;

/// <summary>
/// Applies an elastic easing function that oscillates while accelerating from the start.
/// </summary>
public sealed class EaseInElastic : EasingBase
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
        if (percent == 0 || percent == 1) return percent;
        return -float.Pow(2, 10 * (percent - 1)) * float.Sin((percent - 1.075f) * (2 * MathF.PI) / 0.3f);
    }
}
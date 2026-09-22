using System;

namespace Sachssoft.Engine.Timing.Easing;

/// <summary>
/// Applies an elastic easing function that oscillates at both the beginning and end.
/// </summary>
public sealed class EaseInOutElastic : EasingBase
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
        percent *= 2;
        if (percent < 1)
            return -0.5f * float.Pow(2, 10 * (percent - 1)) * float.Sin((percent - 1.1125f) * (2 * MathF.PI) / 0.45f);
        return 0.5f * float.Pow(2, -10 * (percent - 1)) * float.Sin((percent - 1.1125f) * (2 * MathF.PI) / 0.45f) + 1;
    }
}
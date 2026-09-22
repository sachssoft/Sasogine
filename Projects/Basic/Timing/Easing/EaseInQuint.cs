namespace Sachssoft.Engine.Timing.Easing;

/// <summary>
/// Applies a quintic easing function that accelerates from the start.
/// </summary>
public sealed class EaseInQuint : EasingBase
{
    /// <summary>
    /// Calculates the eased value for the specified normalized progress.
    /// </summary>
    /// <param name="percent">
    /// The normalized progress, typically between 0.0 and 1.0.
    /// </param>
    /// <returns>The value produced by the easing function.</returns>
    public override float GetValue(float percent) => percent * percent * percent * percent * percent;
}

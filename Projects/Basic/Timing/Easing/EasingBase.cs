namespace Sachssoft.Engine.Timing.Easing;

/// <summary>
/// Provides the base implementation for easing functions that map normalized
/// progress to a modified value.
/// </summary>
public abstract class EasingBase
{
    /// <summary>
    /// Calculates the eased value for the specified normalized progress.
    /// </summary>
    /// <param name="percent">
    /// The normalized progress, typically between 0.0 and 1.0.
    /// </param>
    /// <returns>The value produced by the easing function.</returns>
    public abstract float GetValue(float percent);
}

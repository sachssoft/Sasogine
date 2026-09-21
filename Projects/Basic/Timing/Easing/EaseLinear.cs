namespace Sachssoft.Sasogine.Timing.Easing;

/// <summary>
/// Applies linear easing without modifying the normalized progress.
/// </summary>
public sealed class EaseLinear : EasingBase
{
    /// <summary>
    /// Calculates the eased value for the specified normalized progress.
    /// </summary>
    /// <param name="percent">
    /// The normalized progress, typically between 0.0 and 1.0.
    /// </param>
    /// <returns>The value produced by the easing function.</returns>
    public override float GetValue(float percent) => percent;
}

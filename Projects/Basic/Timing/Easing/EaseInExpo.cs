namespace Sachssoft.Engine.Timing.Easing;

/// <summary>
/// Applies an exponential easing function that accelerates from the start.
/// </summary>
public sealed class EaseInExpo : EasingBase
{
    /// <summary>
    /// Calculates the eased value for the specified normalized progress.
    /// </summary>
    /// <param name="percent">
    /// The normalized progress, typically between 0.0 and 1.0.
    /// </param>
    /// <returns>The value produced by the easing function.</returns>
    public override float GetValue(float percent) =>
        percent <= 0 ? 0 : float.Pow(2, 10 * (percent - 1));
}

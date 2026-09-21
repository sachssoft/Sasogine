namespace Sachssoft.Sasogine.Timing.Easing;

/// <summary>
/// Applies a bounce easing function that produces bouncing behavior at the beginning.
/// </summary>
public sealed class EaseInBounce : EasingBase
{
    private readonly EaseOutBounce _out = new();

    /// <summary>
    /// Calculates the eased value for the specified normalized progress.
    /// </summary>
    /// <param name="percent">
    /// The normalized progress, typically between 0.0 and 1.0.
    /// </param>
    /// <returns>The value produced by the easing function.</returns>
    public override float GetValue(float percent) =>
        1 - _out.GetValue(1 - percent);
}
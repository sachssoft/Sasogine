namespace Sachssoft.Sasogine.Timing.Easing;

/// <summary>
/// Applies a bounce easing function that produces bouncing behavior at both the beginning and end.
/// </summary>
public sealed class EaseInOutBounce : EasingBase
{
    private readonly EaseInBounce _in = new();
    private readonly EaseOutBounce _out = new();

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
            return 0.5f * _in.GetValue(percent * 2);
        return 0.5f * _out.GetValue(percent * 2 - 1) + 0.5f;
    }
}
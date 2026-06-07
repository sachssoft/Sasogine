namespace Sachssoft.Sasogine.Components.Rendering.Animation.Timings;

public sealed class EaseInOutCubic : AnimationTimingBase
{
    public override float GetValue(float percent)
    {
        return percent < 0.5f
            ? 4 * percent * percent * percent
            : (percent - 1) * (2 * percent - 2) * (2 * percent - 2) + 1;
    }
}
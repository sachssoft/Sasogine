namespace Sachssoft.Sasogine.Components.Rendering.Animation.Timings;

public sealed class EaseInOutQuart : AnimationTimingBase
{
    public override float GetValue(float percent)
    {
        if (percent < 0.5f)
            return 8 * percent * percent * percent * percent;
        percent--;
        return 1 - 8 * percent * percent * percent * percent;
    }
}
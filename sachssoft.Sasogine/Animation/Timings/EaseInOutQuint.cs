namespace Sachssoft.Sasogine.Animation.Timings;

public sealed class EaseInOutQuint : AnimationTimingBase
{
    public override float GetValue(float percent)
    {
        if (percent < 0.5f)
            return 16 * percent * percent * percent * percent * percent;
        percent--;
        return 1 + 16 * percent * percent * percent * percent * percent;
    }
}
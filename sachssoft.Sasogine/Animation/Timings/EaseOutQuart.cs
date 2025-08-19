namespace Sachssoft.Sasogine.Animation.Timings;

public sealed class EaseOutQuart : AnimationTimingBase
{
    public override float GetValue(float percent)
    {
        percent--;
        return 1 - (percent * percent * percent * percent);
    }
}
namespace sachssoft.Sasogine.Animation.Timings;

public sealed class EaseOutQuint : AnimationTimingBase
{
    public override float GetValue(float percent)
    {
        percent--;
        return 1 + percent * percent * percent * percent * percent;
    }
}
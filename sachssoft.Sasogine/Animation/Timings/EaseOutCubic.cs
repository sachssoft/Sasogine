namespace sachssoft.Sasogine.Animation.Timings;

public sealed class EaseOutCubic : AnimationTimingBase
{
    public override float GetValue(float percent)
    {
        percent -= 1;
        return percent * percent * percent + 1;
    }
}

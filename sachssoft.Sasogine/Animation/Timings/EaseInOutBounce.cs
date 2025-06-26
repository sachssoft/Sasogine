using System;

namespace sachssoft.Sasogine.Animation.Timings;

public sealed class EaseInOutBounce : AnimationTimingBase
{
    private readonly EaseInBounce _in = new();
    private readonly EaseOutBounce _out = new();

    public override float GetValue(float percent)
    {
        if (percent < 0.5f)
            return 0.5f * _in.GetValue(percent * 2);
        return 0.5f * _out.GetValue(percent * 2 - 1) + 0.5f;
    }
}
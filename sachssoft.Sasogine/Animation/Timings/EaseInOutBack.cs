using System;

namespace sachssoft.Sasogine.Animation.Timings;

public sealed class EaseInOutBack : AnimationTimingBase
{
    public override float GetValue(float percent)
    {
        const float s = 1.70158f * 1.525f;
        percent *= 2;
        if (percent < 1)
            return 0.5f * (percent * percent * ((s + 1) * percent - s));
        percent -= 2;
        return 0.5f * (percent * percent * ((s + 1) * percent + s) + 2);
    }
}
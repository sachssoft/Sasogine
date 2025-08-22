using System;

namespace Sachssoft.Sasogine.Animation.Timings;

public sealed class EaseOutCirc : AnimationTimingBase
{
    public override float GetValue(float percent)
    {
        percent--;
        return (float)Math.Sqrt(1 - percent * percent);
    }
}
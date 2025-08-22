using System;

namespace Sachssoft.Sasogine.Animation.Timings;

public sealed class EaseOutElastic : AnimationTimingBase
{
    public override float GetValue(float percent)
    {
        if (percent == 0 || percent == 1) return percent;
        return (float)Math.Pow(2, -10 * percent) * (float)Math.Sin((percent - 0.075f) * (2 * MathF.PI) / 0.3f) + 1;
    }
}
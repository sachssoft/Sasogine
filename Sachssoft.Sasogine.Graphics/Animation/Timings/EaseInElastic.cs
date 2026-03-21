using System;

namespace Sachssoft.Sasogine.Animation.Timings;

public sealed class EaseInElastic : AnimationTimingBase
{
    public override float GetValue(float percent)
    {
        if (percent == 0 || percent == 1) return percent;
        return -(float)Math.Pow(2, 10 * (percent - 1)) * (float)Math.Sin((percent - 1.075f) * (2 * MathF.PI) / 0.3f);
    }
}
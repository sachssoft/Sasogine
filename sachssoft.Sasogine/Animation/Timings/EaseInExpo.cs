using System;

namespace sachssoft.Sasogine.Animation.Timings;

public sealed class EaseInExpo : AnimationTimingBase
{
    public override float GetValue(float percent) =>
        percent <= 0 ? 0 : (float)Math.Pow(2, 10 * (percent - 1));
}

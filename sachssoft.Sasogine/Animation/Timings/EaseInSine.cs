using System;

namespace sachssoft.Sasogine.Animation.Timings;

public sealed class EaseInSine : AnimationTimingBase
{
    public override float GetValue(float percent) =>
        (float)(1 - Math.Cos(percent * Math.PI / 2));
}

using System;

namespace Sachssoft.Sasogine.Animation.Timings;

public sealed class EaseOutSine : AnimationTimingBase
{
    public override float GetValue(float percent) =>
        (float)Math.Sin(percent * Math.PI / 2);
}

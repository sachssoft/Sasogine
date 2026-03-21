using System;

namespace Sachssoft.Sasogine.Animation.Timings;

public sealed class EaseInOutSine : AnimationTimingBase
{
    public override float GetValue(float percent) =>
        (float)(-(Math.Cos(Math.PI * percent) - 1) / 2);
}

using System;

namespace Sachssoft.Sasogine.Animation.Timings;

public sealed class EaseOutExpo : AnimationTimingBase
{
    public override float GetValue(float percent) =>
        percent >= 1 ? 1 : 1 - (float)Math.Pow(2, -10 * percent);
}
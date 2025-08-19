using System;

namespace Sachssoft.Sasogine.Animation.Timings;

public sealed class EaseInCirc : AnimationTimingBase
{
    public override float GetValue(float percent) =>
        1 - (float)Math.Sqrt(1 - percent * percent);
}
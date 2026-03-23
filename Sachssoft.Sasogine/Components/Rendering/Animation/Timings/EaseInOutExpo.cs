using Sachssoft.Sasogine.Components.Rendering.Animation;
using System;

namespace Sachssoft.Sasogine.Components.Rendering.Animation.Timings;

public sealed class EaseInOutExpo : AnimationTimingBase
{
    public override float GetValue(float percent)
    {
        if (percent == 0) return 0;
        if (percent == 1) return 1;
        if (percent < 0.5f)
            return 0.5f * float.Pow(2, 20 * percent - 10);
        return 1 - 0.5f * float.Pow(2, -20 * percent + 10);
    }
}
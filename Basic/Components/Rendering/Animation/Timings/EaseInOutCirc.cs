using Sachssoft.Sasogine.Components.Rendering.Animation;
using System;

namespace Sachssoft.Sasogine.Components.Rendering.Animation.Timings;

public sealed class EaseInOutCirc : AnimationTimingBase
{
    public override float GetValue(float percent)
    {
        if (percent < 0.5f)
            return 0.5f * (1 - float.Sqrt(1 - 4 * percent * percent));
        percent = percent * 2 - 1;
        return 0.5f * (float.Sqrt(1 - percent * percent) + 1);
    }
}
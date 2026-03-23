using Sachssoft.Sasogine.Components.Rendering.Animation;
using System;

namespace Sachssoft.Sasogine.Components.Rendering.Animation.Timings;

public sealed class EaseInOutElastic : AnimationTimingBase
{
    public override float GetValue(float percent)
    {
        if (percent == 0 || percent == 1) return percent;
        percent *= 2;
        if (percent < 1)
            return -0.5f * float.Pow(2, 10 * (percent - 1)) * float.Sin((percent - 1.1125f) * (2 * MathF.PI) / 0.45f);
        return 0.5f * float.Pow(2, -10 * (percent - 1)) * float.Sin((percent - 1.1125f) * (2 * MathF.PI) / 0.45f) + 1;
    }
}
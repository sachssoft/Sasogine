using Sachssoft.Sasogine.Components.Rendering.Animation;

namespace Sachssoft.Sasogine.Components.Rendering.Animation.Timings;

public sealed class EaseInOutQuad : AnimationTimingBase
{
    public override float GetValue(float percent)
    {
        return percent < 0.5f
            ? 2 * percent * percent
            : -1 + (4 - 2 * percent) * percent;
    }
}

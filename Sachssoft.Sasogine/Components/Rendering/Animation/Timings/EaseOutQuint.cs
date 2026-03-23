using Sachssoft.Sasogine.Components.Rendering.Animation;

namespace Sachssoft.Sasogine.Components.Rendering.Animation.Timings;

public sealed class EaseOutQuint : AnimationTimingBase
{
    public override float GetValue(float percent)
    {
        percent--;
        return 1 + percent * percent * percent * percent * percent;
    }
}
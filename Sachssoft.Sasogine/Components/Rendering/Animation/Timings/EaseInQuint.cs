using Sachssoft.Sasogine.Components.Rendering.Animation;

namespace Sachssoft.Sasogine.Components.Rendering.Animation.Timings;

public sealed class EaseInQuint : AnimationTimingBase
{
    public override float GetValue(float percent) => percent * percent * percent * percent * percent;
}

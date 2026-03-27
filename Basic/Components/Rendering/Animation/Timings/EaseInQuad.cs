using Sachssoft.Sasogine.Components.Rendering.Animation;

namespace Sachssoft.Sasogine.Components.Rendering.Animation.Timings;

public sealed class EaseInQuad : AnimationTimingBase
{
    public override float GetValue(float percent) => percent * percent;
}

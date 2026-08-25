namespace Sachssoft.Sasogine.Components.Rendering.Animation.Timings;

public sealed class EaseOutQuad : AnimationTimingBase
{
    public override float GetValue(float percent) => percent * (2 - percent);
}

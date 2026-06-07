namespace Sachssoft.Sasogine.Components.Rendering.Animation.Timings;

public sealed class EaseInQuart : AnimationTimingBase
{
    public override float GetValue(float percent) => percent * percent * percent * percent;
}
namespace Sachssoft.Sasogine.Animation.Timings;

public sealed class EaseInQuad : AnimationTimingBase
{
    public override float GetValue(float percent) => percent * percent;
}

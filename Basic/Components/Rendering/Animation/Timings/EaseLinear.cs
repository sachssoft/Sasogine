namespace Sachssoft.Sasogine.Components.Rendering.Animation.Timings;

public sealed class EaseLinear : AnimationTimingBase
{
    public override float GetValue(float percent) => percent;
}

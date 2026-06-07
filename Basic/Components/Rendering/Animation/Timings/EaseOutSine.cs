namespace Sachssoft.Sasogine.Components.Rendering.Animation.Timings;

public sealed class EaseOutSine : AnimationTimingBase
{
    public override float GetValue(float percent) =>
        float.Sin(percent * float.Pi / 2);
}

namespace Sachssoft.Sasogine.Components.Rendering.Animation.Timings;

public sealed class EaseOutExpo : AnimationTimingBase
{
    public override float GetValue(float percent) =>
        percent >= 1 ? 1 : 1 - float.Pow(2, -10 * percent);
}
namespace Sachssoft.Sasogine.Components.Rendering.Animation.Timings;

public sealed class EaseInCirc : AnimationTimingBase
{
    public override float GetValue(float percent) =>
        1 - float.Sqrt(1 - percent * percent);
}
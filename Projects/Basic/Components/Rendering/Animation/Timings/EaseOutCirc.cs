namespace Sachssoft.Sasogine.Components.Rendering.Animation.Timings;

public sealed class EaseOutCirc : AnimationTimingBase
{
    public override float GetValue(float percent)
    {
        percent--;
        return float.Sqrt(1 - percent * percent);
    }
}
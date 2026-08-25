namespace Sachssoft.Sasogine.Components.Rendering.Animation.Timings;

public sealed class EaseInBack : AnimationTimingBase
{
    public override float GetValue(float percent)
    {
        const float s = 1.70158f;
        return percent * percent * ((s + 1) * percent - s);
    }
}
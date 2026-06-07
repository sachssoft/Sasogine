namespace Sachssoft.Sasogine.Components.Rendering.Animation.Timings;

public sealed class EaseOutBounce : AnimationTimingBase
{
    public override float GetValue(float percent)
    {
        if (percent < 1 / 2.75f)
        {
            return 7.5625f * percent * percent;
        }
        else if (percent < 2 / 2.75f)
        {
            percent -= 1.5f / 2.75f;
            return 7.5625f * percent * percent + 0.75f;
        }
        else if (percent < 2.5 / 2.75)
        {
            percent -= 2.25f / 2.75f;
            return 7.5625f * percent * percent + 0.9375f;
        }
        else
        {
            percent -= 2.625f / 2.75f;
            return 7.5625f * percent * percent + 0.984375f;
        }
    }
}
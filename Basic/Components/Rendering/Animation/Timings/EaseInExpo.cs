namespace Sachssoft.Sasogine.Components.Rendering.Animation.Timings;

public sealed class EaseInExpo : AnimationTimingBase
{
    public override float GetValue(float percent) =>
        percent <= 0 ? 0 : float.Pow(2, 10 * (percent - 1));
}

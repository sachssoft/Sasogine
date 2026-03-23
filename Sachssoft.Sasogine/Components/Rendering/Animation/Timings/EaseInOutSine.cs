using Sachssoft.Sasogine.Components.Rendering.Animation;
using System;

namespace Sachssoft.Sasogine.Components.Rendering.Animation.Timings;

public sealed class EaseInOutSine : AnimationTimingBase
{
    public override float GetValue(float percent) =>
        (float)(-(float.Cos(float.Pi * percent) - 1) / 2);
}

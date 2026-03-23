using Sachssoft.Sasogine.Components.Rendering.Animation;
using System;

namespace Sachssoft.Sasogine.Components.Rendering.Animation.Timings;

public sealed class EaseInSine : AnimationTimingBase
{
    public override float GetValue(float percent) =>
        (float)(1 - float.Cos(percent * float.Pi / 2));
}

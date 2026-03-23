using Sachssoft.Sasogine.Components.Rendering.Animation;
using System;

namespace Sachssoft.Sasogine.Components.Rendering.Animation.Timings;

public sealed class EaseInBounce : AnimationTimingBase
{
    private readonly EaseOutBounce _out = new();

    public override float GetValue(float percent) =>
        1 - _out.GetValue(1 - percent);
}
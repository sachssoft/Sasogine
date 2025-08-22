namespace Sachssoft.Sasogine.Animation;

public abstract class AnimationTimingBase
{
    // Berechnet den Animationswert basierend auf dem Prozentsatz der verstrichenen Zeit (0.0 bis 1.0)
    public abstract float GetValue(float percent);
}
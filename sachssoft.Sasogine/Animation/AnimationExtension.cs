using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Animation;

public static class AnimationExtension
{
    public static void StartAnimation(this IAnimatable animatable)
    {
        if (animatable.Animations != null && animatable.Animations.Count > 0)
        {
            foreach (var animation in animatable.Animations)
            {
                animation.Start(animatable.Position, animatable.Rotation);
            }
        }
        else
        {
            // Optional: Log a message or handle the case when there are no animations
        }
    }

    public static void ResetAnimation(this IAnimatable animatable)
    {
        if (animatable.Animations != null && animatable.Animations.Count > 0)
        {
            foreach (var animation in animatable.Animations)
            {
                animation.Reset();
            }
        }
    }

    public static void UpdateAnimation(this IAnimatable animatable, GameContext context)
    {
        var position = animatable.StartPosition;
        var rotation = animatable.StartRotation;

        if (animatable.Animations != null && animatable.Animations.Count > 0)
        {
            foreach (var animation in animatable.Animations)
            {
                position += animation.AddPosition(context.ElapsedTimeInSeconds);
                rotation += MathHelper.ToRadians(animation.AddRotationDegree(context.ElapsedTimeInSeconds));
            }

            animatable.OnAnimated(animatable.Position, position, animatable.Rotation, rotation);
        }
    }
}
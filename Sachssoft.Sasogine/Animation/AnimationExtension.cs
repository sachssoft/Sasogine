using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Animation
{
    /// <summary>
    /// Provides extension methods for objects implementing <see cref="IAnimatable"/>
    /// to control animations, including starting, resetting, and updating them.
    /// </summary>
    public static class AnimationExtension
    {
        /// <summary>
        /// Starts all animations of the specified <see cref="IAnimatable"/> object.
        /// Initializes each animation with the current position and rotation.
        /// </summary>
        /// <param name="animatable">The animatable object whose animations will be started.</param>
        public static void StartAnimation(this IAnimatable animatable)
        {
            if (animatable.Animations != null && animatable.Animations.Count > 0)
            {
                foreach (var animation in animatable.Animations)
                {
                    animation.Start(animatable.WorldPosition, animatable.WorldRotation);
                }
            }
        }

        /// <summary>
        /// Resets all animations of the specified <see cref="IAnimatable"/> object.
        /// Resets timers and internal states of each animation.
        /// </summary>
        /// <param name="animatable">The animatable object whose animations will be reset.</param>
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

        /// <summary>
        /// Updates all animations of the specified <see cref="IAnimatable"/> object.
        /// Calculates the cumulative position and rotation based on elapsed time
        /// and invokes <see cref="IAnimatable.OnAnimated"/> with updated values.
        /// </summary>
        /// <param name="animatable">The animatable object to update.</param>
        /// <param name="context">The current game context providing elapsed time information.</param>
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

                animatable.OnAnimated(animatable.WorldPosition, position, animatable.WorldRotation, rotation);
            }
        }
    }
}

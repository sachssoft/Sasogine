namespace Sachssoft.Sasogine.Animation
{
    /// <summary>
    /// Base class for defining timing functions for animations.
    /// Provides a way to map a normalized progress (0.0 to 1.0) to a modified value.
    /// </summary>
    public abstract class AnimationTimingBase
    {
        /// <summary>
        /// Calculates the animation value based on the normalized progress.
        /// </summary>
        /// <param name="percent">
        /// The normalized progress of the animation, typically between 0.0 (start) and 1.0 (end).
        /// </param>
        /// <returns>
        /// The calculated value after applying the timing function.
        /// </returns>
        public abstract float GetValue(float percent);
    }
}

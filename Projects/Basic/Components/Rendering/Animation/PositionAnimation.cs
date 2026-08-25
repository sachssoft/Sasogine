using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Components.Rendering.Animation
{
    /// <summary>
    /// Provides a position animation that moves an object back and forth
    /// along a defined distance over time.
    ///
    /// The animation automatically reverses direction when reaching the
    /// start or end position.
    /// </summary>
    public class PositionAnimation : AnimationBase<PositionAnimationDefinition>
    {
        private float _progress = 0f;
        private float _direction = 1f;

        /// <summary>
        /// Initializes a new instance of the <see cref="PositionAnimation"/> class
        /// using a default animation definition.
        /// </summary>
        public PositionAnimation() : base(new PositionAnimationDefinition())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PositionAnimation"/> class.
        /// </summary>
        /// <param name="definition">
        /// The definition containing animation configuration data.
        /// </param>
        public PositionAnimation(PositionAnimationDefinition definition) : base(definition)
        {
        }

        /// <summary>
        /// Creates a default position animation definition.
        /// </summary>
        /// <returns>
        /// A new <see cref="PositionAnimationDefinition"/> instance.
        /// </returns>
        protected override PositionAnimationDefinition ResolveDefinition()
        {
            return new PositionAnimationDefinition();
        }

        /// <summary>
        /// Calculates the current position offset based on animation progress.
        /// The movement is interpolated between the start and target distance
        /// and reverses direction at the limits.
        /// </summary>
        /// <param name="elapsedTime">
        /// The elapsed time since the previous update in milliseconds.
        /// </param>
        /// <returns>
        /// The calculated position offset relative to the animation start position.
        /// </returns>
        protected override Vector2 AddPositionOverride(float elapsedTime)
        {
            // Calculate progress increment based on speed and elapsed time
            float progressIncrement = (elapsedTime * Definition.Speed) / 100f;

            // Reverse direction if progress reaches boundaries
            if (_progress >= 1f)
            {
                _direction = -1f;
            }
            else if (_progress <= 0f)
            {
                _direction = 1f;
            }

            // Update progress
            _progress += progressIncrement * _direction;

            // Calculate the new position based on distance and progress
            Vector2 newPosition = Definition.Distance * _progress;

            return newPosition;
        }
    }
}

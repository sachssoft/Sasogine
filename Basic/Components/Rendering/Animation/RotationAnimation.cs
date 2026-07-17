namespace Sachssoft.Sasogine.Components.Rendering.Animation
{
    /// <summary>
    /// Provides a rotation animation that continuously rotates an object over time.
    ///
    /// The rotation speed and direction are defined by the animation definition.
    /// The resulting angle is normalized to the range of 0 to 360 degrees.
    /// </summary>
    public class RotationAnimation : AnimationBase<RotationAnimationDefinition>
    {
        private float _currentRotation;

        /// <summary>
        /// Initializes a new instance of the <see cref="RotationAnimation"/> class
        /// using a default rotation animation definition.
        /// </summary>
        public RotationAnimation() : base(new RotationAnimationDefinition())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RotationAnimation"/> class.
        /// </summary>
        /// <param name="definition">
        /// The definition containing rotation speed and animation settings.
        /// </param>
        public RotationAnimation(RotationAnimationDefinition definition) : base(definition)
        {
        }

        /// <summary>
        /// Creates a default rotation animation definition.
        /// </summary>
        /// <returns>
        /// A new <see cref="RotationAnimationDefinition"/> instance.
        /// </returns>
        protected override RotationAnimationDefinition ResolveDefinition()
        {
            return new RotationAnimationDefinition();
        }

        /// <summary>
        /// Calculates the current rotation angle based on elapsed time and rotation speed.
        /// The returned value is normalized to the range 0 to 360 degrees.
        /// </summary>
        /// <param name="elapsedTime">
        /// The elapsed time since the previous update in milliseconds.
        /// </param>
        /// <returns>
        /// The current rotation angle in degrees.
        /// </returns>
        protected override float AddRotationOverride(float elapsedTime)
        {
            // Update rotation based on elapsed time and speed
            _currentRotation += elapsedTime * -Definition.Speed;

            // Keep rotation in range [0, 360)
            if (_currentRotation >= 360f)
            {
                _currentRotation -= 360f;
            }
            else if (_currentRotation < 0f)
            {
                _currentRotation += 360f;
            }

            return _currentRotation;
        }
    }
}

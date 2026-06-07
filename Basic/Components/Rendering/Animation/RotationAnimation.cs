namespace Sachssoft.Sasogine.Components.Rendering.Animation
{
    /// <summary>
    /// Animates a rotation over time in degrees.
    /// </summary>
    public class RotationAnimation : AnimationBase<IRotationAnimationDefinition>
    {
        private float _speed;
        private float _currentRotation;

        public override void ApplyDefinition()
        {
            base.ApplyDefinition();

            _speed = Definition.Speed;
        }

        public override void ApplyDefinitionChange(string? key)
        {
            base.ApplyDefinitionChange(key);

            switch (key)
            {
                case nameof(IRotationAnimationDefinition.Speed):
                    _speed = Definition.Speed;
                    break;
            }
        }

        /// <summary>
        /// Calculates the current rotation based on elapsed time and speed.
        /// The rotation loops continuously within the range [0, 360) degrees.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time since the last update in milliseconds.</param>
        /// <returns>Current rotation in degrees.</returns>
        protected override float AddRotationOverride(float elapsedTime)
        {
            // Update rotation based on elapsed time and speed
            _currentRotation += elapsedTime * -_speed;

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

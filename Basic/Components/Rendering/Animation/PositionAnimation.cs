using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Components.Rendering.Animation
{
    /// <summary>
    /// Animates a position over time, moving back and forth along a specified distance.
    /// </summary>
    public class PositionAnimation : AnimationBase<PositionAnimationDefinition>
    {
        private float _speed;
        private Vector2 _distance;
        private float _progress = 0f;
        private float _direction = 1f;

        protected override PositionAnimationDefinition ResolveDefinition()
        {
            return new PositionAnimationDefinition();
        }

        public override void ApplyDefinition()
        {
            base.ApplyDefinition();

            _speed = Definition.Speed;
            _distance = Definition.Distance;
        }

        public override void ApplyDefinitionChange(string? key)
        {
            base.ApplyDefinitionChange(key);

            switch (key)
            {
                case nameof(PositionAnimationDefinition.Speed):
                    _speed = Definition.Speed;
                    break;
                case nameof(PositionAnimationDefinition.Distance):
                    _distance = Definition.Distance;
                    break;
            }
        }

        /// <summary>
        /// Overrides the position calculation for this animation.
        /// Updates the progress and calculates the new position based on distance.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time since the last update in milliseconds.</param>
        /// <returns>New position vector relative to the start position.</returns>
        protected override Vector2 AddPositionOverride(float elapsedTime)
        {
            // Calculate progress increment based on speed and elapsed time
            float progressIncrement = (elapsedTime * _speed) / 100f;

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
            Vector2 newPosition = _distance * _progress;

            return newPosition;
        }
    }
}

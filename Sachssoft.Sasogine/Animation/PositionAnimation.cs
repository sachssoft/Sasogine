using Microsoft.Xna.Framework;
using Sachssoft.Inspection;
using Sachssoft.Inspection.Descriptors;

namespace Sachssoft.Sasogine.Animation
{
    /// <summary>
    /// Animates a position over time, moving back and forth along a specified distance.
    /// </summary>
    public class PositionAnimation : AnimationBase
    {
        private float _progress = 0f;
        private float _direction = 1f;

        /// <summary>
        /// Overrides the position calculation for this animation.
        /// Updates the progress and calculates the new position based on distance.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time since the last update in milliseconds.</param>
        /// <returns>New position vector relative to the start position.</returns>
        protected override Vector2 AddPositionOverride(float elapsedTime)
        {
            // Calculate progress increment based on speed and elapsed time
            float progressIncrement = (elapsedTime * Speed) / 100f;

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
            Vector2 newPosition = Distance * _progress;

            return newPosition;
        }

        #region Properties

        /// <summary>
        /// Defines the distance the animation moves in 2D space.
        /// </summary>
        public static readonly IProperty DistanceProperty =
            new StoredProperty<PositionAnimation, Vector2>(
                nameof(Distance),
                category: Sachssoft.Sasogine.Observables.PropertyCategories.Animation);

        /// <summary>
        /// Gets or sets the distance vector along which the animation moves.
        /// </summary>
        public virtual Vector2 Distance
        {
            get => GetValue<Vector2>(DistanceProperty);
            set => SetValue(DistanceProperty, value);
        }

        #endregion
    }
}

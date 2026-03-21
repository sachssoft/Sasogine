using System;

namespace Sachssoft.Sasogine.Animation
{
    /// <summary>
    /// Animates a rotation over time in degrees.
    /// </summary>
    public class RotationAnimation : AnimationBase
    {
        private float _currentRotation;

        /// <summary>
        /// Calculates the current rotation based on elapsed time and speed.
        /// The rotation loops continuously within the range [0, 360) degrees.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time since the last update in milliseconds.</param>
        /// <returns>Current rotation in degrees.</returns>
        protected override float AddRotationDegreeOverride(float elapsedTime)
        {
            // Update rotation based on elapsed time and speed
            _currentRotation += elapsedTime * -Speed;

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

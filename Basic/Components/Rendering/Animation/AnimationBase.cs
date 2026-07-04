using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Sasogine.Components.Rendering.Animation
{
    /// <summary>
    /// Represents the base class for all animations.
    /// Provides timing, position, rotation, and progress management.
    /// Supports pausing, resetting, and infinite or delayed animations.
    /// </summary>
    public abstract class AnimationBase<TDefinition> : ResourceComponentBase<TDefinition>, IAnimationComponent
        where TDefinition : AnimationDefinition, new()
    {
        private int _duration;
        private bool _infinite;
        private int _delay;
        private Vector2 _startPosition;
        private float _startRotation;
        private bool _pause;
        private long _startTicks;

        /// <summary>
        /// Starts the animation at the specified position and rotation.
        /// Resets the timer and calls <see cref="OnStart"/>.
        /// </summary>
        /// <param name="position">The initial position of the animation.</param>
        /// <param name="rotation">The initial rotation in degrees.</param>
        public void Start(Vector2 position, float rotation)
        {
            _startPosition = position;
            _startRotation = rotation;

            StartTimer();
            OnStart();
        }

        /// <summary>
        /// Called immediately when the animation starts.
        /// Override to add custom start logic.
        /// </summary>
        protected virtual void OnStart() { }

        /// <summary>
        /// Resets the animation timer and calls <see cref="OnReset"/>.
        /// </summary>
        public void Reset()
        {
            StartTimer();
            OnReset();
        }

        /// <summary>
        /// Called immediately when the animation is reset.
        /// Override to implement custom reset behavior.
        /// </summary>
        protected virtual void OnReset() { }

        /// <summary>
        /// Pauses or resumes the animation. Calling multiple times toggles the state.
        /// </summary>
        public void Pause() => _pause = !_pause;

        /// <summary>
        /// Returns the current animation position based on elapsed time.
        /// Returns the initial position if paused or not yet started.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time since last update in milliseconds.</param>
        public Vector2 AddPosition(float elapsedTime) => (!_pause && AllowUpdate())
            ? AddPositionOverride(elapsedTime)
            : _startPosition;

        /// <summary>
        /// Override this method to calculate position updates for the animation.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time since last update in milliseconds.</param>
        /// <returns>New position vector.</returns>
        protected virtual Vector2 AddPositionOverride(float elapsedTime) => Vector2.Zero;

        /// <summary>
        /// Returns the current rotation in radians based on elapsed time.
        /// Returns the initial rotation if paused or not yet started.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time since last update in milliseconds.</param>
        public float AddRotation(float elapsedTime) => (!_pause && AllowUpdate())
            ? AddRotationOverride(elapsedTime)
            : _startRotation;

        /// <summary>
        /// Override this method to calculate rotation updates for the animation.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time since last update in milliseconds.</param>
        /// <returns>New rotation in radians.</returns>
        protected virtual float AddRotationOverride(float elapsedTime) => 0f;

        /// <summary>
        /// Returns the initial position of the animation.
        /// </summary>
        protected Vector2 StartPosition => _startPosition;

        /// <summary>
        /// Returns the initial rotation of the animation in degrees.
        /// </summary>
        protected float StartRotation => _startRotation;

        /// <summary>
        /// Calculates the progress of the animation as a value between 0 and 1.
        /// Returns 0 if not started, 1 if finished or infinite.
        /// </summary>
        /// <returns>Normalized progress value (0..1).</returns>
        protected float GetProgress()
        {
            long now = Environment.TickCount64;

            if (now < _startTicks)
                return 0f;

            if (_infinite)
                return 1f;

            return float.Clamp((float)(now - _startTicks) / _duration, 0f, 1f);
        }

        protected override void ConfigureFromDefinition()
        {
            base.ConfigureFromDefinition();

            _duration = Definition.Duration;
            _infinite = Definition.Infinite;
            _delay = Definition.Delay;
        }

        private void StartTimer() => _startTicks = Environment.TickCount64 + _delay;

        private bool AllowUpdate()
        {
            long now = Environment.TickCount64;

            if (now < _startTicks)
                return false;

            if (_infinite)
                return true;

            return now <= _startTicks + _duration;
        }
    }
}

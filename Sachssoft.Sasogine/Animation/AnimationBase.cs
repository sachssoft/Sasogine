using Microsoft.Xna.Framework;
using Sachssoft.Observables;
using Sachssoft.Observables.Descriptors;
using System;

namespace Sachssoft.Sasogine.Animation
{
    /// <summary>
    /// Represents the base class for all animations.
    /// Provides timing, position, rotation, and progress management.
    /// Supports pausing, resetting, and infinite or delayed animations.
    /// </summary>
    public abstract class AnimationBase : NotifyingElement
    {
        private Vector2 _start_position;
        private float _start_rotation;
        private bool _pause;
        private bool _infinite = true;
        private long _start_ticks;

        #region Public Methods

        /// <summary>
        /// Starts the animation at the specified position and rotation.
        /// Resets the timer and calls <see cref="OnStart"/>.
        /// </summary>
        /// <param name="position">The initial position of the animation.</param>
        /// <param name="rotation">The initial rotation in degrees.</param>
        public void Start(Vector2 position, float rotation)
        {
            _start_position = position;
            _start_rotation = rotation;

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
            : _start_position;

        /// <summary>
        /// Override this method to calculate position updates for the animation.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time since last update in milliseconds.</param>
        /// <returns>New position vector.</returns>
        protected virtual Vector2 AddPositionOverride(float elapsedTime) => Vector2.Zero;

        /// <summary>
        /// Returns the current rotation in degrees based on elapsed time.
        /// Returns the initial rotation if paused or not yet started.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time since last update in milliseconds.</param>
        public float AddRotationDegree(float elapsedTime) => (!_pause && AllowUpdate())
            ? AddRotationDegreeOverride(elapsedTime)
            : _start_rotation;

        /// <summary>
        /// Override this method to calculate rotation updates for the animation.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time since last update in milliseconds.</param>
        /// <returns>New rotation in degrees.</returns>
        protected virtual float AddRotationDegreeOverride(float elapsedTime) => 0f;

        #endregion

        #region Properties

        public readonly static IProperty SpeedProperty =
            new StoredProperty<AnimationBase, float>(
                nameof(Speed),
                defaultValue: 10f,
                category: Sachssoft.Sasogine.Observables.PropertyCategories.Animation,
                descriptors: [new RoundValueDescriptor(2)]);

        /// <summary>
        /// Gets or sets the speed multiplier of the animation.
        /// Affects all position and rotation calculations.
        /// </summary>
        public virtual float Speed
        {
            get => GetValue<float>(SpeedProperty);
            set => SetValue(SpeedProperty, value);
        }

        public readonly static IProperty DurationProperty =
            new StoredProperty<AnimationBase, int>(
                nameof(Duration),
                defaultValue: 100,
                category: Sachssoft.Sasogine.Observables.PropertyCategories.Animation,
                coercion: (o, baseValue) => Math.Max(baseValue, 0));

        /// <summary>
        /// Gets or sets the total duration of the animation in milliseconds.
        /// Must be zero or positive. Ignored if <see cref="Infinite"/> is true.
        /// </summary>
        public virtual int Duration
        {
            get => GetValue<int>(DurationProperty);
            set => SetValue(DurationProperty, value);
        }

        public readonly static IProperty InfiniteProperty =
            new StoredProperty<AnimationBase, bool>(
                nameof(Infinite),
                category: Sachssoft.Sasogine.Observables.PropertyCategories.Animation);

        /// <summary>
        /// Gets or sets whether the animation runs indefinitely.
        /// Overrides <see cref="Duration"/> if true.
        /// </summary>
        public virtual bool Infinite
        {
            get => GetValue<bool>(InfiniteProperty);
            set => SetValue(InfiniteProperty, value);
        }

        public readonly static IProperty DelayProperty =
            new StoredProperty<AnimationBase, int>(
                nameof(Delay),
                defaultValue: 0,
                category: Sachssoft.Sasogine.Observables.PropertyCategories.Animation,
                coercion: (o, baseValue) => Math.Max(baseValue, 0));

        /// <summary>
        /// Gets or sets the start delay of the animation in milliseconds.
        /// Must be zero or positive.
        /// </summary>
        public virtual int Delay
        {
            get => GetValue<int>(DelayProperty);
            set => SetValue(DelayProperty, value);
        }

        #endregion

        #region Protected Members

        /// <summary>
        /// Returns the initial position of the animation.
        /// </summary>
        protected Vector2 StartPosition => _start_position;

        /// <summary>
        /// Returns the initial rotation of the animation in degrees.
        /// </summary>
        protected float StartRotation => _start_rotation;

        /// <summary>
        /// Calculates the progress of the animation as a value between 0 and 1.
        /// Returns 0 if not started, 1 if finished or infinite.
        /// </summary>
        /// <returns>Normalized progress value (0..1).</returns>
        protected float GetProgress()
        {
            long now = Environment.TickCount64;

            if (now < _start_ticks)
                return 0f;

            if (_infinite)
                return 1f;

            return Math.Clamp((float)(now - _start_ticks) / Duration, 0f, 1f);
        }

        #endregion

        #region Private Methods

        private void StartTimer() => _start_ticks = Environment.TickCount64 + Delay;

        private bool AllowUpdate()
        {
            long now = Environment.TickCount64;

            if (now < _start_ticks)
                return false;

            if (_infinite)
                return true;

            return now <= _start_ticks + Duration;
        }

        #endregion
    }
}

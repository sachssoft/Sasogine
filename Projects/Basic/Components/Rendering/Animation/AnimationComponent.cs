using Microsoft.Xna.Framework;
using Sachssoft.Engine.Common;
using Sachssoft.Engine.Timing.Easing;
using System;

namespace Sachssoft.Engine.Components.Rendering;

/// <summary>
/// Provides a base implementation for animation components with timing,
/// progress tracking, position and rotation handling.
/// </summary>
/// <remarks>
/// Supports delayed starts, pausing, resetting, finite durations and
/// infinite animations. An optional easing function can be applied to the
/// normalized animation progress.
/// </remarks>
public abstract class AnimationComponent : ResourceComponentBase, IAnimationComponent
{
    private Point2 _startPosition;
    private float _startRotation;
    private bool _pause;
    private long _startTicks;
    private long _pauseTicks;

    /// <summary>
    /// Gets or sets the animation speed.
    /// </summary>
    public float Speed { get; set; }

    /// <summary>
    /// Gets or sets the animation duration in milliseconds.
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the animation runs indefinitely.
    /// </summary>
    public bool Infinite { get; set; }

    /// <summary>
    /// Gets or sets the delay in milliseconds before the animation starts.
    /// </summary>
    public int Delay { get; set; }

    /// <summary>
    /// Gets or sets the optional easing function applied to the normalized
    /// animation progress.
    /// </summary>
    /// <remarks>
    /// When no easing function is specified, the animation progress remains linear.
    /// </remarks>
    public EasingBase? Easing { get; set; }

    /// <summary>
    /// Starts the animation at the specified position and rotation.
    /// </summary>
    /// <param name="position">The initial position of the animation.</param>
    /// <param name="rotation">The initial rotation in degrees.</param>
    public void Start(Point2 position, float rotation)
    {
        _startPosition = position;
        _startRotation = rotation;
        StartTimer();
        OnStart();
    }

    /// <summary>
    /// Called when the animation starts.
    /// </summary>
    protected virtual void OnStart() { }

    /// <summary>
    /// Resets the animation timer and internal animation state.
    /// </summary>
    public void Reset()
    {
        StartTimer();
        OnReset();
    }

    /// <summary>
    /// Called when the animation is reset.
    /// </summary>
    protected virtual void OnReset() { }

    /// <summary>
    /// Toggles the paused state of the animation.
    /// </summary>
    public void Pause()
    {
        long now = Environment.TickCount64;

        if (_pause)
        {
            _startTicks += now - _pauseTicks;
            _pause = false;
        }
        else
        {
            _pauseTicks = now;
            _pause = true;
        }
    }

    /// <summary>
    /// Calculates the current position contribution of the animation.
    /// </summary>
    /// <param name="elapsedTime">The elapsed time since the previous update.</param>
    /// <returns>The current position contribution.</returns>
    public Vector2 AddPosition(float elapsedTime) => (!_pause && AllowUpdate())
        ? AddPositionOverride(elapsedTime)
        : Vector2.Zero;

    /// <summary>
    /// Calculates the position contribution implemented by a derived animation.
    /// </summary>
    /// <param name="elapsedTime">The elapsed time since the previous update.</param>
    /// <returns>The calculated position contribution.</returns>
    protected virtual Vector2 AddPositionOverride(float elapsedTime) => Vector2.Zero;

    /// <summary>
    /// Calculates the current rotation contribution of the animation.
    /// </summary>
    /// <param name="elapsedTime">The elapsed time since the previous update.</param>
    /// <returns>The current rotation contribution.</returns>
    public float AddRotation(float elapsedTime) => (!_pause && AllowUpdate())
        ? AddRotationOverride(elapsedTime)
        : 0f;

    /// <summary>
    /// Calculates the rotation contribution implemented by a derived animation.
    /// </summary>
    /// <param name="elapsedTime">The elapsed time since the previous update.</param>
    /// <returns>The calculated rotation contribution.</returns>
    protected virtual float AddRotationOverride(float elapsedTime) => 0f;

    /// <summary>
    /// Gets the position from which the animation was started.
    /// </summary>
    protected Point2 StartPosition => _startPosition;

    /// <summary>
    /// Gets the rotation from which the animation was started.
    /// </summary>
    protected float StartRotation => _startRotation;

    /// <summary>
    /// Gets the normalized animation progress with the configured easing
    /// function applied.
    /// </summary>
    /// <returns>
    /// The eased animation progress, or the linear progress when no easing
    /// function is configured.
    /// </returns>
    protected float GetProgress()
    {
        long now = Environment.TickCount64;

        if (now < _startTicks)
            return 0f;

        if (Infinite)
            return 1f;

        if (Duration <= 0)
            return 1f;

        float progress = float.Clamp(
            (float)(now - _startTicks) / Duration,
            0f,
            1f);

        return Easing?.GetValue(progress) ?? progress;
    }

    private void StartTimer()
    {
        _pause = false;
        _pauseTicks = 0;
        _startTicks = Environment.TickCount64 + Delay;
    }

    private bool AllowUpdate()
    {
        long now = Environment.TickCount64;

        if (now < _startTicks)
            return false;

        if (Infinite)
            return true;

        return now <= _startTicks + Duration;
    }
}
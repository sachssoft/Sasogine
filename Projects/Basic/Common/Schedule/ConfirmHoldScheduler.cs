using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Scenes;
using System;

namespace Sachssoft.Sasogine.Common.Schedule;

/// <summary>
/// Repeatedly triggers an action after an initial delay
/// and at a configurable interval.
/// </summary>
public sealed class RepeatWithDelayScheduler
{
    private TimeSpan _elapsed;
    private bool _isRunning;
    private bool _isFirstTick = true;

    /// <summary>
    /// Gets or sets the delay before the first repeated trigger occurs.
    /// </summary>
    public TimeSpan RepeatDelay { get; set; } =
        TimeSpan.FromMilliseconds(300);

    /// <summary>
    /// Gets or sets the interval between repeated triggers.
    /// </summary>
    public TimeSpan RepeatInterval { get; set; } =
        TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Gets a value indicating whether the scheduler is currently running.
    /// </summary>
    public bool IsRunning => _isRunning;

    /// <summary>
    /// Gets or sets a value indicating whether the scheduler continues
    /// triggering after the initial trigger.
    /// </summary>
    public bool IsRecurring { get; set; } = true;

    /// <summary>
    /// Occurs when the scheduler is triggered.
    /// </summary>
    public event Action? Triggered;

    /// <summary>
    /// Occurs when the scheduler is stopped.
    /// </summary>
    public event Action? Stopped;

    /// <summary>
    /// Starts the scheduler and triggers it immediately.
    /// </summary>
    public void Start()
    {
        if (_isRunning)
            return;

        _elapsed = TimeSpan.Zero;
        _isRunning = true;
        _isFirstTick = true;

        Triggered?.Invoke();

        if (!IsRecurring)
            Stop();
    }

    /// <summary>
    /// Stops the scheduler.
    /// </summary>
    public void Stop()
    {
        if (!_isRunning)
            return;

        _isRunning = false;
        _elapsed = TimeSpan.Zero;

        Stopped?.Invoke();
    }

    /// <summary>
    /// Updates the scheduler using the specified game time.
    /// </summary>
    /// <param name="gameTime">
    /// Provides timing information for the current update cycle.
    /// </param>
    public void Update(GameTime gameTime)
    {
        ArgumentNullException.ThrowIfNull(gameTime);

        Update(gameTime.ElapsedGameTime);
    }

    /// <summary>
    /// Updates the scheduler using the specified scene update context.
    /// </summary>
    /// <param name="context">
    /// Provides information about the current scene update.
    /// </param>
    public void Update(SceneUpdateContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        Update(context.GameTime.ElapsedGameTime);
    }

    /// <summary>
    /// Updates the scheduler using the specified elapsed time.
    /// </summary>
    /// <param name="elapsed">
    /// The elapsed time since the previous update.
    /// </param>
    public void Update(TimeSpan elapsed)
    {
        if (!_isRunning || !IsRecurring)
            return;

        if (elapsed <= TimeSpan.Zero)
            return;

        _elapsed += elapsed;

        if (_isFirstTick)
        {
            if (_elapsed < RepeatDelay)
                return;

            _elapsed -= RepeatDelay;
            _isFirstTick = false;

            Triggered?.Invoke();
        }

        if (RepeatInterval <= TimeSpan.Zero)
        {
            Triggered?.Invoke();
            _elapsed = TimeSpan.Zero;
            return;
        }

        while (_elapsed >= RepeatInterval)
        {
            _elapsed -= RepeatInterval;
            Triggered?.Invoke();

            if (!_isRunning)
                break;
        }
    }
}
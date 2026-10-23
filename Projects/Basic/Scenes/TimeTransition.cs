using Microsoft.Xna.Framework;
using System;
using System.Threading.Tasks;

namespace Sachssoft.Engine.Scenes;

/// <summary>
/// Provides a drawable transition that completes after a fixed duration of game time.
/// </summary>
public abstract class TimeTransition : ITransition
{
    private readonly TaskCompletionSource _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private TimeSpan _elapsed;

    /// <summary>
    /// Initializes a time-based transition.
    /// </summary>
    /// <param name="duration">The nonnegative transition duration.</param>
    /// <exception cref="ArgumentOutOfRangeException">The duration is negative.</exception>
    protected TimeTransition(TimeSpan duration)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(duration, TimeSpan.Zero);
        Duration = duration;

        if (duration == TimeSpan.Zero)
            _completion.TrySetResult();
    }

    /// <summary>
    /// Gets the configured transition duration.
    /// </summary>
    public TimeSpan Duration { get; }

    /// <summary>
    /// Gets the elapsed transition time, capped at <see cref="Duration"/>.
    /// </summary>
    public TimeSpan Elapsed => _elapsed;

    /// <summary>
    /// Gets the normalized completion progress, from zero to one.
    /// </summary>
    public float Progress => Duration == TimeSpan.Zero
        ? 1f
        : float.Clamp((float)(_elapsed.TotalSeconds / Duration.TotalSeconds), 0f, 1f);

    /// <inheritdoc/>
    public Task Completion => _completion.Task;

    /// <inheritdoc/>
    public bool IsCompleted => Completion.IsCompletedSuccessfully;

    /// <inheritdoc/>
    public virtual void Update(GameTime gameTime)
    {
        ArgumentNullException.ThrowIfNull(gameTime);

        if (Completion.IsCompleted)
            return;

        if (gameTime.ElapsedGameTime < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(gameTime), "Elapsed game time must not be negative.");

        var remaining = Duration - _elapsed;
        _elapsed += gameTime.ElapsedGameTime >= remaining ? remaining : gameTime.ElapsedGameTime;

        if (_elapsed >= Duration)
            _completion.TrySetResult();
    }

    /// <inheritdoc/>
    public abstract void Draw(SceneDrawContext context);
}

using System;

namespace sachssoft.Sasogine.Gameplay;

/// <summary>
/// Represents a countdown timer based on elapsed time.
/// </summary>
public readonly struct CountdownInfo
{
    /// <summary>
    /// The total duration of the countdown.
    /// </summary>
    public TimeSpan Duration { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CountdownInfo"/> struct with the specified duration.
    /// </summary>
    /// <param name="duration">The total duration of the countdown.</param>
    public CountdownInfo(TimeSpan duration)
    {
        Duration = duration;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CountdownInfo"/> struct with the specified duration in seconds.
    /// </summary>
    /// <param name="seconds">The total duration in seconds.</param>
    public CountdownInfo(int seconds)
    {
        Duration = TimeSpan.FromSeconds(seconds);
    }

    /// <summary>
    /// Gets a value indicating whether the countdown has expired (elapsed time is equal or greater than duration).
    /// </summary>
    /// <param name="elapsed">The elapsed time since the countdown started.</param>
    /// <returns>True if countdown expired; otherwise false.</returns>
    public bool IsExpired(TimeSpan elapsed)
        => elapsed >= Duration;

    /// <summary>
    /// Gets a value indicating whether the countdown has expired based on gameplay time.
    /// </summary>
    /// <param name="gameplayTime">The gameplay time providing elapsed time.</param>
    /// <returns>True if the countdown has expired; otherwise, false.</returns>
    public bool IsExpired(IGameplayTime gameplayTime)
        => IsExpired(gameplayTime.ElapsedGameTime);

    /// <summary>
    /// Calculates the remaining time given the elapsed time.
    /// </summary>
    /// <param name="elapsed">The elapsed time since the countdown started.</param>
    /// <returns>The remaining time; zero if expired.</returns>
    public TimeSpan TimeRemaining(TimeSpan elapsed)
    {
        var remaining = Duration - elapsed;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    /// <summary>
    /// Calculates the remaining time based on the gameplay time.
    /// </summary>
    /// <param name="gameplayTime">The gameplay time providing elapsed time.</param>
    /// <returns>The remaining time; zero if expired.</returns>
    public TimeSpan TimeRemaining(IGameplayTime gameplayTime)
    {
        return TimeRemaining(gameplayTime.ElapsedGameTime);
    }

    /// <summary>
    /// Calculates the remaining time in whole seconds given the elapsed time.
    /// </summary>
    /// <param name="elapsed">The elapsed time since the countdown started.</param>
    /// <returns>The remaining seconds; zero if expired.</returns>
    public int TimeRemainingSeconds(TimeSpan elapsed)
    {
        var remaining = Duration - elapsed;
        return remaining > TimeSpan.Zero ? (int)remaining.TotalSeconds : 0;
    }

    /// <summary>
    /// Calculates the remaining time in whole seconds given the elapsed time from an IGameplayTime instance.
    /// </summary>
    /// <param name="gameplay_time">An object providing the elapsed gameplay time.</param>
    /// <returns>The remaining seconds; zero if expired.</returns>
    public int TimeRemainingSeconds(IGameplayTime gameplay_time)
    {
        if (gameplay_time == null)
            throw new ArgumentNullException(nameof(gameplay_time));
        return TimeRemainingSeconds(gameplay_time.ElapsedGameTime);
    }

    /// <summary>
    /// Returns a new <see cref="CountdownInfo"/> with the duration decreased by the elapsed time.
    /// </summary>
    /// <param name="elapsed">The elapsed time to subtract.</param>
    /// <returns>A new <see cref="CountdownInfo"/> instance with updated duration (never negative).</returns>
    public CountdownInfo Elapse(TimeSpan elapsed)
    {
        var new_duration = Duration - elapsed;
        if (new_duration < TimeSpan.Zero)
            new_duration = TimeSpan.Zero;
        return new CountdownInfo(new_duration);
    }

    /// <summary>
    /// Returns a new <see cref="CountdownInfo"/> with the duration decreased by the elapsed time provided by <see cref="IGameplayTime"/>.
    /// </summary>
    /// <param name="gameplay_time">An object implementing <see cref="IGameplayTime"/> to provide the elapsed time.</param>
    /// <returns>A new <see cref="CountdownInfo"/> instance with updated duration (never negative).</returns>
    public CountdownInfo Elapse(IGameplayTime gameplay_time)
    {
        var elapsed = gameplay_time.ElapsedGameTime;
        var new_duration = Duration - elapsed;
        if (new_duration < TimeSpan.Zero)
            new_duration = TimeSpan.Zero;
        return new CountdownInfo(new_duration);
    }

    /// <summary>
    /// Returns a new <see cref="CountdownInfo"/> with the duration increased by the specified time span.
    /// </summary>
    /// <param name="time_to_add">The time span to add to the countdown duration.</param>
    /// <returns>A new <see cref="CountdownInfo"/> instance with increased duration.</returns>
    public CountdownInfo Add(TimeSpan time_to_add)
    {
        var new_duration = Duration + time_to_add;
        return new CountdownInfo(new_duration);
    }

    /// <summary>
    /// Returns a new <see cref="CountdownInfo"/> with the duration increased by the specified seconds.
    /// </summary>
    /// <param name="seconds">The number of seconds to add to the countdown duration.</param>
    /// <returns>A new <see cref="CountdownInfo"/> instance with increased duration.</returns>
    public CountdownInfo Add(int seconds)
    {
        return Add(TimeSpan.FromSeconds(seconds));
    }

    /// <summary>
    /// Returns a string representation of the countdown duration formatted as "hh:mm:ss".
    /// </summary>
    /// <returns>A string representing the countdown duration.</returns>
    public override string ToString()
    {
        return Duration.ToString(@"hh\:mm\:ss");
    }

    /// <summary>
    /// Returns the countdown duration as a string representing whole seconds.
    /// </summary>
    /// <returns>A string representing the total seconds.</returns>
    public string ToStringSeconds()
        => ((int)Duration.TotalSeconds).ToString();
}

using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Scenes;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Common.Schedule;

/// <summary>
/// Schedules actions for delayed or repeated execution based on game time.
/// </summary>
public sealed class ActionScheduler
{
    private readonly List<ScheduleEntry> _schedule = new();

    /// <summary>
    /// Gets the number of currently scheduled actions.
    /// </summary>
    public int ScheduleCount => _schedule.Count;

    /// <summary>
    /// Adds a scheduled action with an associated parameter.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the parameter passed to the action.
    /// </typeparam>
    /// <param name="delay">
    /// The delay before the action is executed.
    /// Negative values are treated as zero.
    /// </param>
    /// <param name="parameter">
    /// The parameter passed to the action.
    /// </param>
    /// <param name="action">
    /// The action to execute.
    /// </param>
    /// <param name="mode">
    /// The execution mode of the scheduled action.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    public void Add<T>(
        TimeSpan delay,
        T? parameter,
        Action<T?, SchedulerEventArgs> action,
        ActionSchedulerMode mode = ActionSchedulerMode.Once)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (delay < TimeSpan.Zero)
            delay = TimeSpan.Zero;

        _schedule.Add(new ScheduleEntry
        {
            Delay = delay,
            RemainingTime = delay,
            Parameter = parameter,
            Action = (value, args) => action((T?)value, args),
            Mode = mode
        });
    }

    /// <summary>
    /// Adds a scheduled action.
    /// </summary>
    /// <param name="delay">
    /// The delay before the action is executed.
    /// Negative values are treated as zero.
    /// </param>
    /// <param name="action">
    /// The action to execute.
    /// </param>
    /// <param name="mode">
    /// The execution mode of the scheduled action.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    public void Add(
        TimeSpan delay,
        Action<object?, SchedulerEventArgs> action,
        ActionSchedulerMode mode = ActionSchedulerMode.Once)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (delay < TimeSpan.Zero)
            delay = TimeSpan.Zero;

        _schedule.Add(new ScheduleEntry
        {
            Delay = delay,
            RemainingTime = delay,
            Action = action,
            Mode = mode
        });
    }

    /// <summary>
    /// Updates all scheduled actions using the specified scene update context.
    /// </summary>
    /// <param name="context">
    /// Provides information about the current scene update.
    /// </param>
    public void Update(SceneUpdateContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var time = context.GameTime;

        for (var i = _schedule.Count - 1; i >= 0; i--)
        {
            var entry = _schedule[i];

            entry.RemainingTime -= time.ElapsedGameTime;

            if (entry.RemainingTime > TimeSpan.Zero)
                continue;

            var eventArgs = new SchedulerEventArgs(time);

            entry.Action(entry.Parameter, eventArgs);

            switch (entry.Mode)
            {
                case ActionSchedulerMode.Once:
                    _schedule.RemoveAt(i);
                    break;

                case ActionSchedulerMode.Repeat:
                    if (eventArgs.Cancel)
                    {
                        _schedule.RemoveAt(i);
                    }
                    else
                    {
                        entry.RemainingTime += entry.Delay;
                    }

                    break;

                case ActionSchedulerMode.OnceThenUpdate:
                    if (eventArgs.Cancel)
                    {
                        _schedule.RemoveAt(i);
                    }
                    else
                    {
                        entry.RemainingTime = TimeSpan.Zero;
                    }

                    break;
            }
        }
    }

    /// <summary>
    /// Updates all scheduled actions using the specified game time.
    /// </summary>
    /// <param name="time">
    /// Provides timing information for the current game update.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="time"/> is <see langword="null"/>.
    /// </exception>
    public void Update(GameTime time)
    {
        ArgumentNullException.ThrowIfNull(time);

        for (var i = _schedule.Count - 1; i >= 0; i--)
        {
            var entry = _schedule[i];

            entry.RemainingTime -= time.ElapsedGameTime;

            if (entry.RemainingTime > TimeSpan.Zero)
                continue;

            var eventArgs = new SchedulerEventArgs(time);

            entry.Action(entry.Parameter, eventArgs);

            switch (entry.Mode)
            {
                case ActionSchedulerMode.Once:
                    _schedule.RemoveAt(i);
                    break;

                case ActionSchedulerMode.Repeat:
                    if (eventArgs.Cancel)
                    {
                        _schedule.RemoveAt(i);
                    }
                    else
                    {
                        entry.RemainingTime += entry.Delay;
                    }

                    break;

                case ActionSchedulerMode.OnceThenUpdate:
                    if (eventArgs.Cancel)
                    {
                        _schedule.RemoveAt(i);
                    }
                    else
                    {
                        entry.RemainingTime = TimeSpan.Zero;
                    }

                    break;
            }
        }
    }

    /// <summary>
    /// Removes all scheduled actions.
    /// </summary>
    public void Clear()
    {
        _schedule.Clear();
    }

    private sealed class ScheduleEntry
    {
        public required Action<object?, SchedulerEventArgs> Action { get; init; }

        public TimeSpan Delay { get; init; }

        public TimeSpan RemainingTime { get; set; }

        public object? Parameter { get; init; }

        public ActionSchedulerMode Mode { get; init; }
    }
}
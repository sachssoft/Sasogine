using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;

namespace sachssoft.Sasogine.Schedule;

public sealed class ActionScheduler
{
    private readonly List<ScheduleEntry> _schedule = new();

    private sealed class ScheduleEntry
    {
        public TimeSpan delay;
        public TimeSpan remaining_time;
        public object? parameter;
        public Action<object?, CancelEventArgs>? action;
        public ActionSchedulerMode mode;
        public bool executed_once;
    }

    public void Add<T>(TimeSpan delay, T? parameter, Action<T?, CancelEventArgs> action, ActionSchedulerMode mode = ActionSchedulerMode.Once)
    {
        if (delay < TimeSpan.Zero)
            delay = TimeSpan.Zero;

        _schedule.Add(new ScheduleEntry
        {
            delay = delay,
            remaining_time = delay,
            parameter = parameter,
            action = (o, c) => action.Invoke((T?)o, c),
            mode = mode,
            executed_once = false
        });
    }

    public void Add(TimeSpan delay, Action<object?, CancelEventArgs> action, ActionSchedulerMode mode = ActionSchedulerMode.Once)
    {
        if (delay < TimeSpan.Zero)
            delay = TimeSpan.Zero;

        _schedule.Add(new ScheduleEntry
        {
            delay = delay,
            remaining_time = delay,
            parameter = null,
            action = (o, c) => action.Invoke(o, c),
            mode = mode,
            executed_once = false
        });
    }

    public void Update(GameContext context) 
        => Update(context.GameTime.ElapsedGameTime);

    public void Update(TimeSpan elapsed)
    {
        for (int i = _schedule.Count - 1; i >= 0; i--)
        {
            var schedule = _schedule[i];
            schedule.remaining_time -= elapsed;

            if (schedule.remaining_time > TimeSpan.Zero)
                continue;

            var event_args = new CancelEventArgs();
            schedule.action?.Invoke(schedule.parameter, event_args);
            schedule.executed_once = true;

            switch (schedule.mode)
            {
                case ActionSchedulerMode.Once:
                    _schedule.RemoveAt(i);
                    break;

                case ActionSchedulerMode.Repeat:
                    if (event_args.Cancel)
                        _schedule.RemoveAt(i);
                    else
                        schedule.remaining_time += schedule.delay;
                    break;

                case ActionSchedulerMode.OnceThenUpdate:
                    if (event_args.Cancel)
                        _schedule.RemoveAt(i);
                    else
                        schedule.remaining_time = TimeSpan.Zero; // Wieder sofort beim nächsten Update
                    break;
            }
        }
    }

    public int ScheduleCount => _schedule.Count;

    public void Clear()
    {
        _schedule.Clear();
    }
}

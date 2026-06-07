using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Features;

public class TaskProgressTracker
{
    private readonly List<Func<IProgress<float>, Task>> _tasks = new();
    private CancellationTokenSource? _cts;
    private bool _is_ready;
    private bool _tasks_started;
    private TimeSpan _start_delay = TimeSpan.FromMilliseconds(300);
    private TimeSpan _end_delay = TimeSpan.FromMilliseconds(50);
    private TaskProgressTrackerErrorBehavior _error_behavior;
    private int _error_max_retry_count = 5;
    private int _retry_count = 0;
    private bool _continue_after_retry_fail = false;

    public event EventHandler<ProgressChangedEventArgs>? ProgressChanged;
    public event EventHandler? Started;
    public event EventHandler? Aborted;
    public event EventHandler? Completed;
    public event EventHandler? TaskStarting;
    public event EventHandler? TaskFinished;
    public event EventHandler? TaskFailedPermanently;

    public void Start()
    {
        if (_tasks_started)
            throw new InvalidOperationException("Schedule have already been started.");

        _cts = new CancellationTokenSource();

        OnStarted();

        _tasks_started = true;

        _ = RunQueuedTasksAsync(_cts.Token);
    }

    public void Cancel()
    {
        if (!_tasks_started || _cts == null)
            return;

        _cts.Cancel();
    }

    public void AddTask(Func<IProgress<float>, Task> task)
    {
        EnsureNotStarted();
        if (task is null)
            throw new ArgumentNullException(nameof(task));

        _tasks.Add(task);
    }

    public void AddTask(Func<Task> task)
    {
        if (task is null)
            throw new ArgumentNullException(nameof(task));

        AddTask(progress => task()); // Progress ignorieren
    }

    public void AddTask(Action sync_task)
    {
        if (sync_task is null)
            throw new ArgumentNullException(nameof(sync_task));

        AddTask(progress =>
        {
            return Task.Run(() =>
            {
                sync_task();
                progress.Report(1f);
            });
        });
    }

    public void AddTaskDelay(TimeSpan delay)
    {
        AddTask(async progress =>
        {
            await Task.Delay(delay);
            progress.Report(1f);
        });
    }

    private async Task RunQueuedTasksAsync(CancellationToken cancellation_token)
    {
        try
        {
            await OnTaskRunBeforeAsync().ConfigureAwait(false);
            OnProgressChanged(0f);

            if (StartDelay.Ticks > 0)
                await Task.Delay(StartDelay, cancellation_token).ConfigureAwait(false);

            int task_count = _tasks.Count;
            if (task_count == 0)
            {
                OnProgressChanged(1f);
                _is_ready = true;
                OnCompleted();
                return;
            }

            for (int i = 0; i < task_count; i++)
            {
                cancellation_token.ThrowIfCancellationRequested();

                float task_progress_start = (float)i / task_count;
                float task_progress_end = (float)(i + 1) / task_count;
                bool progress_reported = false;

                var task_progress = new Progress<float>(p =>
                {
                    progress_reported = true;
                    float total_percent = task_progress_start + p * (task_progress_end - task_progress_start);
                    OnProgressChanged(total_percent);
                });

                int retry_count = 0;

                while (true)
                {
                    try
                    {
                        await _tasks[i].Invoke(task_progress).ConfigureAwait(false);

                        if (!progress_reported)
                            OnProgressChanged(task_progress_end);

                        break; // Erfolgreich → raus aus Retry-Loop
                    }
                    catch (Exception ex)
                    {
                        retry_count++;

                        if (ErrorBehavior == TaskProgressTrackerErrorBehavior.StopOnError)
                            throw;

                        if (ErrorBehavior == TaskProgressTrackerErrorBehavior.ContinueOnError)
                        {
                            break;
                        }

                        if (ErrorBehavior == TaskProgressTrackerErrorBehavior.Retry)
                        {
                            if (retry_count >= _error_max_retry_count)
                            {
                                TaskFailedPermanently?.Invoke(this, new TaskFailedEventArgs(i, ex));
                                if (_continue_after_retry_fail)
                                {
                                    break;
                                }
                                else
                                {
                                    throw;
                                }
                            }

                            await Task.Delay(200, cancellation_token);
                            continue;
                        }

                        throw;
                    }

                }
            }

            OnProgressChanged(1f);

            if (EndDelay.Ticks > 0)
                await Task.Delay(EndDelay, cancellation_token).ConfigureAwait(false);

            _is_ready = true;
            OnCompleted();
        }
        catch (OperationCanceledException)
        {
            OnCanceled();
        }
        finally
        {
            await OnTaskRunAfterAsync().ConfigureAwait(false);
        }
    }

    protected virtual Task OnTaskRunBeforeAsync()
    {
        TaskStarting?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    protected virtual Task OnTaskRunAfterAsync()
    {
        TaskFinished?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    protected virtual void OnStarted()
    {
        Started?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnProgressChanged(float percent)
    {
        ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(percent));
    }

    protected virtual void OnCanceled()
    {
        Aborted?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnCompleted()
    {
        Completed?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnTaskFailedPermanently()
    {
        TaskFailedPermanently?.Invoke(this, EventArgs.Empty);
    }

    public bool IsReady => _is_ready;

    public TimeSpan StartDelay
    {
        get => _start_delay;
        set
        {
            EnsureNotStarted();
            if (value.Ticks < 0) throw new ArgumentOutOfRangeException(nameof(StartDelay), "Delay must be non-negative.");
            _start_delay = value;
        }
    }

    public TimeSpan EndDelay
    {
        get => _end_delay;
        set
        {
            EnsureNotStarted();
            if (value.Ticks < 0) throw new ArgumentOutOfRangeException(nameof(EndDelay), "Delay must be non-negative.");
            _end_delay = value;
        }
    }

    public TaskProgressTrackerErrorBehavior ErrorBehavior
    {
        get => _error_behavior;
        set
        {
            EnsureNotStarted();
            _error_behavior = value;
        }
    }

    public int ErrorMaxRetryCount
    {
        get => _error_max_retry_count;
        set
        {
            EnsureNotStarted();

            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(ErrorMaxRetryCount), "Value must be zero or greater.");

            _error_max_retry_count = value;
        }
    }

    public bool ContinueAfterRetryFail
    {
        get => _continue_after_retry_fail;
        set
        {
            EnsureNotStarted();
            _continue_after_retry_fail = value;
        }
    }

    private void EnsureNotStarted()
    {
        if (_tasks_started)
            throw new InvalidOperationException("Cannot modify tasks or delays after starting.");
    }
}

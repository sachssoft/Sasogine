using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Common.Schedule;

/// <summary>
/// Manages and executes a sequence of asynchronous or synchronous tasks
/// while reporting their overall progress.
/// </summary>
/// <remarks>
/// Tasks are executed sequentially in the order in which they are added.
/// The tracker supports cancellation, configurable start and end delays,
/// retry behavior, and progress reporting.
/// </remarks>
public class TaskProgressTracker
{
    private readonly List<Func<IProgress<float>, Task>> _tasks = new();

    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isReady;
    private bool _tasksStarted;

    private TimeSpan _startDelay = TimeSpan.FromMilliseconds(300);
    private TimeSpan _endDelay = TimeSpan.FromMilliseconds(50);

    private TaskProgressTrackerErrorBehavior _errorBehavior;
    private int _errorMaxRetryCount = 5;
    private bool _continueAfterRetryFail;

    /// <summary>
    /// Occurs when the overall progress changes.
    /// </summary>
    public event EventHandler<ProgressChangedEventArgs>? ProgressChanged;

    /// <summary>
    /// Occurs when task execution starts.
    /// </summary>
    public event EventHandler? Started;

    /// <summary>
    /// Occurs when task execution is canceled.
    /// </summary>
    public event EventHandler? Aborted;

    /// <summary>
    /// Occurs when all scheduled tasks have completed.
    /// </summary>
    public event EventHandler? Completed;

    /// <summary>
    /// Occurs before the queued tasks begin executing.
    /// </summary>
    public event EventHandler? TaskStarting;

    /// <summary>
    /// Occurs after queued task execution has finished.
    /// </summary>
    public event EventHandler? TaskFinished;

    /// <summary>
    /// Occurs when a task permanently fails after exhausting all retry attempts.
    /// </summary>
    public event EventHandler<TaskFailedEventArgs>? TaskFailedPermanently;

    /// <summary>
    /// Gets a value indicating whether task execution has completed successfully.
    /// </summary>
    public bool IsReady => _isReady;

    /// <summary>
    /// Gets or sets the delay applied before queued task execution begins.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the specified delay is negative.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when task execution has already started.
    /// </exception>
    public TimeSpan StartDelay
    {
        get => _startDelay;
        set
        {
            EnsureNotStarted();

            if (value < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    "Delay must be non-negative.");
            }

            _startDelay = value;
        }
    }

    /// <summary>
    /// Gets or sets the delay applied after all queued tasks have completed.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the specified delay is negative.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when task execution has already started.
    /// </exception>
    public TimeSpan EndDelay
    {
        get => _endDelay;
        set
        {
            EnsureNotStarted();

            if (value < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    "Delay must be non-negative.");
            }

            _endDelay = value;
        }
    }

    /// <summary>
    /// Gets or sets the behavior used when a scheduled task fails.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when task execution has already started.
    /// </exception>
    public TaskProgressTrackerErrorBehavior ErrorBehavior
    {
        get => _errorBehavior;
        set
        {
            EnsureNotStarted();
            _errorBehavior = value;
        }
    }

    /// <summary>
    /// Gets or sets the maximum number of attempts allowed when retry behavior is enabled.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the specified value is negative.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when task execution has already started.
    /// </exception>
    public int ErrorMaxRetryCount
    {
        get => _errorMaxRetryCount;
        set
        {
            EnsureNotStarted();

            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    "Value must be zero or greater.");
            }

            _errorMaxRetryCount = value;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether task execution continues
    /// after a task permanently fails.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when task execution has already started.
    /// </exception>
    public bool ContinueAfterRetryFail
    {
        get => _continueAfterRetryFail;
        set
        {
            EnsureNotStarted();
            _continueAfterRetryFail = value;
        }
    }

    /// <summary>
    /// Starts execution of the queued tasks.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when task execution has already started.
    /// </exception>
    public void Start()
    {
        if (_tasksStarted)
        {
            throw new InvalidOperationException(
                "Task execution has already been started.");
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _tasksStarted = true;

        OnStarted();

        _ = RunQueuedTasksAsync(_cancellationTokenSource.Token);
    }

    /// <summary>
    /// Requests cancellation of the current task execution.
    /// </summary>
    public void Cancel()
    {
        if (!_tasksStarted)
            return;

        _cancellationTokenSource?.Cancel();
    }

    /// <summary>
    /// Adds an asynchronous task that can report progress.
    /// </summary>
    /// <param name="task">
    /// The task to add.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="task"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when task execution has already started.
    /// </exception>
    public void AddTask(Func<IProgress<float>, Task> task)
    {
        ArgumentNullException.ThrowIfNull(task);

        EnsureNotStarted();
        _tasks.Add(task);
    }

    /// <summary>
    /// Adds an asynchronous task that does not report progress.
    /// </summary>
    /// <param name="task">
    /// The task to add.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="task"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when task execution has already started.
    /// </exception>
    public void AddTask(Func<Task> task)
    {
        ArgumentNullException.ThrowIfNull(task);

        AddTask(_ => task());
    }

    /// <summary>
    /// Adds a synchronous operation to the task queue.
    /// </summary>
    /// <param name="task">
    /// The synchronous operation to add.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="task"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when task execution has already started.
    /// </exception>
    public void AddTask(Action task)
    {
        ArgumentNullException.ThrowIfNull(task);

        AddTask(progress =>
        {
            return Task.Run(() =>
            {
                task();
                progress.Report(1f);
            });
        });
    }

    /// <summary>
    /// Adds a delay to the task queue.
    /// </summary>
    /// <param name="delay">
    /// The duration of the delay.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="delay"/> is negative.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when task execution has already started.
    /// </exception>
    public void AddTaskDelay(TimeSpan delay)
    {
        if (delay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(delay),
                "Delay must be non-negative.");
        }

        AddTask(async progress =>
        {
            await Task.Delay(delay).ConfigureAwait(false);
            progress.Report(1f);
        });
    }

    /// <summary>
    /// Called before queued task execution begins.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    protected virtual Task OnTaskRunBeforeAsync()
    {
        TaskStarting?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called after queued task execution has finished.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    protected virtual Task OnTaskRunAfterAsync()
    {
        TaskFinished?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Raises the <see cref="Started"/> event.
    /// </summary>
    protected virtual void OnStarted()
    {
        Started?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="ProgressChanged"/> event.
    /// </summary>
    /// <param name="progress">
    /// The overall progress value.
    /// </param>
    protected virtual void OnProgressChanged(float progress)
    {
        ProgressChanged?.Invoke(
            this,
            new ProgressChangedEventArgs(progress));
    }

    /// <summary>
    /// Raises the <see cref="Aborted"/> event.
    /// </summary>
    protected virtual void OnCanceled()
    {
        Aborted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="Completed"/> event.
    /// </summary>
    protected virtual void OnCompleted()
    {
        Completed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="TaskFailedPermanently"/> event.
    /// </summary>
    /// <param name="taskIndex">
    /// The index of the failed task.
    /// </param>
    /// <param name="exception">
    /// The exception that caused the task to fail.
    /// </param>
    protected virtual void OnTaskFailedPermanently(
        int taskIndex,
        Exception exception)
    {
        TaskFailedPermanently?.Invoke(
            this,
            new TaskFailedEventArgs(taskIndex, exception));
    }

    private async Task RunQueuedTasksAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            await OnTaskRunBeforeAsync().ConfigureAwait(false);

            OnProgressChanged(0f);

            if (_startDelay > TimeSpan.Zero)
            {
                await Task.Delay(
                    _startDelay,
                    cancellationToken).ConfigureAwait(false);
            }

            var taskCount = _tasks.Count;

            if (taskCount == 0)
            {
                OnProgressChanged(1f);

                _isReady = true;
                OnCompleted();

                return;
            }

            for (var i = 0; i < taskCount; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var taskProgressStart = (float)i / taskCount;
                var taskProgressEnd = (float)(i + 1) / taskCount;

                var progressReported = false;

                var taskProgress = new Progress<float>(progress =>
                {
                    progressReported = true;

                    var totalProgress =
                        taskProgressStart +
                        progress *
                        (taskProgressEnd - taskProgressStart);

                    OnProgressChanged(totalProgress);
                });

                var retryCount = 0;

                while (true)
                {
                    try
                    {
                        await _tasks[i](taskProgress).ConfigureAwait(false);

                        if (!progressReported)
                            OnProgressChanged(taskProgressEnd);

                        break;
                    }
                    catch (OperationCanceledException)
                        when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        if (_errorBehavior ==
                            TaskProgressTrackerErrorBehavior.StopOnError)
                        {
                            throw;
                        }

                        if (_errorBehavior ==
                            TaskProgressTrackerErrorBehavior.ContinueOnError)
                        {
                            OnProgressChanged(taskProgressEnd);
                            break;
                        }

                        if (_errorBehavior ==
                            TaskProgressTrackerErrorBehavior.Retry)
                        {
                            retryCount++;

                            if (retryCount >= _errorMaxRetryCount)
                            {
                                OnTaskFailedPermanently(
                                    i,
                                    exception);

                                if (_continueAfterRetryFail)
                                {
                                    OnProgressChanged(taskProgressEnd);
                                    break;
                                }

                                throw;
                            }

                            await Task.Delay(
                                200,
                                cancellationToken).ConfigureAwait(false);

                            continue;
                        }

                        throw;
                    }
                }
            }

            OnProgressChanged(1f);

            if (_endDelay > TimeSpan.Zero)
            {
                await Task.Delay(
                    _endDelay,
                    cancellationToken).ConfigureAwait(false);
            }

            _isReady = true;

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

    private void EnsureNotStarted()
    {
        if (_tasksStarted)
        {
            throw new InvalidOperationException(
                "Cannot modify the task schedule after execution has started.");
        }
    }
}
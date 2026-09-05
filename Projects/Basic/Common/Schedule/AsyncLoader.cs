using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Scenes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Common.Schedule;

/// <summary>
/// Represents an asynchronous loading operation that can report progress
/// and schedule actions for execution on the main thread.
/// </summary>
public sealed class AsyncLoader : IScheduledOperation
{
    private readonly Func<AsyncLoader, Task> _loader;
    private readonly Queue<Action> _mainThreadQueue = new();

    private Task? _task;
    private bool _completed;
    private bool _activated;
    private float _progress;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncLoader"/> class.
    /// </summary>
    /// <param name="loader">
    /// The asynchronous loading operation to execute.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="loader"/> is <see langword="null"/>.
    /// </exception>
    public AsyncLoader(Func<AsyncLoader, Task> loader)
    {
        _loader = loader ?? throw new ArgumentNullException(nameof(loader));
    }

    /// <summary>
    /// Gets the current loading progress.
    /// </summary>
    public float Progress => _progress;

    /// <summary>
    /// Gets a value indicating whether the loading operation has completed.
    /// </summary>
    public bool IsCompleted => _completed;

    /// <summary>
    /// Gets a value indicating whether the loading operation is currently running.
    /// </summary>
    public bool IsLoading =>
        _task != null && !_task.IsCompleted;

    /// <summary>
    /// Gets a value indicating whether the loading operation failed.
    /// </summary>
    public bool HasError => Error != null;

    /// <summary>
    /// Gets the exception that occurred during loading, if any.
    /// </summary>
    public Exception? Error { get; private set; }

    /// <summary>
    /// Gets the result of the scheduled operation.
    /// </summary>
    public object? Result => null;

    /// <summary>
    /// Activates the loading operation.
    /// </summary>
    public void Activate()
    {
        if (_activated)
            return;

        _activated = true;
        _task = ExecuteAsync();
    }

    /// <summary>
    /// Reports the current loading progress.
    /// </summary>
    /// <param name="value">
    /// The progress value.
    /// </param>
    public void ReportProgress(float value)
    {
        _progress = value;
    }

    /// <summary>
    /// Enqueues an action for execution on the main thread.
    /// </summary>
    /// <param name="action">
    /// The action to enqueue.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    public void EnqueueMainThread(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        lock (_mainThreadQueue)
        {
            _mainThreadQueue.Enqueue(action);
        }
    }

    /// <summary>
    /// Updates the loading operation using the specified game time.
    /// </summary>
    /// <param name="gameTime">
    /// Provides timing information for the current update cycle.
    /// </param>
    public void Update(GameTime gameTime)
    {
        ArgumentNullException.ThrowIfNull(gameTime);

        UpdateCore();
    }

    /// <summary>
    /// Updates the loading operation using the specified scene update context.
    /// </summary>
    /// <param name="context">
    /// Provides information about the current scene update.
    /// </param>
    public void Update(SceneUpdateContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        UpdateCore();
    }

    private async Task ExecuteAsync()
    {
        try
        {
            await _loader(this).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            Error = exception;
        }
    }

    private void UpdateCore()
    {
        ExecuteMainThreadQueue();

        if (!_activated ||
            _task == null ||
            _completed ||
            !_task.IsCompleted)
        {
            return;
        }

        _completed = true;
    }

    private void ExecuteMainThreadQueue()
    {
        while (true)
        {
            Action action;

            lock (_mainThreadQueue)
            {
                if (_mainThreadQueue.Count == 0)
                    return;

                action = _mainThreadQueue.Dequeue();
            }

            action();
        }
    }
}
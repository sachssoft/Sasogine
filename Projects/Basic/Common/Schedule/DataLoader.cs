using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Geometry;
using Sachssoft.Sasogine.Scenes;
using System;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Common.Schedule;

/// <summary>
/// Loads data asynchronously and supports automatic retry on failure.
/// </summary>
/// <typeparam name="T">
/// The type of data produced by the loader.
/// </typeparam>
public sealed class DataLoader<T> : IScheduledOperation
{
    private readonly Func<Task<T>> _loadFunc;
    private readonly int _maxRetries;

    private Task<T>? _loadTask;
    private bool _isActivated;
    private bool _isCompleted;
    private int _retryCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataLoader{T}"/> class.
    /// </summary>
    /// <param name="loadFunc">
    /// The asynchronous function used to load the data.
    /// </param>
    /// <param name="maxRetries">
    /// The maximum number of retry attempts after a failed load.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="loadFunc"/> is <see langword="null"/>.
    /// </exception>
    public DataLoader(
        Func<Task<T>> loadFunc,
        int maxRetries = 3)
    {
        _loadFunc = loadFunc ??
            throw new ArgumentNullException(nameof(loadFunc));

        _maxRetries = Math.Max(0, maxRetries);
    }

    /// <summary>
    /// Gets the loaded result.
    /// </summary>
    public T? Result { get; private set; }

    /// <summary>
    /// Gets the exception that caused the loading operation to fail.
    /// </summary>
    public Exception? Error { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the loading operation has completed.
    /// </summary>
    public bool IsCompleted => _isCompleted;

    /// <summary>
    /// Gets a value indicating whether the loading operation is currently running.
    /// </summary>
    public bool IsLoading =>
        _loadTask != null &&
        !_loadTask.IsCompleted;

    /// <summary>
    /// Gets a value indicating whether the loading operation failed.
    /// </summary>
    public bool HasError => Error != null;

    /// <summary>
    /// Gets or sets the action invoked when loading completes successfully.
    /// </summary>
    public Action? Completed { get; set; }

    /// <summary>
    /// Gets or sets the action invoked when loading permanently fails.
    /// </summary>
    public Action? Failed { get; set; }

    object? IScheduledOperation.Result => Result;

    /// <summary>
    /// Activates the loading operation.
    /// </summary>
    public void Activate()
    {
        if (_isActivated)
            return;

        _isActivated = true;
        StartLoad();
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

    /// <summary>
    /// Restarts the loading operation and resets the retry count.
    /// </summary>
    public void Reload()
    {
        _retryCount = 0;
        _isActivated = true;

        StartLoad();
    }

    private void StartLoad()
    {
        _loadTask = _loadFunc();

        Error = null;
        Result = default;
        _isCompleted = false;
    }

    private void UpdateCore()
    {
        if (!_isActivated ||
            _loadTask == null ||
            _isCompleted)
        {
            return;
        }

        if (!_loadTask.IsCompleted)
            return;

        if (_loadTask.IsFaulted)
        {
            Error = _loadTask.Exception?.GetBaseException();

            if (_retryCount < _maxRetries)
            {
                _retryCount++;

                StartLoad();
                return;
            }

            _isCompleted = true;

            Failed?.Invoke();
            return;
        }

        if (_loadTask.IsCanceled)
        {
            Error = new TaskCanceledException(_loadTask);

            _isCompleted = true;

            Failed?.Invoke();
            return;
        }

        Result = _loadTask.GetAwaiter().GetResult();

        _isCompleted = true;

        Completed?.Invoke();
    }
}
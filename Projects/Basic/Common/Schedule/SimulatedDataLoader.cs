using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Scenes;
using System;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Common.Schedule;

/// <summary>
/// Represents a simulated scheduled loading operation that completes
/// with a predefined result after a configurable delay.
/// </summary>
/// <typeparam name="T">
/// The type of result produced by the simulated operation.
/// </typeparam>
/// <remarks>
/// This type is intended primarily for testing, prototyping, or scenarios
/// where no actual data source is available.
/// </remarks>
public sealed class SimulatedDataLoader<T> : IScheduledOperation
{
    private readonly T _result;
    private readonly int _delayMilliseconds;

    private Task? _task;
    private Exception? _error;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulatedDataLoader{T}"/>
    /// class with the specified result and delay.
    /// </summary>
    /// <param name="result">
    /// The result returned by the simulated loading operation.
    /// </param>
    /// <param name="delayMilliseconds">
    /// The simulated loading delay, in milliseconds.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="delayMilliseconds"/> is negative.
    /// </exception>
    public SimulatedDataLoader(
        T result,
        int delayMilliseconds = 1000)
    {
        if (delayMilliseconds < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(delayMilliseconds),
                "Delay must be zero or greater.");
        }

        _result = result;
        _delayMilliseconds = delayMilliseconds;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulatedDataLoader{T}"/>
    /// class with the specified delay and a default result.
    /// </summary>
    /// <param name="delayMilliseconds">
    /// The simulated loading delay, in milliseconds.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="delayMilliseconds"/> is negative.
    /// </exception>
    public SimulatedDataLoader(int delayMilliseconds = 1000)
    {
        if (delayMilliseconds < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(delayMilliseconds),
                "Delay must be zero or greater.");
        }

        _result = default!;
        _delayMilliseconds = delayMilliseconds;
    }

    /// <summary>
    /// Gets a value indicating whether the simulated operation has completed.
    /// </summary>
    public bool IsCompleted { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the simulated operation is currently loading.
    /// </summary>
    public bool IsLoading { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the simulated operation has failed.
    /// </summary>
    public bool HasError => _error != null;

    /// <summary>
    /// Gets the exception that caused the simulated operation to fail, if any.
    /// </summary>
    public Exception? Error => _error;

    /// <summary>
    /// Gets the predefined result of the simulated operation.
    /// </summary>
    public object? Result => _result;

    /// <summary>
    /// Activates the simulated loading operation.
    /// </summary>
    public void Activate()
    {
        if (IsLoading || IsCompleted)
            return;

        IsLoading = true;
        IsCompleted = false;
        _error = null;

        _task = SimulateLoadAsync();
    }

    /// <summary>
    /// Updates the state of the simulated loading operation.
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
    /// Updates the state of the simulated loading operation
    /// using the specified scene update context.
    /// </summary>
    /// <param name="context">
    /// Provides information about the current scene update.
    /// </param>
    public void Update(SceneUpdateContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        Update(context.GameTime);
    }

    private async Task SimulateLoadAsync()
    {
        try
        {
            await Task.Delay(_delayMilliseconds).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _error = exception;
        }
    }

    private void UpdateCore()
    {
        if (_task == null ||
            !_task.IsCompleted ||
            IsCompleted)
        {
            return;
        }

        if (_task.IsFaulted)
            _error = _task.Exception?.GetBaseException();

        IsCompleted = true;
        IsLoading = false;
    }
}
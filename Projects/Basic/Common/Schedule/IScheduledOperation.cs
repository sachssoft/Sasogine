using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Sasogine.Common.Schedule;

/// <summary>
/// Defines an operation that can be activated and updated
/// as part of a scheduled execution cycle.
/// </summary>
public interface IScheduledOperation
{
    /// <summary>
    /// Activates the scheduled operation.
    /// </summary>
    void Activate();

    /// <summary>
    /// Updates the scheduled operation using the specified game time.
    /// </summary>
    /// <param name="gameTime">
    /// Provides timing information for the current update cycle.
    /// </param>
    void Update(GameTime gameTime);

    /// <summary>
    /// Gets a value indicating whether the operation has completed.
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// Gets a value indicating whether the operation is currently running.
    /// </summary>
    bool IsLoading { get; }

    /// <summary>
    /// Gets a value indicating whether the operation has failed.
    /// </summary>
    bool HasError { get; }

    /// <summary>
    /// Gets the exception that caused the operation to fail, if any.
    /// </summary>
    Exception? Error { get; }

    /// <summary>
    /// Gets the result produced by the operation, if any.
    /// </summary>
    object? Result { get; }
}
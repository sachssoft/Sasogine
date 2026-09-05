using System;

namespace Sachssoft.Sasogine.Common.Schedule;

/// <summary>
/// Provides data for an event raised when a scheduled task fails.
/// </summary>
public class TaskFailedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TaskFailedEventArgs"/> class.
    /// </summary>
    /// <param name="taskIndex">The index of the task that failed.</param>
    /// <param name="exception">
    /// The exception that caused the task to fail, or <see langword="null"/>
    /// if no exception is available.
    /// </param>
    public TaskFailedEventArgs(int taskIndex, Exception? exception)
    {
        TaskIndex = taskIndex;
        Exception = exception;
    }

    /// <summary>
    /// Gets the index of the task that failed.
    /// </summary>
    public int TaskIndex { get; }

    /// <summary>
    /// Gets the exception that caused the task to fail, if available.
    /// </summary>
    public Exception? Exception { get; }
}
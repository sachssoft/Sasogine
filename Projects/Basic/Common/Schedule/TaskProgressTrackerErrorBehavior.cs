namespace Sachssoft.Sasogine.Common.Schedule;

/// <summary>
/// Specifies how a <see cref="TaskProgressTracker"/> handles errors
/// that occur while executing scheduled tasks.
/// </summary>
public enum TaskProgressTrackerErrorBehavior
{
    /// <summary>
    /// Stops task execution when an error occurs.
    /// </summary>
    StopOnError,

    /// <summary>
    /// Continues with the next task when an error occurs.
    /// </summary>
    ContinueOnError,

    /// <summary>
    /// Retries the failed task according to the configured retry settings.
    /// </summary>
    Retry
}
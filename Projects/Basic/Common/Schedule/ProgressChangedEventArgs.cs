using System;

namespace Sachssoft.Sasogine.Common.Schedule;

/// <summary>
/// Provides data for an event that reports progress of scheduled task execution.
/// </summary>
public class ProgressChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProgressChangedEventArgs"/> class.
    /// </summary>
    /// <param name="percent">The current progress value.</param>
    public ProgressChangedEventArgs(float percent)
    {
        Percent = percent;
    }

    /// <summary>
    /// Gets the current progress value.
    /// </summary>
    public float Percent { get; }
}
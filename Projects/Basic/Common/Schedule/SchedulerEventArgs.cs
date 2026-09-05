using Microsoft.Xna.Framework;
using System;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Common.Schedule;

/// <summary>
/// Provides data for a scheduled action and allows the action
/// to cancel further scheduled execution.
/// </summary>
public class SchedulerEventArgs : CancelEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SchedulerEventArgs"/> class.
    /// </summary>
    /// <param name="gameTime">
    /// Provides timing information for the current update cycle.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="gameTime"/> is <see langword="null"/>.
    /// </exception>
    public SchedulerEventArgs(GameTime gameTime)
    {
        GameTime = gameTime ??
            throw new ArgumentNullException(nameof(gameTime));
    }

    /// <summary>
    /// Gets the timing information for the current update cycle.
    /// </summary>
    public GameTime GameTime { get; }
}
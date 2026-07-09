using System;

namespace Sachssoft.Sasogine.Services.Platform
{
    /// <summary>
    /// Platform service for scheduling and managing notifications.
    /// 
    /// Mobile platforms (Android/iOS) use native local notifications.
    /// Desktop platforms can use system tray notifications or ignore them.
    /// 
    /// Example usage:
    /// <code>
    /// // Schedule a notification 10 seconds from now
    /// notificationService.Schedule(
    ///     "Reminder",
    ///     "Time to check MinerMania!",
    ///     DateTime.Now.AddSeconds(10)
    /// );
    /// 
    /// // Cancel all scheduled notifications
    /// notificationService.CancelAll();
    /// </code>
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Schedules a notification at the specified time.
        /// </summary>
        /// <param name="title">The title of the notification.</param>
        /// <param name="message">The message/body of the notification.</param>
        /// <param name="when">Time when the notification should appear.</param>
        void Schedule(string title, string message, DateTime when);

        /// <summary>
        /// Cancels all scheduled notifications.
        /// </summary>
        void CancelAll();
    }
}
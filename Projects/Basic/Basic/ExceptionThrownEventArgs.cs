using System;

namespace Sachssoft.Sasogine;

/// <summary>
/// Provides event data for an exception that has been thrown.
/// </summary>
public class ExceptionThrownEventArgs : EventArgs
{
    /// <summary>
    /// Gets the exception that was thrown.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the exception
    /// has been handled by an event subscriber.
    /// </summary>
    public bool Handled { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionThrownEventArgs"/> class.
    /// </summary>
    /// <param name="exception">
    /// The exception that was thrown.
    /// </param>
    public ExceptionThrownEventArgs(Exception exception)
    {
        Exception = exception ??
            throw new ArgumentNullException(nameof(exception));
    }

    /// <summary>
    /// Raises an exception event.
    /// </summary>
    /// <param name="handler">
    /// The event handler to invoke.
    /// </param>
    /// <param name="sender">
    /// The object that raised the event.
    /// </param>
    /// <param name="exception">
    /// The exception that was thrown.
    /// </param>
    /// <remarks>
    /// If the event is not handled, no additional action is performed.
    /// </remarks>
    public static void Raise(
        EventHandler<ExceptionThrownEventArgs>? handler,
        object? sender,
        Exception exception)
    {
        Raise(
            handler,
            sender,
            exception,
            null);
    }

    /// <summary>
    /// Raises an exception event and invokes a callback when the exception
    /// remains unhandled.
    /// </summary>
    /// <param name="handler">
    /// The event handler to invoke.
    /// </param>
    /// <param name="sender">
    /// The object that raised the event.
    /// </param>
    /// <param name="exception">
    /// The exception that was thrown.
    /// </param>
    /// <param name="unhandledCallback">
    /// The callback invoked when the exception is not handled.
    /// </param>
    public static void Raise(
        EventHandler<ExceptionThrownEventArgs>? handler,
        object? sender,
        Exception exception,
        Action? unhandledCallback)
    {
        var args = new ExceptionThrownEventArgs(exception);

        handler?.Invoke(sender, args);

        if (!args.Handled)
            unhandledCallback?.Invoke();
    }
}
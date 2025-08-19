using System;

namespace Sachssoft.Sasogine;

public class ExceptionThrownEventArgs : EventArgs
{
    public Exception Exception { get; }

    public bool Handled { get; set; }

    public ExceptionThrownEventArgs(Exception exception)
    {
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
    }

    public static void Raise(EventHandler<ExceptionThrownEventArgs>? handler, object? sender, Exception exception)
        => Raise(handler, sender, exception, null);

    public static void Raise(EventHandler<ExceptionThrownEventArgs>? handler, object? sender, Exception exception, Action? unhandled_callback)
    {
        var args = new ExceptionThrownEventArgs(exception);
        handler?.Invoke(sender, args);

        if (!args.Handled && unhandled_callback != null)
        {
            unhandled_callback.Invoke();
        }
    }
}

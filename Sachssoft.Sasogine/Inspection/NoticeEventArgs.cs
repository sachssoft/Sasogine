using System;

namespace Sachssoft.Sasogine.Inspection;

public class NoticeEventArgs : EventArgs
{
    public string? Message { get; }
    public int Code { get; }
    public NoticeKind Kind { get; }
    public bool Handled { get; set; }

    public NoticeEventArgs(string? message, int code, NoticeKind kind)
    {
        Message = message;
        Code = code;
        Kind = kind;
    }

    public static void Raise(
        EventHandler<NoticeEventArgs>? handler,
        object? sender,
        string? message,
        int code,
        NoticeKind kind = NoticeKind.Info,
        Action? unhandled_callback = null)
    {
        var args = new NoticeEventArgs(message, code, kind);
        handler?.Invoke(sender, args);

        if (!args.Handled)
        {
            unhandled_callback?.Invoke();
        }
    }
}

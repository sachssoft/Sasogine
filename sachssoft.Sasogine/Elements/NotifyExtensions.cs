using System;

namespace sachssoft.Sasogine.Elements;

public static class NotifyExtensions
{
    public static void NotifyNotice(
        this INotifyNotice provider,
        string message,
        int code,
        NoticeKind kind = NoticeKind.Info,
        Action? unhandled_callback = null)
    {
        var args = new NoticeEventArgs(message, code, kind);
        provider.RaiseNoticeSent(args);

        if (!args.Handled)
        {
            unhandled_callback?.Invoke();
        }
    }

    public static void NotifyNotice<TEnum>(
        this INotifyNotice provider,
        string? message,
        TEnum code,
        NoticeKind kind = NoticeKind.Info,
        Action? unhandled_callback = null)
        where TEnum : struct, Enum
    {
        var args = new NoticeEventArgs(message, Convert.ToInt32(code), kind);
        provider.RaiseNoticeSent(args);

        if (!args.Handled)
        {
            unhandled_callback?.Invoke();
        }
    }

    public static void NotifyNotice<TEnum>(
        this INotifyNotice provider,
        TEnum code,
        NoticeKind kind = NoticeKind.Info,
        Action? unhandled_callback = null)
        where TEnum : struct, Enum
    {
        NotifyNotice<TEnum>(provider, null, code, kind, unhandled_callback);
    }
}
using System;

namespace sachssoft.Sasogine.Elements;

public interface INotifyNotice
{
    event EventHandler<NoticeEventArgs>? NoticeSent;

    void RaiseNoticeSent(NoticeEventArgs e);
}

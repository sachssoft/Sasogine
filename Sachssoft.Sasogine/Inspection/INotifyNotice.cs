using System;

namespace Sachssoft.Sasogine.Inspection;

public interface INotifyNotice
{
    event EventHandler<NoticeEventArgs>? NoticeSent;

    void RaiseNoticeSent(NoticeEventArgs e);
}

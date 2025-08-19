using System;

namespace Sachssoft.Sasogine.Surface.Events;

public class CancellableEventArgs : EventArgs
{
    public bool Cancel { get; set; }

    public CancellableEventArgs()
    {
    }
}

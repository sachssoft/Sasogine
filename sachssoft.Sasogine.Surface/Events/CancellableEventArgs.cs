using System;

namespace sachssoft.Sasogine.Surface.Events;

public class CancellableEventArgs : EventArgs
{
    public bool Cancel { get; set; }

    public CancellableEventArgs()
    {
    }
}

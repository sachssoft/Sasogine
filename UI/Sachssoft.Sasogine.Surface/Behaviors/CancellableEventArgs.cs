using System;

namespace Sachssoft.Sasogine.Surface.Behaviors;

public class CancellableEventArgs : EventArgs
{
    public bool Cancel { get; set; }

    public CancellableEventArgs()
    {
    }
}

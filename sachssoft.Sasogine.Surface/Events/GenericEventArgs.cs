using System;

namespace sachssoft.Sasogine.Surface.Events;

public sealed class GenericEventArgs<T> : EventArgs
{
    public T Data { get; private set; }

    public GenericEventArgs(T value)
    {
        Data = value;
    }
}

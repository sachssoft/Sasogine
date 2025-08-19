using System;

namespace Sachssoft.Sasogine.Surface.Events;

public sealed class GenericEventArgs<T> : EventArgs
{
    public T Data { get; private set; }

    public GenericEventArgs(T value)
    {
        Data = value;
    }
}

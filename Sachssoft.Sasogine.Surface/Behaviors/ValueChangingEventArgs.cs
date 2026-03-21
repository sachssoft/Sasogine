namespace Sachssoft.Sasogine.Surface.Behaviors;

public class ValueChangingEventArgs<T> : CancellableEventArgs
{
    public ValueChangingEventArgs(T oldValue, T newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }

    public T OldValue
    {
        get; private set;
    }

    public T NewValue
    {
        get; set;
    }
}

/* 
 * © 2024 Tobias Sachs
 * ItemChangedEventArgs
 * 11.07.2024 
 * Updated 24.05.2025
*/

using System;

namespace sachssoft.Sasogine.Elements;

public class ItemChangedEventArgs : EventArgs
{

    public ItemChangedEventArgs(IIdentifiable source, object? old_value, object? new_value)
    {
        Source = source;
        OldValue = old_value;
        NewValue = new_value;
    }

    public IIdentifiable Source
    {
        get;
    }

    public object? OldValue
    {
        get;
    }

    public object? NewValue
    {
        get;
    }

}

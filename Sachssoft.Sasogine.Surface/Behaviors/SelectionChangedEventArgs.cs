using System;

namespace Sachssoft.Sasogine.Surface.Behaviors;

public class SelectionChangedEventArgs : EventArgs
{
    public int[] OldIndices
    {
        get; private set;
    }

    public int[] NewIndices
    {
        get; private set;
    }

    public SelectionChangedEventArgs(int[] oldIndices, int[] newIndices)
    {
        OldIndices = oldIndices;
        NewIndices = newIndices;
    }
}

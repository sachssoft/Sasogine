using System;

namespace Sachssoft.Sasogine.Features;

public class ProgressChangedEventArgs : EventArgs
{
    public ProgressChangedEventArgs(float percent)
    {
        Percent = percent;
    }

    public float Percent
    {
        get;
    }
}

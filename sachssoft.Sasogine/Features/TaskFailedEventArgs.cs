using System;

namespace sachssoft.Sasogine.Features;

public class TaskFailedEventArgs : EventArgs
{
    public int TaskIndex { get; }
    public Exception? Exception { get; }

    public TaskFailedEventArgs(int task_index, Exception? exception)
    {
        TaskIndex = task_index;
        Exception = exception;
    }
}

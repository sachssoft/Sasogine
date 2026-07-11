using System;

namespace Sachssoft.Sasogine.Common.Schedule;

public interface IScheduledOperation
{
    void Activate();
    void Update();

    bool IsCompleted { get; }
    bool IsLoading { get; }
    bool HasError { get; }
    Exception? Error { get; }
    object? Result { get; }
}
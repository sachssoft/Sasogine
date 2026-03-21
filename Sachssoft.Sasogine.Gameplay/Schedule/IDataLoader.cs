using System;

namespace Sachssoft.Sasogine.Schedule;

public interface IDataLoader
{
    void Activate();
    void Update();

    bool IsCompleted { get; }
    bool IsLoading { get; }
    bool HasError { get; }
    Exception? Error { get; }
    object? Result { get; }
}
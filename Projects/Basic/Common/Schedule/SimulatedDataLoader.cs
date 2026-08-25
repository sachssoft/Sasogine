using System;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Common.Schedule;

/// <summary>
/// Simuliert einen DataLoader mit vorgegebenem Ergebnis und künstlicher Verzögerung.
/// Nützlich für Tests oder als Platzhalter ohne echte Datenquelle.
/// </summary>
/// 

public class SimulatedDataLoader<T> : IScheduledOperation
{
    private readonly T _result;
    private readonly int _delayMilliseconds;
    private Task? _task;
    private Exception? _error;

    public bool IsCompleted { get; private set; }
    public bool IsLoading { get; private set; }
    public bool HasError => _error != null;
    public Exception? Error => _error;
    public object? Result => _result;

    public SimulatedDataLoader(T result, int delayMilliseconds = 1000)
    {
        _result = result;
        _delayMilliseconds = delayMilliseconds;
    }

    public SimulatedDataLoader(int delayMilliseconds = 1000)
    {
        _result = default;
        _delayMilliseconds = delayMilliseconds;
    }

    public void Activate()
    {
        if (IsLoading || IsCompleted)
            return;

        IsLoading = true;
        _error = null;
        _task = SimulateLoadAsync();
    }

    private async Task SimulateLoadAsync()
    {
        try
        {
            await Task.Delay(_delayMilliseconds);
            // Hier könnte man optional Fehler simulieren
        }
        catch (Exception ex)
        {
            _error = ex;
        }
        finally
        {
            IsCompleted = true;
            IsLoading = false;
        }
    }

    public void Update()
    {
        if (_task == null)
            return;

        if (_task.IsCompleted)
        {
            if (_task.IsFaulted)
                _error = _task.Exception?.GetBaseException();

            IsCompleted = true;
            IsLoading = false;
        }
    }
}

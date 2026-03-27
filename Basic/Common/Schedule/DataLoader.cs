using Sachssoft.Sasogine.Common.Schedule;
using System;
using System.Threading.Tasks;

public class DataLoader<T> : IDataLoader
{
    private readonly Func<Task<T>> _loadFunc;
    private Task<T>? _loadTask;
    private bool _isActivated = false;
    private int _retryCount = 0;
    private readonly int _maxRetries;
    private bool _isCompleted = false;

    public T? Result { get; private set; }
    public Exception? Error { get; private set; }
    public bool IsCompleted => _isCompleted;

    public bool IsLoading => _loadTask != null && !_loadTask.IsCompleted;
    public bool HasError => Error != null;

    public Action? Completed { get; set; }

    public Action? Failed { get; set; }

    // Explizite Implementierung für die nicht-generische Schnittstelle:
    object? IDataLoader.Result => Result;

    public DataLoader(Func<Task<T>> loadFunc, int maxRetries = 3)
    {
        _loadFunc = loadFunc;
        _maxRetries = maxRetries;
    }

    public void Activate()
    {
        if (_isActivated) return;
        _isActivated = true;
        StartLoad();
    }

    private void StartLoad()
    {
        _loadTask?.Dispose();
        _loadTask = _loadFunc();
        Error = null;
        Result = default;
    }

    public void Update()
    {
        if (!_isActivated || _loadTask == null || _isCompleted)
            return;

        if (_loadTask.IsCompleted)
        {
            if (_loadTask.IsFaulted)
            {
                Error = _loadTask.Exception?.GetBaseException();

                if (_retryCount < _maxRetries)
                {
                    _retryCount++;
                    StartLoad(); // Retry
                    return;
                }

                _isCompleted = true; // Fehlerhafter Abschluss nach max. Versuchen
                Failed?.Invoke();
            }
            else
            {
                Result = _loadTask.Result;
                _isCompleted = true; // Erfolgreich abgeschlossen
                Completed?.Invoke();
            }
        }
    }

    public void Reload()
    {
        _retryCount = 0;
        StartLoad();
    }
}

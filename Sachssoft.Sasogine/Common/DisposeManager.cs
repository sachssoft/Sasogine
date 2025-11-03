using System;
using System.Collections.Generic;

public class DisposeManager : IDisposable
{
    private readonly List<IDisposable> _disposables = new();
    private bool _disposed;

    public T Register<T>(T disposable) where T : IDisposable
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(DisposeManager));

        if (disposable != null)
            _disposables.Add(disposable);

        return disposable;
    }

    public void Dispose()
    {
        if (_disposed) return;

        foreach (var d in _disposables)
        {
            try { d.Dispose(); } catch { }
        }

        _disposables.Clear();
        _disposed = true;
    }
}

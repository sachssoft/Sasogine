using System;
using System.Collections.Generic;

namespace sachssoft.Sasogine.Schedule;

public class DataLoaderSequence<TKey> where TKey : notnull
{
    // Statt Wrapper: direkt IDataLoader (nicht generisch)
    private readonly Dictionary<TKey, IDataLoader> _loaders = new();
    private readonly List<TKey> _keysInOrder = new();

    private int _currentIndex = -1;
    private bool _isActivated = false;

    public bool IsCompleted { get; private set; }
    public bool IsLoading { get; private set; }
    public Exception? Error { get; private set; }
    public bool HasError => Error != null;

    private readonly Dictionary<TKey, object?> _results = new();
    private readonly Dictionary<TKey, Action<object?>> _completedCallbacks = new();
    private readonly Dictionary<TKey, Action<Exception?, object?>> _failedCallbacks = new();

    // Fügt einen DataLoader<T> hinzu, aber speichert als IDataLoader Interface

    public void Add<T>(TKey key, DataLoader<T> loader)
    {
        Add(key, loader, (obj) => { }, (ex, obj) => { /* default: nichts tun */ });
    }

    public void Add<T>(TKey key, DataLoader<T> loader, Action<T> completed)
    {
        Add(key, loader, completed, (ex, obj) => { /* default: nichts tun */ });
    }

    public void Add<T>(TKey key, DataLoader<T> loader, Action<T> completed, Action<Exception?, T?> failed)
    {
        if (_loaders.ContainsKey(key))
            throw new ArgumentException($"Key {key} already added");

        _loaders[key] = loader;
        _completedCallbacks[key] = obj => completed((T)obj!);
        _failedCallbacks[key] = (ex, obj) => failed(ex, (T?)obj);
        _keysInOrder.Add(key);
    }

    public void Add(TKey key, IDataLoader loader, Action<IDataLoader> completed, Action<Exception?, IDataLoader?> failed)
    {
        if (_loaders.ContainsKey(key))
            throw new ArgumentException($"Key {key} already added");

        _loaders[key] = loader;
        _completedCallbacks[key] = obj => completed((IDataLoader)obj!);
        _failedCallbacks[key] = (ex, obj) => failed(ex, (IDataLoader?)obj);
        _keysInOrder.Add(key);
    }

    public void Activate()
    {
        if (_isActivated) return;

        _isActivated = true;
        IsLoading = true;
        _currentIndex = 0;
        StartCurrentLoader();
    }

    private void StartCurrentLoader()
    {
        if (_currentIndex >= _keysInOrder.Count)
        {
            IsLoading = false;
            IsCompleted = true;
            return;
        }

        var key = _keysInOrder[_currentIndex];
        var loader = _loaders[key];

        loader.Activate();
    }

    public void Update()
    {
        if (!IsLoading || IsCompleted || HasError) return;

        var key = _keysInOrder[_currentIndex];
        var loader = _loaders[key];

        loader.Update();

        if (loader.HasError)
        {
            Error = loader.Error;

            if (_failedCallbacks.TryGetValue(key, out var failedCallback))
            {
                failedCallback(Error, loader.Result);
            }

            IsLoading = false;
            return;
        }

        if (loader.IsCompleted)
        {
            var result = loader.Result;
            _results[key] = result;

            if (_completedCallbacks.TryGetValue(key, out var callback))
                callback(result);

            _currentIndex++;
            StartCurrentLoader();
        }
    }

    public T? GetResult<T>(TKey key) where T : class
    {
        if (_results.TryGetValue(key, out var resultObj))
            return resultObj as T;
        return default;
    }
}

using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Common.Schedule;

public class DataLoaderSequence<TKey> where TKey : notnull
{
    private readonly Dictionary<TKey, IScheduledOperation> _loaders = new();
    private readonly List<TKey> _keysInOrder = new();
    private Action? _allCompletedCallback;
    private int _currentIndex = -1;
    private bool _isActivated = false;

    public bool IsCompleted { get; private set; }
    public bool IsLoading { get; private set; }
    public Exception? Error { get; private set; }
    public bool HasError => Error != null;
    public TKey CurrentKey => _keysInOrder[_currentIndex];

    private readonly Dictionary<TKey, object?> _results = new();
    private readonly Dictionary<TKey, Action<object?>> _completedCallbacks = new();
    private readonly Dictionary<TKey, Action<Exception?, object?>> _failedCallbacks = new();

    public void Add<T>(TKey key, DataLoader<T> loader)
    {
        Add(key, loader, (obj) => { }, (ex, obj) => { });
    }

    public void Add<T>(TKey key, DataLoader<T> loader, Action<T> completed)
    {
        Add(key, loader, completed, (ex, obj) => { });
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

    public void Add(TKey key, IScheduledOperation loader, Action<IScheduledOperation> completed, Action<Exception?, IScheduledOperation?> failed)
    {
        if (_loaders.ContainsKey(key))
            throw new ArgumentException($"Key {key} already added");

        _loaders[key] = loader;
        _completedCallbacks[key] = obj => completed((IScheduledOperation)obj!);
        _failedCallbacks[key] = (ex, obj) => failed(ex, (IScheduledOperation?)obj);
        _keysInOrder.Add(key);
    }

    public void Add(TKey key, IScheduledOperation loader)
    {
        Add(key, loader, (obj) => { }, (ex, obj) => { });
    }

    public void Add(TKey key, IScheduledOperation loader, Action<IScheduledOperation> completed)
    {
        Add(key, loader, completed, (ex, obj) => { });
    }

    public void SetAllCompletedCallback(Action callback)
    {
        _allCompletedCallback = callback;
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
            _allCompletedCallback?.Invoke();
            return;
        }

        var key = _keysInOrder[_currentIndex];
        var loader = _loaders[key];

        loader.Activate();
    }

    public void Update()
    {
        if (!IsLoading || IsCompleted || HasError)
        {
            return;
        }

        var key = _keysInOrder[_currentIndex];
        var loader = _loaders[key];

        try
        {
            loader.Update();
        }
        catch (Exception ex)
        {
            Error = ex;

            if (_failedCallbacks.TryGetValue(key, out var failedCallback))
            {
                failedCallback(ex, loader.Result);
            }

            IsLoading = false;
            return;
        }

        if (loader.HasError)
        {
            Error = loader.Error;

            if (_failedCallbacks.TryGetValue(key, out var failedCallback))
            {
                failedCallback(Error, loader.Result);
            }
            else
            {
                throw Error!;
            }

            IsLoading = false;
            return;
        }

        if (loader.IsCompleted)
        {
            var result = loader.Result;
            _results[_keysInOrder[_currentIndex]] = result;

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

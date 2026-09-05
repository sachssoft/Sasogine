using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Scenes;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Common.Schedule;

/// <summary>
/// Executes a sequence of scheduled loading operations in a defined order
/// and stores their results by key.
/// </summary>
/// <typeparam name="TKey">
/// The type used to identify scheduled loading operations.
/// </typeparam>
public sealed class DataLoaderSequence<TKey>
    where TKey : notnull
{
    private readonly Dictionary<TKey, IScheduledOperation> _loaders = new();
    private readonly List<TKey> _keysInOrder = new();
    private readonly Dictionary<TKey, object?> _results = new();
    private readonly Dictionary<TKey, Action<object?>> _completedCallbacks = new();
    private readonly Dictionary<TKey, Action<Exception?, object?>> _failedCallbacks = new();

    private Action? _allCompletedCallback;
    private int _currentIndex = -1;
    private bool _isActivated;

    /// <summary>
    /// Gets a value indicating whether all scheduled loading operations
    /// have completed.
    /// </summary>
    public bool IsCompleted { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the sequence is currently loading.
    /// </summary>
    public bool IsLoading { get; private set; }

    /// <summary>
    /// Gets the error that caused the loading sequence to stop.
    /// </summary>
    public Exception? Error { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the loading sequence has encountered an error.
    /// </summary>
    public bool HasError => Error != null;

    /// <summary>
    /// Gets the key of the currently active loading operation.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no loading operation is currently active.
    /// </exception>
    public TKey CurrentKey
    {
        get
        {
            if (_currentIndex < 0 ||
                _currentIndex >= _keysInOrder.Count)
            {
                throw new InvalidOperationException(
                    "No loading operation is currently active.");
            }

            return _keysInOrder[_currentIndex];
        }
    }

    /// <summary>
    /// Adds a data loader to the sequence.
    /// </summary>
    /// <typeparam name="T">
    /// The type of data produced by the loader.
    /// </typeparam>
    /// <param name="key">
    /// The key used to identify the loader.
    /// </param>
    /// <param name="loader">
    /// The loader to add.
    /// </param>
    public void Add<T>(
        TKey key,
        DataLoader<T> loader)
    {
        Add(
            key,
            loader,
            _ => { },
            (_, _) => { });
    }

    /// <summary>
    /// Adds a data loader to the sequence with a completion callback.
    /// </summary>
    /// <typeparam name="T">
    /// The type of data produced by the loader.
    /// </typeparam>
    /// <param name="key">
    /// The key used to identify the loader.
    /// </param>
    /// <param name="loader">
    /// The loader to add.
    /// </param>
    /// <param name="completed">
    /// The action invoked when loading completes successfully.
    /// </param>
    public void Add<T>(
        TKey key,
        DataLoader<T> loader,
        Action<T> completed)
    {
        Add(
            key,
            loader,
            completed,
            (_, _) => { });
    }

    /// <summary>
    /// Adds a data loader to the sequence with completion and failure callbacks.
    /// </summary>
    /// <typeparam name="T">
    /// The type of data produced by the loader.
    /// </typeparam>
    /// <param name="key">
    /// The key used to identify the loader.
    /// </param>
    /// <param name="loader">
    /// The loader to add.
    /// </param>
    /// <param name="completed">
    /// The action invoked when loading completes successfully.
    /// </param>
    /// <param name="failed">
    /// The action invoked when loading fails.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="loader"/>, <paramref name="completed"/>,
    /// or <paramref name="failed"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the specified key has already been added.
    /// </exception>
    public void Add<T>(
        TKey key,
        DataLoader<T> loader,
        Action<T> completed,
        Action<Exception?, T?> failed)
    {
        ArgumentNullException.ThrowIfNull(loader);
        ArgumentNullException.ThrowIfNull(completed);
        ArgumentNullException.ThrowIfNull(failed);

        EnsureKeyAvailable(key);

        _loaders.Add(key, loader);

        _completedCallbacks.Add(
            key,
            result => completed((T)result!));

        _failedCallbacks.Add(
            key,
            (exception, result) =>
                failed(exception, (T?)result));

        _keysInOrder.Add(key);
    }

    /// <summary>
    /// Adds a scheduled operation to the sequence.
    /// </summary>
    /// <param name="key">
    /// The key used to identify the operation.
    /// </param>
    /// <param name="loader">
    /// The operation to add.
    /// </param>
    public void Add(
        TKey key,
        IScheduledOperation loader)
    {
        Add(
            key,
            loader,
            _ => { },
            (_, _) => { });
    }

    /// <summary>
    /// Adds a scheduled operation to the sequence with a completion callback.
    /// </summary>
    /// <param name="key">
    /// The key used to identify the operation.
    /// </param>
    /// <param name="loader">
    /// The operation to add.
    /// </param>
    /// <param name="completed">
    /// The action invoked when the operation completes successfully.
    /// </param>
    public void Add(
        TKey key,
        IScheduledOperation loader,
        Action<IScheduledOperation> completed)
    {
        Add(
            key,
            loader,
            completed,
            (_, _) => { });
    }

    /// <summary>
    /// Adds a scheduled operation to the sequence with completion
    /// and failure callbacks.
    /// </summary>
    /// <param name="key">
    /// The key used to identify the operation.
    /// </param>
    /// <param name="loader">
    /// The operation to add.
    /// </param>
    /// <param name="completed">
    /// The action invoked when the operation completes successfully.
    /// </param>
    /// <param name="failed">
    /// The action invoked when the operation fails.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="loader"/>, <paramref name="completed"/>,
    /// or <paramref name="failed"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the specified key has already been added.
    /// </exception>
    public void Add(
        TKey key,
        IScheduledOperation loader,
        Action<IScheduledOperation> completed,
        Action<Exception?, IScheduledOperation?> failed)
    {
        ArgumentNullException.ThrowIfNull(loader);
        ArgumentNullException.ThrowIfNull(completed);
        ArgumentNullException.ThrowIfNull(failed);

        EnsureKeyAvailable(key);

        _loaders.Add(key, loader);

        _completedCallbacks.Add(
            key,
            _ => completed(loader));

        _failedCallbacks.Add(
            key,
            (exception, _) => failed(exception, loader));

        _keysInOrder.Add(key);
    }

    /// <summary>
    /// Sets the callback invoked after all scheduled operations
    /// have completed successfully.
    /// </summary>
    /// <param name="callback">
    /// The callback to invoke.
    /// </param>
    public void SetAllCompletedCallback(Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        _allCompletedCallback = callback;
    }

    /// <summary>
    /// Activates the loading sequence.
    /// </summary>
    public void Activate()
    {
        if (_isActivated)
            return;

        _isActivated = true;
        IsLoading = true;
        IsCompleted = false;
        Error = null;

        _currentIndex = 0;

        StartCurrentLoader();
    }

    /// <summary>
    /// Updates the loading sequence using the specified game time.
    /// </summary>
    /// <param name="gameTime">
    /// Provides timing information for the current update cycle.
    /// </param>
    public void Update(GameTime gameTime)
    {
        ArgumentNullException.ThrowIfNull(gameTime);

        UpdateCore(loader => loader.Update(gameTime));
    }

    /// <summary>
    /// Updates the loading sequence using the specified scene update context.
    /// </summary>
    /// <param name="context">
    /// Provides information about the current scene update.
    /// </param>
    public void Update(SceneUpdateContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        UpdateCore(loader => loader.Update(context.GameTime));
    }

    /// <summary>
    /// Gets the result associated with the specified key.
    /// </summary>
    /// <typeparam name="T">
    /// The expected result type.
    /// </typeparam>
    /// <param name="key">
    /// The key of the loading operation.
    /// </param>
    /// <returns>
    /// The stored result, or <see langword="null"/> if no compatible result exists.
    /// </returns>
    public T? GetResult<T>(TKey key)
        where T : class
    {
        if (_results.TryGetValue(key, out var result))
            return result as T;

        return default;
    }

    private void UpdateCore(
        Action<IScheduledOperation> update)
    {
        if (!IsLoading ||
            IsCompleted ||
            HasError)
        {
            return;
        }

        var key = _keysInOrder[_currentIndex];
        var loader = _loaders[key];

        try
        {
            update(loader);
        }
        catch (Exception exception)
        {
            HandleFailure(
                key,
                loader,
                exception);

            return;
        }

        if (loader.HasError)
        {
            HandleFailure(
                key,
                loader,
                loader.Error);

            return;
        }

        if (!loader.IsCompleted)
            return;

        var result = loader.Result;

        _results[key] = result;

        if (_completedCallbacks.TryGetValue(
            key,
            out var completedCallback))
        {
            completedCallback(result);
        }

        _currentIndex++;

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

    private void HandleFailure(
        TKey key,
        IScheduledOperation loader,
        Exception? exception)
    {
        Error = exception ??
            new InvalidOperationException(
                "The scheduled operation failed without providing an exception.");

        IsLoading = false;

        if (_failedCallbacks.TryGetValue(
            key,
            out var failedCallback))
        {
            failedCallback(
                Error,
                loader.Result);
        }
    }

    private void EnsureKeyAvailable(TKey key)
    {
        if (_loaders.ContainsKey(key))
        {
            throw new ArgumentException(
                $"The key '{key}' has already been added.",
                nameof(key));
        }
    }
}
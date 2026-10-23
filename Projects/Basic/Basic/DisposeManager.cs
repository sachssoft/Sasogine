using System;
using System.Collections.Generic;

namespace Sachssoft.Engine;

/// <summary>
/// Provides centralized lifetime management for disposable resources and cleanup actions.
/// Registered resources are disposed in reverse registration order.
/// </summary>
/// <remarks>
/// Each disposable resource can be registered only once.
/// Cleanup actions are executed when the manager is disposed.
/// The manager continues disposing remaining resources even if individual
/// disposal operations throw exceptions.
/// </remarks>
public class DisposeManager : IDisposable
{
    private readonly object _sync = new();
    private readonly List<IDisposable> _disposables = new();
    private bool _disposed;

    /// <summary>
    /// Gets a snapshot of the currently registered disposable resources.
    /// </summary>
    protected IEnumerable<IDisposable> Disposables
    {
        get
        {
            lock (_sync)
                return _disposables.ToArray();
        }
    }

    /// <summary>
    /// Gets a value indicating whether this manager has been disposed.
    /// </summary>
    public bool IsDisposed
    {
        get
        {
            lock (_sync)
                return _disposed;
        }
    }

    /// <summary>
    /// Registers a disposable resource for automatic disposal.
    /// </summary>
    /// <typeparam name="T">The type of the disposable resource.</typeparam>
    /// <param name="disposable">The resource to register.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="disposable"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the resource has already been registered.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the manager has already been disposed.
    /// </exception>
    public void Register<T>(T disposable)
        where T : IDisposable
    {
        ArgumentNullException.ThrowIfNull(disposable);

        lock (_sync)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            foreach (IDisposable existing in _disposables)
            {
                if (ReferenceEquals(existing, disposable))
                {
                    throw new ArgumentException(
                        "The disposable resource has already been registered.",
                        nameof(disposable));
                }
            }

            _disposables.Add(disposable);
        }
    }

    /// <summary>
    /// Creates and registers a disposable resource using the specified factory.
    /// </summary>
    /// <param name="factory">The factory used to create the resource.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the factory is null or returns null.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the manager has already been disposed.
    /// </exception>
    public void Register(Func<IDisposable> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        lock (_sync)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            IDisposable disposable = factory()
                ?? throw new ArgumentNullException(
                    nameof(factory),
                    "The disposable factory returned null.");

            try
            {
                Register(disposable);
            }
            catch
            {
                // The manager could not take ownership of the resource.
                // Release it immediately to prevent a resource leak.
                disposable.Dispose();
                throw;
            }
        }
    }

    /// <summary>
    /// Registers an action to be executed when the manager is disposed.
    /// </summary>
    /// <param name="disposeAction">The cleanup action to execute.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="disposeAction"/> is null.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the manager has already been disposed.
    /// </exception>
    public void Register(Action disposeAction)
    {
        ArgumentNullException.ThrowIfNull(disposeAction);

        Register(new ActionDisposable(disposeAction));
    }

    /// <summary>
    /// Disposes all registered resources in reverse registration order.
    /// </summary>
    /// <remarks>
    /// Disposal continues even if individual resources throw exceptions.
    /// All encountered exceptions are collected and thrown as an
    /// <see cref="AggregateException"/> after disposal completes.
    /// Calling this method more than once has no effect.
    /// </remarks>
    /// <exception cref="AggregateException">
    /// Thrown when one or more resources fail during disposal.
    /// </exception>
    public virtual void Dispose()
    {
        IDisposable[] disposables;

        lock (_sync)
        {
            if (_disposed)
                return;

            _disposed = true;

            disposables = _disposables.ToArray();
            _disposables.Clear();
        }

        List<Exception>? exceptions = null;

        for (int i = disposables.Length - 1; i >= 0; i--)
        {
            try
            {
                disposables[i].Dispose();
            }
            catch (Exception ex)
            {
                (exceptions ??= new List<Exception>()).Add(ex);
            }
        }

        if (exceptions is { Count: > 0 })
        {
            throw new AggregateException(
                "One or more resources failed during disposal.",
                exceptions);
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Wraps a cleanup action as a disposable resource.
    /// </summary>
    private sealed class ActionDisposable : IDisposable
    {
        private Action? _action;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionDisposable"/> class.
        /// </summary>
        /// <param name="action">The cleanup action.</param>
        public ActionDisposable(Action action)
        {
            _action = action;
        }

        /// <summary>
        /// Executes the cleanup action at most once.
        /// </summary>
        public void Dispose()
        {
            Action? action = System.Threading.Interlocked.Exchange(
                ref _action,
                null);

            action?.Invoke();
        }
    }
}
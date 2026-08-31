using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Provides centralized lifetime management for disposable resources.
    ///
    /// Registered <see cref="IDisposable"/> instances are automatically disposed
    /// in reverse registration order when the manager itself is disposed.
    /// </summary>
    public class DisposeManager : IDisposable
    {
        private readonly List<IDisposable> _disposables = new();
        private bool _disposed;

        /// <summary>
        /// Gets a snapshot of the currently registered disposable resources.
        /// </summary>
        protected IEnumerable<IDisposable> Disposables => _disposables.ToArray();

        /// <summary>
        /// Gets whether this manager has already been disposed.
        /// </summary>
        public bool IsDisposed => _disposed;

        /// <summary>
        /// Registers a disposable resource for automatic disposal.
        /// </summary>
        /// <typeparam name="T">Type of the disposable resource.</typeparam>
        /// <param name="disposable">Resource to register.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="disposable"/> is null.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the manager has already been disposed.
        /// </exception>
        public void Register<T>(T disposable)
            where T : IDisposable
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            ArgumentNullException.ThrowIfNull(disposable);

            _disposables.Add(disposable);
        }

        /// <summary>
        /// Creates and registers a disposable resource for automatic disposal.
        /// </summary>
        /// <param name="factory">
        /// Factory used to create the disposable resource.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="factory"/> is null or returns null.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the manager has already been disposed.
        /// </exception>
        public void Register(Func<IDisposable> factory)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            ArgumentNullException.ThrowIfNull(factory);

            var disposable = factory();

            if (disposable == null)
            {
                throw new ArgumentNullException(
                    nameof(factory),
                    "The disposable factory returned null.");
            }

            _disposables.Add(disposable);
        }

        /// <summary>
        /// Disposes all registered resources in reverse registration order
        /// and clears the manager.
        ///
        /// Calling this method more than once has no effect.
        /// </summary>
        public virtual void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            try
            {
                for (var i = _disposables.Count - 1; i >= 0; i--)
                {
                    _disposables[i].Dispose();
                }
            }
            finally
            {
                _disposables.Clear();
            }
        }
    }
}
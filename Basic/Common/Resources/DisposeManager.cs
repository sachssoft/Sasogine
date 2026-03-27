using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Common
{
    public class DisposeManager : IDisposable
    {
        private readonly List<IDisposable> _disposables = new();
        private bool _disposed;

        protected IEnumerable<IDisposable> Disposables => _disposables.ToArray();

        public bool IsDisposed => _disposed;

        public void Register<T>(T disposable) where T : IDisposable
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DisposeManager));

            if (disposable != null)
                _disposables.Add(disposable);
        }

        public void Register(Func<IDisposable> disposeAction)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DisposeManager));

            if (disposeAction != null)
                _disposables.Add(disposeAction.Invoke());
        }

        public virtual void Dispose()
        {
            if (_disposed) return;

            foreach (var d in _disposables)
            {
                d.Dispose();
            }

            _disposables.Clear();
            _disposed = true;
        }
    }
}

using System;
using System.Threading;

namespace Sachssoft.Sasogine.Common.Performance
{
    public sealed class DirectLazy<T>
        where T : class
    {
        private T? _value;
        private readonly Func<T> _factory;
        private readonly bool _threadSafe;

        public DirectLazy(Func<T> factory, bool threadSafe = false)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _threadSafe = threadSafe;
        }

        public T Value
        {
            get
            {
                if (_value != null) return _value;

                if (_threadSafe)
                {
                    var newValue = _factory();
                    var temp = Interlocked.CompareExchange(ref _value, newValue, null);
                    return temp ?? newValue;
                }
                else
                {
                    return _value ??= _factory();
                }
            }
        }

        public bool IsValueCreated => _value != null;
    }
}
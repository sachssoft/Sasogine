using System;

namespace Sachssoft.Sasogine.Common.Performance
{
    /// <summary>
    /// Provides thread-safe lazy initialization for reference types.
    ///
    /// The value is created only when <see cref="Value"/> is accessed for the first time.
    /// When multiple threads access the value concurrently, the factory is guaranteed
    /// to execute only once.
    /// </summary>
    /// <typeparam name="T">Reference type of the lazily initialized value.</typeparam>
    public sealed class ThreadSafeLazy<T>
        where T : class
    {
        private readonly Func<T> _factory;
        private readonly object _lock = new();

        private T? _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadSafeLazy{T}"/> class.
        /// </summary>
        /// <param name="factory">
        /// Factory used to create the value when it is accessed for the first time.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="factory"/> is null.
        /// </exception>
        public ThreadSafeLazy(Func<T> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// Gets the lazily initialized value.
        ///
        /// If multiple threads access this property concurrently before initialization,
        /// only one thread executes the factory.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the factory returns null.
        /// </exception>
        public T Value
        {
            get
            {
                if (_value != null)
                    return _value;

                lock (_lock)
                {
                    return _value ??= CreateValue();
                }
            }
        }

        /// <summary>
        /// Gets whether the value has already been created.
        /// </summary>
        public bool IsValueCreated => _value != null;

        private T CreateValue()
        {
            return _factory()
                ?? throw new InvalidOperationException(
                    $"The {nameof(ThreadSafeLazy<T>)} factory returned null.");
        }
    }
}
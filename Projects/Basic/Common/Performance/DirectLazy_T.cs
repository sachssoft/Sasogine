using System;

namespace Sachssoft.Engine.Common.Performance
{
    /// <summary>
    /// Provides lightweight lazy initialization for reference types.
    ///
    /// The value is created only when <see cref="Value"/> is accessed for the first time.
    /// This implementation is not thread-safe and is intended for performance-sensitive
    /// scenarios where synchronization is not required.
    /// </summary>
    /// <typeparam name="T">Reference type of the lazily initialized value.</typeparam>
    public sealed class DirectLazy<T>
        where T : class
    {
        private readonly Func<T> _factory;
        private T? _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectLazy{T}"/> class.
        /// </summary>
        /// <param name="factory">
        /// Factory used to create the value when it is accessed for the first time.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="factory"/> is null.
        /// </exception>
        public DirectLazy(Func<T> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// Gets the lazily initialized value.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the factory returns null.
        /// </exception>
        public T Value => _value ??= CreateValue();

        /// <summary>
        /// Gets whether the value has already been created.
        /// </summary>
        public bool IsValueCreated => _value != null;

        private T CreateValue()
        {
            return _factory()
                ?? throw new InvalidOperationException(
                    $"The {nameof(DirectLazy<T>)} factory returned null.");
        }
    }
}
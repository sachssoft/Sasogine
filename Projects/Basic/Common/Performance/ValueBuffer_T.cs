using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Common.Performance
{
    /// <summary>
    /// Lightweight buffer holding old and new values.
    ///
    /// Tracks changes of a value frame-by-frame or per update.
    /// The first assignment via <see cref="EnsureChange"/> always marks a change
    /// to ensure initialization logic runs.
    ///
    /// Advantages over events:
    /// - Very performant, no delegates or events
    /// - Struct-based, no heap allocation for the buffer itself
    /// - Works with any type
    /// - Ideal for frequent updates in game loops
    /// </summary>
    /// <typeparam name="T">Type of the buffered value.</typeparam>
    public struct ValueBuffer<T> : IEquatable<ValueBuffer<T>>
    {
        private bool _wasEnsured;

        /// <summary>
        /// Previous value.
        /// </summary>
        public T OldValue { get; private set; }

        /// <summary>
        /// Current value.
        /// </summary>
        public T NewValue { get; private set; }

        /// <summary>
        /// True if OldValue and NewValue differ.
        /// </summary>
        public readonly bool HasChanged =>
            !EqualityComparer<T>.Default.Equals(OldValue, NewValue);

        /// <summary>
        /// True if the buffer has already been initialized or ensured.
        /// </summary>
        public readonly bool WasEnsured => _wasEnsured;

        /// <summary>
        /// Initializes an empty buffer.
        /// </summary>
        public ValueBuffer()
        {
            OldValue = default!;
            NewValue = default!;
            _wasEnsured = false;
        }

        /// <summary>
        /// Initializes the buffer with the specified value.
        /// OldValue and NewValue are initialized with the same value.
        /// </summary>
        public ValueBuffer(T value)
        {
            OldValue = value;
            NewValue = value;
            _wasEnsured = true;
        }

        /// <summary>
        /// Explicitly sets a new value.
        /// OldValue becomes the previous NewValue.
        /// </summary>
        public void Set(T value)
        {
            OldValue = NewValue;
            NewValue = value;
            _wasEnsured = true;
        }

        /// <summary>
        /// Checks whether the specified value differs from the current value.
        ///
        /// Returns true when the value changed or when the buffer
        /// has not yet been initialized.
        /// </summary>
        public bool EnsureChange(T value)
        {
            if (_wasEnsured &&
                EqualityComparer<T>.Default.Equals(NewValue, value))
                return false;

            OldValue = NewValue;
            NewValue = value;
            _wasEnsured = true;

            return true;
        }

        /// <summary>
        /// Resets OldValue and NewValue to the specified value.
        /// The buffer is considered initialized afterwards.
        /// </summary>
        public void Reset(T value)
        {
            OldValue = value;
            NewValue = value;
            _wasEnsured = true;
        }

        /// <summary>
        /// Checks whether the value changed and consumes the change.
        ///
        /// Returns true only once for the current change.
        /// </summary>
        public bool ConsumeChange()
        {
            if (!HasChanged)
                return false;

            OldValue = NewValue;
            return true;
        }

        /// <summary>
        /// Returns the current value.
        /// </summary>
        public readonly T Get() => NewValue;

        /// <inheritdoc/>
        public readonly bool Equals(ValueBuffer<T> other) =>
            EqualityComparer<T>.Default.Equals(NewValue, other.NewValue);

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj) =>
            obj is ValueBuffer<T> other && Equals(other);

        /// <inheritdoc/>
        public override readonly int GetHashCode() =>
            NewValue is null
                ? 0
                : EqualityComparer<T>.Default.GetHashCode(NewValue);

        /// <summary>
        /// Implicitly creates a ValueBuffer from a value.
        /// </summary>
        public static implicit operator ValueBuffer<T>(T value) =>
            new(value);

        /// <summary>
        /// Implicitly returns the current value.
        /// </summary>
        public static implicit operator T(ValueBuffer<T> buffer) =>
            buffer.NewValue;

        /// <summary>
        /// Compares two buffers by their current value.
        /// </summary>
        public static bool operator ==(
            ValueBuffer<T> left,
            ValueBuffer<T> right) =>
            left.Equals(right);

        /// <summary>
        /// Compares two buffers by their current value.
        /// </summary>
        public static bool operator !=(
            ValueBuffer<T> left,
            ValueBuffer<T> right) =>
            !left.Equals(right);

        /// <summary>
        /// Compares the current buffer value with a value of type T.
        /// </summary>
        public static bool operator ==(
            ValueBuffer<T> left,
            T right) =>
            EqualityComparer<T>.Default.Equals(left.NewValue, right);

        /// <summary>
        /// Compares the current buffer value with a value of type T.
        /// </summary>
        public static bool operator !=(
            ValueBuffer<T> left,
            T right) =>
            !EqualityComparer<T>.Default.Equals(left.NewValue, right);

        /// <summary>
        /// Compares a value of type T with the current buffer value.
        /// </summary>
        public static bool operator ==(
            T left,
            ValueBuffer<T> right) =>
            EqualityComparer<T>.Default.Equals(left, right.NewValue);

        /// <summary>
        /// Compares a value of type T with the current buffer value.
        /// </summary>
        public static bool operator !=(
            T left,
            ValueBuffer<T> right) =>
            !EqualityComparer<T>.Default.Equals(left, right.NewValue);

        /// <inheritdoc/>
        public override readonly string ToString() =>
            $"Old: {OldValue}, New: {NewValue}, Ensured: {_wasEnsured}";
    }
}
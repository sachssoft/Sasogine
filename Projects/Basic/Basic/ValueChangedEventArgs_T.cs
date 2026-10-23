using System;

namespace Sachssoft.Engine;

/// <summary>
/// Provides event data for a value that has changed.
/// </summary>
/// <typeparam name="T">The type of the value that changed.</typeparam>
public class ValueChangedEventArgs<T> : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueChangedEventArgs{T}"/> class.
    /// </summary>
    /// <param name="oldValue">
    /// The value before the change, or <see langword="null"/> if no previous value existed.
    /// </param>
    /// <param name="newValue">
    /// The value after the change, or <see langword="null"/> if the value was cleared.
    /// </param>
    public ValueChangedEventArgs(T? oldValue, T? newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }

    /// <summary>
    /// Gets the value before the change, or <see langword="null"/> if no previous value existed.
    /// </summary>
    public T? OldValue { get; }

    /// <summary>
    /// Gets the value after the change, or <see langword="null"/> if the value was cleared.
    /// </summary>
    public T? NewValue { get; }
}
using System.ComponentModel;

namespace Sachssoft.Engine;

/// <summary>
/// Provides event data for a value that is about to change,
/// including an option to cancel the change.
/// </summary>
/// <typeparam name="T">The type of the value being changed.</typeparam>
public class ValueChangingEventArgs<T> : CancelEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueChangingEventArgs{T}"/> class.
    /// </summary>
    /// <param name="oldValue">
    /// The value before the change, or <see langword="null"/> if no previous value exists.
    /// </param>
    /// <param name="newValue">
    /// The proposed new value, or <see langword="null"/> if the value is being cleared.
    /// </param>
    public ValueChangingEventArgs(T? oldValue, T? newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }

    /// <summary>
    /// Gets the value before the change, or <see langword="null"/> if no previous value exists.
    /// </summary>
    public T? OldValue { get; }

    /// <summary>
    /// Gets the proposed new value, or <see langword="null"/> if the value is being cleared.
    /// </summary>
    public T? NewValue { get; }
}
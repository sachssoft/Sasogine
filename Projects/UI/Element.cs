
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Sachssoft.Engine.Controls;

/// <summary>
/// Provides a base implementation for elements that notify subscribers
/// when their property values change.
/// </summary>
/// <remarks>
/// Implements <see cref="INotifyPropertyChanged"/> and provides helper methods
/// for updating fields, raising property change notifications, and allowing
/// derived elements to react to property changes.
/// </remarks>
public abstract class Element : INotifyPropertyChanged
{
    /// <summary>
    /// Occurs when an effective property value has changed.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Updates a field when its value changes and raises the supplied property change notification.
    /// </summary>
    /// <typeparam name="T">The type of the field being updated.</typeparam>
    /// <param name="field">A reference to the field whose value may be updated.</param>
    /// <param name="value">The new value to assign to the field.</param>
    /// <param name="notification">The cached event arguments identifying the changed property.</param>
    /// <returns>
    /// <see langword="true"/> if the field value changed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected bool SetNotify<T>(
        ref T field,
        T value,
        PropertyChangedEventArgs notification)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;

        NotifyPropertyChanged(notification);
        return true;
    }

    /// <summary>
    /// Raises a property change notification and invokes the corresponding
    /// notification handler before notifying subscribers.
    /// </summary>
    /// <param name="notification">The cached event arguments identifying the changed property.</param>
    protected void NotifyPropertyChanged(PropertyChangedEventArgs notification)
    {
        OnPropertyChanged(notification);
        PropertyChanged?.Invoke(this, notification);
    }

    /// <summary>
    /// Provides an extension point for derived elements to react to property changes
    /// before subscribers receive the corresponding notification.
    /// </summary>
    /// <param name="notification">The event arguments identifying the changed property.</param>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs notification)
    {
    }

    /// <summary>
    /// Forwards a collection-related property change notification to the element's notification pipeline.
    /// </summary>
    /// <param name="notification">The cached event arguments identifying the affected property.</param>
    internal void CollectionChanged(PropertyChangedEventArgs notification)
    {
        NotifyPropertyChanged(notification);
    }
}

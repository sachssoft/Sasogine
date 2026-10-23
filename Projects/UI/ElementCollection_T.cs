
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Sachssoft.Engine.Controls;

/// <summary>
/// Represents a collection of elements that notifies its owner when items
/// are added, replaced, removed, cleared, or explicitly modified.
/// </summary>
/// <remarks>
/// Subscribes to <see cref="INotifyPropertyChanged.PropertyChanged"/>
/// for collection items implementing <see cref="INotifyPropertyChanged"/>.
/// Structural changes and item property changes are forwarded to the owning element
/// through a cached <see cref="PropertyChangedEventArgs"/> instance.
/// </remarks>
/// <typeparam name="T">The type of items stored in the collection.</typeparam>
public sealed class ElementCollection<T> : Collection<T>
{
    /// <summary>
    /// Stores the property change event arguments forwarded to the owning element.
    /// </summary>
    private readonly PropertyChangedEventArgs _notification;

    /// <summary>
    /// Stores the element that receives collection change notifications.
    /// </summary>
    private readonly Element _owner;

    /// <summary>
    /// Initializes a collection associated with the specified owning element.
    /// </summary>
    /// <param name="owner">The element that receives collection change notifications.</param>
    /// <param name="notification">
    /// The cached event arguments identifying the owner's affected property.
    /// </param>
    internal ElementCollection(Element owner, PropertyChangedEventArgs notification)
    {
        _owner = owner;
        _notification = notification;
    }

    /// <inheritdoc/>
    protected override void InsertItem(int index, T item)
    {
        base.InsertItem(index, item);

        if (item is INotifyPropertyChanged element)
            element.PropertyChanged += ItemChanged;

        Changed();
    }

    /// <inheritdoc/>
    protected override void SetItem(int index, T item)
    {
        if (this[index] is INotifyPropertyChanged previous)
            previous.PropertyChanged -= ItemChanged;

        base.SetItem(index, item);

        if (item is INotifyPropertyChanged element)
            element.PropertyChanged += ItemChanged;

        Changed();
    }

    /// <inheritdoc/>
    protected override void RemoveItem(int index)
    {
        if (this[index] is INotifyPropertyChanged previous)
            previous.PropertyChanged -= ItemChanged;

        base.RemoveItem(index);

        Changed();
    }

    /// <inheritdoc/>
    protected override void ClearItems()
    {
        foreach (T item in this)
        {
            if (item is INotifyPropertyChanged element)
                element.PropertyChanged -= ItemChanged;
        }

        base.ClearItems();
        Changed();
    }

    /// <summary>
    /// Notifies the owning element that the collection or one of its items has changed.
    /// </summary>
    private void Changed()
    {
        _owner.CollectionChanged(_notification);
    }

    /// <summary>
    /// Handles property changes raised by items in the collection.
    /// </summary>
    /// <param name="sender">The item that raised the property change event.</param>
    /// <param name="args">The event arguments describing the changed item property.</param>
    private void ItemChanged(object? sender, PropertyChangedEventArgs args)
    {
        Changed();
    }
}

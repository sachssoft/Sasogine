using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Sachssoft.Engine.Controls;
/// <summary>Notifies its owner only on structural or explicit child changes.</summary>
/// <typeparam name = "T">The item type.</typeparam>
public sealed class ElementCollection<T> : Collection<T>
{
    private readonly Element _owner;
    private readonly PropertyChangedEventArgs _notification;
    internal ElementCollection(Element owner, PropertyChangedEventArgs notification)
    {
        _owner = owner;
        _notification = notification;
    }

    private void Changed() => _owner.CollectionChanged(_notification);
    private void ItemChanged(object? sender, PropertyChangedEventArgs args) => Changed();
    /// <inheritdoc/>
    protected override void InsertItem(int index, T item)
    {
        base.InsertItem(index, item);
        if (item is INotifyPropertyChanged e)
            e.PropertyChanged += ItemChanged;
        Changed();
    }

    /// <inheritdoc/>
    protected override void SetItem(int index, T item)
    {
        if (this[index] is INotifyPropertyChanged old)
            old.PropertyChanged -= ItemChanged;
        base.SetItem(index, item);
        if (item is INotifyPropertyChanged e)
            e.PropertyChanged += ItemChanged;
        Changed();
    }

    /// <inheritdoc/>
    protected override void RemoveItem(int index)
    {
        if (this[index] is INotifyPropertyChanged old)
            old.PropertyChanged -= ItemChanged;
        base.RemoveItem(index);
        Changed();
    }

    /// <inheritdoc/>
    protected override void ClearItems()
    {
        foreach (T item in this)
            if (item is INotifyPropertyChanged old)
                old.PropertyChanged -= ItemChanged;
        base.ClearItems();
        Changed();
    }
}

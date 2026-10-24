using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Sachssoft.Engine.Controls;

/// <summary>Owns carousel pages immediately, including pages outside the active frame traversal.</summary>
public sealed class CarouselItemCollection : Collection<Control>, INotifyCollectionChanged
{
    /// <summary>Receives ownership reservations and reconciles selection after mutations.</summary>
    private readonly Carousel _owner;

    /// <summary>Occurs after page ownership and selection have been updated.</summary>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <summary>Creates a page collection attached to one carousel.</summary>
    /// <param name = "owner">The carousel responsible for page ownership.</param>
    internal CarouselItemCollection(Carousel owner) => _owner = owner;

    /// <inheritdoc/>
    protected override void InsertItem(int index, Control item)
    {
        _owner.VerifyTreeMutation();
        if ((uint)index > (uint)Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        _owner.ReservePage(item);
        base.InsertItem(index, item);
        _owner.PagesChanged();
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
    }

    /// <inheritdoc/>
    protected override void SetItem(int index, Control item)
    {
        _owner.VerifyTreeMutation();
        Control previous = this[index];
        if (ReferenceEquals(previous, item))
            return;
        _owner.ReservePage(item);
        base.SetItem(index, item);
        _owner.ReleasePage(previous);
        _owner.PagesChanged();
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, previous, index));
    }

    /// <inheritdoc/>
    protected override void RemoveItem(int index)
    {
        _owner.VerifyTreeMutation();
        Control previous = this[index];
        base.RemoveItem(index);
        _owner.ReleasePage(previous);
        _owner.PagesChanged();
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, previous, index));
    }

    /// <inheritdoc/>
    protected override void ClearItems()
    {
        _owner.VerifyTreeMutation();
        if (Count == 0)
            return;
        foreach (Control page in this)
            _owner.ReleasePage(page);
        base.ClearItems();
        _owner.PagesChanged();
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>Moves an existing page without releasing its ownership, preserving the selected page identity.</summary>
    /// <param name = "oldIndex">The page's current index.</param>
    /// <param name = "newIndex">The destination index.</param>
    public void Move(int oldIndex, int newIndex)
    {
        _owner.VerifyTreeMutation();
        Control item = this[oldIndex];
        if ((uint)newIndex >= (uint)Count)
            throw new ArgumentOutOfRangeException(nameof(newIndex));
        if (oldIndex == newIndex)
            return;
        base.RemoveItem(oldIndex);
        base.InsertItem(newIndex, item);
        _owner.PagesChanged();
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
    }
}

using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Sachssoft.Engine.Controls;

/// <summary>Stores direct items and prevents edits while an external item source is assigned.</summary>
public sealed class ItemCollection : Collection<object?>, INotifyCollectionChanged
{
    /// <summary>Stores the control that validates collection mutations.</summary>
    private readonly ItemsControl _owner;

    /// <inheritdoc/>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <summary>Creates the direct item collection for one owner.</summary>
    /// <param name="owner">The control owning this collection.</param>
    internal ItemCollection(ItemsControl owner)
    {
        _owner = owner;
    }

    /// <summary>Moves an item without creating a replacement presentation.</summary>
    /// <param name="oldIndex">The current zero-based index.</param>
    /// <param name="newIndex">The final zero-based index.</param>
    public void Move(int oldIndex, int newIndex)
    {
        _owner.VerifyItemsMutation();
        if ((uint)oldIndex >= (uint)Count || (uint)newIndex >= (uint)Count)
            throw new System.ArgumentOutOfRangeException(nameof(newIndex));
        if (oldIndex == newIndex)
            return;
        object? item = this[oldIndex];
        base.RemoveItem(oldIndex);
        base.InsertItem(newIndex, item);
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
    }

    /// <inheritdoc/>
    protected override void InsertItem(int index, object? item)
    {
        _owner.VerifyItemsMutation();
        base.InsertItem(index, item);
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
    }

    /// <inheritdoc/>
    protected override void SetItem(int index, object? item)
    {
        _owner.VerifyItemsMutation();
        object? previous = this[index];
        base.SetItem(index, item);
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, previous, index));
    }

    /// <inheritdoc/>
    protected override void RemoveItem(int index)
    {
        _owner.VerifyItemsMutation();
        object? previous = this[index];
        base.RemoveItem(index);
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, previous, index));
    }

    /// <inheritdoc/>
    protected override void ClearItems()
    {
        _owner.VerifyItemsMutation();
        if (Count == 0)
            return;
        base.ClearItems();
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}

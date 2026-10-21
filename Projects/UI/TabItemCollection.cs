using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Sachssoft.Engine.Controls;

/// <summary>Reserves every tab page immediately, including pages outside the active traversal.</summary>
public sealed class TabItemCollection : Collection<TabItem>
{
    private static readonly PropertyChangedEventArgs ItemsChangedArgs = new(nameof(TabControl.Items));
    private readonly TabControl _owner;

    internal TabItemCollection(TabControl owner) => _owner = owner;

    private void ItemChanged(object? sender, PropertyChangedEventArgs args) => Changed();
    private void Changed() => _owner.CollectionChanged(ItemsChangedArgs);

    private void Reserve(TabItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _owner.ReserveChild(item);
        item.TabOwner = _owner;
        item.PropertyChanged += ItemChanged;
    }

    private void Release(TabItem item)
    {
        item.PropertyChanged -= ItemChanged;
        item.TabOwner = null;
        _owner.ReleaseTab(item);
    }

    /// <inheritdoc/>
    protected override void InsertItem(int index, TabItem item)
    {
        Reserve(item);
        base.InsertItem(index, item);
        Changed();
    }

    /// <inheritdoc/>
    protected override void SetItem(int index, TabItem item)
    {
        if (ReferenceEquals(this[index], item)) return;
        Reserve(item);
        TabItem previous = this[index];
        base.SetItem(index, item);
        Release(previous);
        Changed();
    }

    /// <inheritdoc/>
    protected override void RemoveItem(int index)
    {
        _owner.VerifyTreeMutation();
        TabItem previous = this[index];
        base.RemoveItem(index);
        Release(previous);
        Changed();
    }

    /// <inheritdoc/>
    protected override void ClearItems()
    {
        _owner.VerifyTreeMutation();
        foreach (TabItem item in this) Release(item);
        base.ClearItems();
        Changed();
    }
}

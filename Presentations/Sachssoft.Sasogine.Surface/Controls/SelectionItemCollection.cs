using Sachssoft.Sasogine.Surface.Controls;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using Sachssoft.Sasogine.Surface.Styles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

public class SelectionItemCollection<TItem> : ISelectableItemPresenterCollection, IEnumerable<TItem>
    where TItem : SelectableItem
{
    private readonly List<TItem> _presenterItems = new();
    private readonly HashSet<TItem> _presenterSet = new();
    private readonly object _syncRoot = new();
    private readonly SelectorBase _owner;

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public SelectionItemCollection(SelectorBase owner)
    {
        ArgumentNullException.ThrowIfNull(owner);
        _owner = owner;
    }

    public object? this[int index]
    {
        get
        {
            lock (_syncRoot) return _presenterItems[index].Content;
        }
        set => ReplaceAt(index, value);
    }

    public bool IsFixedSize => false;

    public bool IsReadOnly => false;

    public int Count
    {
        get
        {
            lock (_syncRoot) return _presenterItems.Count;
        }
    }

    public bool IsSynchronized => false;

    public object SyncRoot => _syncRoot;


    #region IList Implementation

    public int Add(object? value)
    {
        TItem presenter;
        int index;

        lock (_syncRoot)
        {
            presenter = CreatePresenter(value);
            EnsureUniquePresenter(presenter);

            _presenterItems.Add(presenter);
            _presenterSet.Add(presenter);
            index = _presenterItems.Count - 1;

            _owner.ItemPresenterAdded(presenter, index);
        }

        var handler = CollectionChanged;
        handler?.Invoke(this, new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Add, presenter, index));

        return index;
    }

    public void Clear()
    {
        List<TItem> removed;

        lock (_syncRoot)
        {
            if (_presenterItems.Count == 0)
                return;

            removed = new List<TItem>(_presenterItems);
            _presenterItems.Clear();
            _presenterSet.Clear();
        }

        foreach (var item in removed)
            UnloadPresenter(item);

        _owner.CollectionReset();

        var handler = CollectionChanged;
        handler?.Invoke(this, new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Reset));
    }

    public bool Contains(object? value)
    {
        lock (_syncRoot)
            return _presenterItems.Any(p => Equals(p.Content, value));
    }

    public void CopyTo(Array array, int index)
    {
        ArgumentNullException.ThrowIfNull(array);
        if (array.Rank != 1) throw new ArgumentException("Array must be one-dimensional.", nameof(array));
        if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));

        lock (_syncRoot)
        {
            if (array.Length - index < _presenterItems.Count)
                throw new ArgumentException("Array too small.", nameof(array));

            for (int i = 0; i < _presenterItems.Count; i++)
                array.SetValue(_presenterItems[i].Content, index + i);
        }
    }

    public IEnumerator<TItem> GetEnumerator()
    {
        lock (_syncRoot)
            return _presenterItems.GetEnumerator();
    }

    public int IndexOf(object? value)
    {
        lock (_syncRoot)
        {
            for (int i = 0; i < _presenterItems.Count; i++)
                if (Equals(_presenterItems[i].Content, value))
                    return i;
            return -1;
        }
    }

    public void Insert(int index, object? value)
    {
        TItem presenter;

        lock (_syncRoot)
        {
            presenter = CreatePresenter(value);
            EnsureUniquePresenter(presenter);

            _presenterItems.Insert(index, presenter);
            _presenterSet.Add(presenter);

            _owner.ItemPresenterAdded(presenter, index);
        }

        var handler = CollectionChanged;
        handler?.Invoke(this, new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Add, presenter, index));
    }

    public void Remove(object? value)
    {
        int index = IndexOf(value);
        if (index >= 0)
            RemoveAt(index);
    }

    public void RemoveAt(int index)
    {
        TItem removed;

        lock (_syncRoot)
        {
            if (index < 0 || index >= _presenterItems.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            removed = _presenterItems[index];
            _presenterItems.RemoveAt(index);
            _presenterSet.Remove(removed);
        }

        UnloadPresenter(removed);
        _owner.ItemPresenterRemoved(removed, index);

        var handler = CollectionChanged;
        handler?.Invoke(this, new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Remove, removed, index));
    }

    #endregion

    #region Move / Swap / Replace

    public void Move(int fromIndex, int toIndex)
    {
        if (fromIndex == toIndex) return;

        TItem item;
        lock (_syncRoot)
        {
            if (fromIndex < 0 || fromIndex >= Count || toIndex < 0 || toIndex >= Count)
                throw new ArgumentOutOfRangeException();

            item = _presenterItems[fromIndex];
            _presenterItems.RemoveAt(fromIndex);
            _presenterItems.Insert(toIndex, item);

            // Owner informieren – kein Neuladen nötig
            _owner.ItemPresenterRemoved(item, fromIndex);
            _owner.ItemPresenterAdded(item, toIndex);
        }

        var handler = CollectionChanged;
        handler?.Invoke(this, new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Move, item, toIndex, fromIndex));
    }

    public void Swap(int indexA, int indexB)
    {
        if (indexA == indexB) return;

        lock (_syncRoot)
        {
            if (indexA < 0 || indexA >= Count || indexB < 0 || indexB >= Count)
                throw new ArgumentOutOfRangeException();

            (_presenterItems[indexA], _presenterItems[indexB]) =
            (_presenterItems[indexB], _presenterItems[indexA]);

            _owner.ItemPresenterRemoved(_presenterItems[indexA], indexB);
            _owner.ItemPresenterRemoved(_presenterItems[indexB], indexA);
            _owner.ItemPresenterAdded(_presenterItems[indexA], indexA);
            _owner.ItemPresenterAdded(_presenterItems[indexB], indexB);
        }

        var handler = CollectionChanged;
        handler?.Invoke(this, new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Reset));
    }

    private void ReplaceAt(int index, object? value)
    {
        TItem oldItem, newItem;
        lock (_syncRoot)
        {
            newItem = CreatePresenter(value);
            EnsureUniquePresenter(newItem);

            oldItem = _presenterItems[index];
            _presenterItems[index] = newItem;

            _presenterSet.Remove(oldItem);
            _presenterSet.Add(newItem);
        }

        UnloadPresenter(oldItem);
        _owner.ItemPresenterRemoved(oldItem, index);

        _owner.ItemPresenterAdded(newItem, index);

        var handler = CollectionChanged;
        handler?.Invoke(this, new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Replace, newItem, oldItem, index));
    }

    #endregion

    #region Helpers

    public bool TryGetPresenter(object? content, out TItem? presenter)
    {
        lock (_syncRoot)
        {
            presenter = _presenterItems.FirstOrDefault(p => Equals(p.Content, content));
            return presenter != null;
        }
    }

    public TItem GetPresenterByIndex(int index)
    {
        lock (_syncRoot)
            return _presenterItems[index];
    }

    public IEnumerable<TItem> GetPresents()
    {
        lock (_syncRoot)
            return _presenterItems.ToList(); // Snapshot
    }

    private TItem CreatePresenter(object? content)
    {
        var item = content as TItem ?? _owner.CreateItemPresenter(content);
        //_owner.ApplyFromStyle(Stylesheet.Current.FindStyle(item.GetType(), item.StyleId));
        item.IsSelectedChanged += ListItem_IsSelectedChanged;
        item.Owner = _owner;
        return (TItem)item;
    }

    private void UnloadPresenter(TItem presenter)
    {
        presenter.IsSelectedChanged -= ListItem_IsSelectedChanged;
        if (presenter is IDisposable d)
            d.Dispose();
        presenter.Owner = null;
    }

    private void ListItem_IsSelectedChanged(object? sender, EventArgs e)
    {
        if (sender is not TItem changedItem || !changedItem.IsSelected)
            return;

        lock (_syncRoot)
        {
            if (!_owner.AllowMultiple)
            {
                foreach (var item in _presenterItems)
                {
                    if (!ReferenceEquals(item, changedItem) && item.IsSelected)
                    {
                        item.IsSelectedChanged -= ListItem_IsSelectedChanged;
                        item.IsSelected = false;
                        item.IsSelectedChanged += ListItem_IsSelectedChanged;
                    }
                }
            }

            var indices = _presenterItems
                .Select((x, i) => x.IsSelected ? i : -1)
                .Where(i => i >= 0)
                .ToArray();

            _owner.ApplySelectionIndices(indices);
        }
    }

    private void EnsureUniquePresenter(TItem presenter)
    {
        ArgumentNullException.ThrowIfNull(presenter);
        if (_presenterSet.Contains(presenter))
            throw new ArgumentException("The same ListItem instance cannot be added twice.", nameof(presenter));
    }

    #endregion

    #region ISelectableItemPresenterCollection

    bool ISelectableItemPresenterCollection.TryGetPresenter(object? content, out SelectableItem? presenter)
    {
        var result = TryGetPresenter(content, out var p);
        presenter = p;
        return result;
    }

    SelectableItem ISelectableItemPresenterCollection.GetPresenterByIndex(int index)
        => GetPresenterByIndex(index);

    IEnumerable<SelectableItem> ISelectableItemPresenterCollection.GetPresenters()
        => GetPresents();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _presenterItems.GetEnumerator();
    }

    #endregion
}

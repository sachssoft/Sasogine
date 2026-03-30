using Sachssoft.Sasogine.Surface.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

public class LayoutWidgetCollection : IList<Widget>
{
    private Widget[] _items;
    private int _count;
    private Widget[]? _sortedItemCache;

    internal LayoutWidgetCollection(int capacity = 8)
    {
        _items = new Widget[capacity];
        _count = 0;
        _sortedItemCache = null;
    }

    public int Count => _count;
    public bool IsReadOnly => false;

    public Widget this[int index]
    {
        get
        {
            if (index < 0 || index >= _count) throw new ArgumentOutOfRangeException();
            return _items[index];
        }
        set
        {
            if (index < 0 || index >= _count) throw new ArgumentOutOfRangeException();
            _items[index] = value;
            _sortedItemCache = null;
        }
    }

    private void EnsureCapacity(int required)
    {
        if (required > _items.Length)
        {
            int newSize = Math.Max(_items.Length * 2, required);
            Array.Resize(ref _items, newSize);
        }
    }

    public void Add(Widget item)
    {
        EnsureCapacity(_count + 1);
        _items[_count++] = item;
        _sortedItemCache = null;
    }

    public void AddRange(IEnumerable<Widget> items)
    {
        foreach (var item in items)
        {
            Add(item); // Add sorgt für Capacity + Cache
        }
    }

    public void Clear()
    {
        Array.Clear(_items, 0, _count);
        _count = 0;
        _sortedItemCache = null;
    }

    public bool Contains(Widget item) => IndexOf(item) >= 0;

    public void CopyTo(Widget[] array, int arrayIndex) => Array.Copy(_items, 0, array, arrayIndex, _count);

    public IEnumerator<Widget> GetEnumerator()
    {
        if (_sortedItemCache == null)
        {
            // Nur aktuelle Widgets kopieren
            var currentWidgets = new Widget[_count];
            Array.Copy(_items, currentWidgets, _count);

            // Sortieren nach ZIndex, stabil (gleicher ZIndex: ursprüngliche Reihenfolge bleibt)
            _sortedItemCache = currentWidgets
                .Select((w, i) => (w, i)) // Index merken
                .OrderBy(x => x.w.ZIndex.Value)
                .ThenBy(x => x.i)
                .Select(x => x.w)
                .ToArray();
        }

        for (int i = 0; i < _sortedItemCache.Length; i++)
            yield return _sortedItemCache[i];
    }
    
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    public int IndexOf(Widget item)
    {
        for (int i = 0; i < _count; i++)
            if (_items[i] == item) return i;
        return -1;
    }

    public void Insert(int index, Widget item)
    {
        if (index < 0 || index > _count) throw new ArgumentOutOfRangeException();
        EnsureCapacity(_count + 1);

        if (index < _count)
            Array.Copy(_items, index, _items, index + 1, _count - index);

        _items[index] = item;
        _count++;
        _sortedItemCache = null;
    }

    public void InsertRange(int index, IEnumerable<Widget> items)
    {
        foreach (var item in items)
        {
            Insert(index++, item);
        }
    }

    public bool Remove(Widget item)
    {
        int index = IndexOf(item);
        if (index < 0) return false;
        RemoveAt(index);
        return true;
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= _count) throw new ArgumentOutOfRangeException();
        _count--;
        if (index < _count)
            Array.Copy(_items, index + 1, _items, index, _count - index);
        _items[_count] = null!;
        _sortedItemCache = null;
    }

    public void RemoveRange(int index, int count)
    {
        if (index < 0 || count < 0 || index + count > _count)
            throw new ArgumentOutOfRangeException();

        int remaining = _count - (index + count);
        if (remaining > 0)
            Array.Copy(_items, index + count, _items, index, remaining);

        Array.Clear(_items, _count - count, count);
        _count -= count;
        _sortedItemCache = null;
    }

    public void SortZIndex() => _sortedItemCache = null;

    public IEnumerable<Widget> GetUnsortedEnumerator()
    {
        for (int i = 0; i < _count; i++)
            yield return _items[i];
    }
}

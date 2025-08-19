using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Sachssoft.Sasogine.Surface.Visuals.Controls;

// By Tobias Sachs
public class WidgetCollection : ObservableCollection<Widget>
{
    private bool _suppress_notification = false;

    public void AddRange(IEnumerable<Widget> widgets)
    {
        if (!TryAddRange(widgets))
            throw new ArgumentException("AddRange failed.");
    }

    public void InsertRange(int index, IEnumerable<Widget> widgets)
    {
        if (!TryInsertRange(index, widgets))
            throw new ArgumentException("InsertRange failed.");
    }

    public void RemoveRange(int index, int count)
    {
        if (!TryRemoveRange(index, count))
            throw new ArgumentException("RemoveRange failed.");
    }

    public void RemoveRange(IEnumerable<Widget> widgets)
    {
        if (!TryRemoveRange(widgets))
            throw new ArgumentException("RemoveRange failed.");
    }

    public void MoveRange(int old_index, int new_index, int count)
    {
        if (!TryMoveRange(old_index, new_index, count))
            throw new ArgumentException("MoveRange failed.");
    }

    public bool TryAddRange(IEnumerable<Widget> widgets)
    {
        if (widgets == null) return false;
        var list = widgets.ToList();
        if (list.Count == 0) return false;

        _suppress_notification = true;
        foreach (var widget in list)
            Add(widget);
        _suppress_notification = false;

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        return true;
    }

    public bool TryRemoveRange(int index, int count)
    {
        if (index < 0 || count <= 0 || index + count > Count)
            return false;

        _suppress_notification = true;
        for (int i = 0; i < count; i++)
            RemoveAt(index);
        _suppress_notification = false;

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        return true;
    }

    public bool TryRemoveRange(IEnumerable<Widget> widgets)
    {
        if (widgets == null)
            return false;

        var any_removed = false;
        _suppress_notification = true;

        foreach (var widget in widgets.ToList()) // Kopie nötig, falls Quelle dieselbe Collection ist
        {
            if (Remove(widget)) // nutzt TryRemove intern
                any_removed = true;
        }

        _suppress_notification = false;

        if (any_removed)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            return true;
        }

        return false;
    }

    public bool TryInsertRange(int index, IEnumerable<Widget> widgets)
    {
        if (widgets == null || index < 0 || index > Count) return false;
        var list = widgets.ToList();
        if (list.Count == 0) return false;

        _suppress_notification = true;
        for (int i = 0; i < list.Count; i++)
            Insert(index + i, list[i]);
        _suppress_notification = false;

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        return true;
    }

    public bool TryMoveRange(int old_index, int new_index, int count)
    {
        if (count <= 0 ||
            old_index < 0 || new_index < 0 ||
            old_index + count > Count || new_index > Count)
            return false;

        if (old_index == new_index)
            return false;

        var items = new List<Widget>();
        for (int i = 0; i < count; i++)
        {
            items.Add(this[old_index]);
            RemoveAt(old_index);
        }

        if (new_index > old_index)
            new_index -= count;

        for (int i = 0; i < items.Count; i++)
        {
            Insert(new_index + i, items[i]);
        }

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        return true;
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (!_suppress_notification)
            base.OnCollectionChanged(e);
    }
}

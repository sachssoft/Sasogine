using System.Collections;
using System.Collections.Generic;

namespace Sachssoft.Engine.Controls;
/// <summary>Provides ordered children with ownership and deferred mutation managed by their container.</summary>
public sealed class ControlCollection : IReadOnlyList<Control>
{
    private readonly ContainerControl _owner;
    private readonly List<Control> _items;
    internal ControlCollection(ContainerControl owner, List<Control> items)
    {
        _owner = owner;
        _items = items;
    }

    /// <summary>Gets the number of committed children.</summary>
    public int Count => _items.Count;

    /// <summary>Gets a child in insertion order.</summary>
    public Control this[int index] => _items[index];
    /// <summary>Adds an unattached child. Traversal-time changes commit at the frame boundary.</summary>
    public void Add(Control control) => _owner.Add(control);
    /// <summary>Removes a child if present.</summary>
    public void Remove(Control control) => _owner.Remove(control);
    /// <summary>Removes all children.</summary>
    public void Clear() => _owner.Clear();
    /// <summary>Tests whether this container owns the specified child.</summary>
    public bool Contains(Control control)
    {
        for (int i = 0; i < _items.Count; i++)
            if (ReferenceEquals(_items[i], control))
                return true;
        return false;
    }

    /// <summary>Returns an allocation-free enumerator over committed children.</summary>
    public List<Control>.Enumerator GetEnumerator() => _items.GetEnumerator();
    IEnumerator<Control> IEnumerable<Control>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

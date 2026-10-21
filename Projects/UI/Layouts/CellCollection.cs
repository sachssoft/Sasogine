using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Sachssoft.Engine.Controls.Layouts;
/// <summary>Stores explicit grid placements by control identity without changing child ownership.</summary>
/// <remarks>Unassigned children use the first cell. Release a placement before putting the same control again.</remarks>
public sealed class CellCollection : IReadOnlyCollection<KeyValuePair<Control, GridCell>>
{
    private static readonly PropertyChangedEventArgs CellsChangedArgs = new(nameof(Grid.Cells));
    private readonly Grid _owner;
    private readonly Dictionary<Control, GridCell> _items = new(ReferenceEqualityComparer.Instance);
    internal CellCollection(Grid owner) => _owner = owner;
    /// <summary>Gets the number of explicit placements.</summary>
    public int Count => _items.Count;

    /// <summary>Gets the explicit placement for a control.</summary>
    public GridCell this[Control control] => _items[control];
    /// <summary>Registers a placement. The control may be added to Children before or after this call.</summary>
    /// <exception cref = "ArgumentException">The same control instance already has a placement.</exception>
    /// <exception cref = "InvalidOperationException">The control belongs to another container or would create a cycle.</exception>
    public void Put(Control control, int row = 0, int column = 0, int rowSpan = 1, int columnSpan = 1)
    {
        ArgumentNullException.ThrowIfNull(control);
        if (_items.ContainsKey(control))
            throw new ArgumentException("This control already has a cell. Release it before putting it again.", nameof(control));
        var cell = new GridCell(row, column, rowSpan, columnSpan);
        if (control.Parent is not null && !ReferenceEquals(control.Parent, _owner) || control.Parent is null && control.Workspace is not null)
            throw new InvalidOperationException("The control belongs to another owner.");
        for (Control? ancestor = _owner; ancestor is not null; ancestor = ancestor.Parent)
            if (ReferenceEquals(ancestor, control))
                throw new InvalidOperationException("A grid cell cannot refer to its owner or an ancestor.");
        _items.Add(control, cell);
        Changed();
    }

    /// <summary>Removes a placement, leaving the control in Children. Returns whether a placement existed.</summary>
    public bool Release(Control control)
    {
        ArgumentNullException.ThrowIfNull(control);
        if (!_items.Remove(control))
            return false;
        Changed();
        return true;
    }

    /// <summary>Removes all explicit placements without removing children.</summary>
    public void Clear()
    {
        if (_items.Count == 0)
            return;
        _items.Clear();
        Changed();
    }

    /// <summary>Gets a placement if one exists.</summary>
    public bool TryGetValue(Control control, out GridCell cell) => _items.TryGetValue(control, out cell);
    private void Changed() => _owner.CollectionChanged(CellsChangedArgs);
    /// <summary>Returns an allocation-free enumerator over explicit placements.</summary>
    public Dictionary<Control, GridCell>.Enumerator GetEnumerator() => _items.GetEnumerator();
    IEnumerator<KeyValuePair<Control, GridCell>> IEnumerable<KeyValuePair<Control, GridCell>>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

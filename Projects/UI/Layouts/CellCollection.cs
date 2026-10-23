using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Sachssoft.Engine.Controls.Layouts;
/// <summary>
/// Stores explicit grid placements by control identity without changing child ownership.
/// </summary>
/// <remarks>Unassigned children use the first cell. Put creates or updates a placement without changing ownership.</remarks>
public sealed class CellCollection : IReadOnlyCollection<KeyValuePair<Control, GridCell>>
{
    private static readonly PropertyChangedEventArgs CellsChangedArgs = new(nameof(Grid.Cells));
    private readonly Grid _owner;
    private readonly Dictionary<Control, GridCell> _items = new(ReferenceEqualityComparer.Instance);
    internal CellCollection(Grid owner) => _owner = owner;
    /// <summary>
    /// Gets the number of explicit placements.
    /// </summary>
    public int Count => _items.Count;

    /// <summary>
    /// Gets the explicit placement for a control.
    /// </summary>
    public GridCell this[Control control] => _items[control];
    /// <summary>
    /// Registers or updates a placement. The control may be added to Children before or after this call.
    /// </summary>
    /// <exception cref = "InvalidOperationException">The control belongs to another container or would create a cycle.</exception>
    public void Put(Control control, int row = 0, int column = 0, int rowSpan = 1, int columnSpan = 1)
    {
        ArgumentNullException.ThrowIfNull(control);
        var cell = new GridCell(row, column, rowSpan, columnSpan);
        if (control.RootOwner is not null || control.Parent is not null && !ReferenceEquals(control.Parent, _owner) || control.Parent is null && control.Workspace is not null)
            throw new InvalidOperationException("The control belongs to another owner.");
        for (Control? ancestor = _owner; ancestor is not null; ancestor = ancestor.Parent)
            if (ReferenceEquals(ancestor, control))
                throw new InvalidOperationException("A grid cell cannot refer to its owner or an ancestor.");
        if (_items.TryGetValue(control, out GridCell previous) && previous == cell)
            return;
        _owner.VerifyTreeMutation();
        _items[control] = cell;
        Changed();
    }

    /// <summary>
    /// Removes a placement, leaving the control in Children. Returns whether a placement existed.
    /// </summary>
    public bool Remove(Control control)
    {
        ArgumentNullException.ThrowIfNull(control);
        _owner.VerifyTreeMutation();
        if (!_items.Remove(control))
            return false;
        Changed();
        return true;
    }

    /// <summary>
    /// Removes all explicit placements without removing children.
    /// </summary>
    public void Clear()
    {
        _owner.VerifyTreeMutation();
        if (_items.Count == 0)
            return;
        _items.Clear();
        Changed();
    }

    /// <summary>
    /// Gets a placement if one exists.
    /// </summary>
    public bool TryGetValue(Control control, out GridCell cell) => _items.TryGetValue(control, out cell);
    private void Changed() => _owner.CollectionChanged(CellsChangedArgs);
    /// <summary>
    /// Returns an allocation-free enumerator over explicit placements.
    /// </summary>
    public Dictionary<Control, GridCell>.Enumerator GetEnumerator() => _items.GetEnumerator();
    IEnumerator<KeyValuePair<Control, GridCell>> IEnumerable<KeyValuePair<Control, GridCell>>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

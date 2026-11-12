using Sachssoft.Engine;

namespace Sachssoft.Engine.Controls;

/// <summary>Reuses display controls for a bounded viewport owned by a specialized control.</summary>
internal sealed class ViewportTemplatePool
{
    /// <summary>Owns the committed display controls.</summary>
    private readonly ContainerControl _owner;

    /// <summary>Stores a ring of realized controls and their bound data.</summary>
    private readonly List<(Control Control, object? Data, bool Bound)> _items = new();

    /// <summary>Stores the template used to create the current controls.</summary>
    private IRecyclingItemTemplate? _template;

    /// <summary>Initializes an empty viewport pool.</summary>
    /// <param name="owner">The container that owns the realized controls.</param>
    internal ViewportTemplatePool(ContainerControl owner) => _owner = owner;

    /// <summary>Adjusts the number of retained controls and replaces incompatible presentations.</summary>
    /// <param name="count">The required number of controls.</param>
    /// <param name="template">The template for the controls.</param>
    internal void Resize(int count, IRecyclingItemTemplate template)
    {
        if (!ReferenceEquals(_template, template))
        {
            Clear();
            _template = template;
        }
        while (_items.Count > count)
        {
            _owner.Remove(_items[^1].Control);
            _items.RemoveAt(_items.Count - 1);
        }
        while (_items.Count < count)
        {
            Control control = template.Create(null) ?? throw new InvalidOperationException("The viewport template returned null.");
            control.IsHitTestVisible = false;
            control.IsFocusable = false;
            _owner.Add(control);
            _items.Add((control, null, false));
        }
    }

    /// <summary>Releases all retained controls.</summary>
    internal void Clear()
    {
        foreach (var entry in _items)
            _owner.Remove(entry.Control);
        _items.Clear();
        _template = null;
    }

    /// <summary>Schedules rebinds after an in-place data change.</summary>
    internal void InvalidateBindings()
    {
        for (int i = 0; i < _items.Count; i++)
            _items[i] = (_items[i].Control, null, false);
    }

    /// <summary>Rebinds and positions the slot corresponding to a visible index.</summary>
    /// <param name="index">The display index.</param>
    /// <param name="data">The current data context.</param>
    /// <param name="bounds">The cell bounds in workspace coordinates.</param>
    /// <param name="layout">The layout context.</param>
    /// <param name="selected">Whether this data entry is selected.</param>
    internal void Arrange(int index, object? data, Bounds2 bounds, ControlLayoutContext layout, bool selected)
    {
        int slot = index % _items.Count;
        var (control, previous, bound) = _items[slot];
        if (!ReferenceEquals(control.CommittedParent, _owner))
            return;
        control.IsItemSelected = selected;
        if (!bound || !ReferenceEquals(previous, data))
        {
            Workspace? workspace = _owner.Workspace;
            Control? previousRoot = workspace?.LocalLayoutRoot;
            if (workspace is not null)
                workspace.LocalLayoutRoot = control;
            try
            {
                control.DataContext = data;
                _template!.Rebind(control, data);
                Workspace.PrepareTree(control);
                _items[slot] = (control, data, true);
            }
            finally
            {
                if (workspace is not null)
                    workspace.LocalLayoutRoot = previousRoot;
            }
        }
        control.Measure(layout, new Size2(bounds.Width, bounds.Height));
        control.Arrange(bounds, true);
    }
}

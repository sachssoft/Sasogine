using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Sachssoft.Engine.Controls.Layouts;

namespace Sachssoft.Engine.Controls;

/// <summary>Presents direct items or an enumerable source through item and panel templates.</summary>
/// <remarks>Source changes are reconciled before layout. Existing item presentations survive moves
/// and changes to other items. Virtualizing panels realize only the visible range. Sources must
/// change on the UI thread.</remarks>
public class ItemsControl : ContainerControl
{
    /// <summary>Identifies changes to the item template.</summary>
    private static readonly PropertyChangedEventArgs ItemTemplateChangedArgs = new(nameof(ItemTemplate));

    /// <summary>Identifies changes to the panel template.</summary>
    private static readonly PropertyChangedEventArgs ItemsPanelChangedArgs = new(nameof(ItemsPanel));

    /// <summary>Identifies replacement of the active panel.</summary>
    private static readonly PropertyChangedEventArgs ItemsPanelRootChangedArgs = new(nameof(ItemsPanelRoot));

    /// <summary>Identifies changes to the external source.</summary>
    private static readonly PropertyChangedEventArgs ItemsSourceChangedArgs = new(nameof(ItemsSource));

    /// <summary>Stores current presentations in source order.</summary>
    private readonly List<Control> _presentations = new();

    /// <summary>Stores the source identity associated with each presentation.</summary>
    private readonly List<object?> _sources = new();

    /// <summary>Indicates that the source requires reconciliation.</summary>
    private bool _itemsDirty = true;

    /// <summary>Indicates that every presentation must be recreated.</summary>
    private bool _templateDirty;

    /// <summary>Prevents reentrant reconciliation from template callbacks.</summary>
    private bool _updating;

    /// <summary>Stores the owned panel hosting materialized items.</summary>
    private Control? _panel;

    /// <summary>Stores the optional external data source.</summary>
    private IEnumerable? _itemsSource;

    /// <summary>Stores the collection currently observed for external changes.</summary>
    private INotifyCollectionChanged? _subscription;

    /// <summary>Stores the optional item factory.</summary>
    private ITemplate<object?, Control>? _itemTemplate;

    /// <summary>Stores the optional panel factory.</summary>
    private ITemplate<ItemsControl, ContainerControl>? _itemsPanel;

    /// <summary>Stores the actual layout panel, which can be inside a scroll viewer.</summary>
    private ContainerControl _panelRoot;

    /// <summary>Stores the indexed source for the current virtual layout.</summary>
    private IList? _virtualSource;

    /// <summary>Stores pooled controls and their last bound source indices.</summary>
    private readonly List<(Control Control, int Index)> _virtualPool = new();

    /// <summary>Creates an empty items host using a vertical stack by default.</summary>
    public ItemsControl()
    {
        Items = new ItemCollection(this);
        Items.CollectionChanged += SourceChanged;
        _panelRoot = new StackPanel();
        SetOwnedControl(ref _panel, _panelRoot, ItemsPanelRootChangedArgs);
    }

    /// <summary>Gets the editable direct items. Changes are rejected while ItemsSource is assigned.</summary>
    public ItemCollection Items
    {
        get;
    }

    /// <summary>Gets or sets the external source; null restores direct items.</summary>
    /// <remarks>Assigning a source requires an empty direct collection. Observable sources are
    /// subscribed while attached to a workspace; reattachment refreshes changes made while detached.
    /// Call RefreshItems after changing a non-observable source.</remarks>
    public IEnumerable? ItemsSource
    {
        get => _itemsSource;
        set
        {
            VerifyTreeMutation();
            if (ReferenceEquals(_itemsSource, value))
                return;
            if (value is not null && Items.Count != 0)
                throw new InvalidOperationException("Clear direct Items before assigning ItemsSource.");
            Unsubscribe();
            _itemsSource = value;
            Subscribe();
            RefreshItems();
            NotifyPropertyChanged(ItemsSourceChangedArgs);
        }
    }

    /// <summary>Gets or sets the factory creating a fresh unowned control for each source item.</summary>
    /// <remarks>Null uses ItemContainer and ContentControl's text/control presentation.
    /// Replacing this template recreates presentations at the next preparation boundary.</remarks>
    public ITemplate<object?, Control>? ItemTemplate
    {
        get => _itemTemplate;
        set
        {
            VerifyTreeMutation();
            if (ReferenceEquals(_itemTemplate, value))
                return;
            if (ItemsPanelRoot is VirtualizingPanel && value is not null and not IRecyclingItemTemplate)
                throw new InvalidOperationException("A virtualizing panel requires a recycling item template.");
            _itemTemplate = value;
            _templateDirty = true;
            RefreshItems();
            NotifyPropertyChanged(ItemTemplateChangedArgs);
        }
    }

    /// <summary>Gets or sets the factory for the items layout. Null creates a vertical stack.</summary>
    /// <remarks>The factory must return an empty unowned container. Existing presentations are
    /// transferred into the new panel without recreating their content.</remarks>
    public ITemplate<ItemsControl, ContainerControl>? ItemsPanel
    {
        get => _itemsPanel;
        set
        {
            VerifyTreeMutation();
            if (ReferenceEquals(_itemsPanel, value))
                return;
            ContainerControl next = value is null ? new Layouts.StackPanel()
                : value.Create(this) ?? throw new InvalidOperationException("The panel template returned null.");
            ValidateChild(next);
            if (next.Children.Count != 0)
                throw new InvalidOperationException("An items panel template must create an empty container.");
            ContainerControl previous = ItemsPanelRoot;
            if (next is VirtualizingPanel && _itemTemplate is not null and not IRecyclingItemTemplate)
                throw new InvalidOperationException("A virtualizing panel requires a recycling item template.");

            bool wasVirtual = previous is VirtualizingPanel;
            bool isVirtual = next is VirtualizingPanel;
            foreach (Control presentation in _presentations)
                previous.Remove(presentation);
            if (previous is VirtualizingPanel previousVirtual)
            {
                ClearVirtualPool(previousVirtual);
                previousVirtual.ItemsOwner = null;
                _virtualSource = null;
            }

            Control host = next;
            if (next is VirtualizingPanel virtualPanel)
            {
                virtualPanel.ItemsOwner = this;
                host = new ScrollViewer { Content = next, HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden };
            }
            if (wasVirtual || isVirtual)
            {
                _presentations.Clear();
                _sources.Clear();
                RefreshItems();
            }
            _panelRoot = next;
            SetOwnedControl(ref _panel, host, ItemsPanelRootChangedArgs);
            foreach (Control presentation in _presentations)
                next.Add(presentation);
            _itemsPanel = value;
            NotifyPropertyChanged(ItemsPanelChangedArgs);
        }
    }

    /// <summary>Gets the owned container created by the active panel template.</summary>
    public ContainerControl ItemsPanelRoot => _panelRoot;

    /// <summary>Gets the number of source items in the last synchronized layout.</summary>
    public int ItemCount => ItemsPanelRoot is VirtualizingPanel ? _virtualSource?.Count ?? 0 : _presentations.Count;

    /// <summary>Gets the viewport created for a virtualizing panel.</summary>
    public ScrollViewer? ItemsScrollViewer => _panel as ScrollViewer;

    /// <summary>Gets or sets the optional selection lookup used by a selector that hosts these items.</summary>
    internal Func<int, bool>? SelectionState { get; set; }

    /// <summary>Gets or sets the selector that should retain keyboard focus when a row is recycled.</summary>
    internal Control? SelectionOwner { get; set; }

    /// <summary>Refreshes selection state for every realized presentation.</summary>
    internal void RefreshSelectionState()
    {
        if (SelectionState is not { } selected)
            return;
        if (ItemsPanelRoot is VirtualizingPanel)
        {
            foreach (var item in _virtualPool)
                item.Control.IsItemSelected = item.Index >= 0 && selected(item.Index);
        }
        else
            for (int i = 0; i < _presentations.Count; i++)
                _presentations[i].IsItemSelected = selected(i);
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => nameof(ItemsControl);

    /// <inheritdoc/>
    protected override bool DrawsDecoration => false;

    /// <summary>Gets an existing presentation by its source-order index.</summary>
    /// <param name="index">The zero-based index in the last synchronized source.</param>
    /// <returns>The presentation retained for that item.</returns>
    public Control ContainerFromIndex(int index) => ItemsPanelRoot is VirtualizingPanel
        ? GetVirtualPresentation(index) ?? throw new ArgumentOutOfRangeException(nameof(index), "The virtual item is not realized.")
        : _presentations[index];

    /// <summary>Gets the presentation for an index if it is currently realized.</summary>
    /// <param name="index">The source index to look up.</param>
    /// <returns>The realized control, or null when the item is outside the realized range.</returns>
    public Control? TryContainerFromIndex(int index) => ItemsPanelRoot is VirtualizingPanel
        ? GetVirtualPresentation(index)
        : (uint)index < (uint)_presentations.Count ? _presentations[index] : null;

    /// <summary>Finds the source index for a realized control or one of its descendants.</summary>
    /// <param name="control">The control belonging to an item presentation.</param>
    /// <returns>The index, or minus one when the control is outside an item.</returns>
    public int IndexFromContainer(Control control)
    {
        ArgumentNullException.ThrowIfNull(control);
        for (Control? item = control; item is not null; item = item.Parent)
        {
            if (!ReferenceEquals(item.Parent, ItemsPanelRoot))
                continue;
            if (ItemsPanelRoot is VirtualizingPanel)
            {
                foreach (var entry in _virtualPool)
                    if (ReferenceEquals(entry.Control, item))
                        return entry.Index;
                return -1;
            }
            return _presentations.FindIndex(candidate => ReferenceEquals(candidate, item));
        }
        return -1;
    }

    /// <summary>Reveals a data item in the virtual viewport.</summary>
    /// <param name="index">The source index to reveal.</param>
    public void ScrollIntoView(int index)
    {
        if ((uint)index < (uint)ItemCount && ItemsPanelRoot is VirtualizingPanel panel)
            panel.Reveal(index);
    }

    /// <summary>Schedules reconciliation after changes to a non-observable source.</summary>
    public void RefreshItems()
    {
        VerifyTreeMutation();
        _itemsDirty = true;
        InvalidateMeasure();
    }

    /// <inheritdoc/>
    public override void Add(Control child) => Items.Add(child);

    /// <inheritdoc/>
    public override void Remove(Control child) => Items.Remove(child);

    /// <inheritdoc/>
    public override void Clear() => Items.Clear();

    /// <inheritdoc/>
    protected override void OnAttached()
    {
        base.OnAttached();
        Subscribe();
        RefreshItems();
    }

    /// <inheritdoc/>
    protected override void OnDetached()
    {
        Unsubscribe();
        base.OnDetached();
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        _panel!.Measure(context, available);
        return _panel.DesiredSize;
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 contentBounds) => _panel!.Arrange(contentBounds, true);

    /// <inheritdoc/>
    internal override void Prepare()
    {
        base.Prepare();
        if (_itemsDirty)
            SynchronizeItems();
    }

    /// <summary>Rejects conflicting direct items and mutations during drawing or reconciliation.</summary>
    internal void VerifyItemsMutation()
    {
        VerifyTreeMutation();
        if (_itemsSource is not null)
            throw new InvalidOperationException("Direct Items are read-only while ItemsSource is assigned.");
        if (_updating)
            throw new InvalidOperationException("A template cannot mutate its owner's item collection.");
    }

    /// <summary>Marks source changes for batched reconciliation.</summary>
    /// <param name="sender">The notifying collection.</param>
    /// <param name="args">The structural change, including moves and resets.</param>
    private void SourceChanged(object? sender, NotifyCollectionChangedEventArgs args) => RefreshItems();

    /// <summary>Observes the external source only while this control is attached.</summary>
    private void Subscribe()
    {
        if (Workspace is not null && _subscription is null && _itemsSource is INotifyCollectionChanged source)
        {
            _subscription = source;
            source.CollectionChanged += SourceChanged;
        }
    }

    /// <summary>Releases the external collection subscription.</summary>
    private void Unsubscribe()
    {
        if (_subscription is not null)
            _subscription.CollectionChanged -= SourceChanged;
        _subscription = null;
    }

    /// <summary>Reconciles source identities, retaining unaffected and moved presentations.</summary>
    private void SynchronizeItems()
    {
        if (_updating)
            throw new InvalidOperationException("Item template creation cannot be reentered.");
        _updating = true;
        try
        {
            if (ItemsPanelRoot is VirtualizingPanel panel)
            {
                SynchronizeVirtualSource(panel);
                return;
            }
            var reusable = new Dictionary<object, Queue<Control>>(ReferenceEqualityComparer.Instance);
            var nullItems = new Queue<Control>();
            if (!_templateDirty)
            {
                for (int i = 0; i < _sources.Count; i++)
                {
                    if (_sources[i] is not object source)
                    {
                        nullItems.Enqueue(_presentations[i]);
                        continue;
                    }
                    if (!reusable.TryGetValue(source, out Queue<Control>? queue))
                    {
                        queue = new Queue<Control>();
                        reusable.Add(source, queue);
                    }
                    queue.Enqueue(_presentations[i]);
                }
            }

            var sources = new List<object?>();
            var presentations = new List<Control>();
            var unique = new HashSet<Control>(ReferenceEqualityComparer.Instance);
            foreach (object? source in _itemsSource ?? Items)
            {
                Queue<Control>? queue = source is null ? nullItems : reusable.GetValueOrDefault(source);
                bool reused = queue is { Count: > 0 };
                Control control = reused ? queue!.Dequeue()
                    : _itemTemplate is null ? source as Control ?? new ItemContainer(source)
                    : _itemTemplate.Create(source) ?? throw new InvalidOperationException("The item template returned null.");
                if (!unique.Add(control))
                    throw new InvalidOperationException("An item template returned the same control more than once.");
                if (!ReferenceEquals(control.Parent, ItemsPanelRoot))
                {
                    if (control.Parent is not null || control.RootOwner is not null || ReferenceEquals(control, this))
                        throw new InvalidOperationException("An item template must return an unowned control.");
                    for (Control? ancestor = Parent; ancestor is not null; ancestor = ancestor.Parent)
                        if (ReferenceEquals(ancestor, control))
                            throw new InvalidOperationException("An item template cannot create an ancestry cycle.");
                }
                sources.Add(source);
                presentations.Add(control);
                if (!reused && source is not Control)
                {
                    control.DataContext = source;
                    if (_itemTemplate is IRecyclingItemTemplate recycling)
                        recycling.Rebind(control, source);
                }
            }

            foreach (Control previous in _presentations)
                if (!unique.Contains(previous))
                    ItemsPanelRoot.Remove(previous);
            for (int i = 0; i < presentations.Count; i++)
            {
                Control presentation = presentations[i];
                if (!ReferenceEquals(presentation.Parent, ItemsPanelRoot))
                    ItemsPanelRoot.Add(presentation);
                ItemsPanelRoot.Move(presentation, i);
            }
            _sources.Clear();
            _sources.AddRange(sources);
            _presentations.Clear();
            _presentations.AddRange(presentations);
            RefreshSelectionState();
            _itemsDirty = false;
            _templateDirty = false;
        }
        finally
        {
            _updating = false;
        }
    }

    /// <summary>Retains indexed data without creating presentations for offscreen items.</summary>
    /// <param name="panel">The virtual panel that owns the pooled controls.</param>
    private void SynchronizeVirtualSource(VirtualizingPanel panel)
    {
        IEnumerable source = _itemsSource ?? Items;
        if (source is not IList indexed)
        {
            var snapshot = new List<object?>();
            foreach (object? item in source)
                snapshot.Add(item);
            indexed = snapshot;
        }

        if (_templateDirty)
            ClearVirtualPool(panel);
        else
            for (int i = 0; i < _virtualPool.Count; i++)
                if (_virtualPool[i].Index >= 0)
                    _virtualPool[i] = (_virtualPool[i].Control, -2);
        _virtualSource = indexed;
        _itemsDirty = false;
        _templateDirty = false;
        panel.InvalidateMeasure();
    }

    /// <summary>Releases all containers owned by a virtual panel.</summary>
    /// <param name="panel">The panel that owns the containers.</param>
    private void ClearVirtualPool(VirtualizingPanel panel)
    {
        foreach (var entry in _virtualPool)
            panel.Remove(entry.Control);
        _virtualPool.Clear();
    }

    /// <summary>Updates the bounded pool and rebinds slots entering the realized range.</summary>
    /// <param name="panel">The panel requesting realization.</param>
    /// <param name="first">The first required source index.</param>
    /// <param name="count">The number of required presentations.</param>
    internal void RealizeVirtualRange(VirtualizingPanel panel, int first, int count)
    {
        if (_virtualSource is not { } source)
            return;

        while (_virtualPool.Count > count)
        {
            panel.Remove(_virtualPool[^1].Control);
            _virtualPool.RemoveAt(_virtualPool.Count - 1);
        }

        while (_virtualPool.Count < count)
        {
            int slot = _virtualPool.Count;
            int index = first + (int)((slot - (long)first % count + count) % count);
            Control control = _itemTemplate is null ? new ItemContainer(null)
                : _itemTemplate.Create(source[index]) ?? throw new InvalidOperationException("The item template returned null.");
            panel.Add(control);
            _virtualPool.Add((control, -1));
        }

        for (int index = first; index < first + count; index++)
        {
            int slot = index % count;
            var (control, previousIndex) = _virtualPool[slot];
            if (index == previousIndex || !ReferenceEquals(control.CommittedParent, panel))
                continue;

            object? item = source[index];
            if (item is Control)
                throw new InvalidOperationException("A virtualized items source must contain data rather than owned controls.");
            Workspace? workspace = Workspace;
            if (previousIndex != -1 && !ReferenceEquals(control.DataContext, item))
                workspace?.ResetRecycledInteraction(control, SelectionOwner);
            Control? previousRoot = workspace?.LocalLayoutRoot;
            if (workspace is not null)
                workspace.LocalLayoutRoot = control;
            try
            {
                control.DataContext = item;
                control.IsItemSelected = SelectionState?.Invoke(index) ?? false;
                if (_itemTemplate is IRecyclingItemTemplate template)
                    template.Rebind(control, item);
                else
                    ((ItemContainer)control).Content = item;
                Sachssoft.Engine.Controls.Workspace.PrepareTree(control);
                _virtualPool[slot] = (control, index);
            }
            finally
            {
                if (workspace is not null)
                    workspace.LocalLayoutRoot = previousRoot;
            }
        }
    }

    /// <summary>Finds the pool slot currently presenting the requested index.</summary>
    /// <param name="index">The source index.</param>
    /// <returns>The realized presentation or null.</returns>
    internal Control? GetVirtualPresentation(int index)
    {
        if (index < 0 || _virtualPool.Count == 0)
            return null;
        var entry = _virtualPool[index % _virtualPool.Count];
        return entry.Index == index ? entry.Control : null;
    }
}

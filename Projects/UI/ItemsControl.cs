using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Sachssoft.Engine.Controls;

/// <summary>Presents direct items or an enumerable source through item and panel templates.</summary>
/// <remarks>Source changes are reconciled before layout. Existing item presentations survive moves
/// and changes to other items. This base control materializes its items; specialized virtualized
/// controls retain their own presentation strategies. Sources must change on the UI thread.</remarks>
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

    /// <summary>Creates an empty items host using a vertical stack by default.</summary>
    public ItemsControl()
    {
        Items = new ItemCollection(this);
        Items.CollectionChanged += SourceChanged;
        SetOwnedControl(ref _panel, new Layouts.StackPanel(), ItemsPanelRootChangedArgs);
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
            foreach (Control presentation in _presentations)
                previous.Remove(presentation);
            SetOwnedControl(ref _panel, next, ItemsPanelRootChangedArgs);
            foreach (Control presentation in _presentations)
                next.Add(presentation);
            _itemsPanel = value;
            NotifyPropertyChanged(ItemsPanelChangedArgs);
        }
    }

    /// <summary>Gets the owned container created by the active panel template.</summary>
    public ContainerControl ItemsPanelRoot => (ContainerControl)_panel!;

    /// <summary>Gets the current number of materialized items.</summary>
    public int ItemCount => _presentations.Count;

    /// <inheritdoc/>
    protected override string DefaultStyleName => nameof(ItemsControl);

    /// <inheritdoc/>
    protected override bool DrawsDecoration => false;

    /// <summary>Gets an existing presentation by its source-order index.</summary>
    /// <param name="index">The zero-based index in the last synchronized source.</param>
    /// <returns>The presentation retained for that item.</returns>
    public Control ContainerFromIndex(int index) => _presentations[index];

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
        ItemsPanelRoot.Measure(context, available);
        return ItemsPanelRoot.DesiredSize;
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 contentBounds) => ItemsPanelRoot.Arrange(contentBounds, true);

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
                Control control = queue is { Count: > 0 } ? queue.Dequeue()
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
            _itemsDirty = false;
            _templateDirty = false;
        }
        finally
        {
            _updating = false;
        }
    }
}

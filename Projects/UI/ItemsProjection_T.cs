using System.Collections;
using System.Collections.Specialized;

namespace Sachssoft.Engine.Controls;

/// <summary>Projects an observable data source onto an existing specialized item collection.</summary>
/// <typeparam name="T">The item produced by the control's template.</typeparam>
internal sealed class ItemsProjection<T> where T : class
{
    /// <summary>Schedules preparation on the owner.</summary>
    private readonly Action _invalidate;

    /// <summary>Stores projected items and the source identity from which each was created.</summary>
    private readonly List<(object? Source, T Item)> _entries = new();

    /// <summary>Stores the externally supplied source.</summary>
    private IEnumerable? _source;

    /// <summary>Tracks whether collection notifications are currently subscribed.</summary>
    private bool _attached;

    /// <summary>Tracks whether reconciliation has been scheduled.</summary>
    private bool _dirty;

    /// <summary>Tracks whether a changed template invalidates existing projections.</summary>
    private bool _recreate;

    /// <summary>Tracks whether the external source owns the target collection.</summary>
    private bool _ownsTarget;

    /// <summary>Creates a projection with a callback scheduling its next preparation pass.</summary>
    /// <param name="invalidate">Schedules preparation on the owner.</param>
    internal ItemsProjection(Action invalidate) => _invalidate = invalidate;

    /// <summary>Gets the currently assigned data source.</summary>
    internal IEnumerable? Source => _source;

    /// <summary>Gets whether the source will be synchronized at the next preparation boundary.</summary>
    internal bool NeedsSynchronization => _dirty && _ownsTarget;

    /// <summary>Gets whether the target collection is controlled by the source.</summary>
    internal bool OwnsItems => _ownsTarget;

    /// <summary>Gets whether the projection is applying its own collection changes.</summary>
    internal bool IsSynchronizing { get; private set; }

    /// <summary>Assigns a new source. A direct collection must be empty before it becomes source-driven.</summary>
    /// <param name="source">The external source, or null to restore direct items.</param>
    /// <param name="hasDirectItems">Whether the target currently holds caller-owned direct items.</param>
    internal void SetSource(IEnumerable? source, bool hasDirectItems)
    {
        if (ReferenceEquals(_source, source))
            return;
        if (source is not null && !_ownsTarget && hasDirectItems)
            throw new InvalidOperationException("Clear direct Items before assigning ItemsSource.");
        Unsubscribe();
        _source = source;
        if (source is not null)
            _ownsTarget = true;
        Subscribe();
        Refresh();
    }

    /// <summary>Schedules reconciliation and optionally invalidates existing presentations.</summary>
    /// <param name="recreate">Whether a template change requires fresh items.</param>
    internal void Refresh(bool recreate = false)
    {
        _dirty = true;
        _recreate |= recreate;
        _invalidate();
    }

    /// <summary>Observes source changes while the control is attached.</summary>
    internal void Attach()
    {
        if (_attached)
            return;
        _attached = true;
        Subscribe();
        if (_ownsTarget)
            Refresh();
    }

    /// <summary>Releases the external collection subscription.</summary>
    internal void Detach()
    {
        Unsubscribe();
        _attached = false;
    }

    /// <summary>Reconciles the projected collection while retaining unchanged item identities.</summary>
    /// <param name="target">The control's existing item collection.</param>
    /// <param name="create">Creates an item for a source entry.</param>
    /// <param name="move">Optionally moves existing items without releasing their ownership.</param>
    /// <returns>Whether synchronization ran.</returns>
    /// <param name="allowRepeatedItems">Whether the target accepts repeated references.</param>
    internal bool Synchronize(IList<T> target, Func<object?, T> create, Action<int, int>? move = null,
        bool allowRepeatedItems = false)
    {
        if (!_dirty || !_ownsTarget)
            return false;
        bool recreate = _recreate;
        _dirty = _recreate = false;
        IsSynchronizing = true;
        try
        {
            var next = new List<(object? Source, T Item)>();
            var reusable = new Dictionary<object, Queue<T>>(ReferenceEqualityComparer.Instance);
            var nullItems = new Queue<T>();
            if (!recreate)
            {
                foreach (var entry in _entries)
                {
                    if (entry.Source is null)
                        nullItems.Enqueue(entry.Item);
                    else
                    {
                        if (!reusable.TryGetValue(entry.Source, out Queue<T>? queue))
                            reusable.Add(entry.Source, queue = new Queue<T>());
                        queue.Enqueue(entry.Item);
                    }
                }
            }

            if (_source is not null)
                foreach (object? source in _source)
                {
                    Queue<T>? queue = source is null ? nullItems : reusable.GetValueOrDefault(source);
                    T item = queue is { Count: > 0 } ? queue.Dequeue() : create(source);
                    next.Add((source, item));
                }

            if (allowRepeatedItems)
            {
                while (target.Count > next.Count)
                    target.RemoveAt(target.Count - 1);
                for (int i = 0; i < next.Count; i++)
                {
                    if (i >= target.Count)
                        target.Add(next[i].Item);
                    else if (!ReferenceEquals(target[i], next[i].Item))
                        target[i] = next[i].Item;
                }
                _entries.Clear();
                _entries.AddRange(next);
                _ownsTarget = _source is not null;
                return true;
            }

            var retained = new HashSet<T>(ReferenceEqualityComparer.Instance);
            foreach (var entry in next)
                if (!retained.Add(entry.Item))
                    throw new InvalidOperationException("The item template returned the same item more than once.");
            for (int i = target.Count - 1; i >= 0; i--)
                if (!retained.Contains(target[i]))
                    target.RemoveAt(i);
            for (int i = 0; i < next.Count; i++)
            {
                T item = next[i].Item;
                if (i < target.Count && ReferenceEquals(target[i], item))
                    continue;
                int oldIndex = i;
                while (oldIndex < target.Count && !ReferenceEquals(target[oldIndex], item))
                    oldIndex++;
                if (oldIndex < target.Count && move is not null)
                    move(oldIndex, i);
                else
                {
                    if (oldIndex < target.Count)
                        target.RemoveAt(oldIndex);
                    target.Insert(i, item);
                }
            }
            _entries.Clear();
            _entries.AddRange(next);
            _ownsTarget = _source is not null;
            return true;
        }
        catch
        {
            _dirty = true;
            _recreate |= recreate;
            throw;
        }
        finally
        {
            IsSynchronizing = false;
        }
    }

    /// <summary>Subscribes to the current observable source while attached.</summary>
    private void Subscribe()
    {
        if (_attached && _source is INotifyCollectionChanged observable)
            observable.CollectionChanged += SourceChanged;
    }

    /// <summary>Releases the current observable source subscription.</summary>
    private void Unsubscribe()
    {
        if (_attached && _source is INotifyCollectionChanged observable)
            observable.CollectionChanged -= SourceChanged;
    }

    /// <summary>Schedules reconciliation after an observable collection mutation.</summary>
    /// <param name="sender">The observed source.</param>
    /// <param name="args">The collection change notification.</param>
    private void SourceChanged(object? sender, NotifyCollectionChangedEventArgs args) => Refresh();
}

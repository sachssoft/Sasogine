using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Sachssoft.Engine.Common.Collections;

/// <summary>
/// Provides a read-only collection of engine objects that is automatically
/// synchronized with a collection of definitions.
/// </summary>
/// <typeparam name="TDefinition">
/// The type of definition used to create engine objects.
/// </typeparam>
/// <typeparam name="TObject">
/// The type of engine object exposed by the collection.
/// </typeparam>
/// <remarks>
/// <para>
/// Each definition is associated with exactly one engine object created by
/// the configured factory.
/// </para>
/// <para>
/// Changes to the definition collection are automatically reflected in this
/// collection. Consumers cannot directly modify the engine object collection.
/// </para>
/// <para>
/// A mutable connection can optionally be established for a specific engine
/// object when the collection is created through
/// <see cref="DefinitionBindingConnection"/>.
/// </para>
/// </remarks>
public class DefinitionBindingCollection<TDefinition, TObject> :
    IReadOnlyBindingCollection<TObject>,
    IDisposable
    where TDefinition : class, IDefinition
    where TObject : class, IEngineObject
{
    private readonly TrackableCollection<TDefinition> _definitions;
    private readonly TrackableCollection<TObject> _objects;
    private readonly IDefinitionBindingFactory<TDefinition, TObject> _factory;
    private readonly IEngineObject? _connectionOwner;
    private readonly MutableList _mutable;

    private readonly Dictionary<TDefinition, TObject> _bindings =
        new(ReferenceEqualityComparer.Instance);

    private bool _disposed;
    private bool _wasConnected;

    private TDefinition? _pendingDefinition;
    private TObject? _pendingObject;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="DefinitionBindingCollection{TDefinition, TObject}"/> class.
    /// </summary>
    /// <param name="definitions">
    /// The definition collection to observe.
    /// </param>
    /// <param name="factory">
    /// The factory used to create and release engine objects.
    /// </param>
    public DefinitionBindingCollection(
        TrackableCollection<TDefinition> definitions,
        IDefinitionBindingFactory<TDefinition, TObject> factory)
        : this(
            definitions,
            factory,
            new TrackableCollection<TObject>(definitions.Count),
            null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="DefinitionBindingCollection{TDefinition, TObject}"/> class
    /// using the specified engine object collection.
    /// </summary>
    /// <param name="definitions">
    /// The definition collection to observe.
    /// </param>
    /// <param name="factory">
    /// The factory used to create and release engine objects.
    /// </param>
    /// <param name="objects">
    /// The collection used to store the bound engine objects.
    /// </param>
    protected DefinitionBindingCollection(
        TrackableCollection<TDefinition> definitions,
        IDefinitionBindingFactory<TDefinition, TObject> factory,
        TrackableCollection<TObject> objects)
        : this(
            definitions,
            factory,
            objects,
            null)
    {
    }

    internal DefinitionBindingCollection(
        TrackableCollection<TDefinition> definitions,
        IDefinitionBindingFactory<TDefinition, TObject> factory,
        IEngineObject? connectionOwner)
        : this(
            definitions,
            factory,
            new TrackableCollection<TObject>(definitions.Count),
            connectionOwner)
    {
    }

    private protected DefinitionBindingCollection(
        TrackableCollection<TDefinition> definitions,
        IDefinitionBindingFactory<TDefinition, TObject> factory,
        TrackableCollection<TObject> objects,
        IEngineObject? connectionOwner)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(objects);

        _definitions = definitions;
        _factory = factory;
        _objects = objects;
        _connectionOwner = connectionOwner;
        _mutable = new MutableList(this);

        foreach (TDefinition definition in definitions)
            Create(definition, _objects.Count);

        _definitions.CollectionChanged += DefinitionsCollectionChanged;
    }

    /// <summary>
    /// Occurs when the engine object collection changes.
    /// </summary>
    public event NotifyCollectionChangedEventHandler? CollectionChanged
    {
        add => _objects.CollectionChanged += value;
        remove => _objects.CollectionChanged -= value;
    }

    /// <summary>
    /// Occurs when a property of the engine object collection changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged
    {
        add => _objects.PropertyChanged += value;
        remove => _objects.PropertyChanged -= value;
    }

    /// <summary>
    /// Gets the number of engine objects in the collection.
    /// </summary>
    public int Count => _objects.Count;

    /// <summary>
    /// Gets the engine object at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index of the engine object.
    /// </param>
    public TObject this[int index] => _objects[index];

    /// <summary>
    /// Gets the internally managed engine object collection.
    /// </summary>
    /// <remarks>
    /// Derived collection types may use this collection to provide additional
    /// lookup functionality. It must not be exposed as mutable public API.
    /// </remarks>
    protected TrackableCollection<TObject> Objects => _objects;

    /// <summary>
    /// Gets the engine object associated with the specified definition.
    /// </summary>
    /// <param name="definition">
    /// The definition whose engine object should be returned.
    /// </param>
    /// <returns>
    /// The associated engine object.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// The definition does not belong to the source collection.
    /// </exception>
    public TObject GetObject(TDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        return _bindings[definition];
    }

    /// <summary>
    /// Attempts to get the engine object associated with the specified definition.
    /// </summary>
    /// <param name="definition">
    /// The definition whose engine object should be returned.
    /// </param>
    /// <param name="result">
    /// The associated engine object if found.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if an associated engine object exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGetObject(
        TDefinition definition,
        out TObject? result)
    {
        ArgumentNullException.ThrowIfNull(definition);

        return _bindings.TryGetValue(definition, out result);
    }

    /// <summary>
    /// Returns an enumerator that iterates through the engine objects.
    /// </summary>
    public IEnumerator<TObject> GetEnumerator() =>
        _objects.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    /// <summary>
    /// Releases the resources used by the binding collection.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _definitions.CollectionChanged -= DefinitionsCollectionChanged;

        foreach (KeyValuePair<TDefinition, TObject> pair in _bindings)
            _factory.ReleaseInstance(pair.Key, pair.Value);

        _bindings.Clear();
        _objects.Clear();

        _disposed = true;

        GC.SuppressFinalize(this);
    }

    internal IBindingCollection<TObject> Connect(
        IEngineObject connectionOwner)
    {
        ThrowIfDisposed();

        ArgumentNullException.ThrowIfNull(connectionOwner);

        if (_connectionOwner is null)
        {
            throw new InvalidOperationException(
                "The collection does not have a connection owner.");
        }

        if (!ReferenceEquals(connectionOwner, _connectionOwner))
        {
            throw new InvalidOperationException(
                "The specified engine object is not the connection owner.");
        }

        if (_wasConnected)
        {
            throw new InvalidOperationException(
                "The collection has already been connected.");
        }

        _wasConnected = true;

        return _mutable;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private void DefinitionsCollectionChanged(
        object? sender,
        NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                HandleAdd(e);
                break;

            case NotifyCollectionChangedAction.Remove:
                HandleRemove(e);
                break;

            case NotifyCollectionChangedAction.Replace:
                HandleReplace(e);
                break;

            case NotifyCollectionChangedAction.Move:
                HandleMove(e);
                break;

            case NotifyCollectionChangedAction.Reset:
                Rebuild();
                break;

            default:
                throw new NotSupportedException(
                    $"Collection action '{e.Action}' is not supported.");
        }
    }

    private void HandleAdd(
        NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is null)
            return;

        int index = e.NewStartingIndex;

        foreach (object? value in e.NewItems)
        {
            if (value is TDefinition definition)
                Create(definition, index++);
        }
    }

    private void HandleRemove(
        NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is null)
            return;

        foreach (object? value in e.OldItems)
        {
            if (value is TDefinition definition)
                Remove(definition);
        }
    }

    private void HandleReplace(
        NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is null ||
            e.NewItems is null)
        {
            return;
        }

        int count = Math.Min(
            e.OldItems.Count,
            e.NewItems.Count);

        for (int i = 0; i < count; i++)
        {
            if (e.OldItems[i] is not TDefinition oldDefinition ||
                e.NewItems[i] is not TDefinition newDefinition)
            {
                continue;
            }

            Replace(
                oldDefinition,
                newDefinition,
                e.NewStartingIndex + i);
        }
    }

    private void HandleMove(
        NotifyCollectionChangedEventArgs e)
    {
        if (e.OldStartingIndex < 0 ||
            e.NewStartingIndex < 0)
        {
            return;
        }

        _objects.Move(
            e.OldStartingIndex,
            e.NewStartingIndex);
    }

    private void Create(
        TDefinition definition,
        int index)
    {
        if (_bindings.ContainsKey(definition))
        {
            throw new InvalidOperationException(
                "The definition is already bound to an engine object.");
        }

        bool usePending =
            ReferenceEquals(definition, _pendingDefinition) &&
            _pendingObject is not null;

        TObject instance = usePending
            ? _pendingObject!
            : _factory.CreateInstance(definition);

        ArgumentNullException.ThrowIfNull(instance);

        try
        {
            _factory.AttachInstance(definition, instance);
            _objects.Insert(index, instance);
            _bindings.Add(definition, instance);
        }
        catch
        {
            _factory.ReleaseInstance(definition, instance);
            throw;
        }
    }

    private void Remove(
        TDefinition definition)
    {
        if (!_bindings.Remove(
                definition,
                out TObject? instance))
        {
            return;
        }

        _objects.Remove(instance);
        _factory.ReleaseInstance(definition, instance);
    }

    private void Replace(
        TDefinition oldDefinition,
        TDefinition newDefinition,
        int index)
    {
        if (!_bindings.TryGetValue(
                oldDefinition,
                out TObject? oldInstance))
        {
            Rebuild();
            return;
        }

        bool usePending =
            ReferenceEquals(newDefinition, _pendingDefinition) &&
            _pendingObject is not null;

        TObject newInstance = usePending
            ? _pendingObject!
            : _factory.CreateInstance(newDefinition);

        ArgumentNullException.ThrowIfNull(newInstance);

        _bindings.Remove(oldDefinition);

        try
        {
            _factory.AttachInstance(newDefinition, newInstance);
            _objects[index] = newInstance;
            _bindings.Add(newDefinition, newInstance);
        }
        catch
        {
            _bindings.Add(oldDefinition, oldInstance);
            _factory.ReleaseInstance(newDefinition, newInstance);
            throw;
        }

        _factory.ReleaseInstance(oldDefinition, oldInstance);
    }

    private void Rebuild()
    {
        foreach (KeyValuePair<TDefinition, TObject> pair in _bindings)
            _factory.ReleaseInstance(pair.Key, pair.Value);

        _bindings.Clear();
        _objects.Clear();

        foreach (TDefinition definition in _definitions)
            Create(definition, _objects.Count);
    }

    private void InsertObject(
        int index,
        TObject item)
    {
        ThrowIfDisposed();

        ArgumentNullException.ThrowIfNull(item);

        if (item.Definition is not TDefinition definition)
        {
            throw new ArgumentException(
                $"The engine object definition must be of type '{typeof(TDefinition).FullName}'.",
                nameof(item));
        }

        if (_objects.Contains(item))
        {
            throw new InvalidOperationException(
                "The engine object already belongs to this collection.");
        }

        if (_bindings.ContainsKey(definition))
        {
            throw new InvalidOperationException(
                "The engine object definition already belongs to this collection.");
        }

        _pendingDefinition = definition;
        _pendingObject = item;

        try
        {
            _definitions.Insert(index, definition);
        }
        finally
        {
            _pendingDefinition = null;
            _pendingObject = null;
        }
    }

    private void ReplaceObject(
        int index,
        TObject item)
    {
        ThrowIfDisposed();

        ArgumentNullException.ThrowIfNull(item);

        if (item.Definition is not TDefinition definition)
        {
            throw new ArgumentException(
                $"The engine object definition must be of type '{typeof(TDefinition).FullName}'.",
                nameof(item));
        }

        TObject oldObject = _objects[index];

        if (ReferenceEquals(oldObject, item))
            return;

        TDefinition oldDefinition = _definitions[index];

        if (!ReferenceEquals(oldDefinition, definition) &&
            _bindings.ContainsKey(definition))
        {
            throw new InvalidOperationException(
                "The engine object definition already belongs to this collection.");
        }

        _pendingDefinition = definition;
        _pendingObject = item;

        try
        {
            _definitions[index] = definition;
        }
        finally
        {
            _pendingDefinition = null;
            _pendingObject = null;
        }
    }

    private sealed class MutableList :
        IBindingCollection<TObject>
    {
        private readonly DefinitionBindingCollection<TDefinition, TObject> _owner;

        public MutableList(
            DefinitionBindingCollection<TDefinition, TObject> owner)
        {
            _owner = owner;
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged
        {
            add => _owner._objects.CollectionChanged += value;
            remove => _owner._objects.CollectionChanged -= value;
        }

        public event PropertyChangedEventHandler? PropertyChanged
        {
            add => _owner._objects.PropertyChanged += value;
            remove => _owner._objects.PropertyChanged -= value;
        }

        public int Count => _owner._objects.Count;

        public bool IsReadOnly => false;

        public TObject this[int index]
        {
            get => _owner._objects[index];
            set => _owner.ReplaceObject(index, value);
        }

        public void Add(
            TObject item)
        {
            _owner.InsertObject(
                _owner._objects.Count,
                item);
        }

        public void Clear()
        {
            _owner.ThrowIfDisposed();
            _owner._definitions.Clear();
        }

        public bool Contains(
            TObject item) =>
            _owner._objects.Contains(item);

        public void CopyTo(
            TObject[] array,
            int arrayIndex) =>
            _owner._objects.CopyTo(
                array,
                arrayIndex);

        public IEnumerator<TObject> GetEnumerator() =>
            _owner._objects.GetEnumerator();

        public int IndexOf(
            TObject item) =>
            _owner._objects.IndexOf(item);

        public void Insert(
            int index,
            TObject item)
        {
            _owner.InsertObject(
                index,
                item);
        }

        public bool Remove(
            TObject item)
        {
            _owner.ThrowIfDisposed();

            int index = _owner._objects.IndexOf(item);

            if (index < 0)
                return false;

            _owner._definitions.RemoveAt(index);

            return true;
        }

        public void RemoveAt(
            int index)
        {
            _owner.ThrowIfDisposed();
            _owner._definitions.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}
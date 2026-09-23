using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Sachssoft.Engine.Common.Collections;

/// <summary>
/// Provides a context-aware read-only collection of engine objects that is
/// synchronized with a trackable definition collection.
/// </summary>
/// <remarks>
/// Definitions exist independently from the runtime context. When
/// <see cref="Context"/> is <see langword="null"/>, no engine objects are
/// bound. Assigning or changing the context rebuilds the runtime bindings.
/// </remarks>
public class DefinitionBindingCollection<TDefinition, TObject, TContext> :
    IReadOnlyList<TObject>,
    INotifyCollectionChanged,
    INotifyPropertyChanged,
    IDisposable
    where TDefinition : class, IDefinition
    where TObject : class, IEngineObject
    where TContext : class
{
    private readonly TrackableCollection<TDefinition> _definitions;
    private readonly TrackableCollection<TObject> _objects;
    private readonly IDefinitionBindingFactory<TDefinition, TObject, TContext> _factory;
    private readonly Dictionary<TDefinition, TObject> _bindings =
        new(ReferenceEqualityComparer.Instance);

    private bool _disposed;
    private TContext? _context;

    /// <summary>
    /// Initializes a new instance of the context-aware definition binding collection.
    /// </summary>
    /// <param name="definitions">The source collection of definitions.</param>
    /// <param name="factory">The factory used to manage engine object bindings.</param>
    /// <param name="context">The optional runtime context.</param>
    public DefinitionBindingCollection(
        TrackableCollection<TDefinition> definitions,
        IDefinitionBindingFactory<TDefinition, TObject, TContext> factory,
        TContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentNullException.ThrowIfNull(factory);

        _definitions = definitions;
        _factory = factory;
        _context = context;
        _objects = new TrackableCollection<TObject>(definitions.Count);

        if (_context is not null)
            CreateObjects();

        _definitions.CollectionChanged += DefinitionsCollectionChanged;
        _objects.PropertyChanged += ObjectsPropertyChanged;
    }

    /// <summary>
    /// Gets or sets the runtime context used to create and manage engine objects.
    /// </summary>
    /// <remarks>
    /// Setting this property to <see langword="null"/> releases all bound engine
    /// objects. Assigning a non-null context creates objects for all definitions.
    /// Replacing a non-null context releases the old objects before recreating them.
    /// </remarks>
    public TContext? Context
    {
        get => _context;
        set
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (ReferenceEquals(_context, value))
                return;

            ReleaseObjects();
            _context = value;

            if (_context is not null)
                CreateObjects();

            OnPropertyChanged(nameof(Context));
        }
    }

    /// <summary>Gets the number of currently bound engine objects.</summary>
    public int Count => _objects.Count;

    /// <summary>Gets the engine object at the specified index.</summary>
    public TObject this[int index] => _objects[index];

    /// <summary>Gets the internal object collection.</summary>
    protected TrackableCollection<TObject> Objects => _objects;

    /// <inheritdoc/>
    public event NotifyCollectionChangedEventHandler? CollectionChanged
    {
        add => _objects.CollectionChanged += value;
        remove => _objects.CollectionChanged -= value;
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets the engine object bound to the specified definition.</summary>
    public TObject GetObject(TDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return _bindings[definition];
    }

    /// <summary>Attempts to get the engine object bound to the specified definition.</summary>
    public bool TryGetObject(TDefinition definition, out TObject? result)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return _bindings.TryGetValue(definition, out result);
    }

    /// <inheritdoc/>
    public IEnumerator<TObject> GetEnumerator() => _objects.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Releases the resources used by the binding collection.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _definitions.CollectionChanged -= DefinitionsCollectionChanged;
        _objects.PropertyChanged -= ObjectsPropertyChanged;

        ReleaseObjects();
        _context = null;

        _disposed = true;

        GC.SuppressFinalize(this);
    }

    /// <summary>Raises the <see cref="PropertyChanged"/> event.</summary>
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void ObjectsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, e);
    }

    private void DefinitionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_context is null)
            return;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                AddDefinitions(e);
                break;
            case NotifyCollectionChangedAction.Remove:
                RemoveDefinitions(e);
                break;
            case NotifyCollectionChangedAction.Replace:
                ReplaceDefinitions(e);
                break;
            case NotifyCollectionChangedAction.Move:
                MoveDefinitions(e);
                break;
            case NotifyCollectionChangedAction.Reset:
                Rebuild();
                break;
        }
    }

    private void AddDefinitions(NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is null)
            return;

        int index = e.NewStartingIndex;
        foreach (object? item in e.NewItems)
        {
            if (item is TDefinition definition)
                Create(definition, index++);
        }
    }

    private void RemoveDefinitions(NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is null)
            return;

        foreach (object? item in e.OldItems)
        {
            if (item is TDefinition definition)
                Remove(definition);
        }
    }

    private void ReplaceDefinitions(NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is null || e.NewItems is null)
            return;

        int count = Math.Min(e.OldItems.Count, e.NewItems.Count);
        for (int i = 0; i < count; i++)
        {
            if (e.OldItems[i] is TDefinition oldDefinition &&
                e.NewItems[i] is TDefinition newDefinition)
            {
                Replace(oldDefinition, newDefinition, e.NewStartingIndex + i);
            }
        }
    }

    private void MoveDefinitions(NotifyCollectionChangedEventArgs e)
    {
        if (e.OldStartingIndex < 0 || e.NewStartingIndex < 0)
            return;

        _objects.Move(e.OldStartingIndex, e.NewStartingIndex);
    }

    private void CreateObjects()
    {
        if (_context is null)
            return;

        foreach (TDefinition definition in _definitions)
            Create(definition, _objects.Count);
    }

    private void ReleaseObjects()
    {
        if (_context is null)
            return;

        foreach (KeyValuePair<TDefinition, TObject> binding in _bindings)
        {
            _factory.ReleaseInstance(binding.Key, binding.Value, _context);
        }

        _bindings.Clear();
        _objects.Clear();
    }

    private void Create(TDefinition definition, int index)
    {
        if (_context is null)
            return;

        if (_bindings.ContainsKey(definition))
            throw new InvalidOperationException("The definition is already bound to an engine object.");

        TObject instance = _factory.CreateInstance(definition, _context);
        ArgumentNullException.ThrowIfNull(instance);

        _factory.AttachInstance(definition, instance, _context);
        _objects.Insert(index, instance);
        _bindings.Add(definition, instance);
    }

    private void Remove(TDefinition definition)
    {
        if (_context is null)
            return;

        if (!_bindings.Remove(definition, out TObject? instance))
            return;

        _objects.Remove(instance);
        _factory.ReleaseInstance(definition, instance, _context);
    }

    private void Replace(TDefinition oldDefinition, TDefinition newDefinition, int index)
    {
        if (_context is null)
            return;

        if (!_bindings.TryGetValue(oldDefinition, out TObject? oldInstance))
        {
            Rebuild();
            return;
        }

        TObject newInstance = _factory.CreateInstance(newDefinition, _context);
        ArgumentNullException.ThrowIfNull(newInstance);

        _factory.AttachInstance(newDefinition, newInstance, _context);
        _bindings.Remove(oldDefinition);
        _objects[index] = newInstance;
        _bindings.Add(newDefinition, newInstance);
        _factory.ReleaseInstance(oldDefinition, oldInstance, _context);
    }

    private void Rebuild()
    {
        ReleaseObjects();

        if (_context is not null)
            CreateObjects();
    }
}

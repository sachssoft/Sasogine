using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Collections;

/// <summary>
/// Represents an ordered, trackable collection of engine-referenceable objects
/// with context-based lifecycle management.
/// </summary>
/// <typeparam name="T">
/// The type of engine-referenceable object contained in the collection.
/// </typeparam>
/// <typeparam name="TContext">
/// The type of engine object context associated with the collection.
/// </typeparam>
/// <remarks>
/// The collection can be initialized with a context. Existing objects that
/// implement <see cref="IInitializableEngineObject"/> are initialized
/// automatically, while objects added later are initialized when inserted.
///
/// Removing or replacing objects deinitializes them automatically. The collection
/// can also be deinitialized or reinitialized with another context.
/// </remarks>
public class ContextualReferencableCollection<T, TContext>
    : ReferencableCollection<T>
    where T : class, IEngineReferenceable
    where TContext : class, IEngineObjectContext
{
    /// <summary>
    /// Initializes a new, empty instance of the
    /// <see cref="ContextualReferencableCollection{T, TContext}"/> class.
    /// </summary>
    public ContextualReferencableCollection()
    {
    }

    /// <summary>
    /// Initializes a new, empty instance of the
    /// <see cref="ContextualReferencableCollection{T, TContext}"/> class
    /// with the specified initial capacity.
    /// </summary>
    /// <param name="capacity">
    /// The initial number of elements that the collection can contain
    /// without resizing.
    /// </param>
    public ContextualReferencableCollection(int capacity)
        : base(capacity)
    {
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ContextualReferencableCollection{T, TContext}"/> class
    /// containing the elements from the specified collection.
    /// </summary>
    /// <param name="collection">
    /// The collection whose elements are added to the new collection.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="collection"/> is <see langword="null"/>.
    /// </exception>
    public ContextualReferencableCollection(IEnumerable<T> collection)
        : base(collection)
    {
    }

    /// <summary>
    /// Occurs when the collection has been initialized.
    /// </summary>
    public event EventHandler? Initialized;

    /// <summary>
    /// Occurs when the collection has been deinitialized.
    /// </summary>
    public event EventHandler? Deinitialized;

    /// <summary>
    /// Occurs when the collection has been reinitialized.
    /// </summary>
    public event EventHandler? Reinitialized;

    /// <summary>
    /// Occurs when the context associated with the collection changes.
    /// </summary>
    public event EventHandler? ContextChanged;

    /// <summary>
    /// Gets the context currently associated with the collection.
    /// </summary>
    /// <value>
    /// The current context, or <see langword="null"/> if the collection is not
    /// initialized.
    /// </value>
    public TContext? Context { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the collection is initialized.
    /// </summary>
    public bool IsInitialized => Context is not null;

    /// <summary>
    /// Initializes the collection using the specified context.
    /// </summary>
    /// <param name="context">
    /// The context used to initialize the collection and its applicable objects.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The collection is already initialized.
    /// </exception>
    public virtual void Initialize(TContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (IsInitialized)
        {
            throw new InvalidOperationException(
                "The collection is already initialized.");
        }

        Context = context;

        foreach (T item in this)
            InitializeItem(item, context);

        OnContextChanged();
        OnInitialized();
    }

    /// <summary>
    /// Deinitializes the collection and all applicable objects.
    /// </summary>
    public virtual void Deinitialize()
    {
        if (!IsInitialized)
            return;

        for (int i = Count - 1; i >= 0; i--)
            DeinitializeItem(this[i]);

        Context = null;

        OnContextChanged();
        OnDeinitialized();
    }

    /// <summary>
    /// Reinitializes the collection using the specified context.
    /// </summary>
    /// <param name="context">
    /// The new context used to initialize the collection and its applicable objects.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    public virtual void Reinitialize(TContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (IsInitialized)
        {
            for (int i = Count - 1; i >= 0; i--)
                DeinitializeItem(this[i]);
        }

        Context = context;

        foreach (T item in this)
            InitializeItem(item, context);

        OnContextChanged();
        OnReinitialized();
    }

    /// <summary>
    /// Initializes an applicable object before it is inserted into the collection.
    /// </summary>
    /// <param name="index">The index at which the object will be inserted.</param>
    /// <param name="item">The object to insert.</param>
    /// <returns>
    /// <see langword="true"/> if the insertion may continue;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected override bool OnInserting(int index, T item)
    {
        if (!base.OnInserting(index, item))
            return false;

        if (Context is not null)
            InitializeItem(item, Context);

        return true;
    }

    /// <summary>
    /// Updates the lifecycle state before an existing object is replaced.
    /// </summary>
    /// <param name="index">The index of the object being replaced.</param>
    /// <param name="oldItem">The existing object.</param>
    /// <param name="newItem">The replacement object.</param>
    /// <returns>
    /// <see langword="true"/> if the replacement may continue;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected override bool OnSetting(int index, T oldItem, T newItem)
    {
        if (!base.OnSetting(index, oldItem, newItem))
            return false;

        if (Context is not null)
        {
            DeinitializeItem(oldItem);
            InitializeItem(newItem, Context);
        }

        return true;
    }

    /// <summary>
    /// Deinitializes an object after it has been removed from the collection.
    /// </summary>
    /// <param name="index">The previous index of the removed object.</param>
    /// <param name="item">The removed object.</param>
    protected override void OnRemoved(int index, T item)
    {
        DeinitializeItem(item);
        base.OnRemoved(index, item);
    }

    /// <summary>
    /// Deinitializes all applicable objects before the collection is cleared.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the collection may be cleared;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected override bool OnClearing()
    {
        if (!base.OnClearing())
            return false;

        if (Context is not null)
        {
            for (int i = Count - 1; i >= 0; i--)
                DeinitializeItem(this[i]);
        }

        return true;
    }

    /// <summary>
    /// Initializes the specified object if it supports engine object
    /// initialization.
    /// </summary>
    /// <param name="item">The object to initialize.</param>
    /// <param name="context">The context used to initialize the object.</param>
    protected virtual void InitializeItem(T item, TContext context)
    {
        if (item is IInitializableEngineObject initializable)
            initializable.Initialize(context);
    }

    /// <summary>
    /// Deinitializes the specified object if it supports engine object
    /// initialization and is currently initialized.
    /// </summary>
    /// <param name="item">The object to deinitialize.</param>
    protected virtual void DeinitializeItem(T item)
    {
        if (item is IInitializableEngineObject initializable &&
            initializable.IsInitialized)
        {
            initializable.Deinitialize();
        }
    }

    /// <summary>
    /// Raises the <see cref="Initialized"/> event.
    /// </summary>
    protected virtual void OnInitialized()
    {
        Initialized?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="Deinitialized"/> event.
    /// </summary>
    protected virtual void OnDeinitialized()
    {
        Deinitialized?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="Reinitialized"/> event.
    /// </summary>
    protected virtual void OnReinitialized()
    {
        Reinitialized?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="ContextChanged"/> event.
    /// </summary>
    protected virtual void OnContextChanged()
    {
        ContextChanged?.Invoke(this, EventArgs.Empty);
    }
}
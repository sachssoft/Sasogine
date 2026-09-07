using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Represents an observable reference to an engine object that automatically
/// updates its resolved value when the associated resolver changes.
/// </summary>
/// <typeparam name="T">
/// The expected type of the referenced engine object.
/// </typeparam>
/// <remarks>
/// <para>
/// The reference stores an object identifier and automatically binds itself to
/// an <see cref="IEngineObjectResolverProvider"/> when
/// <see cref="Resolve(IEngineObjectResolverProvider)"/> is called.
/// </para>
/// <para>
/// When the resolver implements <see cref="INotifyCollectionChanged"/>,
/// collection changes automatically cause the reference to be resolved again.
/// </para>
/// <para>
/// Resolver subscriptions are released when the reference is disposed.
/// </para>
/// </remarks>
public class ObservableReference<T> :
    IReference,
    INotifyPropertyChanged,
    IDisposable
    where T : class, IEngineReferenceable
{
    private IEngineObjectResolverProvider? _provider;
    private string? _id;
    private T? _value;
    private bool _disposed;

    /// <summary>
    /// Initializes a new empty instance of the
    /// <see cref="ObservableReference{T}"/> class.
    /// </summary>
    public ObservableReference()
    {
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ObservableReference{T}"/> class using the specified identifier.
    /// </summary>
    /// <param name="id">
    /// The identifier of the referenced object.
    /// </param>
    public ObservableReference(string? id)
    {
        _id = id;
    }

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets the expected type of the referenced object.
    /// </summary>
    public Type TargetType => typeof(T);

    /// <summary>
    /// Gets or sets the identifier of the referenced object.
    /// </summary>
    /// <remarks>
    /// Changing the identifier automatically updates <see cref="Value"/> when
    /// the reference is currently bound to a resolver provider.
    /// </remarks>
    public string? Id
    {
        get => _id;
        set
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_id == value)
                return;

            _id = value;

            OnPropertyChanged(nameof(Id));
            OnPropertyChanged(nameof(IsEmpty));

            UpdateValue();
        }
    }

    /// <summary>
    /// Gets a value indicating whether the reference does not contain an
    /// identifier.
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(_id);

    /// <summary>
    /// Gets the currently resolved object.
    /// </summary>
    /// <value>
    /// The resolved object, or <see langword="null"/> when the reference cannot
    /// currently be resolved.
    /// </value>
    public T? Value
    {
        get => _value;
        private set
        {
            if (ReferenceEquals(_value, value))
                return;

            _value = value;
            OnPropertyChanged(nameof(Value));
        }
    }

    /// <summary>
    /// Resolves the referenced object using the specified resolver provider and
    /// automatically binds the reference to that provider.
    /// </summary>
    /// <param name="provider">
    /// The resolver provider used to locate and observe the referenced object.
    /// </param>
    /// <returns>
    /// The resolved object when found; otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="provider"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The resolved object is not compatible with <typeparamref name="T"/>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// The reference has already been disposed.
    /// </exception>
    public T? Resolve(IEngineObjectResolverProvider provider)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(provider);

        Bind(provider);

        return Value;
    }

    /// <summary>
    /// Releases resolver subscriptions held by the reference.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        Unbind();

        _disposed = true;

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Returns the identifier of the referenced object.
    /// </summary>
    /// <returns>
    /// The reference identifier, or an empty string when no identifier is set.
    /// </returns>
    public override string ToString()
    {
        return _id ?? string.Empty;
    }

    object? IReference.Resolve(IEngineObjectResolverProvider provider)
    {
        return Resolve(provider);
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the property that changed.
    /// </param>
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Resolves the current identifier using the bound resolver provider and
    /// updates <see cref="Value"/>.
    /// </summary>
    protected virtual void UpdateValue()
    {
        if (_provider == null || string.IsNullOrEmpty(_id))
        {
            Value = null;
            return;
        }

        IEngineReferenceable? referenceable = _provider.Resolver.Find(_id);

        if (referenceable == null)
        {
            Value = null;
            return;
        }

        Value = referenceable as T ??
            throw new InvalidOperationException(
                $"Object '{_id}' is not of type '{typeof(T).Name}'.");
    }

    private void Bind(IEngineObjectResolverProvider provider)
    {
        if (ReferenceEquals(_provider, provider))
        {
            UpdateValue();
            return;
        }

        Unbind();

        _provider = provider;

        if (_provider.Resolver is INotifyCollectionChanged observable)
            observable.CollectionChanged += OnCollectionChanged;

        UpdateValue();
    }

    private void Unbind()
    {
        if (_provider?.Resolver is INotifyCollectionChanged observable)
            observable.CollectionChanged -= OnCollectionChanged;

        _provider = null;
        Value = null;
    }

    private void OnCollectionChanged(
        object? sender,
        NotifyCollectionChangedEventArgs e)
    {
        UpdateValue();
    }
}
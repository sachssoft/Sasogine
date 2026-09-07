using Sachssoft.Sasogine.Common;
using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Assets;

/// <summary>
/// Represents an observable reference to an engine object that automatically
/// updates its resolved value when the associated resolver changes.
/// </summary>
/// <typeparam name="T">
/// The expected type of the referenced engine object.
/// </typeparam>
/// <remarks>
/// <para>
/// The reference stores an object identifier and can be attached to an
/// <see cref="IEngineObjectResolverProvider"/> to maintain a resolved value.
/// </para>
/// <para>
/// When the resolver implements <see cref="INotifyCollectionChanged"/>,
/// collection changes automatically cause the reference to be resolved again.
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
    /// Changing the identifier automatically updates <see cref="Value"/>
    /// when a resolver provider is attached.
    /// </remarks>
    public string? Id
    {
        get => _id;
        set
        {
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
    /// Resolves the referenced object using the specified resolver provider.
    /// </summary>
    /// <param name="provider">
    /// Provider containing the resolver used to locate the referenced object.
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
    public T? ResolveTyped(IEngineObjectResolverProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        if (string.IsNullOrEmpty(_id))
            return null;

        IEngineReferenceable? referenceable = provider.Resolver.Find(_id);

        if (referenceable == null)
            return null;

        return referenceable as T ??
            throw new InvalidOperationException(
                $"Object '{_id}' is not of type '{typeof(T).Name}'.");
    }

    /// <summary>
    /// Attaches the reference to the specified resolver provider.
    /// </summary>
    /// <param name="provider">
    /// The resolver provider used to resolve and observe the referenced object.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="provider"/> is <see langword="null"/>.
    /// </exception>
    public void Attach(IEngineObjectResolverProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        if (ReferenceEquals(_provider, provider))
            return;

        Detach();

        _provider = provider;

        if (_provider.Resolver is INotifyCollectionChanged observable)
            observable.CollectionChanged += OnCollectionChanged;

        UpdateValue();
    }

    /// <summary>
    /// Detaches the reference from its current resolver provider.
    /// </summary>
    public void Detach()
    {
        if (_provider?.Resolver is INotifyCollectionChanged observable)
            observable.CollectionChanged -= OnCollectionChanged;

        _provider = null;
        Value = null;
    }

    /// <summary>
    /// Releases subscriptions held by the reference.
    /// </summary>
    public void Dispose()
    {
        Detach();
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
        return ResolveTyped(provider);
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the property that changed.
    /// </param>
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Resolves the reference using the attached provider and updates
    /// <see cref="Value"/>.
    /// </summary>
    protected virtual void UpdateValue()
    {
        Value = _provider != null
            ? ResolveTyped(_provider)
            : null;
    }

    private void OnCollectionChanged(
        object? sender,
        NotifyCollectionChangedEventArgs e)
    {
        UpdateValue();
    }
}
using Sachssoft.Sasogine.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Assets;

/// <summary>
/// Represents an observable collection of unique assets that can resolve
/// engine objects by identifier or class.
/// </summary>
/// <remarks>
/// Asset instances and non-null identifiers must be unique within the
/// collection. Collection changes are reported through
/// <see cref="INotifyCollectionChanged"/> and
/// <see cref="INotifyPropertyChanged"/>.
/// </remarks>
public class AssetCollection :
    IList<IAsset>,
    IReadOnlyList<IAsset>,
    IEngineObjectResolver,
    INotifyCollectionChanged,
    INotifyPropertyChanged
{
    private readonly List<IAsset> _items;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssetCollection"/> class.
    /// </summary>
    public AssetCollection()
    {
        _items = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AssetCollection"/> class
    /// with the specified assets.
    /// </summary>
    /// <param name="assets">
    /// The assets to initially add to the collection.
    /// </param>
    public AssetCollection(IEnumerable<IAsset> assets)
    {
        ArgumentNullException.ThrowIfNull(assets);

        _items = [];

        foreach (IAsset asset in assets)
        {
            ArgumentNullException.ThrowIfNull(asset);

            EnsureUnique(asset);
            _items.Add(asset);
        }
    }

    /// <summary>
    /// Occurs when the collection changes.
    /// </summary>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets the number of assets contained in the collection.
    /// </summary>
    public int Count => _items.Count;

    /// <summary>
    /// Gets a value indicating whether the collection is read-only.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// Gets or sets the asset at the specified index.
    /// </summary>
    public IAsset this[int index]
    {
        get => _items[index];

        set
        {
            ArgumentNullException.ThrowIfNull(value);

            IAsset previous = _items[index];

            if (ReferenceEquals(previous, value))
                return;

            EnsureUnique(value, previous);

            _items[index] = value;

            OnPropertyChanged("Item[]");

            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Replace,
                    value,
                    previous,
                    index));
        }
    }

    /// <summary>
    /// Adds an asset to the collection.
    /// </summary>
    /// <param name="item">
    /// The asset to add.
    /// </param>
    public virtual void Add(IAsset item)
    {
        ArgumentNullException.ThrowIfNull(item);

        EnsureUnique(item);

        int index = _items.Count;

        _items.Add(item);

        OnPropertyChanged(nameof(Count));
        OnPropertyChanged("Item[]");

        OnCollectionChanged(
            new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add,
                item,
                index));
    }

    /// <summary>
    /// Adds the specified assets to the collection.
    /// </summary>
    /// <param name="assets">
    /// The assets to add.
    /// </param>
    public virtual void AddRange(IEnumerable<IAsset> assets)
    {
        ArgumentNullException.ThrowIfNull(assets);

        foreach (IAsset asset in assets)
            Add(asset);
    }

    /// <summary>
    /// Removes all assets from the collection.
    /// </summary>
    public virtual void Clear()
    {
        if (_items.Count == 0)
            return;

        _items.Clear();

        OnPropertyChanged(nameof(Count));
        OnPropertyChanged("Item[]");

        OnCollectionChanged(
            new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    /// Determines whether the collection contains the specified asset instance.
    /// </summary>
    public virtual bool Contains(IAsset item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return IndexOfReference(item) >= 0;
    }

    /// <summary>
    /// Determines whether an asset with the specified identifier exists.
    /// </summary>
    public virtual bool Contains(string? id)
    {
        return Find(id) is not null;
    }

    /// <summary>
    /// Determines whether an asset with the specified identifier and type exists.
    /// </summary>
    public virtual bool Contains(
        Type assetType,
        string? id)
    {
        return Find(assetType, id) is not null;
    }

    /// <summary>
    /// Determines whether an asset with the specified identifier and type exists.
    /// </summary>
    public virtual bool Contains<TAsset>(string? id)
        where TAsset : class, IAsset
    {
        return Find<TAsset>(id) is not null;
    }

    /// <summary>
    /// Copies the assets to the specified array.
    /// </summary>
    public virtual void CopyTo(
        IAsset[] array,
        int arrayIndex)
    {
        _items.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Returns the index of the specified asset instance.
    /// </summary>
    public virtual int IndexOf(IAsset item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return IndexOfReference(item);
    }

    /// <summary>
    /// Inserts an asset at the specified index.
    /// </summary>
    public virtual void Insert(
        int index,
        IAsset item)
    {
        ArgumentNullException.ThrowIfNull(item);

        EnsureUnique(item);

        _items.Insert(index, item);

        OnPropertyChanged(nameof(Count));
        OnPropertyChanged("Item[]");

        OnCollectionChanged(
            new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add,
                item,
                index));
    }

    /// <summary>
    /// Removes the specified asset instance from the collection.
    /// </summary>
    public virtual bool Remove(IAsset item)
    {
        ArgumentNullException.ThrowIfNull(item);

        int index = IndexOfReference(item);

        if (index < 0)
            return false;

        RemoveAt(index);

        return true;
    }

    /// <summary>
    /// Removes the asset with the specified identifier.
    /// </summary>
    public virtual bool Remove(string? id)
    {
        IAsset? asset = Find(id);

        return asset is not null && Remove(asset);
    }

    /// <summary>
    /// Removes the asset at the specified index.
    /// </summary>
    public virtual void RemoveAt(int index)
    {
        IAsset item = _items[index];

        _items.RemoveAt(index);

        OnPropertyChanged(nameof(Count));
        OnPropertyChanged("Item[]");

        OnCollectionChanged(
            new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Remove,
                item,
                index));
    }

    /// <summary>
    /// Finds an asset with the specified identifier.
    /// </summary>
    public virtual IAsset? Find(string? id)
    {
        if (id is null)
            return null;

        foreach (IAsset asset in _items)
        {
            if (string.Equals(
                asset.Id,
                id,
                StringComparison.Ordinal))
            {
                return asset;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds an asset with the specified identifier and type.
    /// </summary>
    public virtual IAsset? Find(
        Type assetType,
        string? id)
    {
        ValidateAssetType(assetType);

        if (id is null)
            return null;

        foreach (IAsset asset in _items)
        {
            if (string.Equals(
                    asset.Id,
                    id,
                    StringComparison.Ordinal) &&
                assetType.IsInstanceOfType(asset))
            {
                return asset;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds an asset of the specified type with the specified identifier.
    /// </summary>
    public virtual TAsset? Find<TAsset>(string? id)
        where TAsset : class, IAsset
    {
        if (id is null)
            return null;

        foreach (IAsset asset in _items)
        {
            if (string.Equals(
                    asset.Id,
                    id,
                    StringComparison.Ordinal) &&
                asset is TAsset typedAsset)
            {
                return typedAsset;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds all assets with the specified class.
    /// </summary>
    public virtual IEnumerable<IAsset> FindAll(string? @class)
    {
        foreach (IAsset asset in _items)
        {
            if (string.Equals(
                asset.Class,
                @class,
                StringComparison.Ordinal))
            {
                yield return asset;
            }
        }
    }

    /// <summary>
    /// Finds all assets of the specified type with the specified class.
    /// </summary>
    public virtual IEnumerable<TAsset> FindAll<TAsset>(
        string? @class)
        where TAsset : class, IAsset
    {
        foreach (IAsset asset in _items)
        {
            if (string.Equals(
                    asset.Class,
                    @class,
                    StringComparison.Ordinal) &&
                asset is TAsset typedAsset)
            {
                yield return typedAsset;
            }
        }
    }

    /// <summary>
    /// Gets all assets assignable to the specified type.
    /// </summary>
    public virtual IEnumerable<IAsset> GetAll(Type assetType)
    {
        ValidateAssetType(assetType);

        foreach (IAsset asset in _items)
        {
            if (assetType.IsInstanceOfType(asset))
                yield return asset;
        }
    }

    /// <summary>
    /// Gets all assets assignable to the specified type.
    /// </summary>
    public virtual IEnumerable<TAsset> GetAll<TAsset>()
        where TAsset : class, IAsset
    {
        foreach (IAsset asset in _items)
        {
            if (asset is TAsset typedAsset)
                yield return typedAsset;
        }
    }

    /// <summary>
    /// Attempts to find an asset with the specified identifier.
    /// </summary>
    public virtual bool TryGet(
        string? id,
        [NotNullWhen(true)] out IAsset? result)
    {
        result = Find(id);

        return result is not null;
    }

    /// <summary>
    /// Attempts to find an asset with the specified identifier and type.
    /// </summary>
    public virtual bool TryGet(
        Type assetType,
        string? id,
        [NotNullWhen(true)] out IAsset? result)
    {
        result = Find(assetType, id);

        return result is not null;
    }

    /// <summary>
    /// Attempts to find an asset of the specified type with the specified identifier.
    /// </summary>
    public virtual bool TryGet<TAsset>(
        string? id,
        [NotNullWhen(true)] out TAsset? result)
        where TAsset : class, IAsset
    {
        result = Find<TAsset>(id);

        return result is not null;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the assets.
    /// </summary>
    public virtual IEnumerator<IAsset> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    /// <summary>
    /// Raises the <see cref="CollectionChanged"/> event.
    /// </summary>
    protected virtual void OnCollectionChanged(
        NotifyCollectionChangedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        CollectionChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    protected virtual void OnPropertyChanged(string propertyName)
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);

        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Ensures that the specified asset can be inserted into the collection.
    /// </summary>
    protected virtual void EnsureUnique(
        IAsset item,
        IAsset? excludedItem = null)
    {
        ArgumentNullException.ThrowIfNull(item);

        foreach (IAsset existing in _items)
        {
            if (ReferenceEquals(existing, excludedItem))
                continue;

            if (ReferenceEquals(existing, item))
            {
                throw new ArgumentException(
                    "The asset is already contained in the collection.",
                    nameof(item));
            }

            if (item.Id is not null &&
                string.Equals(
                    existing.Id,
                    item.Id,
                    StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    $"An asset with the identifier '{item.Id}' already exists.",
                    nameof(item));
            }
        }
    }

    /// <summary>
    /// Returns the index of the specified asset instance.
    /// </summary>
    protected virtual int IndexOfReference(IAsset item)
    {
        ArgumentNullException.ThrowIfNull(item);

        for (int i = 0; i < _items.Count; i++)
        {
            if (ReferenceEquals(_items[i], item))
                return i;
        }

        return -1;
    }

    /// <summary>
    /// Validates that the specified type represents an asset type.
    /// </summary>
    protected virtual void ValidateAssetType(Type assetType)
    {
        ArgumentNullException.ThrowIfNull(assetType);

        if (!typeof(IAsset).IsAssignableFrom(assetType))
        {
            throw new ArgumentException(
                $"Type '{assetType.FullName}' does not implement " +
                $"'{typeof(IAsset).FullName}'.",
                nameof(assetType));
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEngineReferenceable? IEngineObjectResolver.Find(string? id)
    {
        return Find(id);
    }

    IEnumerable<IEngineReferenceable> IEngineObjectResolver.FindAll(
        string? @class)
    {
        return FindAll(@class);
    }

    bool IEngineObjectResolver.TryGet(
        string? id,
        out IEngineReferenceable? result)
    {
        IAsset? asset = Find(id);

        result = asset;

        return asset is not null;
    }
}
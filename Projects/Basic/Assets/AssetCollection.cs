using Sachssoft.Sasogine.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Assets;

/// <summary>
/// Represents an observable collection of unique assets that can resolve
/// engine objects by identifier or class.
/// </summary>
/// <remarks>
/// Asset instances and non-null identifiers must be unique within the
/// collection. Collection changes are reported through
/// <see cref="INotifyCollectionChanged"/> and <see cref="INotifyPropertyChanged"/>.
/// </remarks>
public class AssetCollection :
    IList<IAsset>,
    IEngineObjectResolver,
    INotifyCollectionChanged,
    INotifyPropertyChanged
{
    private readonly List<IAsset> _items = [];

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
    public void Add(IAsset item)
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
    /// Removes all assets from the collection.
    /// </summary>
    public void Clear()
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
    /// <param name="item">
    /// The asset to locate.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the asset is contained in the collection;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(IAsset item)
    {
        return IndexOfReference(item) >= 0;
    }

    /// <summary>
    /// Copies the assets to the specified array.
    /// </summary>
    /// <param name="array">
    /// The destination array.
    /// </param>
    /// <param name="arrayIndex">
    /// The zero-based index in the destination array at which copying begins.
    /// </param>
    public void CopyTo(IAsset[] array, int arrayIndex)
    {
        _items.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Returns the index of the specified asset instance.
    /// </summary>
    /// <param name="item">
    /// The asset to locate.
    /// </param>
    /// <returns>
    /// The zero-based index of the asset if found; otherwise, <c>-1</c>.
    /// </returns>
    public int IndexOf(IAsset item)
    {
        return IndexOfReference(item);
    }

    /// <summary>
    /// Inserts an asset at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index at which the asset should be inserted.
    /// </param>
    /// <param name="item">
    /// The asset to insert.
    /// </param>
    public void Insert(int index, IAsset item)
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
    /// <param name="item">
    /// The asset to remove.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the asset was removed; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool Remove(IAsset item)
    {
        int index = IndexOfReference(item);

        if (index < 0)
            return false;

        RemoveAt(index);
        return true;
    }

    /// <summary>
    /// Removes the asset at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index of the asset to remove.
    /// </param>
    public void RemoveAt(int index)
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
    /// Returns an enumerator that iterates through the assets.
    /// </summary>
    public IEnumerator<IAsset> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Finds an asset with the specified identifier.
    /// </summary>
    /// <param name="id">
    /// The identifier of the asset to find.
    /// </param>
    /// <returns>
    /// The matching asset, or <see langword="null"/> if no asset was found.
    /// </returns>
    public IAsset? Find(string? id)
    {
        if (id == null)
            return null;

        foreach (IAsset asset in _items)
        {
            if (asset.Id == id)
                return asset;
        }

        return null;
    }

    /// <summary>
    /// Finds an asset of the specified type with the specified identifier.
    /// </summary>
    /// <typeparam name="T">
    /// The asset type to find.
    /// </typeparam>
    /// <param name="id">
    /// The identifier of the asset to find.
    /// </param>
    /// <returns>
    /// The matching asset, or <see langword="null"/> if no asset was found.
    /// </returns>
    public T? Find<T>(string? id)
        where T : class, IAsset
    {
        if (id == null)
            return null;

        foreach (IAsset asset in _items)
        {
            if (asset.Id == id && asset is T typedAsset)
                return typedAsset;
        }

        return null;
    }

    /// <summary>
    /// Finds all assets with the specified class.
    /// </summary>
    /// <param name="class">
    /// The class of the assets to find.
    /// </param>
    /// <returns>
    /// All matching assets.
    /// </returns>
    public IEnumerable<IAsset> FindAll(string? @class)
    {
        foreach (IAsset asset in _items)
        {
            if (asset.Class == @class)
                yield return asset;
        }
    }

    /// <summary>
    /// Finds all assets of the specified type with the specified class.
    /// </summary>
    /// <typeparam name="T">
    /// The asset type to find.
    /// </typeparam>
    /// <param name="class">
    /// The class of the assets to find.
    /// </param>
    /// <returns>
    /// All matching assets.
    /// </returns>
    public IEnumerable<T> FindAll<T>(string? @class)
        where T : class, IAsset
    {
        foreach (IAsset asset in _items)
        {
            if (asset.Class == @class && asset is T typedAsset)
                yield return typedAsset;
        }
    }

    /// <summary>
    /// Attempts to find an asset with the specified identifier.
    /// </summary>
    /// <param name="id">
    /// The identifier of the asset to find.
    /// </param>
    /// <param name="result">
    /// When this method returns, contains the matching asset if found;
    /// otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if an asset was found; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool TryGet(string? id, out IAsset? result)
    {
        result = Find(id);
        return result != null;
    }

    /// <summary>
    /// Attempts to find an asset of the specified type with the specified identifier.
    /// </summary>
    /// <typeparam name="T">
    /// The asset type to find.
    /// </typeparam>
    /// <param name="id">
    /// The identifier of the asset to find.
    /// </param>
    /// <param name="result">
    /// When this method returns, contains the matching asset if found;
    /// otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if an asset was found; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool TryGet<T>(string? id, out T? result)
        where T : class, IAsset
    {
        result = Find<T>(id);
        return result != null;
    }

    /// <summary>
    /// Raises the <see cref="CollectionChanged"/> event.
    /// </summary>
    /// <param name="e">
    /// The event data describing the collection change.
    /// </param>
    protected virtual void OnCollectionChanged(
        NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
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

        return asset != null;
    }

    private void EnsureUnique(
        IAsset item,
        IAsset? excludedItem = null)
    {
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

            if (item.Id != null && existing.Id == item.Id)
            {
                throw new ArgumentException(
                    $"An asset with the identifier '{item.Id}' already exists.",
                    nameof(item));
            }
        }
    }

    private int IndexOfReference(IAsset item)
    {
        for (int i = 0; i < _items.Count; i++)
        {
            if (ReferenceEquals(_items[i], item))
                return i;
        }

        return -1;
    }
}
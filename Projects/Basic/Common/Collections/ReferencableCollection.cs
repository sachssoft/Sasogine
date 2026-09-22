using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Engine.Common.Collections
{
    /// <summary>
    /// Represents an ordered, trackable collection of engine-referenceable objects
    /// with fast lookup by identifier.
    /// </summary>
    /// <typeparam name="T">
    /// The type of engine-referenceable object contained in the collection.
    /// </typeparam>
    /// <remarks>
    /// Objects with a non-null identifier are indexed for fast lookup.
    /// Identifiers are compared using ordinal, case-sensitive comparison and must
    /// be unique within the collection.
    /// </remarks>
    public class ReferencableCollection<T> :
        TrackableCollection<T>,
        IEngineObjectResolver
        where T : class, IEngineReferenceable
    {
        private readonly Dictionary<string, T> _references = new(StringComparer.Ordinal);
        private readonly Dictionary<T, string?> _knownIds = new(ReferenceEqualityComparer.Instance);

        /// <summary>
        /// Initializes a new, empty instance of the
        /// <see cref="ReferencableCollection{T}"/> class.
        /// </summary>
        public ReferencableCollection()
        {
        }

        /// <summary>
        /// Occurs when the reference resolution state of an item changes.
        /// </summary>
        public event EventHandler<ReferenceChangedEventArgs<T>>? ReferenceChanged;

        /// <summary>
        /// Initializes a new, empty instance of the
        /// <see cref="ReferencableCollection{T}"/> class with the specified initial capacity.
        /// </summary>
        /// <param name="capacity">
        /// The initial number of elements that the collection can contain without resizing.
        /// </param>
        public ReferencableCollection(int capacity)
            : base(capacity)
        {
            _references.EnsureCapacity(capacity);
            _knownIds.EnsureCapacity(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferencableCollection{T}"/>
        /// class containing the elements from the specified collection.
        /// </summary>
        /// <param name="collection">
        /// The collection whose elements are added to the new collection.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collection"/> is <see langword="null"/>.
        /// </exception>
        public ReferencableCollection(IEnumerable<T> collection)
        {
            ArgumentNullException.ThrowIfNull(collection);

            foreach (T item in collection)
                Add(item);
        }

        /// <summary>
        /// Finds an object with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier of the object to find.</param>
        /// <returns>
        /// The matching object, or <see langword="null"/> if no matching object exists
        /// or <paramref name="id"/> is null, empty, or consists only of white-space characters.
        /// </returns>
        public T? Find(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            return _references.GetValueOrDefault(id);
        }

        /// <summary>
        /// Attempts to get an object with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier of the object to find.</param>
        /// <param name="result">
        /// When this method returns, contains the matching object if found;
        /// otherwise, <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if an object with the specified identifier was found;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool TryGet(string? id, [MaybeNullWhen(false)] out T result)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                result = null;
                return false;
            }

            return _references.TryGetValue(id, out result);
        }

        /// <inheritdoc/>
        IEngineReferenceable? IEngineObjectResolver.Find(string? id)
        {
            return Find(id) as IEngineReferenceable;
        }

        /// <inheritdoc/>
        bool IEngineObjectResolver.TryGet(
            string? id,
            [MaybeNullWhen(false)] out IEngineReferenceable? result)
        {
            result = Find(id) as IEngineReferenceable;
            return result is not null;
        }

        /// <inheritdoc/>
        IEnumerable<IEngineReferenceable> IEngineObjectResolver.FindAll(string? @class)
        {
            foreach (T item in this)
            {
                if (item is IEngineClass engineObject &&
                    string.Equals(@class, engineObject.Class, StringComparison.Ordinal))
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Validates an item before it is inserted into the collection.
        /// </summary>
        /// <param name="index">The index at which the item will be inserted.</param>
        /// <param name="item">The item to insert.</param>
        /// <returns>
        /// <see langword="true"/> if the insertion may continue;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        protected override bool OnInserting(int index, T item)
        {
            ArgumentNullException.ThrowIfNull(item);
            ValidateId(item.Id, item);
            return base.OnInserting(index, item);
        }

        /// <summary>
        /// Updates the reference index and subscribes to identifier changes
        /// after an item has been inserted.
        /// </summary>
        /// <param name="index">The index at which the item was inserted.</param>
        /// <param name="item">The inserted item.</param>
        protected override void OnInserted(int index, T item)
        {
            AddReference(item);
            Subscribe(item);
            base.OnInserted(index, item);
            OnReferenceChanged(item);
        }

        /// <summary>
        /// Validates a replacement item before an existing item is replaced.
        /// </summary>
        /// <param name="index">The index of the item being replaced.</param>
        /// <param name="oldItem">The existing item.</param>
        /// <param name="newItem">The replacement item.</param>
        /// <returns>
        /// <see langword="true"/> if the replacement may continue;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        protected override bool OnSetting(int index, T oldItem, T newItem)
        {
            ArgumentNullException.ThrowIfNull(newItem);
            ValidateId(newItem.Id, oldItem);
            return base.OnSetting(index, oldItem, newItem);
        }

        /// <summary>
        /// Updates the reference index and subscriptions after an item has been replaced.
        /// </summary>
        /// <param name="index">The index of the replaced item.</param>
        /// <param name="oldItem">The previous item.</param>
        /// <param name="newItem">The replacement item.</param>
        protected override void OnSet(int index, T oldItem, T newItem)
        {
            Unsubscribe(oldItem);
            RemoveReference(oldItem);

            AddReference(newItem);
            Subscribe(newItem);

            base.OnSet(index, oldItem, newItem);

            OnReferenceChanged(oldItem);
            OnReferenceChanged(newItem);
        }

        /// <summary>
        /// Removes the reference and subscription after an item has been removed.
        /// </summary>
        /// <param name="index">The previous index of the removed item.</param>
        /// <param name="item">The removed item.</param>
        protected override void OnRemoved(int index, T item)
        {
            Unsubscribe(item);
            RemoveReference(item);
            base.OnRemoved(index, item);
            OnReferenceChanged(item);
        }

        /// <summary>
        /// Prepares the collection for clearing by removing identifier-change subscriptions.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the collection may be cleared;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        protected override bool OnClearing()
        {
            if (!base.OnClearing())
                return false;

            foreach (T item in this)
                Unsubscribe(item);

            return true;
        }

        /// <summary>
        /// Clears the internal reference indexes after the collection has been cleared.
        /// </summary>
        protected override void OnCleared()
        {
            _references.Clear();
            _knownIds.Clear();
            base.OnCleared();
        }

        /// <summary>
        /// Raises the <see cref="ReferenceChanged"/> event for the specified item.
        /// </summary>
        /// <param name="item">The item affected by the reference change.</param>
        protected virtual void OnReferenceChanged(T item)
        {
            ReferenceChanged?.Invoke(
                this,
                new ReferenceChangedEventArgs<T>(item));
        }

        private void Subscribe(T item)
        {
            _knownIds[item] = item.Id;

            if (item is IEngineReferenceableChanged changed)
                changed.IdChanged += ItemIdChanged;
        }

        private void Unsubscribe(T item)
        {
            if (item is IEngineReferenceableChanged changed)
                changed.IdChanged -= ItemIdChanged;

            _knownIds.Remove(item);
        }

        private void ItemIdChanged(object? sender, EngineObjectChangedEventArgs e)
        {
            if (sender is not T item ||
                !_knownIds.TryGetValue(item, out string? oldId))
            {
                return;
            }

            string? newId = item.Id;

            if (string.Equals(oldId, newId, StringComparison.Ordinal))
                return;

            ValidateId(newId, item);

            if (oldId is not null &&
                _references.TryGetValue(oldId, out T? oldItem) &&
                ReferenceEquals(oldItem, item))
            {
                _references.Remove(oldId);
            }

            if (newId is not null)
                _references.Add(newId, item);

            _knownIds[item] = newId;

            OnReferenceChanged(item);
        }

        private void ValidateId(string? id, T replacingItem)
        {
            if (id is null)
                return;

            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(
                    "Reference identifier cannot be empty or whitespace.",
                    nameof(id));

            if (_references.TryGetValue(id, out T? existing) &&
                !ReferenceEquals(existing, replacingItem))
            {
                throw new ArgumentException(
                    $"An object with the identifier '{id}' already exists in the collection.",
                    nameof(id));
            }
        }

        private void AddReference(T item)
        {
            string? id = item.Id;

            if (id is not null)
                _references.Add(id, item);

            _knownIds[item] = id;
        }

        private void RemoveReference(T item)
        {
            if (!_knownIds.TryGetValue(item, out string? id))
                id = item.Id;

            if (id is not null &&
                _references.TryGetValue(id, out T? existing) &&
                ReferenceEquals(existing, item))
            {
                _references.Remove(id);
            }

            _knownIds.Remove(item);
        }
    }
}
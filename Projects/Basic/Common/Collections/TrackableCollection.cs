using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Sachssoft.Engine.Common.Collections
{
    /// <summary>
    /// Represents an ordered collection that tracks changes and provides
    /// collection and property change notifications.
    /// </summary>
    /// <typeparam name="T">
    /// The type of element contained in the collection.
    /// </typeparam>
    /// <remarks>
    /// Changes are accumulated internally and can be retrieved using
    /// <see cref="ConsumeChanges"/>.
    /// Derived collections can intercept and cancel mutations by overriding
    /// the corresponding methods that are called before each operation.
    /// </remarks>
    public class TrackableCollection<T> :
        IList<T>,
        IReadOnlyList<T>,
        INotifyCollectionChanged,
        INotifyPropertyChanged,
        ITrackable<CollectionChangeSet<T>>
    {
        private static readonly PropertyChangedEventArgs CountChangedArgs = new(nameof(Count));
        private static readonly PropertyChangedEventArgs IndexerChangedArgs = new("Item[]");

        private readonly List<T> _items;
        private List<CollectionChange<T>> _writeChanges;
        private List<CollectionChange<T>> _readChanges;
        private int _version;

        /// <summary>
        /// Initializes a new, empty instance of the
        /// <see cref="TrackableCollection{T}"/> class.
        /// </summary>
        public TrackableCollection()
        {
            _items = [];
            _writeChanges = [];
            _readChanges = [];
        }

        /// <summary>
        /// Initializes a new, empty instance of the
        /// <see cref="TrackableCollection{T}"/> class with the specified initial capacity.
        /// </summary>
        /// <param name="capacity">
        /// The initial number of elements that the collection can contain without resizing.
        /// </param>
        public TrackableCollection(int capacity)
        {
            _items = new List<T>(capacity);
            _writeChanges = new List<CollectionChange<T>>(capacity);
            _readChanges = new List<CollectionChange<T>>(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackableCollection{T}"/>
        /// class containing the elements from the specified collection.
        /// </summary>
        /// <param name="collection">
        /// The collection whose elements are copied to the new collection.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collection"/> is <see langword="null"/>.
        /// </exception>
        public TrackableCollection(IEnumerable<T> collection)
        {
            ArgumentNullException.ThrowIfNull(collection);

            _items = new List<T>(collection);
            _writeChanges = [];
            _readChanges = [];
        }

        /// <summary>
        /// Occurs when the contents of the collection change.
        /// </summary>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        /// <summary>
        /// Occurs when a property value of the collection changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        public int Count => _items.Count;

        /// <summary>
        /// Gets a value indicating whether the collection is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets a value indicating whether unconsumed changes are available.
        /// </summary>
        public bool HasChanges => _writeChanges.Count != 0;

        /// <summary>
        /// Gets the current change version of the collection.
        /// </summary>
        /// <remarks>
        /// The version is incremented whenever a collection mutation is successfully
        /// completed and recorded.
        /// </remarks>
        public int Version => _version;

        /// <summary>
        /// Gets or sets the total number of elements the internal storage can hold
        /// without resizing.
        /// </summary>
        public int Capacity
        {
            get => _items.Capacity;
            set => _items.Capacity = value;
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element.</param>
        /// <returns>The element at the specified index.</returns>
        public T this[int index]
        {
            get => _items[index];
            set => SetItem(index, value);
        }

        /// <summary>
        /// Adds an element to the end of the collection.
        /// </summary>
        /// <param name="item">The element to add.</param>
        public void Add(T item) => InsertItem(_items.Count, item);

        /// <summary>
        /// Inserts an element into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the element is inserted.</param>
        /// <param name="item">The element to insert.</param>
        public void Insert(int index, T item) => InsertItem(index, item);

        /// <summary>
        /// Removes the first occurrence of the specified element.
        /// </summary>
        /// <param name="item">The element to remove.</param>
        /// <returns>
        /// <see langword="true"/> if the element was found and removed;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// The operation can also return <see langword="false"/> when removal is
        /// cancelled by <see cref="OnRemoving"/>.
        /// </remarks>
        public bool Remove(T item)
        {
            int index = _items.IndexOf(item);

            if (index < 0)
                return false;

            return RemoveItem(index);
        }

        /// <summary>
        /// Removes the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        public void RemoveAt(int index)
        {
            if (!RemoveItem(index))
                return;
        }

        /// <summary>
        /// Removes all elements from the collection.
        /// </summary>
        public void Clear() => ClearItems();

        /// <summary>
        /// Moves the element at the specified index to a new index.
        /// </summary>
        /// <param name="oldIndex">The current zero-based index of the element.</param>
        /// <param name="newIndex">The new zero-based index of the element.</param>
        public void Move(int oldIndex, int newIndex) => MoveItem(oldIndex, newIndex);

        /// <summary>
        /// Determines whether the collection contains the specified element.
        /// </summary>
        /// <param name="item">The element to locate.</param>
        /// <returns>
        /// <see langword="true"/> if the element is contained in the collection;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool Contains(T item) => _items.Contains(item);

        /// <summary>
        /// Determines the index of the specified element.
        /// </summary>
        /// <param name="item">The element to locate.</param>
        /// <returns>
        /// The zero-based index of the element if found; otherwise, -1.
        /// </returns>
        public int IndexOf(T item) => _items.IndexOf(item);

        /// <summary>
        /// Copies the elements of the collection to an array, starting at the
        /// specified destination index.
        /// </summary>
        /// <param name="array">The destination array.</param>
        /// <param name="arrayIndex">
        /// The zero-based index in <paramref name="array"/> at which copying begins.
        /// </param>
        public void CopyTo(T[] array, int arrayIndex) =>
            _items.CopyTo(array, arrayIndex);

        /// <summary>
        /// Returns the changes recorded since the previous consume operation.
        /// </summary>
        /// <returns>
        /// A change set containing the pending collection changes, or an empty
        /// change set when no changes are pending.
        /// </returns>
        /// <remarks>
        /// Consuming changes does not modify the contents of the collection.
        /// The returned change set is backed by an internally reused buffer and
        /// remains valid only until the next call to <see cref="ConsumeChanges"/>.
        /// </remarks>
        public CollectionChangeSet<T> ConsumeChanges()
        {
            if (_writeChanges.Count == 0)
                return default;

            _readChanges.Clear();

            List<CollectionChange<T>> oldRead = _readChanges;
            _readChanges = _writeChanges;
            _writeChanges = oldRead;

            return new CollectionChangeSet<T>(_readChanges);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator for the collection.</returns>
        public List<T>.Enumerator GetEnumerator() => _items.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _items.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

        /// <summary>
        /// Called before an element is inserted into the collection.
        /// </summary>
        /// <param name="index">The index at which the element will be inserted.</param>
        /// <param name="item">The element to insert.</param>
        /// <returns>
        /// <see langword="true"/> to continue the insertion;
        /// otherwise, <see langword="false"/> to cancel it.
        /// </returns>
        protected virtual bool OnInserting(int index, T item) => true;

        /// <summary>
        /// Called after an element has been successfully inserted.
        /// </summary>
        /// <param name="index">The index at which the element was inserted.</param>
        /// <param name="item">The inserted element.</param>
        protected virtual void OnInserted(int index, T item)
        {
        }

        /// <summary>
        /// Called before an element is replaced.
        /// </summary>
        /// <param name="index">The index of the element being replaced.</param>
        /// <param name="oldItem">The existing element.</param>
        /// <param name="newItem">The replacement element.</param>
        /// <returns>
        /// <see langword="true"/> to continue the replacement;
        /// otherwise, <see langword="false"/> to cancel it.
        /// </returns>
        protected virtual bool OnSetting(int index, T oldItem, T newItem) => true;

        /// <summary>
        /// Called after an element has been successfully replaced.
        /// </summary>
        /// <param name="index">The index of the replaced element.</param>
        /// <param name="oldItem">The previous element.</param>
        /// <param name="newItem">The replacement element.</param>
        protected virtual void OnSet(int index, T oldItem, T newItem)
        {
        }

        /// <summary>
        /// Called before an element is removed from the collection.
        /// </summary>
        /// <param name="index">The index of the element to remove.</param>
        /// <param name="item">The element to remove.</param>
        /// <returns>
        /// <see langword="true"/> to continue the removal;
        /// otherwise, <see langword="false"/> to cancel it.
        /// </returns>
        protected virtual bool OnRemoving(int index, T item) => true;

        /// <summary>
        /// Called after an element has been successfully removed.
        /// </summary>
        /// <param name="index">The previous index of the removed element.</param>
        /// <param name="item">The removed element.</param>
        protected virtual void OnRemoved(int index, T item)
        {
        }

        /// <summary>
        /// Called before all elements are removed from the collection.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> to continue clearing the collection;
        /// otherwise, <see langword="false"/> to cancel it.
        /// </returns>
        protected virtual bool OnClearing() => true;

        /// <summary>
        /// Called after the collection has been successfully cleared.
        /// </summary>
        protected virtual void OnCleared()
        {
        }

        /// <summary>
        /// Called before an element is moved within the collection.
        /// </summary>
        /// <param name="oldIndex">The current index of the element.</param>
        /// <param name="newIndex">The destination index of the element.</param>
        /// <param name="item">The element being moved.</param>
        /// <returns>
        /// <see langword="true"/> to continue the move;
        /// otherwise, <see langword="false"/> to cancel it.
        /// </returns>
        protected virtual bool OnMoving(int oldIndex, int newIndex, T item) => true;

        /// <summary>
        /// Called after an element has been successfully moved.
        /// </summary>
        /// <param name="oldIndex">The previous index of the element.</param>
        /// <param name="newIndex">The new index of the element.</param>
        /// <param name="item">The moved element.</param>
        protected virtual void OnMoved(int oldIndex, int newIndex, T item)
        {
        }

        /// <summary>
        /// Raises the <see cref="CollectionChanged"/> event.
        /// </summary>
        /// <param name="e">The event data describing the collection change.</param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e) =>
            CollectionChanged?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="e">The event data describing the property change.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e) =>
            PropertyChanged?.Invoke(this, e);

        private void InsertItem(int index, T item)
        {
            if (!OnInserting(index, item))
                return;

            _items.Insert(index, item);
            Record(CollectionChange<T>.Add(item, index));

            OnInserted(index, item);
            RaiseCountAndIndexerChanged();

            if (CollectionChanged is not null)
            {
                OnCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add,
                        item,
                        index));
            }
        }

        private void SetItem(int index, T item)
        {
            T oldItem = _items[index];

            if (EqualityComparer<T>.Default.Equals(oldItem, item))
                return;

            if (!OnSetting(index, oldItem, item))
                return;

            _items[index] = item;
            Record(CollectionChange<T>.Replace(item, oldItem, index));

            OnSet(index, oldItem, item);
            RaiseIndexerChanged();

            if (CollectionChanged is not null)
            {
                OnCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Replace,
                        item,
                        oldItem,
                        index));
            }
        }

        private bool RemoveItem(int index)
        {
            T item = _items[index];

            if (!OnRemoving(index, item))
                return false;

            _items.RemoveAt(index);
            Record(CollectionChange<T>.Remove(item, index));

            OnRemoved(index, item);
            RaiseCountAndIndexerChanged();

            if (CollectionChanged is not null)
            {
                OnCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove,
                        item,
                        index));
            }

            return true;
        }

        private void ClearItems()
        {
            if (_items.Count == 0 || !OnClearing())
                return;

            _items.Clear();
            Record(CollectionChange<T>.Reset());

            OnCleared();
            RaiseCountAndIndexerChanged();

            if (CollectionChanged is not null)
            {
                OnCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Reset));
            }
        }

        private void MoveItem(int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex)
                return;

            T item = _items[oldIndex];

            if (!OnMoving(oldIndex, newIndex, item))
                return;

            _items.RemoveAt(oldIndex);
            _items.Insert(newIndex, item);
            Record(CollectionChange<T>.Move(item, newIndex, oldIndex));

            OnMoved(oldIndex, newIndex, item);
            RaiseIndexerChanged();

            if (CollectionChanged is not null)
            {
                OnCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Move,
                        item,
                        newIndex,
                        oldIndex));
            }
        }

        private void Record(CollectionChange<T> change)
        {
            _writeChanges.Add(change);

            unchecked
            {
                _version++;
            }
        }

        private void RaiseCountAndIndexerChanged()
        {
            if (PropertyChanged is null)
                return;

            OnPropertyChanged(CountChangedArgs);
            OnPropertyChanged(IndexerChangedArgs);
        }

        private void RaiseIndexerChanged()
        {
            if (PropertyChanged is not null)
                OnPropertyChanged(IndexerChangedArgs);
        }
    }
}
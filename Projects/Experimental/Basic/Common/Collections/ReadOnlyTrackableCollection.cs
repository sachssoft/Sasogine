using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Common.Collections
{
    /// <summary>
    /// Provides a read-only view over a <see cref="TrackableCollection{T}"/>
    /// while preserving change tracking and notification support.
    /// </summary>
    /// <typeparam name="T">
    /// The type of element contained in the collection.
    /// </typeparam>
    /// <remarks>
    /// The collection cannot be modified through this wrapper.
    /// Changes made to the underlying collection remain observable through
    /// <see cref="CollectionChanged"/>, <see cref="PropertyChanged"/>,
    /// and the tracking members provided by <see cref="ITrackable{TChanges}"/>.
    /// </remarks>
    public class ReadOnlyTrackableCollection<T> :
        IReadOnlyList<T>,
        INotifyCollectionChanged,
        INotifyPropertyChanged,
        ITrackable<CollectionChangeSet<T>>
    {
        private readonly TrackableCollection<T> _source;
        private NotifyCollectionChangedEventHandler? _collectionChanged;
        private PropertyChangedEventHandler? _propertyChanged;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="ReadOnlyTrackableCollection{T}"/> class that wraps
        /// the specified collection.
        /// </summary>
        /// <param name="source">
        /// The trackable collection to expose as a read-only collection.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> is <see langword="null"/>.
        /// </exception>
        public ReadOnlyTrackableCollection(TrackableCollection<T> source)
        {
            ArgumentNullException.ThrowIfNull(source);
            _source = source;
        }

        /// <summary>
        /// Occurs when the contents of the underlying collection change.
        /// </summary>
        /// <remarks>
        /// Subscription to the underlying collection is established lazily
        /// when the first handler is registered and removed when the last
        /// handler is unregistered.
        /// </remarks>
        public event NotifyCollectionChangedEventHandler? CollectionChanged
        {
            add
            {
                bool subscribe = _collectionChanged is null;
                _collectionChanged += value;

                if (subscribe && _collectionChanged is not null)
                    _source.CollectionChanged += SourceCollectionChanged;
            }
            remove
            {
                _collectionChanged -= value;

                if (_collectionChanged is null)
                    _source.CollectionChanged -= SourceCollectionChanged;
            }
        }

        /// <summary>
        /// Occurs when a property of the underlying collection changes.
        /// </summary>
        /// <remarks>
        /// Subscription to the underlying collection is established lazily
        /// when the first handler is registered and removed when the last
        /// handler is unregistered.
        /// </remarks>
        public event PropertyChangedEventHandler? PropertyChanged
        {
            add
            {
                bool subscribe = _propertyChanged is null;
                _propertyChanged += value;

                if (subscribe && _propertyChanged is not null)
                    _source.PropertyChanged += SourcePropertyChanged;
            }
            remove
            {
                _propertyChanged -= value;

                if (_propertyChanged is null)
                    _source.PropertyChanged -= SourcePropertyChanged;
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the underlying collection.
        /// </summary>
        public int Count => _source.Count;

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the element to retrieve.
        /// </param>
        /// <returns>
        /// The element at the specified index.
        /// </returns>
        public T this[int index] => _source[index];

        /// <summary>
        /// Gets a value indicating whether the underlying collection contains
        /// unconsumed changes.
        /// </summary>
        public bool HasChanges => _source.HasChanges;

        /// <summary>
        /// Gets the current change version of the underlying collection.
        /// </summary>
        public int Version => _source.Version;

        /// <summary>
        /// Returns the changes recorded by the underlying collection since
        /// the previous consume operation.
        /// </summary>
        /// <returns>
        /// A change set containing the pending collection changes, or an empty
        /// change set when no changes are pending.
        /// </returns>
        /// <remarks>
        /// Consuming changes modifies only the tracking state of the underlying
        /// collection. The collection contents remain unchanged.
        /// The returned change set remains valid only until the next call to
        /// <see cref="ConsumeChanges"/>.
        /// </remarks>
        public CollectionChangeSet<T> ConsumeChanges() =>
            _source.ConsumeChanges();

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator for the collection.
        /// </returns>
        public List<T>.Enumerator GetEnumerator() =>
            _source.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator<T> IEnumerable<T>.GetEnumerator() =>
            ((IEnumerable<T>)_source).GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() =>
            ((IEnumerable)_source).GetEnumerator();

        /// <summary>
        /// Forwards collection change notifications from the underlying collection.
        /// </summary>
        /// <param name="sender">
        /// The underlying collection that raised the event.
        /// </param>
        /// <param name="e">
        /// The event data describing the collection change.
        /// </param>
        private void SourceCollectionChanged(
            object? sender,
            NotifyCollectionChangedEventArgs e) =>
            _collectionChanged?.Invoke(this, e);

        /// <summary>
        /// Forwards property change notifications from the underlying collection.
        /// </summary>
        /// <param name="sender">
        /// The underlying collection that raised the event.
        /// </param>
        /// <param name="e">
        /// The event data describing the property change.
        /// </param>
        private void SourcePropertyChanged(
            object? sender,
            PropertyChangedEventArgs e) =>
            _propertyChanged?.Invoke(this, e);
    }
}
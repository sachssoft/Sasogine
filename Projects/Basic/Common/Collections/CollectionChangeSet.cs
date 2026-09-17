using System;
using System.Collections;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Common.Collections
{
    /// <summary>
    /// Represents a set of collection changes accumulated between consume operations.
    /// </summary>
    /// <typeparam name="T">
    /// The type of element affected by the collection changes.
    /// </typeparam>
    /// <remarks>
    /// The change set is a zero-copy view over an internally reusable buffer.
    /// It is valid only until the next call to <c>ConsumeChanges</c> on the
    /// same source collection.
    /// </remarks>
    public readonly struct CollectionChangeSet<T> :
        IReadOnlyList<CollectionChange<T>>
    {
        private readonly List<CollectionChange<T>>? _items;

        /// <summary>
        /// Initializes a new change set backed by the specified change buffer.
        /// </summary>
        /// <param name="items">
        /// The list containing the collection changes.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="items"/> is <see langword="null"/>.
        /// </exception>
        internal CollectionChangeSet(List<CollectionChange<T>> items)
        {
            ArgumentNullException.ThrowIfNull(items);
            _items = items;
        }

        /// <summary>
        /// Gets the number of changes contained in the change set.
        /// </summary>
        public int Count => _items?.Count ?? 0;

        /// <summary>
        /// Gets a value indicating whether the change set contains no changes.
        /// </summary>
        public bool IsEmpty => Count == 0;

        /// <summary>
        /// Gets the change at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the change to retrieve.
        /// </param>
        /// <returns>
        /// The collection change at the specified index.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The change set is empty or <paramref name="index"/> is outside
        /// the valid range.
        /// </exception>
        public CollectionChange<T> this[int index] =>
            _items is not null
                ? _items[index]
                : throw new ArgumentOutOfRangeException(nameof(index));

        /// <summary>
        /// Returns an enumerator that iterates through the collection changes.
        /// </summary>
        /// <returns>
        /// An enumerator for the change set.
        /// </returns>
        public Enumerator GetEnumerator() => new(_items);

        /// <inheritdoc/>
        IEnumerator<CollectionChange<T>>
            IEnumerable<CollectionChange<T>>.GetEnumerator() =>
            GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        /// <summary>
        /// Enumerates the changes contained in a
        /// <see cref="CollectionChangeSet{T}"/>.
        /// </summary>
        public struct Enumerator : IEnumerator<CollectionChange<T>>
        {
            private List<CollectionChange<T>>.Enumerator _enumerator;

            /// <summary>
            /// Initializes an enumerator for the specified change buffer.
            /// </summary>
            /// <param name="items">
            /// The change buffer to enumerate, or <see langword="null"/> for
            /// an empty change set.
            /// </param>
            internal Enumerator(List<CollectionChange<T>>? items)
            {
                _enumerator = (items ?? EmptyHolder.Items).GetEnumerator();
            }

            /// <summary>
            /// Gets the collection change at the current position of the enumerator.
            /// </summary>
            public CollectionChange<T> Current => _enumerator.Current;

            /// <inheritdoc/>
            object IEnumerator.Current => Current;

            /// <summary>
            /// Advances the enumerator to the next collection change.
            /// </summary>
            /// <returns>
            /// <see langword="true"/> if the enumerator was successfully advanced;
            /// otherwise, <see langword="false"/>.
            /// </returns>
            public bool MoveNext() =>
                _enumerator.MoveNext();

            /// <summary>
            /// Releases resources used by the enumerator.
            /// </summary>
            public void Dispose() =>
                _enumerator.Dispose();

            /// <inheritdoc/>
            void IEnumerator.Reset() =>
                throw new NotSupportedException();
        }

        private static class EmptyHolder
        {
            internal static readonly List<CollectionChange<T>> Items = [];
        }
    }
}
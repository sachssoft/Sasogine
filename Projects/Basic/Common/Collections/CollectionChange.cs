using System.Collections.Specialized;

namespace Sachssoft.Engine.Common.Collections
{
    /// <summary>
    /// Describes a single change made to a collection without allocating
    /// additional item arrays.
    /// </summary>
    /// <typeparam name="T">
    /// The type of element affected by the collection change.
    /// </typeparam>
    public readonly struct CollectionChange<T>
    {
        /// <summary>
        /// Initializes a new collection change.
        /// </summary>
        /// <param name="action">
        /// The type of collection change.
        /// </param>
        /// <param name="newItem">
        /// The item added, inserted, moved, or used as a replacement.
        /// </param>
        /// <param name="oldItem">
        /// The item removed, moved, or replaced.
        /// </param>
        /// <param name="newStartingIndex">
        /// The index at which the new item is located, or -1 if not applicable.
        /// </param>
        /// <param name="oldStartingIndex">
        /// The previous index of the old item, or -1 if not applicable.
        /// </param>
        internal CollectionChange(
            NotifyCollectionChangedAction action,
            T? newItem,
            T? oldItem,
            int newStartingIndex,
            int oldStartingIndex)
        {
            Action = action;
            NewItem = newItem;
            OldItem = oldItem;
            NewStartingIndex = newStartingIndex;
            OldStartingIndex = oldStartingIndex;
        }

        /// <summary>
        /// Gets the action that describes the collection change.
        /// </summary>
        public NotifyCollectionChangedAction Action { get; }

        /// <summary>
        /// Gets the new item associated with the change.
        /// </summary>
        /// <remarks>
        /// This value is used for add, replace, and move operations.
        /// It is <see langword="null"/> or the default value of
        /// <typeparamref name="T"/> when not applicable.
        /// </remarks>
        public T? NewItem { get; }

        /// <summary>
        /// Gets the previous item associated with the change.
        /// </summary>
        /// <remarks>
        /// This value is used for remove, replace, and move operations.
        /// It is <see langword="null"/> or the default value of
        /// <typeparamref name="T"/> when not applicable.
        /// </remarks>
        public T? OldItem { get; }

        /// <summary>
        /// Gets the new zero-based index associated with the change.
        /// </summary>
        /// <remarks>
        /// Returns -1 when a new index is not applicable.
        /// </remarks>
        public int NewStartingIndex { get; }

        /// <summary>
        /// Gets the previous zero-based index associated with the change.
        /// </summary>
        /// <remarks>
        /// Returns -1 when an old index is not applicable.
        /// </remarks>
        public int OldStartingIndex { get; }

        /// <summary>
        /// Creates a change describing an item addition.
        /// </summary>
        /// <param name="item">The added item.</param>
        /// <param name="index">The index at which the item was added.</param>
        /// <returns>The resulting collection change.</returns>
        internal static CollectionChange<T> Add(T item, int index) =>
            new(
                NotifyCollectionChangedAction.Add,
                item,
                default,
                index,
                -1);

        /// <summary>
        /// Creates a change describing an item removal.
        /// </summary>
        /// <param name="item">The removed item.</param>
        /// <param name="index">The index from which the item was removed.</param>
        /// <returns>The resulting collection change.</returns>
        internal static CollectionChange<T> Remove(T item, int index) =>
            new(
                NotifyCollectionChangedAction.Remove,
                default,
                item,
                -1,
                index);

        /// <summary>
        /// Creates a change describing an item replacement.
        /// </summary>
        /// <param name="newItem">The replacement item.</param>
        /// <param name="oldItem">The item that was replaced.</param>
        /// <param name="index">The index at which the replacement occurred.</param>
        /// <returns>The resulting collection change.</returns>
        internal static CollectionChange<T> Replace(
            T newItem,
            T oldItem,
            int index) =>
            new(
                NotifyCollectionChangedAction.Replace,
                newItem,
                oldItem,
                index,
                index);

        /// <summary>
        /// Creates a change describing an item move.
        /// </summary>
        /// <param name="item">The moved item.</param>
        /// <param name="newIndex">The new index of the item.</param>
        /// <param name="oldIndex">The previous index of the item.</param>
        /// <returns>The resulting collection change.</returns>
        internal static CollectionChange<T> Move(
            T item,
            int newIndex,
            int oldIndex) =>
            new(
                NotifyCollectionChangedAction.Move,
                item,
                item,
                newIndex,
                oldIndex);

        /// <summary>
        /// Creates a change indicating that the collection was reset.
        /// </summary>
        /// <returns>The resulting reset change.</returns>
        internal static CollectionChange<T> Reset() =>
            new(
                NotifyCollectionChangedAction.Reset,
                default,
                default,
                -1,
                -1);
    }
}
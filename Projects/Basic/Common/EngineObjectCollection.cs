using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Provides an ordered collection of engine objects and supports resolving
    /// objects by identifier or class.
    /// </summary>
    /// <typeparam name="T">
    /// The type of engine object contained in the collection.
    /// </typeparam>
    public class EngineObjectCollection<T> :
        IList<T>,
        IList,
        IEngineObjectResolver
        where T : class, IEngineObject
    {
        private readonly List<T> _objects = new();

        /// <summary>
        /// Occurs when an object is added to the collection.
        /// </summary>
        public event EventHandler? Added;

        /// <summary>
        /// Occurs when an object is removed from the collection.
        /// </summary>
        public event EventHandler? Removed;

        /// <summary>
        /// Gets the number of objects contained in the collection.
        /// </summary>
        public int Count => _objects.Count;

        /// <summary>
        /// Gets a value indicating whether the collection is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets or sets the object at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the object to get or set.
        /// </param>
        /// <returns>
        /// The object at the specified index.
        /// </returns>
        public T this[int index]
        {
            get => _objects[index];
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                _objects[index] = value;
            }
        }

        object? IList.this[int index]
        {
            get => this[index];
            set => this[index] = GetObject(value);
        }

        bool IList.IsReadOnly => false;

        bool IList.IsFixedSize => false;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => ((ICollection)_objects).SyncRoot;

        /// <summary>
        /// Finds an object with the specified identifier.
        /// </summary>
        /// <param name="id">
        /// The identifier of the object to find.
        /// </param>
        /// <returns>
        /// The matching object, or <see langword="null"/> if no matching
        /// object was found.
        /// </returns>
        public T? Find(string? id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            foreach (var item in _objects)
            {
                if (string.Equals(
                    id,
                    item.Id,
                    StringComparison.InvariantCulture))
                {
                    return item;
                }
            }

            return null;
        }

        IEngineReferenceable? IEngineObjectResolver.Find(string? id)
        {
            return Find(id);
        }

        /// <summary>
        /// Finds all objects with the specified class.
        /// </summary>
        /// <param name="class">
        /// The class of the objects to find.
        /// </param>
        /// <returns>
        /// An enumerable sequence containing all matching objects.
        /// </returns>
        public IEnumerable<T> FindAll(string? @class)
        {
            foreach (var item in _objects)
            {
                if (string.Equals(
                    @class,
                    item.Class,
                    StringComparison.InvariantCulture))
                {
                    yield return item;
                }
            }
        }

        IEnumerable<IEngineReferenceable>
            IEngineObjectResolver.FindAll(string? @class)
        {
            return FindAll(@class);
        }

        /// <summary>
        /// Attempts to get an object with the specified identifier.
        /// </summary>
        /// <param name="id">
        /// The identifier of the object to find.
        /// </param>
        /// <param name="result">
        /// When this method returns, contains the matching object if found;
        /// otherwise, <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if a matching object was found;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool TryGet(
            string? id,
            [MaybeNullWhen(false)] out T? result)
        {
            if (!string.IsNullOrEmpty(id))
            {
                foreach (var item in _objects)
                {
                    if (string.Equals(
                        id,
                        item.Id,
                        StringComparison.InvariantCulture))
                    {
                        result = item;
                        return true;
                    }
                }
            }

            result = null;
            return false;
        }

        bool IEngineObjectResolver.TryGet(
            string? id,
            [MaybeNullWhen(false)] out IEngineReferenceable? result)
        {
            result = null;

            if (TryGet(id, out var objectResult))
            {
                result = objectResult;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Adds an object to the end of the collection.
        /// </summary>
        /// <param name="item">
        /// The object to add.
        /// </param>
        public void Add(T item)
        {
            ArgumentNullException.ThrowIfNull(item);

            _objects.Add(item);
            Added?.Invoke(this, EventArgs.Empty);
        }

        int IList.Add(object? value)
        {
            Add(GetObject(value));
            return Count - 1;
        }

        /// <summary>
        /// Inserts an object into the collection at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which the object should be inserted.
        /// </param>
        /// <param name="item">
        /// The object to insert.
        /// </param>
        public void Insert(int index, T item)
        {
            ArgumentNullException.ThrowIfNull(item);

            _objects.Insert(index, item);
            Added?.Invoke(this, EventArgs.Empty);
        }

        void IList.Insert(int index, object? value)
        {
            Insert(index, GetObject(value));
        }

        /// <summary>
        /// Removes the specified object from the collection.
        /// </summary>
        /// <param name="item">
        /// The object to remove.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the object was removed;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool Remove(T item)
        {
            if (item == null)
                return false;

            bool removed = _objects.Remove(item);

            if (removed)
                Removed?.Invoke(this, EventArgs.Empty);

            return removed;
        }

        void IList.Remove(object? value)
        {
            if (value is T item)
                Remove(item);
        }

        /// <summary>
        /// Removes the object at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the object to remove.
        /// </param>
        public void RemoveAt(int index)
        {
            _objects.RemoveAt(index);
            Removed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Removes all objects from the collection.
        /// </summary>
        public void Clear()
        {
            if (_objects.Count == 0)
                return;

            _objects.Clear();
            Removed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Determines whether the collection contains the specified object.
        /// </summary>
        /// <param name="item">
        /// The object to locate in the collection.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the object is contained in the collection;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool Contains(T item)
        {
            return _objects.Contains(item);
        }

        bool IList.Contains(object? value)
        {
            return value is T item && Contains(item);
        }

        /// <summary>
        /// Determines the index of the specified object in the collection.
        /// </summary>
        /// <param name="item">
        /// The object to locate in the collection.
        /// </param>
        /// <returns>
        /// The zero-based index of the object if found;
        /// otherwise, <c>-1</c>.
        /// </returns>
        public int IndexOf(T item)
        {
            return _objects.IndexOf(item);
        }

        int IList.IndexOf(object? value)
        {
            return value is T item
                ? IndexOf(item)
                : -1;
        }

        /// <summary>
        /// Copies the objects to the specified array, starting at the
        /// specified array index.
        /// </summary>
        /// <param name="array">
        /// The destination array.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in the destination array at which copying begins.
        /// </param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _objects.CopyTo(array, arrayIndex);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)_objects).CopyTo(array, index);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the objects in the collection.
        /// </summary>
        /// <returns>
        /// An enumerator for the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _objects.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static T GetObject(object? value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            if (value is not T item)
            {
                throw new ArgumentException(
                    $"Value must be assignable to {typeof(T).Name}.",
                    nameof(value));
            }

            return item;
        }
    }
}
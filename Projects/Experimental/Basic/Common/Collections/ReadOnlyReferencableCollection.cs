using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Common.Collections
{
    /// <summary>
    /// Provides a read-only view over a <see cref="ReferencableCollection{T}"/>
    /// while preserving reference lookup, object resolution, and change tracking support.
    /// </summary>
    /// <typeparam name="T">
    /// The type of engine-referenceable object contained in the collection.
    /// </typeparam>
    /// <remarks>
    /// The collection cannot be modified through this wrapper.
    /// Reference lookup, object resolution, and change tracking are delegated
    /// to the underlying <see cref="ReferencableCollection{T}"/>.
    /// </remarks>
    public class ReadOnlyReferencableCollection<T> :
        ReadOnlyTrackableCollection<T>,
        IEngineObjectResolver
        where T : class, IEngineReferenceable
    {
        private readonly ReferencableCollection<T> _source;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="ReadOnlyReferencableCollection{T}"/> class that wraps
        /// the specified referenceable collection.
        /// </summary>
        /// <param name="source">
        /// The referenceable collection to expose as a read-only collection.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> is <see langword="null"/>.
        /// </exception>
        public ReadOnlyReferencableCollection(ReferencableCollection<T> source)
            : base(source)
        {
            ArgumentNullException.ThrowIfNull(source);
            _source = source;
        }

        /// <summary>
        /// Finds an object with the specified identifier.
        /// </summary>
        /// <param name="id">
        /// The identifier of the object to find.
        /// </param>
        /// <returns>
        /// The matching object, or <see langword="null"/> if no matching object exists.
        /// </returns>
        public T? Find(string? id) =>
            _source.Find(id);

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
        /// <see langword="true"/> if an object with the specified identifier was found;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool TryGet(
            string? id,
            [MaybeNullWhen(false)] out T result) =>
            _source.TryGet(id, out result);

        /// <inheritdoc/>
        IEngineReferenceable? IEngineObjectResolver.Find(string? id) =>
            Find(id);

        /// <inheritdoc/>
        bool IEngineObjectResolver.TryGet(
            string? id,
            [MaybeNullWhen(false)] out IEngineReferenceable? result)
        {
            result = Find(id);
            return result is not null;
        }

        /// <inheritdoc/>
        IEnumerable<IEngineReferenceable> IEngineObjectResolver.FindAll(string? @class)
        {
            foreach (T item in this)
            {
                if (item is IEngineClass engineClass &&
                    string.Equals(
                        @class,
                        engineClass.Class,
                        StringComparison.Ordinal))
                {
                    yield return item;
                }
            }
        }
    }
}
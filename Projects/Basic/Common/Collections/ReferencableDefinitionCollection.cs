using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Common.Collections
{
    /// <summary>
    /// Represents an ordered, trackable collection of engine-referenceable definitions
    /// with fast lookup by identifier.
    /// </summary>
    /// <typeparam name="T">
    /// The type of engine-referenceable definition contained in the collection.
    /// </typeparam>
    /// <remarks>
    /// Definitions with a non-null identifier are indexed for fast lookup.
    /// Identifiers are compared using ordinal, case-sensitive comparison and must
    /// be unique within the collection.
    /// </remarks>
    public class ReferencableDefinitionCollection<T> :
        ReferencableCollection<T>,
        IList<IDefinition>
        where T : class, IDefinition, IEngineReferenceable
    {
        /// <inheritdoc/>
        IDefinition IList<IDefinition>.this[int index]
        {
            get => this[index];
            set => this[index] = GetDefinition(value);
        }

        /// <inheritdoc/>
        void ICollection<IDefinition>.Add(IDefinition item)
        {
            Add(GetDefinition(item));
        }

        /// <inheritdoc/>
        bool ICollection<IDefinition>.Contains(IDefinition item)
        {
            return item is T definition &&
                   Contains(definition);
        }

        /// <inheritdoc/>
        void ICollection<IDefinition>.CopyTo(
            IDefinition[] array,
            int arrayIndex)
        {
            ArgumentNullException.ThrowIfNull(array);

            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            if (array.Length - arrayIndex < Count)
            {
                throw new ArgumentException(
                    "The destination array does not have enough space.",
                    nameof(array));
            }

            for (var i = 0; i < Count; i++)
                array[arrayIndex + i] = this[i];
        }

        /// <inheritdoc/>
        int IList<IDefinition>.IndexOf(IDefinition item)
        {
            return item is T definition
                ? IndexOf(definition)
                : -1;
        }

        /// <inheritdoc/>
        void IList<IDefinition>.Insert(
            int index,
            IDefinition item)
        {
            Insert(index, GetDefinition(item));
        }

        /// <inheritdoc/>
        bool ICollection<IDefinition>.Remove(IDefinition item)
        {
            return item is T definition &&
                   Remove(definition);
        }

        /// <inheritdoc/>
        IEnumerator<IDefinition> IEnumerable<IDefinition>.GetEnumerator()
        {
            foreach (var definition in this)
                yield return definition;
        }

        private static T GetDefinition(IDefinition definition)
        {
            ArgumentNullException.ThrowIfNull(definition);

            if (definition is T value)
                return value;

            throw new ArgumentException(
                $"The definition must be of type '{typeof(T).FullName}'.",
                nameof(definition));
        }
    }
}
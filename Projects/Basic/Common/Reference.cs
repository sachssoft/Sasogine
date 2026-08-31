using System;

namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Represents a typed reference to an engine object that can be resolved
    /// through an <see cref="IEngineObjectResolverProvider"/>.
    /// </summary>
    /// <typeparam name="T">
    /// Type of the referenced engine object.
    /// </typeparam>
    public class Reference<T> : IReference
        where T : class, IEngineReferenceable
    {
        /// <summary>
        /// Initializes a new empty reference.
        /// </summary>
        public Reference()
        {
        }

        /// <summary>
        /// Initializes a new reference using the specified identifier.
        /// </summary>
        /// <param name="id">Identifier of the referenced object.</param>
        public Reference(string? id)
        {
            Id = id;
        }

        /// <summary>
        /// Gets or sets the identifier of the referenced object.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets whether this reference does not contain an identifier.
        /// </summary>
        public bool IsEmpty => string.IsNullOrEmpty(Id);

        /// <inheritdoc/>
        Type IReference.TargetType => typeof(T);

        /// <summary>
        /// Resolves the referenced object using the specified resolver provider.
        /// </summary>
        /// <param name="provider">
        /// Provider containing the resolver used to locate the referenced object.
        /// </param>
        /// <returns>
        /// The resolved object when found; otherwise null.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="provider"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the resolved object is not compatible with <typeparamref name="T"/>.
        /// </exception>
        public T? Resolve(IEngineObjectResolverProvider provider)
        {
            ArgumentNullException.ThrowIfNull(provider);

            if (string.IsNullOrEmpty(Id))
                return null;

            var obj = provider.Resolver.Find(Id);

            if (obj == null)
                return null;

            if (obj is not T result)
            {
                throw new InvalidOperationException(
                    $"Object '{Id}' is not of type '{typeof(T).Name}'.");
            }

            return result;
        }

        object? IReference.Resolve(IEngineObjectResolverProvider provider)
        {
            return Resolve(provider);
        }

        /// <summary>
        /// Returns the identifier of the referenced object.
        /// </summary>
        public override string ToString()
        {
            return Id ?? string.Empty;
        }
    }
}
using System;

namespace Sachssoft.Engine
{
    /// <summary>
    /// Represents a typed reference to an engine object that can be resolved
    /// through an <see cref="IEngineObjectResolver"/> or
    /// <see cref="IEngineObjectResolverProvider"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The expected type of the referenced engine object.
    /// </typeparam>
    public class Reference<T> : IReference<T>
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
        /// <param name="id">
        /// The identifier of the referenced object.
        /// </param>
        public Reference(string? id)
        {
            Id = id;
        }

        /// <summary>
        /// Initializes a new reference using the identifier of the specified
        /// engine object.
        /// </summary>
        /// <param name="obj">
        /// The engine object whose identifier is assigned to the reference.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="obj"/> is <see langword="null"/>.
        /// </exception>
        public Reference(IEngineReferenceable obj)
        {
            ArgumentNullException.ThrowIfNull(obj);

            Id = obj.Id;
        }

        /// <summary>
        /// Gets the expected type of the referenced object.
        /// </summary>
        public Type TargetType => typeof(T);

        /// <summary>
        /// Gets or sets the identifier of the referenced object.
        /// </summary>
        /// <value>
        /// The identifier of the referenced object, or <see langword="null"/>
        /// if no identifier is assigned.
        /// </value>
        public string? Id { get; set; }

        /// <summary>
        /// Gets a value indicating whether the reference does not contain an
        /// identifier.
        /// </summary>
        public bool IsEmpty => string.IsNullOrEmpty(Id);

        /// <summary>
        /// Resolves the referenced object using the specified resolver.
        /// </summary>
        /// <param name="resolver">
        /// The resolver used to locate the referenced object.
        /// </param>
        /// <returns>
        /// The resolved object when found; otherwise, <see langword="null"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="resolver"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the resolved object is not compatible with
        /// <typeparamref name="T"/>.
        /// </exception>
        public virtual T? Resolve(IEngineObjectResolver resolver)
        {
            ArgumentNullException.ThrowIfNull(resolver);

            if (string.IsNullOrEmpty(Id))
                return null;

            IEngineReferenceable? referenceable = resolver.Find(Id);

            if (referenceable is null)
                return null;

            if (referenceable is not T result)
            {
                throw new InvalidOperationException(
                    $"Object '{Id}' is not of type '{typeof(T).Name}'.");
            }

            return result;
        }

        /// <summary>
        /// Resolves the referenced object using the specified resolver provider.
        /// </summary>
        /// <param name="provider">
        /// The provider containing the resolver used to locate the referenced
        /// object.
        /// </param>
        /// <returns>
        /// The resolved object when found; otherwise, <see langword="null"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="provider"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the resolved object is not compatible with
        /// <typeparamref name="T"/>.
        /// </exception>
        public virtual T? Resolve(IEngineObjectResolverProvider provider)
        {
            ArgumentNullException.ThrowIfNull(provider);

            return Resolve(provider.Resolver);
        }

        object? IReference.Resolve(IEngineObjectResolver resolver)
        {
            return Resolve(resolver);
        }

        object? IReference.Resolve(IEngineObjectResolverProvider provider)
        {
            return Resolve(provider);
        }

        /// <summary>
        /// Returns the identifier of the referenced object.
        /// </summary>
        /// <returns>
        /// The reference identifier, or an empty string if no identifier is
        /// assigned.
        /// </returns>
        public override string ToString()
        {
            return Id ?? string.Empty;
        }
    }
}
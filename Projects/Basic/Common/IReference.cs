using System;

namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Represents a reference to an engine object that can be resolved
    /// using an engine object resolver or resolver provider.
    /// </summary>
    public interface IReference
    {
        /// <summary>
        /// Gets the expected type of the referenced object.
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        /// Gets or sets the identifier of the referenced object.
        /// </summary>
        string? Id { get; set; }

        /// <summary>
        /// Resolves the referenced object using the specified resolver.
        /// </summary>
        /// <param name="resolver">
        /// The resolver used to locate the referenced object.
        /// </param>
        /// <returns>
        /// The resolved object when found; otherwise, <see langword="null"/>.
        /// </returns>
        object? Resolve(IEngineObjectResolver resolver);

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
        object? Resolve(IEngineObjectResolverProvider provider);
    }
}
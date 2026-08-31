using System;

namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Represents a reference to an engine object that can be resolved
    /// using an <see cref="IEngineObjectResolverProvider"/>.
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
        /// Resolves the referenced object using the specified resolver provider.
        /// </summary>
        /// <param name="provider">
        /// Provider containing the resolver used to locate the referenced object.
        /// </param>
        /// <returns>
        /// The resolved object when found; otherwise null.
        /// </returns>
        object? Resolve(IEngineObjectResolverProvider provider);
    }
}
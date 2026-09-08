namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Represents a typed reference to an engine object that can be resolved
    /// using an engine object resolver or resolver provider.
    /// </summary>
    /// <typeparam name="T">
    /// The expected type of the referenced engine object.
    /// </typeparam>
    public interface IReference<T> : IReference
        where T : class, IEngineReferenceable
    {
        /// <summary>
        /// Resolves the referenced object using the specified resolver.
        /// </summary>
        /// <param name="resolver">
        /// The resolver used to locate the referenced object.
        /// </param>
        /// <returns>
        /// The resolved object when found; otherwise, <see langword="null"/>.
        /// </returns>
        new T? Resolve(IEngineObjectResolver resolver);

        /// <summary>
        /// Resolves the referenced object using the specified resolver provider.
        /// </summary>
        /// <param name="provider">
        /// The provider containing the resolver used to locate the referenced object.
        /// </param>
        /// <returns>
        /// The resolved object when found; otherwise, <see langword="null"/>.
        /// </returns>
        new T? Resolve(IEngineObjectResolverProvider provider);
    }
}
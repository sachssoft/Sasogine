namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Provides access to an <see cref="IEngineObjectResolver"/> used for
    /// resolving engine objects by identifier or classification.
    /// </summary>
    public interface IEngineObjectResolverProvider
    {
        /// <summary>
        /// Gets the engine object resolver.
        /// </summary>
        IEngineObjectResolver Resolver { get; }
    }
}
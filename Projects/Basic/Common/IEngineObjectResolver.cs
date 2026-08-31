using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Provides lookup functionality for resolving engine objects
    /// by identifier or classification.
    /// </summary>
    public interface IEngineObjectResolver
    {
        /// <summary>
        /// Finds an engine object with the specified identifier.
        /// </summary>
        /// <param name="id">Identifier of the engine object to find.</param>
        /// <returns>
        /// The matching engine object when found; otherwise null.
        /// </returns>
        IEngineReferenceable? Find(string? id);

        /// <summary>
        /// Finds all engine objects associated with the specified classification.
        /// </summary>
        /// <param name="class">Classification of the engine objects to find.</param>
        /// <returns>
        /// An enumerable containing all matching engine objects.
        /// </returns>
        IEnumerable<IEngineReferenceable> FindAll(string? @class);

        /// <summary>
        /// Attempts to find an engine object with the specified identifier.
        /// </summary>
        /// <param name="id">Identifier of the engine object to find.</param>
        /// <param name="result">
        /// When this method returns true, contains the matching engine object;
        /// otherwise null.
        /// </param>
        /// <returns>
        /// True if a matching engine object was found; otherwise false.
        /// </returns>
        bool TryGet(
            string? id,
            [MaybeNullWhen(false)] out IEngineReferenceable? result);
    }
}
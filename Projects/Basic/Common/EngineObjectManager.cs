using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Manages registered engine objects and provides lookup functionality
    /// based on their definitions.
    /// </summary>
    public class EngineObjectManager
    {
        private readonly List<IEngineObject> _objects = new();
        private readonly Dictionary<string, IEngineObject> _objectsById =
            new(StringComparer.Ordinal);

        /// <summary>
        /// Registers an engine object with the manager.
        /// </summary>
        /// <param name="object">Engine object to register.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="object"/> is null.
        /// </exception>
        public void Register(IEngineObject @object)
        {
            ArgumentNullException.ThrowIfNull(@object);

            _objects.Add(@object);

            if (@object.Definition is IEngineObjectDefinition definition &&
                definition.Id != null)
            {
                _objectsById[definition.Id] = @object;
            }
        }

        /// <summary>
        /// Unregisters an engine object from the manager.
        ///
        /// If the object is not registered, no action is performed.
        /// </summary>
        /// <param name="object">Engine object to unregister.</param>
        public void Unregister(IEngineObject @object)
        {
            if (@object == null)
                return;

            if (!_objects.Remove(@object))
                return;

            if (@object.Definition is not IEngineObjectDefinition definition ||
                definition.Id == null)
                return;

            if (!_objectsById.TryGetValue(definition.Id, out var indexedObject) ||
                !ReferenceEquals(indexedObject, @object))
                return;

            _objectsById.Remove(definition.Id);

            // Restore another registered object with the same ID, if one exists.
            for (var i = _objects.Count - 1; i >= 0; i--)
            {
                var candidate = _objects[i];

                if (candidate.Definition is IEngineObjectDefinition candidateDefinition &&
                    string.Equals(
                        candidateDefinition.Id,
                        definition.Id,
                        StringComparison.Ordinal))
                {
                    _objectsById[definition.Id] = candidate;
                    break;
                }
            }
        }

        /// <summary>
        /// Finds a registered engine object associated with the specified definition.
        /// </summary>
        /// <param name="definition">Definition used to locate the engine object.</param>
        /// <returns>
        /// The matching engine object when found; otherwise null.
        /// </returns>
        /// <remarks>
        /// Definitions with an identifier are resolved through an indexed lookup.
        /// Definitions without an identifier are resolved by reference equality.
        /// </remarks>
        public IEngineObject? Find(IEngineObjectDefinition definition)
        {
            if (definition == null)
                return null;

            if (definition.Id != null)
            {
                return _objectsById.TryGetValue(definition.Id, out var result)
                    ? result
                    : null;
            }

            foreach (var @object in _objects)
            {
                if (ReferenceEquals(@object.Definition, definition))
                    return @object;
            }

            return null;
        }

        /// <summary>
        /// Attempts to find a registered engine object associated with
        /// the specified definition.
        /// </summary>
        /// <param name="definition">Definition used to locate the engine object.</param>
        /// <param name="result">
        /// When this method returns true, contains the matching engine object;
        /// otherwise null.
        /// </param>
        /// <returns>
        /// True if a matching engine object was found; otherwise false.
        /// </returns>
        public bool TryFind(
            IEngineObjectDefinition definition,
            [MaybeNullWhen(false)] out IEngineObject? result)
        {
            result = Find(definition);
            return result != null;
        }
    }
}
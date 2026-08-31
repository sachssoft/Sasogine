using System;

namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Provides a registry for factories that create objects identified
    /// by a target identifier and type.
    /// </summary>
    public interface IFactoryRegistry
    {
        /// <summary>
        /// Registers a factory for the specified target identifier and type.
        /// </summary>
        /// <param name="targetId">Identifier used to register the factory.</param>
        /// <param name="targetType">Type of object created by the factory.</param>
        /// <param name="factory">Factory used to create the object.</param>
        void Register(string targetId, Type targetType, Func<object?> factory);

        /// <summary>
        /// Unregisters the factory associated with the specified target identifier.
        /// </summary>
        /// <param name="targetId">Identifier of the factory to unregister.</param>
        void Unregister(string targetId);

        /// <summary>
        /// Creates an object using the factory registered for the specified
        /// target identifier and type.
        /// </summary>
        /// <param name="targetId">Identifier of the registered factory.</param>
        /// <param name="targetType">Expected type of the object to create.</param>
        /// <returns>
        /// The created object, or null if no matching factory is available
        /// or the factory returns null.
        /// </returns>
        object? Create(string targetId, Type targetType);
    }
}
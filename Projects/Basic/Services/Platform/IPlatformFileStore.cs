using Sachssoft.Sasogine.Resources;
using System;

namespace Sachssoft.Sasogine.Services.Platform
{
    /// <summary>
    /// Provides access to platform-specific resource files.
    /// </summary>
    /// <remarks>
    /// This service abstracts the underlying file system or resource storage
    /// implementation and allows the engine to access resources independently
    /// from the target platform.
    /// </remarks>
    public interface IPlatformFileStore
    {
        /// <summary>
        /// Gets a resource source for the specified relative path.
        /// </summary>
        /// <param name="relativePath">
        /// The relative path of the resource to access.
        /// </param>
        /// <returns>
        /// A <see cref="ResourceSourceBase"/> instance representing the resource.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="relativePath"/> is <see langword="null"/>.
        /// </exception>
        ResourceSourceBase GetSource(string relativePath);
    }
}
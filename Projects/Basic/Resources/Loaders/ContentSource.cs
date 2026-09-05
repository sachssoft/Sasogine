using System;
using System.IO;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Resources.Sources
{
    /// <summary>
    /// Represents a resource source provided by the MonoGame content manager.
    /// </summary>
    public class ContentSource : ResourceSourceBase
    {
        /// <inheritdoc/>
        /// <exception cref="NotSupportedException">
        /// Stream access is not supported for content manager resources.
        /// </exception>
        protected override Stream OpenStream()
        {
            throw new NotSupportedException(
                "Stream access is not supported for content manager resources.");
        }

        /// <inheritdoc/>
        /// <exception cref="NotSupportedException">
        /// Asynchronous stream access is not supported for content manager resources.
        /// </exception>
        protected override Task<Stream> OpenStreamAsync()
        {
            throw new NotSupportedException(
                "Asynchronous stream access is not supported for content manager resources.");
        }
    }
}
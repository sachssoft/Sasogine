using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Containers;
using System;
using System.IO;

namespace Sachssoft.Sasogine.Containers.Wrappers
{
    /// <summary>
    /// Lazily wraps image data, either as a byte array or as a deferred stream source from a package,
    /// and converts it into a <see cref="Texture2D"/> when needed.
    /// </summary>
    public sealed class Texture2DWrapper : PackageEntryWrapper<Texture2D>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Texture2DWrapper"/> class,
        /// wrapping the provided image data as a byte array.
        /// </summary>
        /// <param name="binary">Byte array containing the image data.</param>
        public Texture2DWrapper(byte[] binary)
            : base(binary, typeof(byte[]))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture2DWrapper"/> class,
        /// wrapping an image data stream provider from the specified package.
        /// This allows deferred loading of image data to save memory.
        /// </summary>
        /// <param name="package">The package containing the image data.</param>
        /// <param name="streamProvider">
        /// A function that takes the package and returns a <see cref="Stream"/> containing the image data.
        /// </param>
        public Texture2DWrapper(PackageBase package, Func<PackageBase, Stream?> streamProvider)
            : base(package, streamProvider)
        {
        }

        /// <summary>
        /// Gets or sets the <see cref="GraphicsDevice"/> required to create the <see cref="Texture2D"/>.
        /// Must be set before calling <see cref="Unwrap"/> or <see cref="Open"/>.
        /// </summary>
        public GraphicsDevice? GraphicsDevice { get; set; }

        /// <summary>
        /// Transforms the wrapped content into a <see cref="Texture2D"/>.
        /// If the content is a byte array, it creates the texture directly.
        /// If the content is a deferred stream from a package, it calls the provided stream provider to read the data on demand.
        /// </summary>
        /// <param name="wrapContent">
        /// The wrapped content to transform, expected to be either a <see cref="byte[]"/> 
        /// or a tuple of (<see cref="PackageBase"/>, <see cref="Func{PackageBase, Stream}"/>).
        /// </param>
        /// <param name="sourceType">The type of the wrapped content.</param>
        /// <returns>
        /// The created <see cref="Texture2D"/> instance, or <c>null</c> if the stream provider returns <c>null</c>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <see cref="GraphicsDevice"/> is not set before transformation, or if the package is <c>null</c>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Thrown if the wrapped content type is not supported.
        /// </exception>
        protected override Texture2D? Transform(object? wrapContent, Type sourceType)
        {
            if (GraphicsDevice == null)
                throw new InvalidOperationException("GraphicsDevice must be set before unwrapping.");

            if (sourceType == typeof(byte[]))
            {
                var binary = (byte[]?)wrapContent ?? Array.Empty<byte>();
                using var ms = new MemoryStream(binary);
                return Texture2D.FromStream(GraphicsDevice, ms);
            }
            else if (IsPackageType(sourceType))
            {
                using var stream = OpenWrappedStream(wrapContent);
                if (stream == null)
                    return null;
                return Texture2D.FromStream(GraphicsDevice, stream);
            }
            else
            {
                throw new NotSupportedException($"Unsupported source type: {sourceType.FullName}");
            }
        }
    }
}

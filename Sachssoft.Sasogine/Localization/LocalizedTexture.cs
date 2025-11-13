using System;
using System.Collections.Generic;
using System.IO;
using Sachssoft.Sasogine.Resources;
using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Sasogine.Localization
{
    /// <summary>
    /// Represents a localized texture entry that can be lazily loaded from a file or embedded resource.
    /// Implements <see cref="ILocalizedEntryWrapper"/> and provides access to a <see cref="Texture2D"/> instance.
    /// </summary>
    public class LocalizedTexture : ILocalizedEntryWrapper
    {
        private readonly GraphicsDevice _graphicsDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizedTexture"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device used to create the texture.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="graphicsDevice"/> is null.</exception>
        public LocalizedTexture(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
        }

        /// <summary>
        /// Gets the loaded <see cref="Texture2D"/> instance.
        /// </summary>
        public Texture2D? Value { get; private set; }

        /// <summary>
        /// Gets the filename or resource path of the texture as defined in the localization XML file.
        /// </summary>
        public string? Filename { get; private set; }

        /// <summary>
        /// Loads the texture using the provided loader and attribute dictionary.
        /// </summary>
        /// <param name="attributes">The attribute dictionary read from the XML localization file.</param>
        /// <param name="loader">The loader used to access the texture file or embedded resource.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="attributes"/> or <paramref name="loader"/> is null.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the attribute <c>FilePath</c> is missing or empty.</exception>
        public void Load(Dictionary<string, string?> attributes, LoaderBase? loader)
        {
            if (attributes == null)
                throw new ArgumentNullException(nameof(attributes));
            if (loader == null)
                throw new ArgumentNullException(nameof(loader));

            if (!attributes.TryGetValue("FilePath", out var filename) || string.IsNullOrEmpty(filename))
                throw new KeyNotFoundException("The 'FilePath' attribute is missing for LocalizedTexture.");

            Filename = filename;

            using Stream stream = loader.GetStream();
            if (stream == null)
                throw new IOException($"Failed to open stream for texture '{filename}'.");

            // Create texture from stream
            Value = Texture2D.FromStream(_graphicsDevice, stream);
        }

        /// <inheritdoc />
        object? ILocalizedEntryWrapper.Value => Value;
    }
}

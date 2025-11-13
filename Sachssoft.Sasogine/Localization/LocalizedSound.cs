using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using Sachssoft.Sasogine.Resources;

namespace Sachssoft.Sasogine.Localization
{
    /// <summary>
    /// Represents a localized sound entry that can be lazily loaded from a file or embedded resource.
    /// Implements <see cref="ILocalizedEntryWrapper"/> and provides access to a <see cref="SoundEffect"/> instance.
    /// </summary>
    public class LocalizedSound : ILocalizedEntryWrapper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizedSound"/> class.
        /// </summary>
        public LocalizedSound()
        {
        }

        /// <summary>
        /// Gets the loaded <see cref="SoundEffect"/> instance.
        /// </summary>
        public SoundEffect? Value { get; private set; }

        /// <summary>
        /// Gets the filename or resource path of the sound file as defined in the localization XML file.
        /// </summary>
        public string? Filename { get; private set; }

        /// <summary>
        /// Loads the sound file using the provided loader and XML attributes.
        /// </summary>
        /// <param name="attributes">The attribute dictionary read from the XML localization file.</param>
        /// <param name="loader">The loader used to access the sound file or embedded resource.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="attributes"/> or <paramref name="loader"/> is null.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the attribute <c>FilePath</c> is missing or empty.</exception>
        /// <exception cref="IOException">Thrown if the stream cannot be opened.</exception>
        public void Load(Dictionary<string, string?> attributes, LoaderBase? loader)
        {
            if (attributes == null)
                throw new ArgumentNullException(nameof(attributes));
            if (loader == null)
                throw new ArgumentNullException(nameof(loader));

            if (!attributes.TryGetValue("FilePath", out var filename) || string.IsNullOrEmpty(filename))
                throw new KeyNotFoundException("The 'FilePath' attribute is missing for LocalizedSound.");

            Filename = filename;

            using Stream stream = loader.GetStream();
            if (stream == null)
                throw new IOException($"Failed to open stream for sound '{filename}'.");

            // Load sound file (MonoGame SoundEffect supports WAV, MP3, OGG)
            Value = SoundEffect.FromStream(stream);
        }

        /// <inheritdoc />
        object? ILocalizedEntryWrapper.Value => Value;
    }
}

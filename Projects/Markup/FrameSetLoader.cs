using Sachssoft.Sasogine.Resources.Markup.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sachssoft.Sasogine.Resources.Markup
{
    /// <summary>
    /// Provides a base class for loading frame set documents.
    /// </summary>
    public abstract class FrameSetLoader
    {
        private readonly ResourceSourceBase _resourceSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameSetLoader"/> class.
        /// </summary>
        /// <param name="resourceSource">
        /// The resource source containing the frame set document.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="resourceSource"/> is <see langword="null"/>.
        /// </exception>
        protected FrameSetLoader(ResourceSourceBase resourceSource)
        {
            _resourceSource = resourceSource
                ?? throw new ArgumentNullException(nameof(resourceSource));

            Entries = Array.Empty<FrameSetEntry>();
        }

        /// <summary>
        /// Creates a frame set loader for the specified document format.
        /// </summary>
        /// <param name="formatType">
        /// The document format to load.
        /// </param>
        /// <param name="resourceSource">
        /// The resource source containing the frame set document.
        /// </param>
        /// <returns>
        /// A frame set loader for the specified document format.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="resourceSource"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Thrown when <paramref name="formatType"/> is not supported.
        /// </exception>
        public static FrameSetLoader Create(
            DocumentFormatType formatType,
            ResourceSourceBase resourceSource)
        {
            ArgumentNullException.ThrowIfNull(resourceSource);

            return formatType switch
            {
                DocumentFormatType.Xml =>
                    new XmlFrameSetLoader(resourceSource),

                DocumentFormatType.Json =>
                    new JsonFrameSetLoader(resourceSource),

                _ => throw new NotSupportedException(
                    $"The document format '{formatType}' is not supported.")
            };
        }

        /// <summary>
        /// Gets the frame set entries loaded from the document.
        /// </summary>
        public IEnumerable<FrameSetEntry> Entries { get; private set; }

        /// <summary>
        /// Loads the frame set document and updates <see cref="Entries"/>.
        /// </summary>
        /// <returns>
        /// The current frame set loader.
        /// </returns>
        /// <exception cref="InvalidDataException">
        /// Thrown when the underlying loader returns <see langword="null"/>.
        /// </exception>
        public FrameSetLoader Load()
        {
            using Stream stream = _resourceSource.GetStream();

            Entries = (OnLoading(stream)
                ?? throw new InvalidDataException(
                    "The frame set loader returned null."))
                .ToArray();

            return this;
        }

        /// <summary>
        /// Loads frame set entries from the specified stream.
        /// </summary>
        /// <param name="stream">
        /// The stream containing the frame set document.
        /// </param>
        /// <returns>
        /// The frame set entries loaded from the stream.
        /// </returns>
        protected abstract IEnumerable<FrameSetEntry> OnLoading(
            Stream stream);
    }
}
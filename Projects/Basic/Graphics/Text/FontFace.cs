using System;

namespace Sachssoft.Sasogine.Graphics.Text
{
    /// <summary>
    /// Represents a loaded font face defined by its binary data, name,
    /// weight, and style.
    /// </summary>
    /// <remarks>
    /// A font face represents a specific variant of a font family, such as
    /// regular, bold, italic, or bold italic.
    /// </remarks>
    public sealed class FontFace
    {
        private readonly byte[] _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="FontFace"/> class.
        /// </summary>
        /// <param name="data">
        /// The binary font data.
        /// </param>
        /// <param name="name">
        /// The name of the font face.
        /// </param>
        /// <param name="weightDefinition">
        /// The weight associated with the font face.
        /// </param>
        /// <param name="styleDefinition">
        /// The style associated with the font face.
        /// </param>
        public FontFace(
            byte[] data,
            string name,
            FontWeight weightDefinition = FontWeight.Normal,
            FontStyle styleDefinition = FontStyle.Normal)
        {
            ArgumentNullException.ThrowIfNull(data);
            ArgumentException.ThrowIfNullOrEmpty(name);

            if (data.Length == 0)
                throw new ArgumentException("Font data cannot be empty.", nameof(data));

            _data = data;

            Name = name;
            WeightDefinition = weightDefinition;
            StyleDefinition = styleDefinition;
        }

        /// <summary>
        /// Gets the name of the font face.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the weight associated with the font face.
        /// </summary>
        public FontWeight WeightDefinition { get; }

        /// <summary>
        /// Gets the style associated with the font face.
        /// </summary>
        public FontStyle StyleDefinition { get; }

        /// <summary>
        /// Gets the binary font data.
        /// </summary>
        public ReadOnlyMemory<byte> Data => _data;
    }
}
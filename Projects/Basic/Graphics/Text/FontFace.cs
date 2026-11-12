using System;

namespace Sachssoft.Engine.Graphics.Text
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
        private System.Collections.Concurrent.ConcurrentDictionary<float, IFont>? _fonts;

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

        internal byte[] FontBytes => _data;

        /// <summary>Gets a cached font using the engine's default backend.</summary>
        /// <param name="size">The positive logical font size.</param>
        /// <returns>A font whose glyph textures are owned by the drawing device.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> is nonpositive or nonfinite.</exception>
        public IFont GetFont(float size)
        {
            if (!float.IsFinite(size) || size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size));
            var fonts = System.Threading.LazyInitializer.EnsureInitialized(ref _fonts);
            return fonts.GetOrAdd(size, static (value, face) => new FontStashFont(face, value), this);
        }
    }
}

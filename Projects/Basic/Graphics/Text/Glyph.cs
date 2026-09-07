using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Geometry;

namespace Sachssoft.Sasogine.Graphics.Text
{
    /// <summary>
    /// Represents the layout information of a single glyph.
    /// </summary>
    public readonly struct Glyph
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Glyph"/> structure.
        /// </summary>
        /// <param name="codepoint">
        /// The Unicode code point represented by the glyph.
        /// </param>
        /// <param name="position">
        /// The position of the glyph within the text layout.
        /// </param>
        /// <param name="size">
        /// The size of the glyph.
        /// </param>
        /// <param name="advance">
        /// The horizontal and vertical advance applied after the glyph.
        /// </param>
        public Glyph(
            int codepoint,
            Point2 position,
            Size2 size,
            Vector2 advance)
        {
            Codepoint = codepoint;
            Position = position;
            Size = size;
            Advance = advance;
        }

        /// <summary>
        /// Gets the Unicode code point represented by the glyph.
        /// </summary>
        public int Codepoint { get; }

        /// <summary>
        /// Gets the position of the glyph within the text layout.
        /// </summary>
        public Point2 Position { get; }

        /// <summary>
        /// Gets the size of the glyph.
        /// </summary>
        public Size2 Size { get; }

        /// <summary>
        /// Gets the horizontal and vertical advance applied after the glyph.
        /// </summary>
        public Vector2 Advance { get; }
    }
}
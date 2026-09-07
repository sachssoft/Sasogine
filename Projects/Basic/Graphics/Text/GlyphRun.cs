using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Geometry;
using Sachssoft.Sasogine.Graphics.Rendering;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Graphics.Text
{
    /// <summary>
    /// Represents a sequence of positioned glyphs produced by a text layout
    /// operation.
    /// </summary>
    public sealed class GlyphRun
    {
        private readonly List<Glyph> _glyphs = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="GlyphRun"/> class using
        /// the specified font options.
        /// </summary>
        /// <param name="font">
        /// The font options used to create the glyph run.
        /// </param>
        public GlyphRun(FontOptions font)
        {
            ArgumentNullException.ThrowIfNull(font);

            Font = font;
        }

        /// <summary>
        /// Gets the font options used to create the glyph run.
        /// </summary>
        public FontOptions Font { get; }

        /// <summary>
        /// Gets the glyphs contained in the run.
        /// </summary>
        public IReadOnlyList<Glyph> Glyphs => _glyphs;

        /// <summary>
        /// Gets the total size of the laid out glyph run.
        /// </summary>
        public Size2 Size { get; internal set; }

        /// <summary>
        /// Adds a glyph to the run.
        /// </summary>
        /// <param name="glyph">
        /// The glyph to add.
        /// </param>
        internal void Add(Glyph glyph)
        {
            _glyphs.Add(glyph);
        }
    }
}
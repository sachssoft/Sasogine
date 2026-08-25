using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Graphics.Rendering;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Graphics.Text
{
    public sealed class GlyphRun
    {
        public FontOptions Font { get; }

        public List<Glyph> Glyphs { get; } = new();

        public Vector2 Bounds { get; set; }

        public GlyphRun(FontOptions font)
        {
            Font = font;
        }
    }
}

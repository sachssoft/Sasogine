using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Graphics.Text
{
    public sealed class GlyphRun
    {
        public Font Font { get; }

        public List<Glyph> Glyphs { get; } = new();

        public Vector2 Bounds { get; set; }

        public GlyphRun(Font font)
        {
            Font = font;
        }
    }
}

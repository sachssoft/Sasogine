using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Graphics.Text
{
    public readonly struct Glyph
    {
        public int Codepoint { get; init; }

        public Vector2 Position { get; init; }

        public Vector2 Size { get; init; }

        public Vector2 Advance { get; init; }

    }
}

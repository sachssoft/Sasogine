using FontStashSharp;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Sasogine.Graphics.Rendering
{
    [Obsolete("Will be remove")]
    public class TextRendererStyle
    {
        public TextRendererStyle()
        {
        }

        public float FontSize { get; set; }

        public TextHorizontalAlignment Alignment { get; set; }

        public Color Color { get; set; }

        public float CharacterSpacing { get; set; }

        public float LineSpacing { get; set; }

        public FontStashSharp.TextStyle Decoration { get; set; }

    }
}

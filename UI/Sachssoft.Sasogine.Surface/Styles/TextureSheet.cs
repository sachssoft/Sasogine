using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Surface.Visuals.Regions;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Surface.Styles
{
    public sealed class TextureSheet
    {
        public TextureSheet()
        {
        }

        public Dictionary<string, ITextureRegion> Regions { get; } = new Dictionary<string, ITextureRegion>();

        public string? SourceFilePath { get; set; }

        public Texture2D? Source { get; set; }

    }
}

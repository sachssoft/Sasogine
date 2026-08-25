using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Resources
{
    [Obsolete("This type will be removed.")]
    public class SimpleTextureRegion : ITextureRegion
    {
        public IReadOnlyList<Rectangle> Regions { get; }

        public SimpleTextureRegion(Rectangle rect)
        {
            Regions = new[] { rect }; // nur ein Rechteck
        }
    }
}

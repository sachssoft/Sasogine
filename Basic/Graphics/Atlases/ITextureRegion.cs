using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Resources
{
    public interface ITextureRegion
    {
        IReadOnlyList<Rectangle> Regions { get; } // = SourceRects
    }
}

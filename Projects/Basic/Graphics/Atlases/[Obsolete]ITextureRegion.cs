using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Resources
{
    [Obsolete("Replaced by ISourceRegion")]
    public interface ITextureRegion
    {
        IReadOnlyList<Rectangle> Regions { get; } // = SourceRects
    }
}

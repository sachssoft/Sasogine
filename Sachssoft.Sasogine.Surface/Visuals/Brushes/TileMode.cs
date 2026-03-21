using System;

namespace Sachssoft.Sasogine.Surface.Visuals.Brushes
{
    [Flags]
    public enum TileMode
    {
        None = 0,
        TileX = 1,
        TileY = 2,
        TileXY = TileX | TileY
    }
}

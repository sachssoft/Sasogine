using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

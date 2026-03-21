using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Surface.Inputs
{
    public struct PointerInfo
    {
        public Point PreviousMousePosition { get; set; }

        public Point CurrentMousePosition { get; set; }

        public Point PreviousTouchPosition { get; set; }

        public bool WasPreviousTouchSet { get; set; }

        public Point CurrentTouchPosition { get; set; }
    }
}

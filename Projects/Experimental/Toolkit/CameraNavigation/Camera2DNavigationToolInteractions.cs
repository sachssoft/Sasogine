using Sachssoft.Sasogine.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Components.Tools
{
    public sealed class Camera2DNavigationToolInteractions
    {
        public InteractionFlags ZoomIn { get; set; } = InteractionFlags.None;

        public InteractionFlags ZoomOut { get; set; } = InteractionFlags.None;

        public InteractionFlags Move { get; set; } = InteractionFlags.None;
    }
}

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Embedding
{
    public readonly struct EmbeddedMouseState
    {
        public Vector2 Position { get; init; }

        public bool IsLeftPressed { get; init; }

        public bool IsRightPressed { get; init; }
    }
}

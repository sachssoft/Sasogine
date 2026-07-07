using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics;

namespace Sachssoft.Sasogine.Graphics.Rendering
{
    public interface ITransformable
    {
        public Vector2 Translation { get; set; }

        public float Rotation { get; set; }

        public Vector2 Scale { get; set; }

        public Vector2 Pivot { get; set; }
    }
}

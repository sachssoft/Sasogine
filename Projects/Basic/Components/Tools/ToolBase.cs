using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Components.Tools
{
    public abstract class ToolBase
    {
        public bool IsActive { get; set; } = true;

        public object? Tag { get; set; }

        public virtual void Update(SceneUpdateContext context)
        {
        }

        public virtual void Draw(SceneDrawContext context)
        {
        }
    }
}
using Sachssoft.Sasogine.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Components.Tools
{
    public sealed class SelectionTool : ToolBase
    {

        public bool RotationAllowed { get; set; }

        public bool MoveAllowed { get; set; }

        public bool ScaleAllowed { get; set; }

        // Spezialfall zu Scale
        public bool CanResize { get; set; }

        public void Draw(SceneDrawContext context)
        {
            throw new NotImplementedException();
        }

        public void Update(SceneUpdateContext context)
        {
            throw new NotImplementedException();
        }
    }
}

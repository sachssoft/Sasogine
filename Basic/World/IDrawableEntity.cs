using Sachssoft.Sasogine.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.World
{
    public interface IDrawableEntity
    {
        void Draw(SceneDrawContext context);
    }
}

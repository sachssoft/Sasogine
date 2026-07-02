using Sachssoft.Sasogine.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Components
{
    public interface IUpdatableComponent
    {
        bool IsEnabled { get; set; }

        void Update(SceneUpdateContext context);
    }
}

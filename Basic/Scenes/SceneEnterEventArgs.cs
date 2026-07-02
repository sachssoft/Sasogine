using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Scenes
{
    public class SceneEnterEventArgs : EventArgs
    {
        public SceneEnterEventArgs(SceneManager manager, IGameApplication application, IEnumerable<IScene> activeScenes)
        {
            Manager = manager;
            Application = application;
            ActiveScenes = activeScenes;
        }

        public SceneManager Manager { get; }

        public IGameApplication Application { get; }

        public IEnumerable<IScene> ActiveScenes { get; }
    }
}

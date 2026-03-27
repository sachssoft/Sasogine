using System.Collections.Generic;

namespace Sachssoft.Sasogine.Scenes
{
    public class SceneContext
    {

        public required IGameApplication Application { get; init; }

        public required IEnumerable<IScene> ActiveScenes { get; init; }

    }
}

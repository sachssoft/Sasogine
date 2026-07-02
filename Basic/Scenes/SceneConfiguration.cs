using System;

namespace Sachssoft.Sasogine.Scenes
{
    public abstract class SceneConfiguration
    {

        public Func<IGameApplication, IScene, GameContext>? ConfigureGameContext { get; set; }


    }
}

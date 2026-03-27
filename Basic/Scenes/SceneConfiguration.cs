using System;

namespace Sachssoft.Sasogine.Scenes
{
    public abstract class SceneConfiguration
    {

        public Func<IGameApplication, IScene, GameContext>? ConfigureGameContext { get; set; }

        public Func<IGameApplication, ISceneWithRuntime, RuntimeContext>? ConfigureRuntimeContext { get; set; }

        public Func<IGameApplication, ISceneWithPresentation, PresentationContext>? ConfigurePresentationContext { get; set; }


    }
}

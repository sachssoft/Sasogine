using System;

namespace Sachssoft.Sasogine.Scenes
{
    public abstract class SceneConfiguration
    {

        public Func<GameApplication, IScene, GameContext>? ConfigureGameContext { get; set; }

        public Func<GameApplication, ISceneWithRuntime, RuntimeContext>? ConfigureRuntimeContext { get; set; }

        public Func<GameApplication, ISceneWithPresentation, PresentationContext>? ConfigurePresentationContext { get; set; }


    }
}

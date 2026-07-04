using Sachssoft.Sasogine.Components.Rendering.Camera;
using System;

namespace Sachssoft.Sasogine.Scenes
{
    public class SceneUpdateContext : GameContext
    {
        public SceneUpdateContext(
            IGameApplication application,
            IScene scene,
            ICamera[] cameras,
            float frameCounterSmoothing = 0.1f,
            float frameCounterFastWeight = 0.2f)
            : base(application, frameCounterSmoothing, frameCounterFastWeight)
        {
            Scene = scene ?? throw new ArgumentNullException(nameof(scene));
            Cameras = cameras ?? throw new ArgumentNullException(nameof(cameras));
        }

        public IScene Scene { get; }

        public ICamera[] Cameras { get; }
    }
}

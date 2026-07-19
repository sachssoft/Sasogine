using Sachssoft.Sasogine.Basic.Scenes;
using Sachssoft.Sasogine.Graphics.Camera;
using System;

namespace Sachssoft.Sasogine.Scenes
{
    /// <summary>
    /// Provides contextual information required during scene updates.
    /// Contains the current scene and the cameras associated with the update cycle.
    /// </summary>
    public class SceneUpdateContext : GameContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneUpdateContext"/> class.
        /// </summary>
        /// <param name="application">
        /// The game application associated with this update context.
        /// </param>
        /// <param name="scene">
        /// The scene currently being updated.
        /// </param>
        /// <param name="cameras">
        /// The cameras managed by the scene update cycle.
        /// </param>
        /// <param name="frameCounterSmoothing">
        /// The smoothing factor used for frame timing calculations.
        /// </param>
        /// <param name="frameCounterFastWeight">
        /// The weight used for fast frame timing calculations.
        /// </param>
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

            RuntimeMode = Scene.RuntimeMode;
        }


        /// <summary>
        /// Gets the scene currently being updated.
        /// </summary>
        public IScene Scene { get; }


        /// <summary>
        /// Gets the cameras associated with the current update cycle.
        /// </summary>
        public ICamera[] Cameras { get; }

        /// <summary>
        /// Gets the runtime mode of the current scene.
        /// </summary>
        public RuntimeMode RuntimeMode { get; }
    }
}
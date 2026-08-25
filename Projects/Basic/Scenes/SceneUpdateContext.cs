using Sachssoft.Sasogine.Graphics.Cameras;
using System;

namespace Sachssoft.Sasogine.Scenes
{
    /// <summary>
    /// Provides contextual information required during scene updates.
    /// Contains the current scene, runtime settings, and cameras associated with the update cycle.
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
        /// The cameras associated with the scene update cycle.
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

            RuntimeMode = RuntimeMode.Game;
            RuntimeOptions = RuntimeOptions.None;

            if (Scene is ISceneRuntimeSettings runtimeSettings)
            {
                RuntimeMode = runtimeSettings.RuntimeMode;
                RuntimeOptions = runtimeSettings.RuntimeOptions;
            }
        }

        /// <summary>
        /// Gets the scene currently being updated.
        /// </summary>
        public IScene Scene { get; }

        /// <summary>
        /// Gets the cameras associated with the current scene update cycle.
        /// </summary>
        public ICamera[] Cameras { get; }

        /// <summary>
        /// Gets the runtime mode that defines how the current scene is executed.
        /// </summary>
        public RuntimeMode RuntimeMode { get; }

        /// <summary>
        /// Gets the optional runtime features enabled for the current scene execution.
        /// </summary>
        public RuntimeOptions RuntimeOptions { get; }
    }
}
using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Graphics.Cameras;
using Sachssoft.Sasogine.Graphics.Materials;
using System;

namespace Sachssoft.Sasogine.Scenes
{
    /// <summary>
    /// Provides contextual information required during scene rendering.
    /// Contains the current scene, runtime settings, active camera,
    /// default material, frame information, and viewport information
    /// for multi-view rendering.
    /// </summary>
    public class SceneDrawContext : GameContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneDrawContext"/> class.
        /// </summary>
        /// <param name="application">
        /// The game application associated with this rendering context.
        /// </param>
        /// <param name="scene">
        /// The scene currently being rendered.
        /// </param>
        /// <param name="viewCamera">
        /// The camera assigned to the current rendering view.
        /// </param>
        /// <param name="defaultMaterial">
        /// The default material used when a primitive does not provide a custom material.
        /// </param>
        /// <param name="viewIndex">
        /// The zero-based index of the current rendering view.
        /// </param>
        /// <param name="viewCount">
        /// The total number of rendering views managed by the scene.
        /// </param>
        /// <param name="frameCounterSmoothing">
        /// The smoothing factor used for frame timing calculations.
        /// </param>
        /// <param name="frameCounterFastWeight">
        /// The weight used for fast frame timing calculations.
        /// </param>
        public SceneDrawContext(
            IGameApplication application,
            IScene scene,
            ICamera viewCamera,
            IMaterial defaultMaterial,
            int viewIndex,
            int viewCount,
            float frameCounterSmoothing = 0.1f,
            float frameCounterFastWeight = 0.2f)
            : base(application, frameCounterSmoothing, frameCounterFastWeight)
        {
            Scene = scene ?? throw new ArgumentNullException(nameof(scene));

            ViewCamera = viewCamera ?? throw new ArgumentNullException(nameof(viewCamera));
            DefaultMaterial = defaultMaterial ?? throw new ArgumentNullException(nameof(defaultMaterial));

            ViewIndex = viewIndex;
            ViewCount = viewCount;

            RuntimeMode = RuntimeMode.Game;
            RuntimeOptions = RuntimeOptions.None;

            if (Scene is ISceneRuntimeSettings runtimeSettings)
            {
                RuntimeMode = runtimeSettings.RuntimeMode;
                RuntimeOptions = runtimeSettings.RuntimeOptions;
            }
        }

        /// <summary>
        /// Gets the scene currently being rendered.
        /// </summary>
        public IScene Scene { get; }

        /// <summary>
        /// Gets the runtime mode that defines how the current scene is executed.
        /// </summary>
        public RuntimeMode RuntimeMode { get; }

        /// <summary>
        /// Gets the optional runtime features enabled for the current scene execution.
        /// </summary>
        public RuntimeOptions RuntimeOptions { get; }

        /// <summary>
        /// Gets the camera assigned to the current rendering view.
        /// </summary>
        public ICamera ViewCamera { get; }

        /// <summary>
        /// Gets the zero-based index of the current rendering view.
        /// Used for multi-view or split-screen rendering.
        /// </summary>
        public int ViewIndex { get; }

        /// <summary>
        /// Gets the total number of rendering views managed by the scene.
        /// </summary>
        public int ViewCount { get; }

        /// <summary>
        /// Gets the default material used when a primitive does not provide a custom material.
        /// </summary>
        public IMaterial DefaultMaterial { get; }

        /// <summary>
        /// Calculates the viewport rectangle assigned to the current rendering view.
        /// Supports single-view and split-screen layouts with up to four views.
        /// </summary>
        /// <param name="screenBounds">
        /// The available screen area used as the base viewport region.
        /// </param>
        /// <returns>
        /// The viewport rectangle assigned to the current view.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Thrown when the configured view count or view index is not supported.
        /// </exception>
        public Rectangle GetViewport(Rectangle screenBounds)
        {
            int w = screenBounds.Width;
            int h = screenBounds.Height;

            switch (ViewCount)
            {
                case 1:
                    return new Rectangle(0, 0, w, h);
                case 2:
                    return ViewIndex switch
                    {
                        0 => new Rectangle(0, 0, w, h / 2),
                        1 => new Rectangle(0, h / 2, w, h / 2),
                        _ => throw new NotSupportedException()
                    };
                case 3:
                    return ViewIndex switch
                    {
                        0 => new Rectangle(0, 0, w / 2, h / 2),
                        1 => new Rectangle(w / 2, 0, w / 2, h / 2),
                        2 => new Rectangle(0, h / 2, w, h / 2),
                        _ => throw new NotSupportedException()
                    };
                case 4:
                    return ViewIndex switch
                    {
                        0 => new Rectangle(0, 0, w / 2, h / 2),
                        1 => new Rectangle(w / 2, 0, w / 2, h / 2),
                        2 => new Rectangle(0, h / 2, w / 2, h / 2),
                        3 => new Rectangle(w / 2, h / 2, w / 2, h / 2),
                        _ => throw new NotSupportedException()
                    };
                default:
                    throw new NotSupportedException();
            }
        }
    }
}

using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics.Camera;
using Sachssoft.Sasogine.Graphics.Rendering;

namespace Sachssoft.Sasogine.Scenes
{
    /// <summary>
    /// Defines a game scene that manages its lifecycle, state updates,
    /// rendering, views, cameras, and shaders.
    /// </summary>
    public interface IScene
    {
        /// <summary>
        /// Gets a value indicating whether the scene remains loaded
        /// after becoming inactive.
        /// </summary>
        bool IsPersistent { get; }

        /// <summary>
        /// Gets a value indicating whether the scene resources have been loaded.
        /// </summary>
        bool IsLoaded { get; }

        /// <summary>
        /// Gets the number of rendering views managed by this scene.
        /// </summary>
        int ViewCount { get; }

        /// <summary>
        /// Loads all resources required by this scene.
        /// </summary>
        void Load();


        /// <summary>
        /// Unloads all resources owned by this scene.
        /// </summary>
        void Unload();


        /// <summary>
        /// Called when the scene becomes active.
        /// </summary>
        /// <param name="eventArgs">
        /// Provides information about the scene activation.
        /// </param>
        void Enter(SceneEnterEventArgs eventArgs);


        /// <summary>
        /// Called when the scene becomes inactive.
        /// </summary>
        void Exit();


        /// <summary>
        /// Updates the scene state and logic.
        /// </summary>
        /// <param name="context">
        /// Provides update timing and scene-related services.
        /// </param>
        void Update(SceneUpdateContext context);


        /// <summary>
        /// Renders the scene content.
        /// </summary>
        /// <param name="context">
        /// Provides rendering information, active views, cameras, and shaders.
        /// </param>
        void Draw(SceneDrawContext context);


        /// <summary>
        /// Creates a camera instance for a specific scene view.
        /// </summary>
        /// <param name="graphicsDevice">
        /// The graphics device used by the camera.
        /// </param>
        /// <param name="index">
        /// The zero-based view index.
        /// </param>
        /// <returns>
        /// A camera assigned to the specified view.
        /// </returns>
        ICamera CreateCamera(
            GraphicsDevice graphicsDevice,
            int index);


        /// <summary>
        /// Creates the shader instance used for rendering this scene.
        /// </summary>
        /// <param name="graphicsDevice">
        /// The graphics device used by the shader.
        /// </param>
        /// <returns>
        /// A shader instance for scene rendering.
        /// </returns>
        IShader CreateShader(
            GraphicsDevice graphicsDevice);
    }
}
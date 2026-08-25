using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Components;
using Sachssoft.Sasogine.Graphics.Cameras;
using Sachssoft.Sasogine.Graphics.Materials;
using Sachssoft.Sasogine.Graphics.Rendering;
using System;

namespace Sachssoft.Sasogine.Scenes
{
    /// <summary>
    /// Provides a basic implementation of a game scene.
    /// Handles scene lifecycle, component management, updating,
    /// rendering, and default camera/effect creation.
    /// </summary>
    public abstract class BasicSceneBase : IScene, ISceneRuntimeSettings
    {
        private readonly ComponentCollection _components = new();

        private GraphicsDevice? _graphicsDevice;
        private IGameApplication? _application;

        private bool _loaded;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicSceneBase"/> class.
        /// </summary>
        protected BasicSceneBase()
        {
        }

        /// <summary>
        /// Gets a value indicating whether this scene remains loaded
        /// when it is no longer active.
        /// </summary>
        public virtual bool IsPersistent => false;


        /// <summary>
        /// Gets the collection of components managed by this scene.
        /// </summary>
        public ComponentCollection Components => _components;


        /// <summary>
        /// Gets a value indicating whether the scene has been loaded.
        /// </summary>
        public bool IsLoaded => _loaded;


        /// <summary>
        /// Gets the number of views rendered by this scene.
        /// </summary>
        public int ViewCount { get; } = 1;

        /// <summary>
        /// Gets the runtime mode in which the scene is executed.
        /// </summary>
        public virtual RuntimeMode RuntimeMode { get; } = RuntimeMode.Game;

        /// <summary>
        /// Gets the optional runtime features enabled for the scene.
        /// </summary>
        public virtual RuntimeOptions RuntimeOptions { get; } = RuntimeOptions.None;

        /// <summary>
        /// Gets the game application associated with this scene.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the scene has not been initialized.
        /// </exception>
        public IGameApplication Application =>
            _application ?? throw new InvalidOperationException(
                "Scene not initialized.");


        /// <summary>
        /// Gets or sets the color used when clearing the render target.
        /// </summary>
        public Color BackgroundColor { get; set; } =
            Color.CornflowerBlue;


        /// <summary>
        /// Creates the camera used by a specific player in this scene.
        /// Override this method to provide custom camera behavior,
        /// such as player-specific cameras or split-screen views.
        /// </summary>
        /// <param name="graphicsDevice">
        /// The graphics device used by the camera.
        /// </param>
        /// <param name="index">
        /// The zero-based player index this camera belongs to.
        /// </param>
        /// <returns>
        /// A camera instance assigned to the specified player.
        /// </returns>
        public abstract ICamera CreateCamera(
            GraphicsDevice graphicsDevice,
            int index);


        /// <summary>
        /// Creates the default material used for rendering this scene.
        /// </summary>
        /// <param name="graphicsDevice">
        /// The graphics device used by the material.
        /// </param>
        /// <returns>
        /// A default diffuse material for scene rendering.
        /// </returns>
        public virtual IMaterial CreateDefaultMaterial(
            GraphicsDevice graphicsDevice)
        {
            var shader = new BasicShader
            {
                GraphicsDevice = graphicsDevice
            };

            return new DiffuseMaterial(shader);
        }


        /// <summary>
        /// Initializes the scene with the specified application instance.
        /// </summary>
        internal void EnsureInitialized(
            IGameApplication application)
        {
            if (_application != null)
                return;

            _application = application;
            _graphicsDevice = application.GraphicsDevice;
        }


        /// <summary>
        /// Loads all resources required by the scene.
        /// </summary>
        public virtual void Load()
        {
            if (_loaded)
                return;

            _loaded = true;
        }


        /// <summary>
        /// Releases all resources owned by the scene.
        /// </summary>
        public virtual void Unload()
        {
            if (!_loaded)
                return;

            _loaded = false;
        }


        /// <summary>
        /// Called when this scene becomes active.
        /// </summary>
        /// <param name="args">
        /// Provides information about the scene activation.
        /// </param>
        public virtual void Enter(
            SceneEnterEventArgs args)
        {
            EnsureInitialized(args.Application);
        }


        /// <summary>
        /// Called when this scene is no longer active.
        /// </summary>
        public virtual void Exit()
        {
        }


        /// <summary>
        /// Updates all scene components.
        /// </summary>
        /// <param name="context">
        /// Provides update information and active cameras.
        /// </param>
        public virtual void Update(
            SceneUpdateContext context)
        {
            _components.UpdateForEach(context);
        }


        /// <summary>
        /// Draws the scene and all registered components.
        /// </summary>
        /// <param name="context">
        /// Provides rendering information and active camera.
        /// </param>
        public virtual void Draw(
            SceneDrawContext context)
        {
            var gd = context.GraphicsDevice;

            gd.Clear(BackgroundColor);

            _components.DrawForEach(context);
        }


        /// <summary>
        /// Releases all resources used by this scene.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            if (_loaded)
                Unload();

            _graphicsDevice = null;
            _application = null;

            _disposed = true;
        }
    }
}
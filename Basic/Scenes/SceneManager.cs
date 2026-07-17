using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Graphics.Camera;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Scenes
{
    /// <summary>
    /// Manages scene lifecycle, scene transitions, updating, and rendering.
    /// Responsible for creating cameras, rendering contexts, and handling
    /// persistent scenes during the application lifetime.
    /// </summary>
    public sealed class SceneManager : ISceneManager
    {
        private readonly IGameApplication _application;
        private readonly IScene _mainScene;

        private readonly List<IScene> _persistentScenes = new();

        private SceneUpdateContext? _sceneUpdateContext;
        private SceneDrawContext[]? _sceneDrawContexts;

        private bool _isLoaded;
        private IScene _currentScene;


        /// <summary>
        /// Initializes a new instance of the <see cref="SceneManager"/> class.
        /// </summary>
        /// <param name="application">
        /// The application instance managed by this scene manager.
        /// </param>
        /// <param name="mainScene">
        /// The initial scene loaded when the application starts.
        /// </param>
        public SceneManager(
            IGameApplication application,
            IScene mainScene)
        {
            _application = application ??
                throw new ArgumentNullException(nameof(application));

            _mainScene = mainScene ??
                throw new ArgumentNullException(nameof(mainScene));

            _currentScene = _mainScene;
        }


        /// <summary>
        /// Gets a value indicating whether the scene manager has been loaded.
        /// </summary>
        public bool IsLoaded => _isLoaded;


        /// <summary>
        /// Gets the currently active scene.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the scene manager has not been loaded.
        /// </exception>
        public IScene CurrentScene =>
            _isLoaded
                ? _currentScene
                : throw new InvalidOperationException(
                    "SceneManager not loaded.");


        /// <summary>
        /// Gets all scenes that remain active and loaded across scene changes.
        /// </summary>
        public IEnumerable<IScene> ActiveScenes =>
            _persistentScenes.AsReadOnly();


        /// <summary>
        /// Gets a value indicating whether the current scene is the main scene.
        /// </summary>
        public bool IsStartScene =>
            ReferenceEquals(_currentScene, _mainScene);



        /// <summary>
        /// Loads the scene manager and initializes the main scene.
        /// </summary>
        public void Load()
        {
            if (_isLoaded)
                throw new InvalidOperationException(
                    "SceneManager already loaded.");

            InitializeScene(_currentScene);

            _isLoaded = true;
        }


        /// <summary>
        /// Initializes a scene by creating cameras, render contexts,
        /// and invoking lifecycle callbacks.
        /// </summary>
        /// <param name="scene">
        /// The scene to initialize.
        /// </param>
        private void InitializeScene(IScene scene)
        {
            var cameras = CreateCameras(scene);

            _sceneUpdateContext = new SceneUpdateContext(
                _application,
                scene,
                cameras);


            _sceneDrawContexts =
                new SceneDrawContext[scene.ViewCount];


            for (int i = 0; i < cameras.Length; i++)
            {
                _sceneDrawContexts[i] =
                    new SceneDrawContext(
                        _application,
                        scene,
                        cameras[i],
                        scene.CreateShader(
                            _application.GraphicsDevice),
                        i,
                        cameras.Length);
            }


            scene.Enter(CreateEventArgs());


            if (scene.IsPersistent &&
                !_persistentScenes.Contains(scene))
            {
                _persistentScenes.Add(scene);
            }


            scene.Load();
        }


        /// <summary>
        /// Creates all cameras required by the specified scene.
        /// Each camera belongs to a specific player or view index.
        /// </summary>
        /// <param name="scene">
        /// The scene requesting camera creation.
        /// </param>
        /// <returns>
        /// The cameras created for the scene.
        /// </returns>
        private ICamera[] CreateCameras(IScene scene)
        {
            var cameras =
                new ICamera[scene.ViewCount];


            for (int i = 0; i < scene.ViewCount; i++)
            {
                cameras[i] =
                    scene.CreateCamera(
                        _application.GraphicsDevice,
                        i);
            }


            return cameras;
        }


        /// <summary>
        /// Creates event arguments for scene activation.
        /// </summary>
        private SceneEnterEventArgs CreateEventArgs()
        {
            return new SceneEnterEventArgs(
                this,
                _application,
                _persistentScenes.AsReadOnly());
        }


        /// <summary>
        /// Ensures that the scene manager has been loaded.
        /// </summary>
        private void EnsureLoaded()
        {
            if (!_isLoaded)
            {
                throw new InvalidOperationException(
                    "SceneManager not loaded.");
            }
        }



        /// <summary>
        /// Changes the currently active scene.
        /// </summary>
        /// <param name="newScene">
        /// The scene that should become active.
        /// </param>
        public void ChangeScene(IScene newScene)
        {
            EnsureLoaded();

            if (newScene == null)
                throw new ArgumentNullException(nameof(newScene));


            if (ReferenceEquals(_currentScene, newScene))
            {
                throw new SceneException(
                    newScene,
                    "Cannot switch to same scene.");
            }


            SwitchScene(
                newScene,
                false);
        }



        /// <summary>
        /// Leaves the current scene and returns to the main scene.
        /// </summary>
        public void ExitCurrentScene()
        {
            EnsureLoaded();


            if (ReferenceEquals(_currentScene, _mainScene))
            {
                throw new SceneException(
                    _currentScene,
                    "Main scene cannot be exited.");
            }


            SwitchScene(
                _mainScene,
                true);
        }



        /// <summary>
        /// Performs a scene transition.
        /// Handles exit, unload, persistence, and initialization.
        /// </summary>
        private void SwitchScene(
            IScene nextScene,
            bool forceRemove)
        {
            _currentScene.Exit();


            if (!_currentScene.IsPersistent ||
                forceRemove)
            {
                _currentScene.Unload();
                _persistentScenes.Remove(_currentScene);
            }


            _currentScene = nextScene;

            InitializeScene(_currentScene);
        }



        /// <summary>
        /// Updates the current scene.
        /// </summary>
        /// <param name="gameTime">
        /// Timing information for the current frame.
        /// </param>
        public void Update(GameTime gameTime)
        {
            if (!_isLoaded ||
                _sceneUpdateContext == null)
            {
                return;
            }


            _sceneUpdateContext.SetFrameTime(gameTime);

            _currentScene.Update(
                _sceneUpdateContext);
        }



        /// <summary>
        /// Draws all views of the current scene.
        /// Each view uses its own camera and rendering context.
        /// </summary>
        /// <param name="gameTime">
        /// Timing information for the current frame.
        /// </param>
        public void Draw(GameTime gameTime)
        {
            if (!_isLoaded ||
                _sceneDrawContexts == null)
            {
                return;
            }


            for (int i = 0;
                 i < _currentScene.ViewCount;
                 i++)
            {
                _sceneDrawContexts[i]
                    .SetFrameTime(gameTime);

                _currentScene.Draw(
                    _sceneDrawContexts[i]);
            }
        }



        /// <summary>
        /// Requests application shutdown.
        /// </summary>
        public void ExitApplication()
        {
            _application.Exit();
        }
    }
}
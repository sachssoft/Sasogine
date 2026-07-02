using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Components.Rendering.Camera;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Scenes
{
    public sealed class SceneManager : ISceneManager
    {
        private readonly IGameApplication _application;
        private readonly IScene _mainScene;
        private readonly SceneConfiguration? _configuration;

        private readonly List<IScene> _persistentScenes = new();

        private SceneUpdateContext? _sceneUpdateContext;
        private SceneDrawContext[]? _sceneDrawContexts;

        private bool _isLoaded;
        private IScene _currentScene;

        public SceneManager(
            IGameApplication application,
            IScene mainScene,
            SceneConfiguration? configuration = null)
        {
            _application = application ?? throw new ArgumentNullException(nameof(application));
            _mainScene = mainScene ?? throw new ArgumentNullException(nameof(mainScene));
            _configuration = configuration;

            _currentScene = _mainScene;
        }

        public bool IsLoaded => _isLoaded;

        public IScene CurrentScene =>
            _isLoaded
                ? _currentScene
                : throw new InvalidOperationException("SceneManager not loaded.");

        public IEnumerable<IScene> ActiveScenes =>
            _persistentScenes.AsReadOnly();

        public bool IsStartScene => ReferenceEquals(_currentScene, _mainScene);

        // ---------------- INIT ----------------

        public void Load()
        {
            if (_isLoaded)
                throw new InvalidOperationException("SceneManager already loaded.");

            InitializeScene(_currentScene);

            _isLoaded = true;
        }

        private void InitializeScene(IScene scene)
        {
            var cameras = CreateCameras(scene);

            _sceneUpdateContext = new SceneUpdateContext(
                _application,
                scene,
                cameras
            );

            _sceneDrawContexts = new SceneDrawContext[scene.ViewCount];

            for (int i = 0; i < cameras.Length; i++)
            {
                _sceneDrawContexts[i] = new SceneDrawContext(
                    _application,
                    scene,
                    cameras[i],
                    scene.CreateEffectAdapter(_application.GraphicsDevice),
                    i,
                    cameras.Length
                );
            }

            scene.Enter(CreateEventArgs());

            if (scene.IsPersistent && !_persistentScenes.Contains(scene))
                _persistentScenes.Add(scene);

            scene.Load();
        }

        private ICamera[] CreateCameras(IScene scene)
        {
            var cameras = new ICamera[scene.ViewCount];
            for (int i = 0; i < scene.ViewCount; i++)
                cameras[i] = scene.CreateCamera(_application.GraphicsDevice, i);
            return cameras;
        }

        private SceneEnterEventArgs CreateEventArgs()
        {
            return new SceneEnterEventArgs(
                this,
                _application,
                _persistentScenes.AsReadOnly());
        }

        private void EnsureLoaded()
        {
            if (!_isLoaded)
                throw new InvalidOperationException("SceneManager not loaded.");
        }

        // ---------------- SCENE CONTROL ----------------

        public void ChangeScene(IScene newScene)
        {
            EnsureLoaded();

            if (newScene == null)
                throw new ArgumentNullException(nameof(newScene));

            if (ReferenceEquals(_currentScene, newScene))
                throw new SceneException(newScene, "Cannot switch to same scene.");

            SwitchScene(newScene, forceRemove: false);
        }

        public void ExitCurrentScene()
        {
            EnsureLoaded();

            if (ReferenceEquals(_currentScene, _mainScene))
                throw new SceneException(_currentScene, "Main scene cannot be exited.");

            SwitchScene(_mainScene, forceRemove: true);
        }

        private void SwitchScene(IScene nextScene, bool forceRemove)
        {
            // Exit current
            _currentScene.Exit();

            if (!_currentScene.IsPersistent || forceRemove)
            {
                _currentScene.Unload();
                _persistentScenes.Remove(_currentScene);
            }

            // Switch
            _currentScene = nextScene;

            InitializeScene(_currentScene);
        }

        // ---------------- UPDATE ----------------

        public void Update(GameTime gameTime)
        {
            if (!_isLoaded || _sceneUpdateContext == null)
                return;

            _sceneUpdateContext.SetFrameTime(gameTime);
            _currentScene.Update(_sceneUpdateContext);
        }

        // ---------------- DRAW ----------------

        public void Draw(GameTime gameTime)
        {
            if (!_isLoaded || _sceneDrawContexts == null)
                return;

            for (int i = 0; i < _currentScene.ViewCount; i++)
            {
                _sceneDrawContexts[i].SetFrameTime(gameTime);
                _currentScene.Draw(_sceneDrawContexts[i]);
            }
        }

        // ---------------- SYSTEM ----------------

        public void ExitApplication()
        {
            _application.Exit();
        }
    }
}
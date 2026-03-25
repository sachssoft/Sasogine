using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Scenes
{
    public sealed class SceneManager<TApplication> : ISceneManager
        where TApplication : GameApplication
    {
        private readonly TApplication _application;
        private readonly IScene _startScene;
        private readonly SceneConfiguration? _sceneConfiguration;

        private IScene _currentScene;
        private readonly List<IScene> _aliveScenes = new();

        private GameContext? _gameContext;
        private RuntimeContext? _runtimeContext;
        private PresentationContext? _presentationContext;

        public SceneManager(TApplication application, IScene startScene, SceneConfiguration? configuration = null)
        {
            _application = application ?? throw new ArgumentNullException(nameof(application));
            _startScene = startScene ?? throw new ArgumentNullException(nameof(startScene));
            _sceneConfiguration = configuration;

            _currentScene = _startScene;

            InitializeScene(_currentScene);
        }

        public IScene CurrentScene => _currentScene
            ?? throw new SceneException(null, "CurrentScene is not initialized.");

        public IEnumerable<IScene> ActiveScenes => _aliveScenes.AsReadOnly();

        public bool IsStartScene => ReferenceEquals(_currentScene, _startScene);

        private SceneContext CreateContext()
        {
            return new SceneContext
            {
                Application = _application,
                ActiveScenes = _aliveScenes.AsReadOnly()
            };
        }

        private void InitializeScene(IScene scene)
        {
            _gameContext = _sceneConfiguration?.ConfigureGameContext?.Invoke(_application, scene) ??
                    new GameContext(_application);
            _runtimeContext = null;

            // Wenn Szene Runtime hat, initialisieren
            if (scene is ISceneWithRuntime logicScene)
            {
                if (logicScene.Runtime == null)
                    throw new SceneException(logicScene, "Runtime is not initialized for this scene.");

                _runtimeContext = _sceneConfiguration?.ConfigureRuntimeContext?.Invoke(_application, logicScene) ??
                    new RuntimeContext(_application, logicScene.Runtime);

                logicScene.Runtime.EnsureInitialized(_application);
                logicScene.Runtime.Load();
            }

            // Wenn Szene Presentation hat, initialisieren
            if (scene is ISceneWithPresentation viewScene)
            {
                if (viewScene.Presentation == null)
                    throw new SceneException(viewScene, "Presentation is not initialized for this scene.");

                _presentationContext = _sceneConfiguration?.ConfigurePresentationContext?.Invoke(_application, viewScene) ??
                    new PresentationContext(_application, viewScene);

                viewScene.Presentation.Load();
            }

            scene.Load();
            scene.OnEnter(CreateContext());

            if (scene.KeepAlive && !_aliveScenes.Contains(scene))
                _aliveScenes.Add(scene);
        }

        public void ChangeScene(IScene newScene)
        {
            if (newScene == null)
                throw new ArgumentNullException(nameof(newScene));

            if (ReferenceEquals(_currentScene, newScene))
                throw new SceneException(newScene, "Cannot change to the same scene.");

            ExitCurrentScene();

            _currentScene = newScene;
            InitializeScene(_currentScene);
        }

        public void ExitCurrentScene()
        {
            if (_currentScene == null)
                throw new SceneException(null, "No active scene to exit.");

            if (ReferenceEquals(_currentScene, _startScene))
                throw new SceneException(_currentScene, "The start scene cannot be exited.");

            if (_currentScene.KeepAlive)
                throw new SceneException(_currentScene, "Scene cannot be exited because KeepAlive is enabled.");

            _currentScene.OnExit();
            _currentScene.Unload();

            if (_currentScene is ISceneWithRuntime logicScene)
                logicScene.Runtime.Unload();

            if (_currentScene is ISceneWithPresentation viewScene)
                viewScene.Presentation.Unload();

            _currentScene = _startScene;
            InitializeScene(_currentScene);
        }

        public void Update(GameTime gameTime)
        {
            // 1️) Backend / Runtime zuerst
            if (_currentScene is ISceneWithRuntime logicScene)
            {
                _runtimeContext!.Update(gameTime);
                logicScene.Runtime.Update(_runtimeContext);
            }

            // 2️) Szene selbst
            _gameContext!.Update(gameTime);
            _currentScene?.Update(_gameContext);

            // 3) Frontend / Präsensation zuetzt
            if (_currentScene is ISceneWithPresentation viewScene)
            {
                _presentationContext!.Update(gameTime);
                viewScene.Presentation.Update(_presentationContext);
            }
        }

        public void Draw(GameTime gameTime)
        {
            // 1) Backend / Runtime zuerst
            if (_currentScene is ISceneWithRuntime logicScene)
            {
                _runtimeContext!.Update(gameTime);
                logicScene.Runtime.Draw(_runtimeContext);
            }

            // 2️) Szene selbst
            _gameContext!.Update(gameTime);
            _currentScene?.Draw(_gameContext);

            // 3) Frontend / Präsensation zuetzt
            if (_currentScene is ISceneWithPresentation viewScene)
            {
                _presentationContext!.Update(gameTime);
                viewScene.Presentation.Draw(_presentationContext);
            }
        }

        public void ExitApplication()
        {
            _application.Exit();
        }
    }
}
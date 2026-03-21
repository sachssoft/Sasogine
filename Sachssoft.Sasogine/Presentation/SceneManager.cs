using Microsoft.Xna.Framework;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Sachssoft.Sasogine.Presentation
{
    public sealed class SceneManager
    {
        private ImmutableDictionary<Type, (Func<SceneBase> Factory, SceneBase? Instance)> _scenes
            = ImmutableDictionary<Type, (Func<SceneBase>, SceneBase?)>.Empty;

        private readonly GameApplication _application;
        private readonly IPresentationHost _presentationHost;

        private PresentationContext? _sceneContext;
        private volatile bool _renderable = true;

        private Type? _defaultSceneType;
        private Action<SceneBase>? _defaultSceneInit;

        public SceneManager(GameApplication app, IPresentationHost host)
        {
            _application = app ?? throw new ArgumentNullException(nameof(app));
            _presentationHost = host ?? throw new ArgumentNullException(nameof(host));
        }

        public bool IsRenderable => _renderable;

        public GameApplication Application => _application;

        public void Load()
        {
            var currentScene = _presentationHost.Scene;
            if (currentScene == null)
            {
                if (_defaultSceneType == null)
                    throw new GameException("No default scene was set.");

                OpenInternal(_defaultSceneType, _defaultSceneInit);
            }
        }

        public void Update(GameTime gameTime)
        {
            var scene = _presentationHost.Scene;
            if (scene == null)
            {
                _renderable = false;
                return;
            }

            EnsureSceneContext();

            _renderable = scene.CanRender(_sceneContext);

            if (_renderable)
            {
                _sceneContext.Update(gameTime);
                scene.OnUpdate(_sceneContext);
            }
        }

        public void Draw(GameTime gameTime)
        {
            var scene = _presentationHost.Scene;
            if (!_renderable || scene == null)
                return;

            EnsureSceneContext();

            scene.OnDrawContent(_sceneContext);

            if (_sceneContext!.IsVisible)
            {
                _sceneContext.Update(gameTime);
                _presentationHost.Render(_sceneContext);
            }

            scene.OnDrawOverlay(_sceneContext);
        }

        public void Register<TScene>() where TScene : SceneBase, new()
            => RegisterScene(() => new TScene());

        public void RegisterScene<TScene>(Func<TScene> factory) where TScene : SceneBase
        {
            var type = typeof(TScene);
            var snapshot = _scenes;

            if (snapshot.ContainsKey(type))
                throw new InvalidOperationException($"Scene '{type.Name}' is already registered.");

            Interlocked.Exchange(ref _scenes, snapshot.Add(type, (() => factory(), null)));
        }

        public void SetDefault<TScene>(Action<TScene>? init = null) where TScene : SceneBase
        {
            _defaultSceneType = typeof(TScene);
            _defaultSceneInit = init != null ? s => init((TScene)s) : null;
        }

        public void Open<TScene>(Action<TScene>? init = null) where TScene : SceneBase
            => OpenInternal(typeof(TScene), init != null ? s => init((TScene)s) : null);

        public void Close(SceneBase scene)
        {
            var type = scene.GetType();
            if (_scenes.TryGetValue(type, out var item) && item.Instance == scene)
                CloseInternal(type, _defaultSceneType, SceneSwitchMode.Reload, _defaultSceneInit);
            else
                throw new InvalidOperationException("The given scene is not the current instance.");
        }

        private void CloseInternal(Type closeType, Type? nextType, SceneSwitchMode nextSwitchMode, Action<SceneBase>? nextInit)
        {
            if (!_scenes.TryGetValue(closeType, out var item) || item.Instance == null)
                return;

            var scene = item.Instance;
            if (_presentationHost.Scene == scene)
                _presentationHost.Scene = null;

            UnloadScene(scene);

            Interlocked.Exchange(ref _scenes, _scenes.SetItem(closeType, (item.Factory, null)));

            _renderable = false;

            if (nextType != null)
                SwitchInternal(nextType, previousSwitchMode: SceneSwitchMode.Reload, nextSwitchMode: nextSwitchMode, init: nextInit);
        }

        public void Switch<TScene>(SceneSwitchMode switchMode = SceneSwitchMode.Reload, Action<TScene>? init = null) where TScene : SceneBase
            => SwitchInternal(typeof(TScene), previousSwitchMode: switchMode, nextSwitchMode: switchMode, init != null ? s => init((TScene)s) : null);

        private void SwitchInternal(Type type, SceneSwitchMode previousSwitchMode, SceneSwitchMode nextSwitchMode, Action<SceneBase>? init)
        {
            if (!_scenes.TryGetValue(type, out var item))
                throw new InvalidOperationException($"Scene '{type.Name}' is not registered.");

            var scene = item.Instance ?? item.Factory();
            var oldScene = _presentationHost.Scene;

            if (ReferenceEquals(oldScene, scene))
                return;

            if (oldScene != null)
            {
                oldScene.IsActive = false;
                if (previousSwitchMode == SceneSwitchMode.Reload)
                {
                    UnloadScene(oldScene);
                    Interlocked.Exchange(ref _scenes, _scenes.SetItem(oldScene.GetType(), (_scenes[oldScene.GetType()].Factory, null)));
                }
            }

            scene.Scenes = this;
            Interlocked.Exchange(ref _scenes, _scenes.SetItem(type, (item.Factory, scene)));
            _presentationHost.Scene = scene;

            EnsureLoaded(scene);

            init?.Invoke(scene);
            scene.IsActive = true;
        }

        private void OpenInternal(Type type, Action<SceneBase>? init)
        {
            if (!_scenes.TryGetValue(type, out var item))
                throw new InvalidOperationException($"Scene '{type.Name}' is not registered.");

            if (item.Instance != null)
                throw new InvalidOperationException($"Scene '{type.Name}' is already open.");

            var scene = item.Factory();

            scene.Scenes = this;
            Interlocked.Exchange(ref _scenes, _scenes.SetItem(type, (item.Factory, scene)));
            _presentationHost.Scene = scene;

            EnsureLoaded(scene);

            init?.Invoke(scene);
            scene.IsActive = true;
        }

        [MemberNotNull(nameof(_sceneContext))]
        private void EnsureSceneContext()
        {
            var scene = _presentationHost.Scene ?? throw new InvalidOperationException("No scene is active in the host.");
            if (_sceneContext?.Scene != scene)
                _sceneContext = new PresentationContext(_application, scene);
        }

        private void EnsureLoaded(SceneBase scene)
        {
            if (!scene.IsLoaded)
            {
                scene.OnLoad();
                scene.IsLoaded = true;
            }
        }

        private void UnloadScene(SceneBase scene)
        {
            if (scene.IsLoaded)
            {
                scene.OnUnload();
                scene.IsLoaded = false;
            }

            scene.IsActive = false;
            scene.Scenes = null!; // mark scene as detached
        }
    }
}

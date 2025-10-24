using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Interactions;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Presentation
{
    /// <summary>
    /// Manages the lifecycle and switching of scenes within the game.
    /// </summary>
    public sealed class SceneManager
    {
        private readonly Dictionary<Type, (Func<SceneBase> Factory, SceneBase? Instance)> _scenes = new();
        private readonly IHost _host;

        private Type? _defaultSceneType;
        private Action<SceneBase>? _defaultSceneInit;

        private bool _renderable;

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneManager"/> class.
        /// </summary>
        /// <param name="host">The surface host that contains the current scene.</param>
        public SceneManager(IHost host)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
        }

        /// <summary>
        /// Gets a value indicating whether the current scene can be rendered.
        /// </summary>
        public bool IsRenderable => _renderable;

        /// <summary>
        /// Loads the default scene if no scene is currently active.
        /// </summary>
        public void Load()
        {
            if (_host.Scene == null)
            {
                if (_defaultSceneType == null)
                    throw new GameException("No default scene was set.");

                OpenInternal(_defaultSceneType, _defaultSceneInit);
            }
        }

        /// <summary>
        /// Updates the active scene and executes additional per-frame logic.
        /// </summary>
        /// <param name="gameTime">The current game time.</param>
        /// <param name="onUpdate">Action to execute per frame with the <see cref="GameFrameContext"/>.</param>
        public void Update(GameTime gameTime, Action<GameFrameContext> onUpdate)
        {
            var scene = _host.Scene;
            var context = new GameFrameContext(scene, gameTime);
            _renderable = scene?.CanRender(context) ?? false;

            if (_renderable)
            {
                scene?.OnUpdate(context);
                onUpdate(context);
            }
        }

        /// <summary>
        /// Draws the active scene and executes pre/post rendering logic.
        /// </summary>
        /// <param name="gameTime">The current game time.</param>
        /// <param name="onBeforeSurface">Action executed before the surface is rendered.</param>
        /// <param name="onAfterSurface">Action executed after the surface is rendered.</param>
        public void Draw(GameTime gameTime, Action<GameFrameContext> onBeforeSurface, Action<GameFrameContext> onAfterSurface)
        {
            if (!_renderable || _host.Scene == null)
                return;

            var scene = _host.Scene;
            var context = new GameFrameContext(scene, gameTime);

            scene.OnDraw(context);
            onBeforeSurface(context);

            if (context.IsUIVisibled)
            {
                _host.Render(context);
            }

            onAfterSurface(context);
            scene.OnDrawAfterGUI(context);
        }

        /// <summary>
        /// Registers a scene type with a default constructor.
        /// </summary>
        public void Register<TScene>() where TScene : SceneBase, new()
            => RegisterScene(() => new TScene());

        /// <summary>
        /// Registers a scene type with a custom factory method.
        /// </summary>
        public void RegisterScene<TScene>(Func<TScene> factory) where TScene : SceneBase
        {
            var type = typeof(TScene);
            if (_scenes.ContainsKey(type))
                throw new InvalidOperationException($"Scene '{type.Name}' is already registered.");

            _scenes[type] = (() => factory(), null);
        }

        /// <summary>
        /// Sets the default scene to load if no scene is active.
        /// </summary>
        public void SetDefault<TScene>(Action<TScene>? init = null) where TScene : SceneBase
        {
            _defaultSceneType = typeof(TScene);
            _defaultSceneInit = init != null ? s => init((TScene)s) : null;
        }

        /// <summary>
        /// Opens a scene and optionally initializes it.
        /// </summary>
        public void Open<TScene>(Action<TScene>? init = null) where TScene : SceneBase
            => OpenInternal(typeof(TScene), init != null ? s => init((TScene)s) : null);

        /// <summary>
        /// Closes the specified scene type and optionally switches to the default scene.
        /// </summary>
        public void Close<TScene>() where TScene : SceneBase
            => CloseInternal(typeof(TScene), _defaultSceneType, SceneSwitchMode.Reload, _defaultSceneInit);

        /// <summary>
        /// Closes a specific scene instance.
        /// </summary>
        public void Close(SceneBase scene)
        {
            if (_scenes.TryGetValue(scene.GetType(), out var item) && item.Instance == scene)
            {
                CloseInternal(scene.GetType(), _defaultSceneType, SceneSwitchMode.Reload, _defaultSceneInit);
            }
            else
            {
                throw new InvalidOperationException("The given scene is not the current instance.");
            }
        }

        /// <summary>
        /// Closes a scene and optionally opens another scene with a specified switch mode.
        /// </summary>
        public void Close<TScene, TNextScene>(SceneSwitchMode nextSwitchMode = SceneSwitchMode.Reload, Action<TNextScene>? nextInit = null)
            where TScene : SceneBase
            where TNextScene : SceneBase
        {
            CloseInternal(typeof(TScene), typeof(TNextScene), nextSwitchMode,
                nextInit != null ? s => nextInit((TNextScene)s) : null);
        }

        /// <summary>
        /// Switches to another scene with optional initialization and switch modes.
        /// </summary>
        public void Switch<TScene>(
            SceneSwitchMode previousSwitchMode = SceneSwitchMode.Reload,
            SceneSwitchMode nextSwitchMode = SceneSwitchMode.Reload,
            Action<TScene>? init = null) where TScene : SceneBase
            => SwitchInternal(typeof(TScene), previousSwitchMode, nextSwitchMode, init != null ? s => init((TScene)s) : null);

        /// <summary>
        /// Checks if a scene type is currently open.
        /// </summary>
        public bool IsOpen<TScene>() where TScene : SceneBase
            => _scenes.TryGetValue(typeof(TScene), out var item) && item.Instance != null;

        /// <summary>
        /// Checks if the specified scene type is currently active.
        /// </summary>
        public bool IsActive<TScene>() where TScene : SceneBase
            => _host.Scene is TScene;

        private void CloseInternal(Type closeType, Type? nextType, SceneSwitchMode nextSwitchMode = SceneSwitchMode.Reload, Action<SceneBase>? nextInit = null)
        {
            if (!_scenes.TryGetValue(closeType, out var item) || item.Instance == null)
                return;

            var scene = item.Instance;

            if (_host.Scene == scene)
                _host.Scene = null;

            UnloadScene(scene);

            _scenes[closeType] = (item.Factory, null);

            if (nextType != null)
                SwitchInternal(nextType, previousSwitchMode: SceneSwitchMode.Reload, nextSwitchMode: nextSwitchMode, init: nextInit);
        }

        private void SwitchInternal(Type type, SceneSwitchMode previousSwitchMode = SceneSwitchMode.Reload, SceneSwitchMode nextSwitchMode = SceneSwitchMode.Reload, Action<SceneBase>? init = null)
        {
            if (!_scenes.TryGetValue(type, out var item))
                throw new InvalidOperationException($"Scene '{type.Name}' is not registered.");

            var scene = item.Instance ?? item.Factory();
            var oldScene = _host.Scene;

            if (oldScene != null)
            {
                oldScene.IsActive = false;

                if (previousSwitchMode == SceneSwitchMode.Reload)
                {
                    UnloadScene(oldScene);
                    _scenes[oldScene.GetType()] = (_scenes[oldScene.GetType()].Factory, null);
                }
            }

            _scenes[type] = (item.Factory, scene);
            _host.Scene = scene;

            var context = new GameBaseContext(scene);
            EnsureLoaded(scene, context);

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
            _scenes[type] = (item.Factory, scene);
            _host.Scene = scene;

            var context = new GameBaseContext(scene);
            EnsureLoaded(scene, context);

            init?.Invoke(scene);
            scene.IsActive = true;
        }

        private static void EnsureLoaded(SceneBase scene, GameBaseContext context)
        {
            if (!scene.IsLoaded)
            {
                scene.OnLoadPrepareInternal();
                scene.OnLoad(context);
                scene.IsLoaded = true;
            }
        }

        private static void UnloadScene(SceneBase scene)
        {
            if (scene.IsLoaded)
            {
                scene.OnUnload();
                scene.IsLoaded = false;
            }
            scene.IsActive = false;
        }
    }
}

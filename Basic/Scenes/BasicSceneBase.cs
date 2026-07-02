using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Components;
using Sachssoft.Sasogine.Components.Rendering.Camera;
using Sachssoft.Sasogine.Graphics.Rendering;
using System;

namespace Sachssoft.Sasogine.Scenes
{
    public abstract class BasicSceneBase : IScene
    {
        private readonly ComponentCollection _components = new();

        private GraphicsDevice? _graphicsDevice;
        private IGameApplication? _application;

        private bool _loaded;
        private bool _disposed;

        public virtual bool IsPersistent => false;

        public ComponentCollection Components => _components;

        public bool IsLoaded => _loaded;

        public int ViewCount { get; } = 1;

        public IGameApplication Application =>
            _application ?? throw new InvalidOperationException("Scene not initialized.");

        public Color BackgroundColor { get; set; } = Color.CornflowerBlue;

        public abstract ICamera CreateCamera(GraphicsDevice graphicsDevice);

        ICamera IScene.CreateCamera(GraphicsDevice graphicsDevice, int index)
        {
            return CreateCamera(graphicsDevice);
        }

        public virtual IEffectAdapter CreateEffectAdapter(GraphicsDevice graphicsDevice)
        {
            return new BasicEffectAdapter
            {
                GraphicsDevice = graphicsDevice
            };
        }

        // ---------------- INIT ----------------

        //SceneContext IScene.ConfigureContext(IGameApplication application)
        //{
        //    return new SceneContext(
        //        application: application,
        //        scene: this,
        //        camera: CreateCamera(application.GraphicsDevice),
        //        effectAdapter: CreateDefaultEffectAdapter(application.GraphicsDevice)
        //    );
        //}

        internal void EnsureInitialized(IGameApplication application)
        {
            if (_application != null)
                return;

            _application = application;
            _graphicsDevice = application.GraphicsDevice;
        }

        // ---------------- LIFECYCLE ----------------

        public virtual void Load()
        {
            if (_loaded)
                return;

            _loaded = true;
        }

        public virtual void Unload()
        {
            if (!_loaded)
                return;

            _loaded = false;
        }

        public virtual void Enter(SceneEnterEventArgs args)
        {
            EnsureInitialized(args.Application);
        }

        public virtual void Exit() { }

        // ---------------- UPDATE ----------------

        public virtual void Update(SceneUpdateContext context)
        {
            _components.UpdateForEach(context);
        }

        // ---------------- DRAW ----------------

        public virtual void Draw(SceneDrawContext context)
        {
            var gd = context.GraphicsDevice;
            gd.Clear(BackgroundColor);

            _components.DrawForEach(context);
        }

        // ---------------- DISPOSE ----------------

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
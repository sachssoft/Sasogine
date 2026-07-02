//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using Sachssoft.Sasogine.Components;
//using Sachssoft.Sasogine.Components.Rendering.Camera;
//using Sachssoft.Sasogine.Graphics.Rendering;
//using System;

//namespace Sachssoft.Sasogine.Scenes
//{
//    public abstract class ViewportSceneBase : IScene
//    {
//        private readonly ComponentCollection _components = new();
//        private readonly int _viewportCount;
//        private readonly SceneViewportContext[] _sceneViewportContexts;

//        private GraphicsDevice? _graphicsDevice;

//        private RenderTarget2D[] _sceneRenderTargets = Array.Empty<RenderTarget2D>();
//        private SpriteBatch? _spriteBatch;

//        private bool _wasInitialized;
//        private bool _isLoaded;
//        private bool _disposed;

//        public const int MaxViewports = 4;

//        public ViewportSceneBase(int viewportCount = 1)
//        {
//            if (viewportCount < 1 || viewportCount > MaxViewports)
//                throw new ArgumentException("ViewportCount must be 1..4");

//            _viewportCount = viewportCount;
//            _sceneViewportContexts = new SceneViewportContext[_viewportCount];
//            _sceneRenderTargets = new RenderTarget2D[_viewportCount];
//        }

//        public bool IsPersistent => throw new NotImplementedException();

//        public int ViewportCount => _viewportCount;

//        public ICamera PrimaryCamera => _sceneViewportContexts[0].Camera;

//        public IEffectAdapter PrimaryEffectAdapter => _sceneViewportContexts[0].EffectAdapter;

//        public Color BackgroundColor { get; set; } = Color.CornflowerBlue;

//        public ComponentCollection Components => _components;

//        public bool IsLoaded => _isLoaded;

//        // -------------------------------------------------
//        // ABSTRACT
//        // -------------------------------------------------
//        public abstract ICamera CreateCamera(GraphicsDevice graphicsDevice);

//        public virtual IEffectAdapter CreateDefaultEffectAdapter(GraphicsDevice graphicsDevice)
//            => new BasicEffectAdapter { GraphicsDevice = graphicsDevice };

//        // -------------------------------------------------
//        // ENGINE INIT
//        // -------------------------------------------------
//        internal void EnsureInitialized(IGameApplication application)
//        {
//            if (_wasInitialized)
//                return;

//            _graphicsDevice = application.GraphicsDevice;

//            for (int i = 0; i < _viewportCount; i++)
//            {
//                _sceneViewportContexts[i] = new SceneViewportContext(
//                    application, 
//                    this, 
//                    i,
//                    camera: CreateCamera(_graphicsDevice),
//                    effectAdapter: CreateDefaultEffectAdapter(_graphicsDevice));
//            }

//            _wasInitialized = true;
//        }

//        // -------------------------------------------------
//        // LOAD / UNLOAD
//        // -------------------------------------------------
//        public virtual void Load()
//        {
//            if (!_wasInitialized)
//                throw new InvalidOperationException("Runtime not initialized.");

//            if (_isLoaded)
//                throw new InvalidOperationException("Runtime already loaded.");

//            _spriteBatch = new SpriteBatch(_graphicsDevice);

//            CreateRenderTargets();

//            _isLoaded = true;
//        }

//        public virtual void Unload()
//        {
//            if (!_isLoaded)
//                throw new InvalidOperationException("Runtime not loaded.");

//            DisposeRenderTargets();

//            _spriteBatch?.Dispose();
//            _spriteBatch = null;

//            _isLoaded = false;
//        }

//        public abstract void Enter(SceneEnterEventArgs eventArgs);

//        public abstract void Exit();

//        // -------------------------------------------------
//        // UPDATE (STATE ONLY)
//        // -------------------------------------------------
//        public virtual void Update(SceneContext context)
//        {
//            EnsureReady();

//            for (int i = 0; i < _viewportCount; i++)
//            {
//                _sceneViewportContexts[i].Camera.Update(context);
//                _components.UpdateForEach(_sceneViewportContexts[i]);
//            }
//        }

//        // -------------------------------------------------
//        // DRAW (RENDER ONLY)
//        // -------------------------------------------------
//        public virtual void Draw(SceneContext context)
//        {
//            EnsureReady();

//            var gd = context.GraphicsDevice;

//            if (gd.IsDisposed || gd.GraphicsDeviceStatus != GraphicsDeviceStatus.Normal)
//                return;

//            EnsureRenderTargetSize();

//            var viewports = GetSplitViewports(gd.Viewport.Bounds, _viewportCount);

//            // -------------------------
//            // Render Pass
//            // -------------------------
//            for (int i = 0; i < _viewportCount; i++)
//            {
//                gd.SetRenderTarget(_sceneRenderTargets[i]);
//                gd.Clear(BackgroundColor);

//                OnViewportRender(_sceneViewportContexts[i]);
//            }

//            // -------------------------
//            // Present Pass
//            // -------------------------
//            gd.SetRenderTarget(null);
//            gd.Clear(Color.Black);

//            _spriteBatch!.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

//            for (int i = 0; i < _viewportCount; i++)
//            {
//                _spriteBatch.Draw(
//                    _sceneRenderTargets[i],
//                    viewports[i],
//                    Color.White
//                );
//            }

//            _spriteBatch.End();

//            OnRenderPost(context);
//        }

//        // -------------------------------------------------
//        // VIEWPORT ACCESS
//        // -------------------------------------------------
//        public Rectangle GetSplitViewport(int index)
//        {
//            EnsureReady();

//            var viewports = GetSplitViewports(
//                _graphicsDevice!.Viewport.Bounds,
//                ViewportCount
//            );

//            if (index < 0 || index >= viewports.Length)
//                throw new ArgumentOutOfRangeException(nameof(index));

//            return viewports[index];
//        }

//        public Rectangle[] GetSplitViewports(Rectangle bounds, int count)
//        {
//            int w = bounds.Width;
//            int h = bounds.Height;

//            return count switch
//            {
//                1 => new[] { new Rectangle(0, 0, w, h) },

//                2 => new[]
//                {
//                new Rectangle(0, 0, w, h / 2),
//                new Rectangle(0, h / 2, w, h / 2)
//            },

//                3 => new[]
//                {
//                new Rectangle(0, 0, w / 2, h / 2),
//                new Rectangle(w / 2, 0, w / 2, h / 2),
//                new Rectangle(0, h / 2, w, h / 2)
//            },

//                4 => new[]
//                {
//                new Rectangle(0, 0, w / 2, h / 2),
//                new Rectangle(w / 2, 0, w / 2, h / 2),
//                new Rectangle(0, h / 2, w / 2, h / 2),
//                new Rectangle(w / 2, h / 2, w / 2, h / 2)
//            },

//                _ => throw new NotSupportedException()
//            };
//        }

//        // -------------------------------------------------
//        // HOOKS
//        // -------------------------------------------------
//        protected virtual void OnViewportRender(SceneViewportContext context)
//        {
//            _components.DrawForEach(context);
//        }

//        protected virtual void OnRenderPost(GameContext context) { }

//        // -------------------------------------------------
//        // INTERNAL HELPERS
//        // -------------------------------------------------
//        private void EnsureReady()
//        {
//            if (!_wasInitialized || !_isLoaded || _graphicsDevice == null)
//                throw new InvalidOperationException("Runtime not ready.");
//        }

//        private void CreateRenderTargets()
//        {
//            if (_graphicsDevice == null)
//                return;

//            int w = _graphicsDevice.PresentationParameters.BackBufferWidth;
//            int h = _graphicsDevice.PresentationParameters.BackBufferHeight;

//            for (int i = 0; i < _viewportCount; i++)
//            {
//                _sceneRenderTargets[i]?.Dispose();

//                _sceneRenderTargets[i] = new RenderTarget2D(
//                    _graphicsDevice,
//                    w,
//                    h,
//                    false,
//                    _graphicsDevice.PresentationParameters.BackBufferFormat,
//                    DepthFormat.None
//                );
//            }
//        }

//        private void EnsureRenderTargetSize()
//        {
//            if (_graphicsDevice == null)
//                return;

//            int w = _graphicsDevice.PresentationParameters.BackBufferWidth;
//            int h = _graphicsDevice.PresentationParameters.BackBufferHeight;

//            for (int i = 0; i < _viewportCount; i++)
//            {
//                if (_sceneRenderTargets[i] != null &&
//                    _sceneRenderTargets[i].Width == w &&
//                    _sceneRenderTargets[i].Height == h)
//                    continue;

//                _sceneRenderTargets[i]?.Dispose();

//                _sceneRenderTargets[i] = new RenderTarget2D(
//                    _graphicsDevice,
//                    w,
//                    h,
//                    false,
//                    _graphicsDevice.PresentationParameters.BackBufferFormat,
//                    DepthFormat.None
//                );
//            }
//        }

//        private void DisposeRenderTargets()
//        {
//            foreach (var rt in _sceneRenderTargets)
//                rt?.Dispose();

//            _sceneRenderTargets = Array.Empty<RenderTarget2D>();
//        }

//        // -------------------------------------------------
//        // DISPOSE
//        // -------------------------------------------------
//        public void Dispose()
//        {
//            if (_disposed)
//                return;

//            if (_isLoaded)
//                Unload();

//            _disposed = true;
//        }
//    }
//}

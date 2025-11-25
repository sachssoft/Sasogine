using System;
using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Presentation
{
    /// <summary>
    /// Base class for all scenes in the presentation layer.
    /// Supports switching runtime at any time (auto unload/load).
    /// Runtime may be null.
    /// </summary>
    public abstract class SceneBase : IDisposeManagerProvider
    {
        private IPresentationHostElement? _container;
        private bool _isActive;
        private SceneManager? _scenes;
        private GameApplication? _application;

        protected SceneBase()
        {
        }

        public GameApplication Application
        {
            get => _application ?? throw new InvalidOperationException("Scene is not attached to a GameApplication.");
        }

        public IPresentationHostElement? Container
        {
            get => _container;
            protected set => _container = value;
        }

        public DisposeManager DisposeManager { get; } = new DisposeManager();

        public SceneManager Scenes
        {
            get => _scenes ?? throw new InvalidOperationException("Scenes is not set.");
            internal set
            {
                _scenes = value;
                _application = _scenes.Application;
            }
        }

        public IPresentationHost? Host => Container?.Host;

        public bool IsActive
        {
            get => _isActive;
            internal set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    if (_isActive)
                        OnActivated();
                    else
                        OnDeactivated();
                }
            }
        }

        public bool IsLoaded { get; internal set; }

        protected void Leave()
        {
            if (!IsLoaded)
                return;

            IsActive = false;
            OnLeft();
        }

        /// <summary>
        /// Default implementation closes this scene via the SceneManager.
        /// </summary>
        protected virtual void OnLeft()
        {
            _scenes?.Close(this);
        }

        internal protected virtual void OnActivated() { }

        internal protected virtual void OnDeactivated() { }

        internal protected virtual void OnLoad()
        {
            IsLoaded = true;
        }

        internal protected virtual void OnUnload()
        {
            DisposeManager.Dispose();
            IsLoaded = false;
        }

        internal protected virtual void OnClientSizeChanged() { }

        internal virtual bool CanRender(PresentationContext context) => true;

        internal protected virtual void OnUpdate(PresentationContext context)
        {
        }

        internal protected virtual void OnDrawContent(PresentationContext context)
        {
        }

        internal protected virtual void OnDrawOverlay(PresentationContext context) { }

    }
}

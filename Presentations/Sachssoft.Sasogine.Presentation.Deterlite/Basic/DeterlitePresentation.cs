using System;
using Sachssoft.Sasogine.Common.Performance;
using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Presentation.Deterlite
{
    public abstract class DeterlitePresentation : IPresentation
    {
        private readonly IGameApplication _application;
        private readonly IScene _scene;
        private readonly DirectLazy<Workspace> _workspace;
        private readonly IClientResizeAware? _resizeAware;

        private bool _loaded;

        public DeterlitePresentation(IGameApplication application, IScene scene)
        {
            _application = application;
            _scene = scene;
            _workspace = new DirectLazy<Workspace>(() => new Workspace(application), threadSafe: false);

            _resizeAware = _scene as IClientResizeAware;
        }

        public IScene Scene => _scene;

        public IGameApplication Application => _application;

        public Workspace Workspace => _workspace.Value;

        public virtual void Load()
        {
            if (_loaded)
                return;

            // Laden z.B. Font
            OnLayout();
            _workspace.Value.Invalidate();

            _loaded = true;
        }

        public virtual void Unload()
        {
            if (_workspace.IsValueCreated)
                _workspace.Value.Dispose();
        }

        public virtual void Update(PresentationContext context)
        {
            if (_resizeAware != null && _resizeAware.WasClientResize)
                _workspace.Value.EnsureClientResize();

            _workspace.Value.Update(context.GameTime);
        }

        public virtual void Draw(PresentationContext context)
        {
            if (!context.IsVisible)
                return;

            _workspace.Value.Draw(context.GameTime);
            OnRender();
        }

        protected virtual void OnLayout() { }

        protected virtual void OnRender() { }
    }
}

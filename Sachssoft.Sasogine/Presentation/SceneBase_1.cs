using System;
using System.Diagnostics.CodeAnalysis;
using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Presentation
{
    /// <summary>
    /// Base class for all scenes in the presentation layer.
    /// Supports switching runtime at any time (auto unload/load).
    /// Runtime may be null.
    /// </summary>
    public abstract class SceneBase<TRuntime> : SceneBase where TRuntime : RuntimeBase
    {
        [AllowNull] private TRuntime _runtime;
        private RuntimeContext? _runtimeContext;

        protected SceneBase() : base()
        {
        }

        /// <summary>
        /// Gets or sets the runtime for this scene.
        /// Runtime may be null.
        /// If changed while the scene is loaded,
        /// the old runtime is unloaded and the new one loaded automatically.
        /// </summary>
        public TRuntime Runtime
        {
            get => _runtime;
            set
            {
                if (ReferenceEquals(_runtime, value))
                    return;

                bool wasLoaded = IsLoaded;

                // unload old runtime if needed
                if (_runtime != null && wasLoaded)
                {
                    _runtime.Unload();
                }

                _runtime = value;

                // runtime cleared → context cleared
                if (_runtime == null)
                {
                    _runtimeContext = null;
                    return;
                }

                if (Scenes == null)
                    throw new InvalidOperationException("Scenes must be assigned before setting a non-null Runtime.");

                // initialize new runtime instead of Setup
                _runtime.EnsureInitialized(Application);

                // new context for new runtime
                _runtimeContext = new RuntimeContext(Scenes.Application, _runtime);

                // load new runtime if scene is loaded
                if (wasLoaded)
                {
                    _runtime.Load();
                }
            }
        }

        internal protected override void OnLoad()
        {
            base.OnLoad();
            _runtime?.Load();
        }

        internal protected override void OnUnload()
        {
            _runtime?.Unload();
            base.OnUnload();
        }

        internal protected override void OnUpdate(PresentationContext context)
        {
            base.OnUpdate(context);

            if (_runtime != null && _runtimeContext != null)
            {
                _runtimeContext.Update(context.GameTime);
                _runtime.Update(_runtimeContext);
            }
        }

        internal protected override void OnDrawContent(PresentationContext context)
        {
            base.OnDrawContent(context);

            if (_runtime != null && _runtimeContext != null)
            {
                _runtimeContext.Update(context.GameTime);
                _runtime.Draw(_runtimeContext);
            }
        }
    }
}

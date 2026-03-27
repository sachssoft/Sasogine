using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common.Performance;
using Sachssoft.Sasogine.Scenes;
using System;

namespace Sachssoft.Sasogine.UI.ImGui
{
    public abstract class ImGuiPresentation : IPresentation
    {
        private readonly IScene _scene;
        private readonly DirectLazy<ImGuiRenderer> _lazyRenderer;
        private bool _loaded;

        /// <summary>
        /// Zugriff auf den Renderer. Erst beim Load erzeugt.
        /// </summary>
        public ImGuiRenderer Renderer => _lazyRenderer.Value;
        public IScene Scene => _scene;

        public ImGuiPresentation(IGameApplication application, IScene scene)
        {
            _scene = scene ?? throw new ArgumentNullException(nameof(scene));

            if (application is not Game game)
                throw new InvalidOperationException($"Application must be a {typeof(Game).FullName}.");

            // Lazy-Initialisierung: lock-frei, Single-Thread
            _lazyRenderer = new DirectLazy<ImGuiRenderer>(() =>
            {
                var r = new ImGuiRenderer(game);
                return r;
            });
        }

        public virtual void Load()
        {
            if (_loaded)
                return;

            // Renderer beim Load erzeugen, bevor Draw aufgerufen wird
            var r = _lazyRenderer.Value;
            r.RebuildFontAtlas();
            OnRendererInitialized();

            _loaded = true;
        }

        public virtual void Unload()
        {
            // Optional: Renderer freigeben, falls IDisposable
            // var r = _lazyRenderer.Value;
            // r.Dispose();
        }

        public virtual void Update(PresentationContext context)
        {
            // Optional: Input oder andere Updates weiterleiten
        }

        public virtual void Draw(PresentationContext context)
        {
            if (!_loaded)
                return;

            var r = _lazyRenderer.Value; // Schneller Zugriff, Renderer existiert

            r.BeforeLayout(context.GameTime);
            OnLayout(context);
            r.AfterLayout();
        }

        protected virtual void OnRendererInitialized()
        {
        }

        protected virtual void OnLayout(PresentationContext context)
        {
            // Beispiel-Debugfenster
            ImGuiNET.ImGui.Begin("Debug");
            ImGuiNET.ImGui.Text($"FPS: {ImGuiNET.ImGui.GetIO().Framerate:0}");
            ImGuiNET.ImGui.End();
        }
    }
}
using Sachssoft.Sasogine.Presentation.Rendering;
using Sachssoft.Sasogine.Presentation.Styling;
using System;

namespace Sachssoft.Sasogine.Presentation
{
    /// <summary>
    /// Workspace Service: liefert Instanzen wie IRenderContext, IFontContainer etc.
    /// </summary>
    internal class InternalWorkspaceProvider : IWorkspaceProvider
    {
        private IRenderContext? _renderContext;
        private IFontManager? _fontManager;

        /// <summary>
        /// Liefert die passende Instanz vom Typ T. Lazy-Caching für bekannte Typen.
        /// </summary>
        public T Resolve<T>(Workspace workspace) where T : class
        {
            var requestedType = typeof(T);

            // RenderContext bereitstellen
            if (requestedType.IsAssignableFrom(typeof(IRenderContext)))
            {
                if (_renderContext == null)
                    _renderContext = new InternalRenderContext(workspace);

                return _renderContext as T
                    ?? throw new InvalidOperationException($"Resolved instance cannot be cast to {requestedType.Name}.");
            }

            // FontContainer bereitstellen
            if (requestedType.IsAssignableFrom(typeof(IFontService)))
            {
                if (_fontService == null)
                    _fontService = new InternalFontService(_renderContext); // Implementierung intern

                return _fontService as T
                    ?? throw new InvalidOperationException($"Resolved instance cannot be cast to {requestedType.Name}.");
            }

            // Unbekannte Typen
            throw new NotSupportedException($"Resolve<{requestedType.Name}> is not supported by this WorkspaceService.");
        }

        public void NotifySkinChanged(Skin skin)
        {
            _renderContext.
        }
    }
}
using Sachssoft.Sasogine.Presentation.Styling;

namespace Sachssoft.Sasogine.Presentation
{
    /// <summary>
    /// Workspace Provider liefert plattformunabhängig die richtigen Instanzen
    /// wie RenderContext, FontContainer oder andere Workspace-Ressourcen.
    /// </summary>
    public interface IWorkspaceProvider
    {
        /// <summary>
        /// Liefert die passende Instanz vom Typ T, egal ob schon vorhanden oder neu erzeugt.
        /// </summary>
        T Resolve<T>(Workspace workspace) where T : class;

        /// <summary>
        /// Wird aufgerufen, wenn sich Skin/Fonts geändert haben.
        /// Alle relevanten Workspace-Ressourcen können ihren Cache/Assets aktualisieren.
        /// </summary>
        void NotifySkinChanged(Skin skin);
    }
}
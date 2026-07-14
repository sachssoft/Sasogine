using Sachssoft.Sasogine.Presentation.Styling;

namespace Sachssoft.Sasogine.Presentation
{
    public interface IWorkspaceObserver
    {
        /// <summary>
        /// Wird aufgerufen, wenn sich Skin-relevante Assets geändert haben.
        /// RenderContext/Handler kann Fonts / Cache lazy neu laden.
        /// </summary>
        void OnSkinChanged(Skin skin);
    }
}
using Sachssoft.Sasogine.Presentation.Rendering;
using Sachssoft.Sasogine.Presentation.Styling;

namespace Sachssoft.Sasogine.Presentation.Styling
{
    public interface IFontManager
    {
        /// <summary>
        /// Loads fonts from the given skin. Already loaded fonts may be reused.
        /// </summary>
        void LoadSkin(Skin skin);

        /// <summary>
        /// Resolves a font for rendering. 
        /// Returns an opaque FontHandle, internal backend decides how it is implemented.
        /// </summary>
        void ResolveFont(FontFamily family, FontWeight weight, FontStyle style, int size);

        /// <summary>
        /// Unloads all cached fonts and frees resources.
        /// </summary>
        void Unload();
    }
}

using System.Collections.Generic;

namespace Sachssoft.Sasogine.Graphics.Text
{
    public interface IFontBackend
    {
        void Register(string familyName, FontFace variantDefinition);

        void Register(FontFamily fontFamily);

        IEnumerable<string> GetFamilies();

        IEnumerable<FontFace> GetFaces();

        FontFamily? GetFamily(string name);

        //GlyphRun CreateGlyphRun(string text, Font font);

    }
}

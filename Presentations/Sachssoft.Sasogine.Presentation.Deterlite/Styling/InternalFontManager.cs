using FontStashSharp;
using Sachssoft.Sasogine.Presentation.Rendering;
using System;
using System.Collections.Concurrent;

namespace Sachssoft.Sasogine.Presentation.Styling
{
    internal class InternalFontManager : IFontManager
    {
        private readonly IRenderContext? _renderContext;
        private readonly FontSystem _fontSystem;

        public InternalFontManager(IRenderContext renderContext)
        {
            _renderContext = renderContext;

            // FontSystem für FontStashSharp initialisieren
            _fontSystem = new FontSystem();
        }

        public void LoadSkin(Skin skin)
        {

        }

        /// <summary>
        /// Lädt einen Font mit Caching. Wenn der Font bereits geladen ist, wird er zurückgegeben.
        /// </summary>
        public void ResolveFont(FontFamily family, FontWeight weight, FontStyle style, int size)
        {
            if (family == null)
                throw new ArgumentNullException(nameof(family));

            // 1. Hole das passende FontFace aus der Familie
            var face = family.GetFace(weight, style);

            // 2. Cache-Key: (FontFace, Size)
            var key = (face, size);

            // 3. Prüfe Cache
            if (_cache.TryGetValue(key, out var cachedFont))
                return cachedFont;

            // 4. Font laden via FontStashSharp
            // Hier nehmen wir an, dass 'face.Source' eine Datei ist
            var stream = face.Source?.OpenStream()
                         ?? System.IO.File.OpenRead(face.File);

            // FontSystem.AddFont() gibt FontId zurück, wir erzeugen daraus einen TextRenderer / Font
            var fontId = _fontSystem.AddFont(stream);

            // FontSystem.GetFont(fontId, size) gibt ein Font-Objekt
            var font = _fontSystem.GetFont(fontId, size);

            // Optional: wrap in IFont-Adapter, falls IFont von euch speziell ist
            IFont wrappedFont = new FontStashSharpAdapter(font);

            // 5. Cache speichern
            _cache[key] = wrappedFont;

            return wrappedFont;
        }

        public void Unload()
        {
            // Fonts und Cache leeren
            _cache.Clear();
            _fontSystem.Dispose();
        }
    }

    /// <summary>
    /// Adapter von FontStashSharp.Font zu eurem IFont
    /// </summary>
    internal class FontStashSharpAdapter : IFont
    {
        public Font Font { get; }

        public FontStashSharpAdapter(Font font)
        {
            Font = font;
        }
    }
}
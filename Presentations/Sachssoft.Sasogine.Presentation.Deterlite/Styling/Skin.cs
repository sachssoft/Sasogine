using Sachssoft.Sasogine.Presentation.Deterlite.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Styling
{
    public class Skin
    {
        private readonly Workspace _workspace;
        private readonly TextureAtlas[] _atlases;
        private readonly FontTypeface[] _fontTypefaces;

        public Skin(Workspace workspace)
        {
            _workspace = workspace;
        }

        public Workspace Workspace => _workspace;

        public List<StylesheetNamespace> Namespaces { get; } = new();

        public IReadOnlyList<ISkinEntry> Entries { get; init; } 

        public SkinRegistry SkinRegistry { get; } = new();

        public void Load()
        {
        }

        public void Unload()
        {
        }

        public Resource? FindResource(string id) => 
            Entries.Where(x => x.Id == id && x is Resource).FirstOrDefault();

        public TextureAtlas GetAtlas(float scale = 1f)
        {
        }

        public ITextureRegion? FindRegion(string name)
        {
            return null;
        }

        public FontTypeface GetTypeface(string name)
        {
            // Bei Fallback immer erste Typeface
        }

        //private protected override Stylesheet GetStylesheet() => this;

        //#region Atlas

        //private readonly Dictionary<string, TextureSheet> _textureAtlases = new();
        //public Dictionary<string, TextureSheet> TextureAtlases => _textureAtlases;

        //public TextureSheet? FindTextureAtlas(string? name)
        //{
        //    if (_textureAtlases.TryGetValue(name ?? string.Empty, out var atlas))
        //    {
        //        return atlas;
        //    }
        //    return null;
        //}

        //public ITextureRegion? FindRegion(string? atlasName, string? regionName)
        //{
        //    var atlas = FindTextureAtlas(atlasName);

        //    if (atlas != null && atlas.Regions.TryGetValue(regionName ?? string.Empty, out var region))
        //    {
        //        return region;
        //    }
        //    return null;
        //}

        //public ITextureRegion? FindRegion(string? regionName)
        //{
        //    foreach (var atlas in _textureAtlases.Values)
        //    {
        //        if (atlas.Regions.TryGetValue(regionName ?? string.Empty, out var region))
        //        {
        //            return region;
        //        }
        //    }
        //    return null;
        //}

        //#endregion

        //#region Fonts

        //private readonly Dictionary<string, FontTypeface> _typefaces = new();
        //private readonly Dictionary<string, Font> _fonts = new();

        //public Dictionary<string, FontTypeface> Typefaces => _typefaces;
        //public Dictionary<string, Font> Fonts => _fonts;

        //public Font DefaultFont => _fonts.FirstOrDefault().Value;

        //public Font GetFont(string? id)
        //{
        //    if (_fonts.TryGetValue(id ?? string.Empty, out var font))
        //        return font;

        //    throw new NotImplementedException(); //FontTypeface.Default.GetV;
        //}

        //public FontTypeface FindTypefaceOrDefault(string? name)
        //{
        //    foreach(var typefaces in _typefaces.Values)
        //    {
        //        if (typefaces.Name == name)
        //            return typefaces;
        //    }

        //    return FontTypeface.Noto;
        //}

        //#endregion

        //#region Brushes

        //private readonly Dictionary<string, IBrush> _brushes = new();
        //private static Stylesheet _current;

        //public Dictionary<string, IBrush> Brushes => _brushes;

        //public IBrush? FindBrush(string? id)
        //{
        //    if (_brushes.TryGetValue(id ?? string.Empty, out var brush))
        //        return brush;

        //    return null;
        //}

        //#endregion
    }
}

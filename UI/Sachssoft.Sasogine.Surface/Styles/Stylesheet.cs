using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Resources;
using Sachssoft.Sasogine.Resources.Loaders;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Regions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Sachssoft.Sasogine.Surface.Styles
{
    public sealed class Stylesheet : StyleMap
    {
        #region Vorläufig

        static Stylesheet()
        {
            WhiteRegion = new BlankRegion(Color.White);
        }

        internal static BlankRegion WhiteRegion { get; }

        #endregion

        public static Stylesheet LoadDefault(GraphicsDevice graphicsDevice, StylesheetSize size = StylesheetSize.Normal)
        {
            string fileName = size switch
            {
                //StylesheetSize.Double => "default_ui_skin_2x.xmms",
                //_ => "default_ui_skin.xmms"
                //StylesheetSize.Double => "default_ui_skin_2x.xmms",
                _ => "classic_skin"
            };

            fileName += ".stylesheet.xml";

            return StylesheetLoader.Load(
                Path.Combine("resources", fileName),
                new LoaderOptions { GraphicsDevice = graphicsDevice },
                filePath => new EmbeddedResourceLoader(filePath, Assembly.GetExecutingAssembly())
            );
        }

        public static Stylesheet Current
        {
            get
            {
                return _current ?? throw new InvalidOperationException();
            }
            set
            {
                _current = value;
            }
        }

        private protected override Stylesheet GetStylesheet() => this;

        #region Atlas

        private readonly Dictionary<string, TextureSheet> _textureAtlases = new();
        public Dictionary<string, TextureSheet> TextureAtlases => _textureAtlases;

        public TextureSheet? FindTextureAtlas(string? name)
        {
            if (_textureAtlases.TryGetValue(name ?? string.Empty, out var atlas))
            {
                return atlas;
            }
            return null;
        }

        public ITextureRegion? FindRegion(string? atlasName, string? regionName)
        {
            var atlas = FindTextureAtlas(atlasName);

            if (atlas != null && atlas.Regions.TryGetValue(regionName ?? string.Empty, out var region))
            {
                return region;
            }
            return null;
        }

        public ITextureRegion? FindRegion(string? regionName)
        {
            foreach (var atlas in _textureAtlases.Values)
            {
                if (atlas.Regions.TryGetValue(regionName ?? string.Empty, out var region))
                {
                    return region;
                }
            }
            return null;
        }

        #endregion

        #region Fonts

        private readonly Dictionary<string, FontTypeface> _typefaces = new();
        private readonly Dictionary<string, Font> _fonts = new();

        public Dictionary<string, FontTypeface> Typefaces => _typefaces;
        public Dictionary<string, Font> Fonts => _fonts;

        public Font DefaultFont => _fonts.FirstOrDefault().Value;

        public Font GetFont(string? id)
        {
            if (_fonts.TryGetValue(id ?? string.Empty, out var font))
                return font;

            throw new NotImplementedException(); //FontTypeface.Default.GetV;
        }

        public FontTypeface FindTypefaceOrDefault(string? name)
        {
            foreach(var typefaces in _typefaces.Values)
            {
                if (typefaces.Name == name)
                    return typefaces;
            }

            return FontTypeface.Noto;
        }

        #endregion

        #region Brushes

        private readonly Dictionary<string, IBrush> _brushes = new();
        private static Stylesheet _current;

        public Dictionary<string, IBrush> Brushes => _brushes;

        public IBrush? FindBrush(string? id)
        {
            if (_brushes.TryGetValue(id ?? string.Empty, out var brush))
                return brush;

            return null;
        }

        #endregion
    }
}

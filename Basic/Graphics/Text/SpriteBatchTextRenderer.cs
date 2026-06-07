using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Graphics.Text
{
    public sealed class SpriteBatchTextRenderer
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly FontStashSharpBackend _fontBackend;

        public SpriteBatchTextRenderer(SpriteBatch spriteBatch)
        {
            _spriteBatch = spriteBatch;
            _fontBackend = new FontStashSharpBackend();
        }

        // Langfristig Custom Backend integrieren
        //public SpriteBatchTextDrawContext(SpriteBatch spriteBatch, IFontBackend fontBackend)
        //{
        //    _spriteBatch = spriteBatch;
        //    _fontBackend = fontBackend;
        //}

        public IFontBackend FontBackend => _fontBackend;

        public void DrawText(
            string text,
            FontFace fontFace,
            float size,
            Vector2 position,
            Color color,
            CharacterLayoutOptions? characterLayoutOptions = null)
        {
            var spriteFont = _fontBackend.GetSpriteFont(fontFace, size);

            spriteFont.DrawText(
                _spriteBatch,
                text,
                position,
                color,
                characterSpacing: characterLayoutOptions?.CharacterSpacing ?? 0,
                lineSpacing: characterLayoutOptions?.LineSpacing ?? 0
            );

        }

        public void DrawText(
            string text,
            Font font,
            Vector2 position,
            Color color,
            CharacterLayoutOptions? characterLayoutOptions = null)
        {
            var spriteFont = _fontBackend.GetOrCreateSpriteFont(font);

            spriteFont.DrawText(
                _spriteBatch,
                text,
                position,
                color,
                characterSpacing: characterLayoutOptions?.CharacterSpacing ?? 0,
                lineSpacing: characterLayoutOptions?.LineSpacing ?? 0
            );

        }
    }
}

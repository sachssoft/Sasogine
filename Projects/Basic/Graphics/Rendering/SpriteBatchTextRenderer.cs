using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics.Text;
using Sachssoft.Sasogine.Graphics.Text.Internals;

namespace Sachssoft.Sasogine.Graphics.Rendering
{
    /// <summary>
    /// Provides text rendering using a <see cref="SpriteBatch"/>.
    /// </summary>
    public sealed class SpriteBatchTextRenderer
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly FontStashSharpBackend _fontBackend;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteBatchTextRenderer"/> class.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch used to render text.</param>
        public SpriteBatchTextRenderer(SpriteBatch spriteBatch)
        {
            _spriteBatch = spriteBatch;
            _fontBackend = new FontStashSharpBackend();
        }

        /// <summary>
        /// Gets the font backend used by this renderer.
        /// </summary>
        public IFontBackend FontBackend => _fontBackend;

        /// <summary>
        /// Draws text using the specified font face and font size.
        /// </summary>
        /// <param name="text">The text to draw.</param>
        /// <param name="fontFace">The font face used to render the text.</param>
        /// <param name="size">The font size.</param>
        /// <param name="position">The position at which to draw the text.</param>
        /// <param name="color">The color of the text.</param>
        /// <param name="characterLayoutOptions">
        /// Optional character and line spacing settings.
        /// </param>
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
                lineSpacing: characterLayoutOptions?.LineSpacing ?? 0);
        }

        /// <summary>
        /// Draws text using the specified font options.
        /// </summary>
        /// <param name="text">The text to draw.</param>
        /// <param name="fontOptions">The options used to create or retrieve the font.</param>
        /// <param name="position">The position at which to draw the text.</param>
        /// <param name="color">The color of the text.</param>
        /// <param name="characterLayoutOptions">
        /// Optional character and line spacing settings.
        /// </param>
        public void DrawText(
            string text,
            FontOptions fontOptions,
            Vector2 position,
            Color color,
            CharacterLayoutOptions? characterLayoutOptions = null)
        {
            var spriteFont = _fontBackend.GetOrCreateSpriteFont(fontOptions);

            spriteFont.DrawText(
                _spriteBatch,
                text,
                position,
                color,
                characterSpacing: characterLayoutOptions?.CharacterSpacing ?? 0,
                lineSpacing: characterLayoutOptions?.LineSpacing ?? 0);
        }
    }
}
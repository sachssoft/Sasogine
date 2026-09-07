using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics.Text;
using Sachssoft.Sasogine.Graphics.Text.Internals;
using System;

namespace Sachssoft.Sasogine.Graphics.Rendering
{
    /// <summary>
    /// Provides text rendering using a <see cref="SpriteBatch"/> and a managed
    /// font backend.
    /// </summary>
    /// <remarks>
    /// The renderer uses an internal font backend to resolve and cache runtime
    /// font instances required for drawing text.
    /// </remarks>
    public sealed class SpriteBatchTextRenderer
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly FontStashSharpBackend _fontBackend;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="SpriteBatchTextRenderer"/> class.
        /// </summary>
        /// <param name="spriteBatch">
        /// The sprite batch used to render text.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="spriteBatch"/> is <see langword="null"/>.
        /// </exception>
        public SpriteBatchTextRenderer(SpriteBatch spriteBatch)
        {
            ArgumentNullException.ThrowIfNull(spriteBatch);

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
        /// <param name="text">
        /// The text to draw.
        /// </param>
        /// <param name="fontFace">
        /// The font face used to render the text.
        /// </param>
        /// <param name="size">
        /// The font size in points.
        /// </param>
        /// <param name="position">
        /// The position at which the text is drawn.
        /// </param>
        /// <param name="color">
        /// The color of the text.
        /// </param>
        /// <param name="characterLayoutOptions">
        /// Optional character and line spacing options.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="text"/> or <paramref name="fontFace"/> is
        /// <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="size"/> is less than or equal to zero.
        /// </exception>
        public void DrawText(
            string text,
            FontFace fontFace,
            int size,
            Vector2 position,
            Color color,
            CharacterLayoutOptions? characterLayoutOptions = null)
        {
            ArgumentNullException.ThrowIfNull(text);
            ArgumentNullException.ThrowIfNull(fontFace);

            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            var spriteFont = _fontBackend.GetSpriteFont(fontFace, size);

            spriteFont.DrawText(
                _spriteBatch,
                text,
                position,
                color,
                characterSpacing: characterLayoutOptions?.CharacterSpacing ?? 0f,
                lineSpacing: characterLayoutOptions?.LineSpacing ?? 0f);
        }

        /// <summary>
        /// Draws text using the specified font options.
        /// </summary>
        /// <param name="text">
        /// The text to draw.
        /// </param>
        /// <param name="fontOptions">
        /// The font options used to resolve the font.
        /// </param>
        /// <param name="position">
        /// The position at which the text is drawn.
        /// </param>
        /// <param name="color">
        /// The color of the text.
        /// </param>
        /// <param name="characterLayoutOptions">
        /// Optional character and line spacing options.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="text"/> or <paramref name="fontOptions"/> is
        /// <see langword="null"/>.
        /// </exception>
        public void DrawText(
            string text,
            FontOptions fontOptions,
            Vector2 position,
            Color color,
            CharacterLayoutOptions? characterLayoutOptions = null)
        {
            ArgumentNullException.ThrowIfNull(text);
            ArgumentNullException.ThrowIfNull(fontOptions);

            var spriteFont = _fontBackend.GetOrCreateSpriteFont(fontOptions);

            spriteFont.DrawText(
                _spriteBatch,
                text,
                position,
                color,
                characterSpacing: characterLayoutOptions?.CharacterSpacing ?? 0f,
                lineSpacing: characterLayoutOptions?.LineSpacing ?? 0f);
        }
    }
}
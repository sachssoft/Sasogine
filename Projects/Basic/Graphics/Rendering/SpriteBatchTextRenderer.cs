using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Graphics.Text;
using System;

namespace Sachssoft.Engine.Graphics.Rendering
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
        private readonly IFontBackend _fontBackend;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="SpriteBatchTextRenderer"/> class.
        /// </summary>
        /// <param name="spriteBatch">
        /// The sprite batch used to render text.
        /// </param>
        /// <param name="fontBackend">An optional caller-owned backend for resolving fonts.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="spriteBatch"/> is <see langword="null"/>.
        /// </exception>
        public SpriteBatchTextRenderer(SpriteBatch spriteBatch, IFontBackend? fontBackend = null)
        {
            ArgumentNullException.ThrowIfNull(spriteBatch);

            _spriteBatch = spriteBatch;
            _fontBackend = fontBackend ?? new FontStashSharpBackend();
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

            _fontBackend.GetFont(fontFace, size).Draw(_spriteBatch, text, position, color, characterLayoutOptions);
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

            _fontBackend.GetFont(fontOptions.FontName, fontOptions.Size, fontOptions.Weight, fontOptions.Style)
                .Draw(_spriteBatch, text, position, color, characterLayoutOptions);
        }
    }
}

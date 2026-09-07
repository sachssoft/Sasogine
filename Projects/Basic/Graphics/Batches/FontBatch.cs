using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics.Cameras;
using Sachssoft.Sasogine.Graphics.Text;
using Sachssoft.Sasogine.Graphics.Text.Internals;
using System;

namespace Sachssoft.Sasogine.Graphics.Rendering.Batches
{
    /// <summary>
    /// Provides batched text rendering through the Sasogine rendering pipeline.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Fonts are resolved through a shared <see cref="FontRegistry"/> and
    /// rasterized through the configured FontStashSharp backend.
    /// </para>
    /// <para>
    /// Generated glyphs are submitted to a <see cref="MultiFrameBatch"/> and
    /// therefore use the same shader and camera pipeline as other Sasogine
    /// rendering batches.
    /// </para>
    /// </remarks>
    public sealed class FontBatch : IDisposable
    {
        private readonly MultiFrameBatch _frameBatch;
        private readonly FontStashSharpBackend _fontBackend;
        private readonly FontBatchRenderer _fontRenderer;

        private bool _isDrawing;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="FontBatch"/> class.
        /// </summary>
        /// <param name="graphicsDevice">
        /// The graphics device used for font rendering.
        /// </param>
        /// <param name="fontRegistry">
        /// The registry containing the fonts available to the batch.
        /// </param>
        public FontBatch(
            GraphicsDevice graphicsDevice,
            FontRegistry fontRegistry)
        {
            ArgumentNullException.ThrowIfNull(graphicsDevice);
            ArgumentNullException.ThrowIfNull(fontRegistry);

            FontRegistry = fontRegistry;

            _frameBatch = new MultiFrameBatch(graphicsDevice);
            _fontBackend = new FontStashSharpBackend();
            _fontRenderer = new FontBatchRenderer(graphicsDevice, _frameBatch);

            FontRegistry.RegisterTo(_fontBackend);
        }

        /// <summary>
        /// Gets the font registry associated with the batch.
        /// </summary>
        public FontRegistry FontRegistry { get; }

        /// <summary>
        /// Gets the font backend used by the batch.
        /// </summary>
        public IFontBackend FontBackend => _fontBackend;

        /// <summary>
        /// Begins text rendering using the specified shader and camera.
        /// </summary>
        /// <param name="shader">
        /// The shader used to render glyphs.
        /// </param>
        /// <param name="camera">
        /// The camera used to transform rendered text.
        /// </param>
        public void Begin(
            IShader shader,
            ICamera camera)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            ArgumentNullException.ThrowIfNull(shader);
            ArgumentNullException.ThrowIfNull(camera);

            if (_isDrawing)
                throw new InvalidOperationException("The font batch has already begun.");

            _frameBatch.Begin(shader, camera);
            _isDrawing = true;
        }

        /// <summary>
        /// Adds text using the specified font options.
        /// </summary>
        /// <param name="text">
        /// The text to add.
        /// </param>
        /// <param name="font">
        /// The font options used to resolve the font.
        /// </param>
        /// <param name="transform">
        /// The local transformation applied to the text.
        /// </param>
        /// <param name="color">
        /// The text color.
        /// </param>
        /// <param name="layout">
        /// Optional character and line spacing options.
        /// </param>
        public void AddText(
            string text,
            FontOptions font,
            TextTransform transform,
            Color color,
            CharacterLayoutOptions? layout = null)
        {
            CheckBegin();

            ArgumentNullException.ThrowIfNull(text);
            ArgumentNullException.ThrowIfNull(font);

            SpriteFontBase spriteFont = _fontBackend.GetOrCreateSpriteFont(font);

            spriteFont.DrawText(
                _fontRenderer,
                text,
                new Vector2(
                    transform.Position.X,
                    transform.Position.Y),
                color,
                rotation: transform.Rotation,
                origin: new Vector2(
                    transform.Pivot.X,
                    transform.Pivot.Y),
                scale: transform.Scale,
                characterSpacing: layout?.CharacterSpacing ?? 0f,
                lineSpacing: layout?.LineSpacing ?? 0f);
        }

        /// <summary>
        /// Adds text using the specified registered font family.
        /// </summary>
        public void AddText(
            string text,
            string fontName,
            int size,
            TextTransform transform,
            Color color,
            CharacterLayoutOptions? layout = null)
        {
            AddText(
                text,
                new FontOptions(
                    fontName,
                    size: size),
                transform,
                color,
                layout);
        }

        /// <summary>
        /// Adds text using the specified font face.
        /// </summary>
        public void AddText(
            string text,
            FontFace fontFace,
            int size,
            TextTransform transform,
            Color color,
            CharacterLayoutOptions? layout = null)
        {
            CheckBegin();

            ArgumentNullException.ThrowIfNull(text);
            ArgumentNullException.ThrowIfNull(fontFace);

            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            SpriteFontBase spriteFont = _fontBackend.GetSpriteFont(fontFace, size);

            spriteFont.DrawText(
                _fontRenderer,
                text,
                new Vector2(
                    transform.Position.X,
                    transform.Position.Y),
                color,
                rotation: transform.Rotation,
                origin: new Vector2(
                    transform.Pivot.X,
                    transform.Pivot.Y),
                scale: transform.Scale,
                characterSpacing: layout?.CharacterSpacing ?? 0f,
                lineSpacing: layout?.LineSpacing ?? 0f);
        }

        /// <summary>
        /// Ends text rendering.
        /// </summary>
        public void End()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (!_isDrawing)
                throw new InvalidOperationException("The font batch has not begun.");

            _frameBatch.End();
            _isDrawing = false;
        }

        /// <summary>
        /// Releases resources used by the batch.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _frameBatch.Dispose();

            _disposed = true;

            GC.SuppressFinalize(this);
        }

        private void CheckBegin()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (!_isDrawing)
            {
                throw new InvalidOperationException(
                    "Begin must be called before adding text.");
            }
        }
    }
}
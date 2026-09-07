using FontStashSharp.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Sachssoft.Sasogine.Graphics.Rendering.Batches
{
    /// <summary>
    /// Adapts FontStashSharp glyph rendering to a
    /// <see cref="MultiFrameBatch"/>.
    /// </summary>
    internal sealed class FontBatchRenderer : IFontStashRenderer
    {
        private readonly MultiFrameBatch _frameBatch;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="FontBatchRenderer"/> class.
        /// </summary>
        /// <param name="graphicsDevice">
        /// The graphics device used to create and manage glyph textures.
        /// </param>
        /// <param name="frameBatch">
        /// The frame batch used to submit rendered glyphs.
        /// </param>
        public FontBatchRenderer(
            GraphicsDevice graphicsDevice,
            MultiFrameBatch frameBatch)
        {
            ArgumentNullException.ThrowIfNull(graphicsDevice);
            ArgumentNullException.ThrowIfNull(frameBatch);

            GraphicsDevice = graphicsDevice;
            _frameBatch = frameBatch;
        }

        /// <summary>
        /// Gets the graphics device used for font rendering.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; }

        /// <summary>
        /// Submits a rendered glyph to the underlying frame batch.
        /// </summary>
        /// <param name="texture">
        /// The texture containing the glyph.
        /// </param>
        /// <param name="pos">
        /// The position of the glyph.
        /// </param>
        /// <param name="src">
        /// The source rectangle of the glyph within the texture.
        /// </param>
        /// <param name="color">
        /// The color applied to the glyph.
        /// </param>
        /// <param name="rotation">
        /// The rotation of the glyph in radians.
        /// </param>
        /// <param name="scale">
        /// The scale applied to the glyph.
        /// </param>
        /// <param name="depth">
        /// The layer depth of the glyph.
        /// </param>
        public void Draw(
            Texture2D texture,
            Vector2 pos,
            Rectangle? src,
            Color color,
            float rotation,
            Vector2 scale,
            float depth)
        {
            ArgumentNullException.ThrowIfNull(texture);

            var transform = new QuadTransform
            {
                Position = pos,
                Rotation = rotation,
                Scale = scale
            };

            _frameBatch.AddFrame(
                texture,
                transform,
                src ?? texture.Bounds,
                color);
        }
    }
}
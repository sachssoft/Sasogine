using FontStashSharp;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Sachssoft.Sasogine.Surface.Utility;
using Sachssoft.Sasogine.Surface.Basic;

namespace Sachssoft.Sasogine.Surface.Visuals
{
    /// <summary>
    /// Provides a drawing context for UI rendering, supporting textures, text, rich text, and transformations.
    /// </summary>
    public partial class RenderContext : IDisposable
    {
        #region Fields & Properties

        private static RasterizerState _uiRasterizerState;
        private readonly SpriteBatch _renderer;
        private bool _beginCalled;
        private Rectangle _scissor;
        private TextureFiltering _textureFiltering = TextureFiltering.Nearest;

        public GraphicsDevice GraphicsDevice => _renderer.GraphicsDevice;

        /// <summary>
        /// Transformation applied to all drawing operations.
        /// </summary>
        public Transform Transform;

        /// <summary>
        /// Global opacity multiplier for all drawing operations.
        /// </summary>
        public float Opacity { get; set; } = 1.0f;

        /// <summary>
        /// Gets or sets the current device scissor rectangle.
        /// </summary>
        internal Rectangle DeviceScissor
        {
            get => _renderer.GraphicsDevice.ScissorRectangle;
            set => _renderer.GraphicsDevice.ScissorRectangle = value;
        }

        /// <summary>
        /// Gets or sets the logical scissor rectangle for UI clipping.
        /// </summary>
        public Rectangle Scissor
        {
            get => _scissor;
            set
            {
                _scissor = value;

                if (UIEnvironment.DisableClipping) return;

                Flush();
                var device = _renderer.GraphicsDevice;
                value.X += device.Viewport.X;
                value.Y += device.Viewport.Y;

                DeviceScissor = value;
            }
        }

        /// <summary>
        /// Singleton RasterizerState with ScissorTest enabled for UI rendering.
        /// </summary>
        private static RasterizerState UIRasterizerState
        {
            get
            {
                if (_uiRasterizerState != null) return _uiRasterizerState;

                _uiRasterizerState = new RasterizerState { ScissorTestEnable = true };
                return _uiRasterizerState;
            }
        }

        #endregion

        #region Constructor / Dispose

        /// <summary>
        /// Initializes a new instance of <see cref="RenderContext"/> using the game's GraphicsDevice.
        /// </summary>
        public RenderContext()
        {
            _renderer = new SpriteBatch(UIEnvironment.Game.GraphicsDevice);
        }

        /// <summary>
        /// Releases unmanaged resources used by the renderer.
        /// </summary>
        private void ReleaseUnmanagedResources()
        {
            _renderer?.Dispose();
        }

        /// <summary>
        /// Disposes the renderer and suppresses finalization.
        /// </summary>
        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizer to ensure unmanaged resources are released.
        /// </summary>
        ~RenderContext()
        {
            ReleaseUnmanagedResources();
        }

        #endregion

        #region Public Methods - Opacity

        /// <summary>
        /// Multiplies the current opacity by the given value.
        /// </summary>
        /// <param name="opacity">The opacity multiplier.</param>
        public void AddOpacity(float opacity)
        {
            Opacity *= opacity;
        }

        #endregion

        #region Public Methods - Begin / End / Flush

        /// <summary>
        /// Begins a SpriteBatch session for rendering UI.
        /// </summary>
        public void Begin()
        {
            var samplerState = _textureFiltering == TextureFiltering.Nearest
                ? SamplerState.PointClamp
                : SamplerState.LinearClamp;

            _renderer.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                samplerState,
                null,
                UIRasterizerState,
                null);

            _beginCalled = true;
        }

        /// <summary>
        /// Ends the current SpriteBatch session.
        /// </summary>
        public void End()
        {
            _renderer.End();
            _beginCalled = false;
        }

        /// <summary>
        /// Flushes the current batch by ending and beginning a new SpriteBatch session.
        /// </summary>
        public void Flush()
        {
            if (!_beginCalled) return;

            End();
            Begin();
        }

        #endregion

        #region Public Methods - Draw Texture2D

        /// <summary>
        /// Draws a texture at a given position with color and optional scale and rotation.
        /// </summary>
        public void Draw(Texture2D texture, Vector2 position, Color color, Vector2 scale, float rotation = 0.0f) =>
            Draw(texture, position, null, color, rotation, scale);

        /// <summary>
        /// Draws a texture at a given position with optional source rectangle, color, rotation, and scale.
        /// </summary>
        public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 scale, float depth = 0.0f)
        {
            SetTextureFiltering(TextureFiltering.Nearest);
            color = CrossEngineStuff.MultiplyColor(color, Opacity);

            scale *= Transform.Scale;
            rotation += Transform.Rotation;
            position = Transform.Apply(position);

            _renderer.Draw(texture, position, sourceRectangle, color, rotation, Vector2.Zero, scale, SpriteEffects.None, depth);
        }

        /// <summary>
        /// Draws a texture at a destination rectangle with optional source rectangle, color, rotation, and origin.
        /// </summary>
        public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float depth = 0f)
        {
            Vector2 sz = sourceRectangle != null
                ? new Vector2(sourceRectangle.Value.Width, sourceRectangle.Value.Height)
                : new Vector2(texture.Width, texture.Height);

            var pos = new Vector2(destinationRectangle.X, destinationRectangle.Y);
            var scale = new Vector2(destinationRectangle.Width / sz.X, destinationRectangle.Height / sz.Y);

            Draw(texture, pos, sourceRectangle, color, rotation, scale, depth);
        }

        // Overloads for convenience
        public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation = 0f) =>
            Draw(texture, destinationRectangle, sourceRectangle, color, rotation, Vector2.Zero);

        public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color) =>
            Draw(texture, destinationRectangle, sourceRectangle, color, 0f);

        public void Draw(Texture2D texture, Rectangle destinationRectangle, Color color) =>
            Draw(texture, destinationRectangle, null, color, 0f);

        public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color) =>
            Draw(texture, position, sourceRectangle, color, 0f, Vector2.One);

        public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation) =>
            Draw(texture, position, sourceRectangle, color, rotation, Vector2.One);

        public void Draw(Texture2D texture, Vector2 position, Color color) =>
            Draw(texture, position, null, color, 0f, Vector2.One);

        #endregion

        #region Public Methods - Draw Text

        /// <summary>
        /// Draws a string using the given font, color, scale, rotation, and layer depth.
        /// </summary>
        public void DrawString(SpriteFontBase font, string text, Vector2 position, Color color, Vector2 scale, float rotation, float layerDepth = 0.0f)
        {
            SetTextTextureFiltering();
            color = CrossEngineStuff.MultiplyColor(color, Opacity);
            position = Transform.Apply(position);

            scale *= Transform.Scale;
            rotation += Transform.Rotation;

#if MONOGAME || FNA || STRIDE
            font.DrawText(_renderer, text, position, color, rotation, Vector2.Zero, scale, layerDepth);
#else
            if (_fontStashRenderer != null)
            {
                font.DrawText(_fontStashRenderer, text, position, color, rotation, Vector2.Zero, scale);
            }
            else
            {
                font.DrawText(_fontStashRenderer2, text, position, color, rotation, Vector2.Zero, scale);
            }
#endif
        }

        /// <summary>
        /// Overload of DrawString without rotation.
        /// </summary>
        public void DrawString(SpriteFontBase font, string text, Vector2 position, Color color, Vector2 scale, float layerDepth = 0.0f) =>
            DrawString(font, text, position, color, scale, 0f, layerDepth);

        /// <summary>
        /// Overload of DrawString with default scale and rotation.
        /// </summary>
        public void DrawString(SpriteFontBase font, string text, Vector2 position, Color color, float layerDepth = 0.0f) =>
            DrawString(font, text, position, color, Vector2.One, 0f, layerDepth);

        #endregion

        #region Public Methods - Draw RichText

        /// <summary>
        /// Draws rich text with optional scale, rotation, layer depth, and horizontal alignment.
        /// </summary>
        public void DrawRichText(RichTextLayout richText, Vector2 position, Color color,
            Vector2? sourceScale = null, float rotation = 0, float layerDepth = 0.0f,
            HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left)
        {
            SetTextTextureFiltering();
            color = CrossEngineStuff.MultiplyColor(color, Opacity);
            position = Transform.Apply(position);

            var scale = sourceScale ?? Vector2.One;
            scale *= Transform.Scale;
            rotation += Transform.Rotation;

            richText.Draw(_renderer, position, color, rotation, Vector2.Zero, scale, layerDepth,
                horizontalAlignment switch
                {
                    HorizontalAlignment.Left => FontStashSharp.RichText.TextHorizontalAlignment.Left,
                    HorizontalAlignment.Center => FontStashSharp.RichText.TextHorizontalAlignment.Center,
                    HorizontalAlignment.Right => FontStashSharp.RichText.TextHorizontalAlignment.Right,
                    _ => throw new NotImplementedException()
                });
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Sets the texture filtering mode and flushes the batch if changed.
        /// </summary>
        private void SetTextureFiltering(TextureFiltering value)
        {
            if (_textureFiltering == value) return;

            _textureFiltering = value;
            Flush();
        }

        /// <summary>
        /// Sets the texture filtering mode for text rendering depending on UI settings.
        /// </summary>
        private void SetTextTextureFiltering()
        {
            SetTextureFiltering(UIEnvironment.SmoothText ? TextureFiltering.Linear : TextureFiltering.Nearest);
        }

        #endregion
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics;
using Sachssoft.Sasogine.Presentation.Deterlite.Layouts;
using Sachssoft.Sasogine.UI.Deterlite;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Rendering
{
    public sealed class SpriteBatchRenderContext : IRenderContext
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly GraphicsDevice _graphicsDevice;

        private readonly Stack<Bounds> _clipStack = new();
        private readonly Stack<Matrix> _transformStack = new();
        private Matrix _currentTransform = Matrix.Identity;

        private bool _isBegun;
        private static Texture2D? _blankTexture;

        public SpriteBatchRenderContext(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            _spriteBatch = new SpriteBatch(graphicsDevice);

            // Nur einmal setzen
            _blankTexture ??= graphicsDevice.CreateEmptyTexture(Color.White); // Sasogine Mixin
        }

        public void Begin(RasterizerState? rasterizerState = null)
        {
            if (_isBegun) throw new InvalidOperationException("RenderContext already begun");

            _spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                rasterizerState ?? new RasterizerState { CullMode = CullMode.None, ScissorTestEnable = true },
                null,
                _currentTransform
            );

            _isBegun = true;
        }

        public void End()
        {
            if (!_isBegun) throw new InvalidOperationException("RenderContext not begun");

            _spriteBatch.End();
            _isBegun = false;
        }

        public void DrawRectangle(Bounds rect, Color color)
        {
            var position = Vector2.Transform(rect.Location, _currentTransform);
            var size = Vector2.Transform(new Vector2(rect.Width, rect.Height), _currentTransform) - Vector2.Transform(Vector2.Zero, _currentTransform);
            _spriteBatch.Draw(_blankTexture,
                new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y),
                color);
        }

        public void DrawBorder(Bounds rect, Color color, Insets thickness)
        {
            int left = (int)thickness.Left;
            int right = (int)thickness.Right;
            int top = (int)thickness.Top;
            int bottom = (int)thickness.Bottom;

            int x = (int)rect.X;
            int y = (int)rect.Y;
            int w = (int)rect.Width;
            int h = (int)rect.Height;

            // Top
            if (top > 0) DrawRectangle(new Bounds(x, y, w, top), color);
            // Bottom
            if (bottom > 0) DrawRectangle(new Bounds(x, y + h - bottom, w, bottom), color);
            // Left
            if (left > 0) DrawRectangle(new Bounds(x, y, left, h), color);
            // Right
            if (right > 0) DrawRectangle(new Bounds(x + w - right, y, right, h), color);
        }

        public void DrawText(SpriteFont font, string text, Vector2 position, Color color)
        {
            var transformedPos = Vector2.Transform(position, _currentTransform);
            _spriteBatch.DrawString(font, text, transformedPos, color);
        }

        public void PushClip(Bounds rect)
        {
            var current = _graphicsDevice.ScissorRectangle;
            var intersect = Rectangle.Intersect(current, (Rectangle)rect);
            _graphicsDevice.ScissorRectangle = intersect;
            _clipStack.Push(rect);
        }

        public void PopClip()
        {
            if (_clipStack.Count > 0)
            {
                _clipStack.Pop();
                var previous = _clipStack.Count > 0 ? (Rectangle)_clipStack.Peek() : _graphicsDevice.Viewport.Bounds;
                _graphicsDevice.ScissorRectangle = previous;
            }
        }

        // --- angepasst für Transform ---
        public void PushTransform(Transform transform)
        {
            _transformStack.Push(_currentTransform);
            _currentTransform = transform.Matrix * _currentTransform;
        }

        public void PopTransform()
        {
            if (_transformStack.Count > 0)
                _currentTransform = _transformStack.Pop();
        }

        public void Dispose()
        {
            _spriteBatch.Dispose();
            _clipStack.Clear();
            _transformStack.Clear();
        }
    }
}
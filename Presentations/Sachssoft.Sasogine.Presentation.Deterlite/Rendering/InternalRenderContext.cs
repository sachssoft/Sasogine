using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Graphics;
using Sachssoft.Sasogine.Presentation.Layouts;
using Sachssoft.Sasogine.Presentation.Styling;
using System;
using System.Collections.Generic;

using Bounds = Sachssoft.Sasogine.Common.Bounds;
using FSSBounds = FontStashSharp.Bounds;

namespace Sachssoft.Sasogine.Presentation.Rendering;

internal sealed class InternalRenderContext : IRenderContext, IDisposable
{
    private readonly Workspace _workspace;
    private readonly SpriteBatch _spriteBatch;
    private readonly GraphicsDevice _graphicsDevice;

    private readonly Stack<Bounds> _clipStack = new();
    private readonly Stack<Matrix> _transformStack = new();
    private Matrix _currentTransform = Matrix.Identity;

    private bool _isBegun;
    private static Texture2D? _blankTexture;

    private Skin _skin = null!;

    private readonly Dictionary<Font, FontSystem> _fontSystems = new(); 
    private readonly Dictionary<Font, SpriteFontBase> _fontCache = new();

    public InternalRenderContext(Workspace workspace)
    {
        _workspace = workspace;
        _graphicsDevice = workspace.Application.GraphicsDevice;
        _spriteBatch = new SpriteBatch(_graphicsDevice);

        // Nur einmal setzen
        _blankTexture ??= _graphicsDevice.CreateEmptyTexture(Color.White); // Extension von Sasogine
    }

    // SkinChanged Handler
    void IWorkspaceObserver.OnSkinChanged(Skin skin)
    {
        // Dispose aller alten FontSystems
        foreach (var fs in _fontSystems.Values)
            fs.Dispose();

        _fontSystems.Clear();
        _fontCache.Clear();

        foreach (var fontFaceSet in skin.FontFaceSets)
        {
            foreach (var fontFace in fontFaceSet.Faces)
            {
                var font = new Font(
                    name: fontFaceSet.Name,
                    weight: fontFace.Weight,
                    style: fontFace.Style,
                    size: 16
                );

                var fontSystem = new FontSystem();
                using var stream = fontFace.Resource.GetStream();
                fontSystem.AddFont(stream);

                _fontSystems[font] = fontSystem;
            }
        }
    }

    // --- Begin / End
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

    // --- Draw Helpers
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

    public void DrawText(Bounds rect, string text, Font font, TextLayout layout, Color color)
    {
        if (!_fontSystems.TryGetValue(font, out var fontSystem))
            throw new InvalidOperationException($"FontSystem for '{font.Name}' not loaded.");

        if (!_fontCache.TryGetValue(font, out var spriteFont))
        {
            spriteFont = fontSystem.GetFont(font.Size);
            _fontCache[font] = spriteFont;
        }

        Vector2 pos = Vector2.Transform(rect.Location, _currentTransform);
        _spriteBatch.DrawString(spriteFont, text, pos, color);
    }

    public Vector2 MeasureText(string text, Font font, float maxWidth = float.PositiveInfinity, TextLayout? layout = null)
    {
        if (!_fontSystems.TryGetValue(font, out var fontSystem))
            throw new InvalidOperationException($"FontSystem for '{font.Name}' not loaded.");

        if (!_fontCache.TryGetValue(font, out var spriteFont))
        {
            spriteFont = fontSystem.GetFont(font.Size);
            _fontCache[font] = spriteFont;
        }

        Vector2 size = spriteFont.MeasureString(text);
        if (maxWidth < float.PositiveInfinity && size.X > maxWidth)
            size.X = maxWidth;

        return size;
    }

    // --- Clipping
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

    // --- Transformation
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

        // Clear Stacks
        _clipStack.Clear();
        _transformStack.Clear();

        // Dispose aller FontSystems, um GPU-Texturen freizugeben
        foreach (var fs in _fontSystems.Values)
            fs.Dispose();

        _fontSystems.Clear();
        _fontCache.Clear(); // SpriteFonts werden von FontSystem verwaltet, kein Dispose nötig
    }
}
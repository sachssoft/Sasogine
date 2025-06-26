using FontStashSharp;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using sachssoft.Sasogine.Surface.Utility;

namespace sachssoft.Sasogine.Surface.Visuals;

public enum TextureFiltering
{
    Nearest,
    Linear
}

public partial class RenderContext : IDisposable
{
    private static RasterizerState _uiRasterizerState;

    private static RasterizerState UIRasterizerState
    {
        get
        {
            if (_uiRasterizerState != null)
            {
                return _uiRasterizerState;
            }

            _uiRasterizerState = new RasterizerState
            {
                ScissorTestEnable = true
            };
            return _uiRasterizerState;
        }
    }

    private readonly SpriteBatch _renderer;
    private bool _beginCalled;
    private Rectangle _scissor;
    private TextureFiltering _textureFiltering = TextureFiltering.Nearest;
    public Transform Transform;

    internal Rectangle DeviceScissor
    {
        get
        {
            var device = _renderer.GraphicsDevice;
            return device.ScissorRectangle;
        }

        set
        {
            var device = _renderer.GraphicsDevice;
            device.ScissorRectangle = value;
        }
    }


    public Rectangle Scissor
    {
        get
        {
            return _scissor;
        }

        set
        {
            _scissor = value;

            if (UIEnvironment.DisableClipping)
            {
                return;
            }

            Flush();
            var device = _renderer.GraphicsDevice;
            value.X += device.Viewport.X;
            value.Y += device.Viewport.Y;

            DeviceScissor = value;
        }
    }

    public float Opacity { get; set; }

    public RenderContext()
    {
        _renderer = new SpriteBatch(UIEnvironment.Game.GraphicsDevice);
    }

    /// <summary>
    /// Applies opacity
    /// </summary>
    /// <param name="opacity"></param>
    public void AddOpacity(float opacity)
    {
        Opacity *= opacity;
    }

    private void SetTextureFiltering(TextureFiltering value)
    {
        if (_textureFiltering == value)
        {
            return;
        }

        _textureFiltering = value;
        Flush();
    }

    /// <summary>
    /// Draws a texture
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="destinationRectangle"></param>
    /// <param name="sourceRectangle"></param>
    /// <param name="color"></param>
    /// <param name="rotation"></param>
    /// <param name="depth"></param>
    public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, float depth = 0.0f)
    {
        Vector2 sz;
        if (sourceRectangle != null)
        {
            sz = new Vector2(sourceRectangle.Value.Width, sourceRectangle.Value.Height);
        }
        else
        {
            sz = new Vector2(texture.Width, texture.Height);
        }

        var pos = new Vector2(destinationRectangle.X, destinationRectangle.Y);
        var scale = new Vector2(destinationRectangle.Width / sz.X, destinationRectangle.Height / sz.Y);
        Draw(texture, pos, sourceRectangle, color, rotation, scale, depth);
    }

    /// <summary>
    /// Draws a texture
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="destinationRectangle"></param>
    /// <param name="sourceRectangle"></param>
    /// <param name="color"></param>
    /// <param name="rotation"></param>
    public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation) => Draw(texture, destinationRectangle, sourceRectangle, color, rotation, 0.0f);

    /// <summary>
    /// Draws a texture
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="destinationRectangle"></param>
    /// <param name="sourceRectangle"></param>
    /// <param name="color"></param>
    public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color) => Draw(texture, destinationRectangle, sourceRectangle, color, 0);

    /// <summary>
    /// Draws a texture
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="destinationRectangle"></param>
    /// <param name="color"></param>
    public void Draw(Texture2D texture, Rectangle destinationRectangle, Color color) => Draw(texture, destinationRectangle, null, color, 0);

    /// <summary>
    /// Draws a texture
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="position"></param>
    /// <param name="sourceRectangle"></param>
    /// <param name="color"></param>
    /// <param name="rotation"></param>
    /// <param name="depth"></param>
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
    /// Draws a texture
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="pos"></param>
    /// <param name="color"></param>
    /// <param name="scale"></param>
    /// <param name="rotation"></param>
    public void Draw(Texture2D texture, Vector2 pos, Color color, Vector2 scale, float rotation = 0.0f) =>
        Draw(texture, pos, null, color, rotation, scale);

    /// <summary>
    /// Draws a texture
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="position"></param>
    /// <param name="sourceRectangle"></param>
    /// <param name="color"></param>
    /// <param name="rotation"></param>
    public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation) =>
        Draw(texture, position, sourceRectangle, color, rotation, Vector2.One);

    /// <summary>
    /// Draws a texture
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="position"></param>
    /// <param name="sourceRectangle"></param>
    /// <param name="color"></param>
    public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color) =>
        Draw(texture, position, sourceRectangle, color, 0, Vector2.One);

    /// <summary>
    /// Draws a texture
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="position"></param>
    /// <param name="color"></param>
    public void Draw(Texture2D texture, Vector2 position, Color color) =>
        Draw(texture, position, null, color, 0, Vector2.One);

    private void SetTextTextureFiltering()
    {
        if (!UIEnvironment.SmoothText)
        {
            SetTextureFiltering(TextureFiltering.Nearest);
        }
        else
        {
            SetTextureFiltering(TextureFiltering.Linear);
        }
    }

    /// <summary>
    /// Draws a text
    /// </summary>
    /// <param name="text">The text which will be drawn.</param>
    /// <param name="position">The drawing location on screen.</param>
    /// <param name="color">A color mask.</param>
    /// <param name="rotation">A rotation of this text in radians.</param>
    /// <param name="scale">A scaling of this text.</param>
    /// <param name="layerDepth">A depth of the layer of this string.</param>
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

    public void DrawString(SpriteFontBase font, string text, Vector2 position, Color color, Vector2 scale, float layerDepth = 0.0f) =>
        DrawString(font, text, position, color, scale, 0, layerDepth);

    /// <summary>
    /// Draws a text
    /// </summary>
    /// <param name="text">The text which will be drawn.</param>
    /// <param name="position">The drawing location on screen.</param>
    /// <param name="color">A color mask.</param>
    /// <param name="layerDepth">A depth of the layer of this string.</param>
    public void DrawString(SpriteFontBase font, string text, Vector2 position, Color color, float layerDepth = 0.0f) =>
        DrawString(font, text, position, color, Vector2.One, 0, layerDepth);

    /// <summary>
    /// Draws a rich text
    /// </summary>
    /// <param name="richText">The text which will be drawn.</param>
    /// <param name="position">The drawing location on screen.</param>
    /// <param name="color">A color mask.</param>
    /// <param name="sourceScale">A scaling of this text.</param>
    /// <param name="rotation">A rotation of this text in radians.</param>
    /// <param name="layerDepth">A depth of the layer of this string.</param>
    public void DrawRichText(RichTextLayout richText, Vector2 position, Color color,
        Vector2? sourceScale = null, float rotation = 0, float layerDepth = 0.0f,
        TextHorizontalAlignment horizontalAlignment = TextHorizontalAlignment.Left)
    {
        SetTextTextureFiltering();
        color = CrossEngineStuff.MultiplyColor(color, Opacity);
        position = Transform.Apply(position);

        var scale = sourceScale ?? Vector2.One;

        scale *= Transform.Scale;
        rotation += Transform.Rotation;

        richText.Draw(_renderer, position, color, rotation, Vector2.Zero, scale, layerDepth, horizontalAlignment);
    }

    public void Begin()
    {
        var samplerState = _textureFiltering == TextureFiltering.Nearest ? SamplerState.PointClamp : SamplerState.LinearClamp;

        _renderer.Begin(SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            samplerState,
            null,
            UIRasterizerState,
            null);

        _beginCalled = true;
    }

    public void End()
    {
        _renderer.End();
        _beginCalled = false;
    }

    public void Flush()
    {
        if (!_beginCalled)
        {
            return;
        }

        End();
        Begin();
    }

    private void ReleaseUnmanagedResources()
    {
        _renderer?.Dispose();
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~RenderContext()
    {
        ReleaseUnmanagedResources();
    }
}
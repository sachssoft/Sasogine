using FontStashSharp;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Graphics.Rendering
{
    [Obsolete]
    public sealed class TextRenderer : BaseRenderer, IDisposable
    {
        private readonly IntegralFontRenderer _renderer;
        private TextRendererStyle? _defaultStyle;
        private FontSystem? _fontSystem;

        public TextRenderer(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
            _renderer = new IntegralFontRenderer();
            Position = Vector2.Zero;
            Rotation = 0f;
            Scale = Vector2.One;
        }

        public string? Text { get; set; }
        public TextRendererStyle? Style { get; set; }
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public Vector2 Scale { get; set; }

        public FontSystem FontSystem
        {
            get => _fontSystem ?? throw new InvalidOperationException("FontSystem has not been set.");
            set
            {
                _fontSystem = value ?? throw new ArgumentNullException(nameof(value));
                _defaultStyle = null;
            }
        }

        protected override void DrawInternal(GameContext context, Matrix? transform = null, CameraBase? customCamera = null, IEffectAdapter? customEffect = null)
        {
            if (FontSystem == null || string.IsNullOrEmpty(Text))
                return;

            if (_defaultStyle == null)
                _defaultStyle = new TextRendererStyle
                {
                    FontSize = 16f,
                    Color = Color.White,
                    Alignment = TextHorizontalAlignment.Left,
                    CharacterSpacing = 0f,
                    LineSpacing = 0f,
                    Decoration = TextStyle.None
                };

            var style = Style ?? _defaultStyle;

            _renderer.GameContext = context;
            _renderer.Transform = transform ?? Matrix.Identity;
            _renderer.CustomCamera = customCamera;
            _renderer.CustomEffect = customEffect;

            _renderer.DrawText(
                FontSystem,
                style,
                Text,
                Position,
                Rotation,
                Scale
            );
        }

        private class IntegralFontRenderer
        {
            public GameContext? GameContext;
            public IEffectAdapter? CustomEffect;
            public CameraBase? CustomCamera;
            public Matrix Transform;

            public void DrawText(
                FontSystem fontSystem, 
                TextRendererStyle style, 
                string text, 
                Vector2 startPosition, 
                float rotation, 
                Vector2 scale)
            {
            //    if (GameContext == null || string.IsNullOrEmpty(text)) return;

            //    var font = fontSystem.GetFont(style.FontSize);
            //    Texture2D atlasTexture = font.Te;

            //    // Alle Glyphs für den Text abfragen
            //    List<Glyph> glyphs = font.GetGlyphs(
            //        text,
            //        startPosition,
            //        scale,
            //        characterSpacing: style.CharacterSpacing,
            //        lineSpacing: style.LineSpacing
            //    );

            //    // Zeilenweise rendern
            //    Vector2 currentPos = startPosition;
            //    int glyphIndex = 0;
            //    string[] lines = text.Split('\n');

            //    foreach (var line in lines)
            //    {
            //        // Zeilenbreite berechnen für Alignment
            //        float lineWidth = 0f;
            //        for (int i = 0; i < line.Length; i++)
            //            lineWidth += glyphs[glyphIndex + i].XAdvance;

            //        Vector2 lineStart = currentPos;
            //        switch (style.Alignment)
            //        {
            //            case TextHorizontalAlignment.Center:
            //                lineStart.X -= lineWidth / 2f;
            //                break;
            //            case TextHorizontalAlignment.Right:
            //                lineStart.X -= lineWidth;
            //                break;
            //        }

            //        Vector2 pos = lineStart;

            //        for (int i = 0; i < line.Length; i++)
            //        {
            //            Glyph g = glyphs[glyphIndex++];

            //            // Quad für die Glyph
            //            var quad = new QuadPrimitive(
            //                new Vector2(g.Bounds.X, g.Bounds.Y),
            //                new Vector2(g.Bounds.Width, g.Bounds.Height) * scale,
            //                style.Color
            //            );

            //            // Rotation um Glyph-Zentrum
            //            Vector2 origin = new Vector2(g.Bounds.Width / 2f, g.Bounds.Height / 2f);
            //            Matrix rotationMatrix = Matrix.CreateTranslation(-origin.X, -origin.Y, 0) *
            //                                    Matrix.CreateRotationZ(rotation) *
            //                                    Matrix.CreateTranslation(pos.X, pos.Y, 0);

            //            PrimitiveRenderer.Draw(
            //                quad,
            //                GameContext,
            //                g.Texture,
            //                rotationMatrix * Transform,
            //                customCamera: CustomCamera,
            //                customEffect: CustomEffect,
            //                sourceRect: g.Bounds,
            //                flip: FlipMode.Vertical
            //            );

            //            pos.X += g.XAdvance;
            //        }

            //        currentPos.Y += font.MeasureString(line, scale).Y + style.LineSpacing;
            //    }
            }
        }
    }
}

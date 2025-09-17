using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine;
using Sachssoft.Sasogine.Graphics;
using Sachssoft.Sasogine.Graphics.Primitives;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Graphics.Rendering
{
    public sealed class PrimitiveRenderer : BaseRenderer, IDisposable
    {
        private readonly List<PrimitiveBase> _primitives = new();
        private VertexBuffer? _vertexBuffer;
        private IndexBuffer? _indexBuffer;
        private int _vertexBufferSize;
        private int _indexBufferSize;

        public PrimitiveRenderer(GraphicsDevice graphicsDevice) : base(graphicsDevice) { }

        public Texture2D? Texture { get; set; }
        public Rectangle? SourceRect { get; set; }
        public FlipMode FlipMode { get; set; }

        /// <summary>
        /// Schnell-Zeichenfunktion
        /// </summary>
        public static void Draw(
            PrimitiveBase primitive,
            GameContext context,
            Texture2D? texture = null,
            Matrix? transform = null,
            CameraBase? customCamera = null,
            IEffectAdapter? customEffect = null,
            Rectangle? sourceRect = null,
            FlipMode flip = FlipMode.None)
        {
            if (primitive == null) throw new ArgumentNullException(nameof(primitive));
            if (context == null) throw new ArgumentNullException(nameof(context));

            using var renderer = new PrimitiveRenderer(context.GraphicsDevice)
            {
                Texture = texture,
                SourceRect = sourceRect,
                FlipMode = flip
            };
            renderer.Add(primitive);
            renderer.Draw(context, transform, customCamera, customEffect);
        }

        public void Add(PrimitiveBase primitive)
        {
            if (primitive == null) throw new ArgumentNullException(nameof(primitive));
            _primitives.Add(primitive);
        }

        public void Update(GameContext context)
        {
            foreach (var primitive in _primitives)
            {
                if (primitive.Visible)
                    primitive.Update(context);
            }
        }

        protected override void DrawInternal(GameContext context, Matrix? transform = null, CameraBase? customCamera = null, IEffectAdapter? customEffect = null)
        {
            if (_primitives.Count == 0) return;

            int totalVertices = 0, totalIndices = 0;
            foreach (var p in _primitives)
            {
                if (!p.Visible) continue;
                totalVertices += p.VertexCount;
                totalIndices += p.IndexCount;
            }

            if (totalVertices == 0 || totalIndices == 0) return;

            // VertexBuffer ggf. anpassen
            if (_vertexBuffer == null || _vertexBufferSize < totalVertices)
            {
                _vertexBuffer?.Dispose();
                _vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColorNormalTexture), totalVertices, BufferUsage.WriteOnly);
                _vertexBufferSize = totalVertices;
            }

            // IndexBuffer ggf. anpassen
            if (_indexBuffer == null || _indexBufferSize < totalIndices)
            {
                _indexBuffer?.Dispose();
                _indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, totalIndices, BufferUsage.WriteOnly);
                _indexBufferSize = totalIndices;
            }

            var vertices = new VertexPositionColorNormalTexture[totalVertices];
            var indices = new short[totalIndices];

            int vOffset = 0, iOffset = 0;
            foreach (var p in _primitives)
            {
                if (!p.Visible) continue;
                p.Fill(vertices, vOffset, indices, iOffset, (short)vOffset, Texture);
                vOffset += p.VertexCount;
                iOffset += p.IndexCount;
            }

            // SourceRect & FlipMode UV-Anpassung
            if (Texture != null && SourceRect.HasValue)
            {
                var src = SourceRect.Value;
                float invW = 1f / Texture.Width;
                float invH = 1f / Texture.Height;

                Vector2 uvMin = new Vector2(src.X * invW, src.Y * invH);
                Vector2 uvMax = new Vector2((src.X + src.Width) * invW, (src.Y + src.Height) * invH);

                bool flipH = FlipMode == FlipMode.Horizontal || FlipMode == FlipMode.Both;
                bool flipV = FlipMode == FlipMode.Vertical || FlipMode == FlipMode.Both;

                for (int i = 0; i < vertices.Length; i++)
                {
                    var uv = vertices[i].TextureCoordinate;

                    float u = flipH ? MathHelper.Lerp(uvMax.X, uvMin.X, uv.X) : MathHelper.Lerp(uvMin.X, uvMax.X, uv.X);
                    float v = flipV ? MathHelper.Lerp(uvMax.Y, uvMin.Y, uv.Y) : MathHelper.Lerp(uvMin.Y, uvMax.Y, uv.Y);

                    vertices[i].TextureCoordinate = new Vector2(u, v);
                }
            }

            _vertexBuffer.SetData(vertices);
            _indexBuffer.SetData(indices);

            GraphicsDevice.SetVertexBuffer(_vertexBuffer);
            GraphicsDevice.Indices = _indexBuffer;

            var effect = customEffect ?? context.Runtime.Effect;
            var camera = customCamera ?? context.Runtime.Camera
                         ?? throw new InvalidOperationException("No camera available for rendering.");

            effect.Texture = Texture;
            effect.World = transform ?? Matrix.Identity;
            effect.View = camera.View;
            effect.Projection = camera.Projection;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, totalIndices / 3);
            }
        }

        protected override void OnUninitialize()
        {
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
            _vertexBuffer = null;
            _indexBuffer = null;
            _vertexBufferSize = 0;
            _indexBufferSize = 0;
            _primitives.Clear();

            base.OnUninitialize();
        }

        public void Dispose()
        {
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
            _vertexBuffer = null;
            _indexBuffer = null;
            _primitives.Clear();
            GC.SuppressFinalize(this);
        }
    }
}
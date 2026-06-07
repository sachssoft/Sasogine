using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine;
using Sachssoft.Sasogine.Components.Rendering.Camera;
using Sachssoft.Sasogine.Graphics;
using System;

namespace Sachssoft.Graphics.Primitives;

[Obsolete("Maybe QuadPrimitive")]
public sealed class TilePrimitive : IDisposable
{
    private readonly GraphicsDevice _graphics_device;
    private readonly Vector2 _size;
    private readonly Color _color;
    private Texture2D? _texture;

    // Statische Buffers, shared für alle Tiles gleicher Größe/Farbe
    private static VertexBuffer? _static_vertex_buffer;
    private static IndexBuffer? _static_index_buffer;
    private static Vector2 _static_size = Vector2.Zero;
    private static Color _static_color = Color.White;
    private static bool _static_buffers_created;

    private bool _disposed;

    public TilePrimitive()
    : this(IGameApplication.Current.GraphicsDevice, Vector2.One, Color.White)
    {
    }

    public TilePrimitive(GraphicsDevice graphics_device, Vector2 size, Color color)
    {
        _graphics_device = graphics_device;
        _size = size;
        _color = color;
    }

    public TilePrimitive(GraphicsDevice graphics_device, Texture2D texture, Color color)
        : this(graphics_device, new Vector2(texture.Width, texture.Height), color)
    {
        _texture = texture;
    }

    public Texture2D? Texture => _texture;
    public Vector2 Size => _size;
    public Color Color => _color;

    private void BuildStaticBuffers()
    {
        if (_static_buffers_created
            && _static_size == _size
            && _static_color == _color)
            return;

        var vertices = new VertexPositionColorTexture[]
        {
            new VertexPositionColorTexture(new Vector3(0f, 0f, 0f), _color, new Vector2(0f, 1f)),       // Bottom-left
            new VertexPositionColorTexture(new Vector3(_size.X, 0f, 0f), _color, new Vector2(1f, 1f)),  // Bottom-right
            new VertexPositionColorTexture(new Vector3(_size.X, _size.Y, 0f), _color, new Vector2(1f, 0f)), // Top-right
            new VertexPositionColorTexture(new Vector3(0f, _size.Y, 0f), _color, new Vector2(0f, 0f))   // Top-left
        };

        var indices = new short[] { 0, 1, 2, 2, 0, 3 };

        _static_vertex_buffer?.Dispose();
        _static_index_buffer?.Dispose();

        _static_vertex_buffer = new VertexBuffer(_graphics_device, typeof(VertexPositionColorTexture), vertices.Length, BufferUsage.WriteOnly);
        _static_vertex_buffer.SetData(vertices);

        _static_index_buffer = new IndexBuffer(_graphics_device, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
        _static_index_buffer.SetData(indices);

        _static_size = _size;
        _static_color = _color;
        _static_buffers_created = true;
    }

    public void Draw(IEffectAdapter effect, CameraBase camera, Matrix transform)
    {
        Draw(_texture, effect, camera, transform);
    }

    public void Draw(Texture2D? texture, IEffectAdapter effect, CameraBase? camera, Matrix transform, Rectangle? source_rect = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TilePrimitive), "Cannot draw because the TilePrimitive has been disposed.");

        _texture = texture;
        BuildStaticBuffers(source_rect);

        if (_static_vertex_buffer == null || _static_index_buffer == null)
            throw new InvalidOperationException("Static buffers not built.");

        _graphics_device.SetVertexBuffer(_static_vertex_buffer);
        _graphics_device.Indices = _static_index_buffer;

        effect.Texture = texture;
        effect.World = transform * (camera?.World ?? Matrix.Identity);

        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        }

        effect.Texture = null;
    }

    private void BuildStaticBuffers(Rectangle? source_rect = null)
    {
        if (_static_buffers_created && _static_size == _size && _static_color == _color)
            return;

        Vector2 uv_top_left = new Vector2(0f, 0f);
        Vector2 uv_bottom_right = new Vector2(1f, 1f);

        if (_texture != null && source_rect.HasValue)
        {
            var rect = source_rect.Value;
            float tex_width = _texture.Width;
            float tex_height = _texture.Height;

            uv_top_left = new Vector2(rect.X / tex_width, rect.Y / tex_height);
            uv_bottom_right = new Vector2((rect.X + rect.Width) / tex_width, (rect.Y + rect.Height) / tex_height);
        }

        var vertices = new VertexPositionColorTexture[]
        {
            new VertexPositionColorTexture(new Vector3(0f, 0f, 0f), _color, new Vector2(uv_top_left.X, uv_bottom_right.Y)), // Bottom-left
            new VertexPositionColorTexture(new Vector3(_size.X, 0f, 0f), _color, new Vector2(uv_bottom_right.X, uv_bottom_right.Y)), // Bottom-right
            new VertexPositionColorTexture(new Vector3(_size.X, _size.Y, 0f), _color, new Vector2(uv_bottom_right.X, uv_top_left.Y)), // Top-right
            new VertexPositionColorTexture(new Vector3(0f, _size.Y, 0f), _color, new Vector2(uv_top_left.X, uv_top_left.Y)) // Top-left
        };

        var indices = new short[] { 0, 1, 2, 2, 0, 3 };

        _static_vertex_buffer?.Dispose();
        _static_index_buffer?.Dispose();

        _static_vertex_buffer = new VertexBuffer(_graphics_device, typeof(VertexPositionColorTexture), vertices.Length, BufferUsage.WriteOnly);
        _static_vertex_buffer.SetData(vertices);

        _static_index_buffer = new IndexBuffer(_graphics_device, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
        _static_index_buffer.SetData(indices);

        _static_size = _size;
        _static_color = _color;
        _static_buffers_created = true;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _static_vertex_buffer?.Dispose();
        _static_index_buffer?.Dispose();
        _static_vertex_buffer = null;
        _static_index_buffer = null;
        _static_buffers_created = false;

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    ~TilePrimitive()
    {
        Dispose();
    }
}

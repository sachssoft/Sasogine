using FontStashSharp;
using FontStashSharp.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Sachssoft.Engine.Controls.Drawing;
/// <summary>Buffers clipped UI geometry in reusable GPU buffers and groups safely compatible draw items.</summary>
/// <remarks>
/// The owning draw context controls the frame lifetime and local coordinate state.
/// Texture ownership stays with the caller. Overlapping items retain painter order;
/// only disjoint items may move past another texture group. The batch uses premultiplied alpha.
/// </remarks>
public sealed class ControlFrameBatch : IFontStashRenderer2, IDisposable
{
    private readonly VertexPositionColorTexture[] _vertices;
    private readonly short[] _indices, _orderedIndices;
    private readonly ControlBatchItem[] _items;
    private readonly ControlBatchGroup[] _groups;
    private readonly DynamicVertexBuffer _vertexBuffer;
    private readonly DynamicIndexBuffer _indexBuffer;
    private readonly BasicEffect _effect;
    private readonly RasterizerState _rasterizer;
    private int _vertexCount, _indexCount, _itemCount, _groupCount;
    private Vector2 _offset;
    private float _scale;
    private bool _halfPixel;
    private Bounds2 _clip;
    private bool _active, _disposed;
    /// <summary>Creates reusable CPU/GPU buffers. Capacity is a vertex count between 32 and 65,532.</summary>
    public ControlFrameBatch(GraphicsDevice graphicsDevice, int capacity = 16384)
    {
        GraphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
        if (capacity < 32 || capacity > 65532)
            throw new ArgumentOutOfRangeException(nameof(capacity));
        _vertices = new VertexPositionColorTexture[capacity];
        _indices = new short[capacity * 3];
        _orderedIndices = new short[_indices.Length];
        _items = new ControlBatchItem[capacity / 3];
        _groups = new ControlBatchGroup[_items.Length];
        _vertexBuffer = new DynamicVertexBuffer(graphicsDevice, VertexPositionColorTexture.VertexDeclaration, capacity, BufferUsage.WriteOnly);
        _indexBuffer = new DynamicIndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, _indices.Length, BufferUsage.WriteOnly);
        _effect = new BasicEffect(graphicsDevice)
        {
            TextureEnabled = true,
            VertexColorEnabled = true,
            LightingEnabled = false
        };
        _rasterizer = new RasterizerState
        {
            CullMode = CullMode.None,
            ScissorTestEnable = false
        };
    }

    /// <inheritdoc/>
    public GraphicsDevice GraphicsDevice { get; }
    /// <summary>Gets draw calls issued by this batch during the current or most recently completed frame.</summary>
    public int DrawCalls { get; private set; }
    /// <summary>Gets the number of accepted textured quads during the frame.</summary>
    public int QuadCount { get; private set; }
    /// <summary>Gets nonempty buffer submissions during the frame.</summary>
    public int FlushCount { get; private set; }

    internal void Begin(int width, int height)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_active)
            throw new InvalidOperationException("The frame batch is already active.");
        _halfPixel = GraphicsDevice.UseHalfPixelOffset;
        Matrix projection = Matrix.CreateOrthographicOffCenter(0, width, height, 0, 0, -1);
        if (_halfPixel)
        {
            projection.M41 -= .5f * projection.M11;
            projection.M42 -= .5f * projection.M22;
        }
        _effect.Projection = projection;
        _effect.View = _effect.World = Matrix.Identity;
        DrawCalls = QuadCount = FlushCount = 0;
        _active = true;
    }

    internal void SetControl(Vector2 offset, float scale, Bounds2 pixelClip)
    {
        _offset = offset;
        _scale = scale;
        // Clipping precedes the projection, so compensate its optional half-pixel translation.
        _clip = _halfPixel ? new Bounds2(pixelClip.X + .5f, pixelClip.Y + .5f, pixelClip.Width, pixelClip.Height) : pixelClip;
    }

    /// <summary>Appends a local textured sprite while preserving the order of overlapping geometry.</summary>
    public void Draw(Texture2D texture, Vector2 position, Rectangle? source, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0)
    {
        EnsureActive();
        ArgumentNullException.ThrowIfNull(texture);
        Rectangle region = source ?? texture.Bounds;
        if (region.Width <= 0 || region.Height <= 0 || color.A == 0)
            return;
        if (!float.IsFinite(position.X) || !float.IsFinite(position.Y) || !float.IsFinite(rotation) || !float.IsFinite(origin.X) || !float.IsFinite(origin.Y) || !float.IsFinite(scale.X) || !float.IsFinite(scale.Y) || !float.IsFinite(layerDepth))
            throw new ArgumentException("Sprite coordinates must be finite.");
        float u0 = region.Left / (float)texture.Width, v0 = region.Top / (float)texture.Height;
        float u1 = region.Right / (float)texture.Width, v1 = region.Bottom / (float)texture.Height;
        if ((effects & SpriteEffects.FlipHorizontally) != 0)
            (u0, u1) = (u1, u0);
        if ((effects & SpriteEffects.FlipVertically) != 0)
            (v0, v1) = (v1, v0);
        float sin = rotation == 0 ? 0 : MathF.Sin(rotation), cos = rotation == 0 ? 1 : MathF.Cos(rotation);
        float left = -origin.X * scale.X, top = -origin.Y * scale.Y;
        float right = left + region.Width * scale.X, bottom = top + region.Height * scale.Y;
        Vector3 Point(float x, float y) => new(position.X + x * cos - y * sin, position.Y + x * sin + y * cos, layerDepth);
        var tl = new VertexPositionColorTexture(Point(left, top), color, new Vector2(u0, v0));
        var tr = new VertexPositionColorTexture(Point(right, top), color, new Vector2(u1, v0));
        var bl = new VertexPositionColorTexture(Point(left, bottom), color, new Vector2(u0, v1));
        var br = new VertexPositionColorTexture(Point(right, bottom), color, new Vector2(u1, v1));
        DrawQuad(texture, ref tl, ref tr, ref bl, ref br);
    }

    /// <summary>Appends FontStashSharp glyphs directly to this frame batch.</summary>
    public void DrawText(SpriteFontBase font, string text, Vector2 position, Color color)
    {
        EnsureActive();
        ArgumentNullException.ThrowIfNull(font);
        ArgumentNullException.ThrowIfNull(text);
        font.DrawText((IFontStashRenderer2)this, text, position, color);
    }

    /// <inheritdoc/>
    public void DrawQuad(Texture2D texture, ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight, ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight)
    {
        EnsureActive();
        ArgumentNullException.ThrowIfNull(texture);
        if (_clip.Width <= 0 || _clip.Height <= 0)
            return;
        var a = Transform(topLeft);
        var b = Transform(topRight);
        var c = Transform(bottomLeft);
        var d = Transform(bottomRight);
        float x = MathF.Min(MathF.Min(a.Position.X, b.Position.X), MathF.Min(c.Position.X, d.Position.X));
        float y = MathF.Min(MathF.Min(a.Position.Y, b.Position.Y), MathF.Min(c.Position.Y, d.Position.Y));
        float right = MathF.Max(MathF.Max(a.Position.X, b.Position.X), MathF.Max(c.Position.X, d.Position.X));
        float bottom = MathF.Max(MathF.Max(a.Position.Y, b.Position.Y), MathF.Max(c.Position.Y, d.Position.Y));
        var bounds = new Bounds2(x, y, right - x, bottom - y);
        if (bounds.Width <= 0 || bounds.Height <= 0 || !ControlBatchGeometry.Overlaps(bounds, _clip))
            return;
        Reserve(14, 30, 2);
        int vertexStart = _vertexCount, indexStart = _indexCount;
        if (x >= _clip.Left && right <= _clip.Right && y >= _clip.Top && bottom <= _clip.Bottom)
        {
            _vertices[_vertexCount++] = a;
            _vertices[_vertexCount++] = b;
            _vertices[_vertexCount++] = c;
            _vertices[_vertexCount++] = d;
            Index(vertexStart);
            Index(vertexStart + 1);
            Index(vertexStart + 2);
            Index(vertexStart + 1);
            Index(vertexStart + 3);
            Index(vertexStart + 2);
        }
        else
        {
            // Clip original triangles independently to preserve their UV/color interpolation.
            Span<VertexPositionColorTexture> clipped = stackalloc VertexPositionColorTexture[12];
            AddPolygon(clipped, ControlBatchGeometry.ClipTriangle(a, b, c, _clip, clipped));
            AddPolygon(clipped, ControlBatchGeometry.ClipTriangle(b, d, c, _clip, clipped));
            if (_indexCount == indexStart)
                return;
            bounds = Control.Intersect(bounds, _clip);
        }

        int group = FindGroup(texture, bounds);
        int item = _itemCount++;
        _items[item] = new ControlBatchItem
        {
            Start = indexStart,
            Count = _indexCount - indexStart,
            Next = -1
        };
        ref var g = ref _groups[group];
        if (g.Last < 0)
            g.First = item;
        else
            _items[g.Last].Next = item;
        g.Last = item;
        g.Bounds = ControlBatchGeometry.Union(g.Bounds, bounds);
        QuadCount++;
    }

    private VertexPositionColorTexture Transform(VertexPositionColorTexture value)
    {
        value.Position.X = (value.Position.X + _offset.X) * _scale;
        value.Position.Y = (value.Position.Y + _offset.Y) * _scale;
        return value;
    }

    private void AddPolygon(ReadOnlySpan<VertexPositionColorTexture> polygon, int count)
    {
        if (count < 3)
            return;
        int start = _vertexCount;
        polygon[..count].CopyTo(_vertices.AsSpan(start));
        _vertexCount += count;
        for (int i = 1; i < count - 1; i++)
        {
            Index(start);
            Index(start + i);
            Index(start + i + 1);
        }
    }

    private void Index(int value) => _indices[_indexCount++] = unchecked((short)value);
    private void Reserve(int vertices, int indices, int items)
    {
        if (_vertexCount + vertices > _vertices.Length || _indexCount + indices > _indices.Length || _itemCount + items > _items.Length)
            Flush();
    }

    private int FindGroup(Texture2D texture, Bounds2 bounds)
    {
        // Conservative union bounds can reject a safe merge, but cannot permit an unsafe one.
        int first = Math.Max(0, _groupCount - 32);
        for (int i = _groupCount - 1; i >= first; i--)
        {
            ref var g = ref _groups[i];
            if (ReferenceEquals(g.Texture, texture))
                return i;
            if (ControlBatchGeometry.Overlaps(g.Bounds, bounds))
                break;
        }

        int index = _groupCount++;
        _groups[index] = new ControlBatchGroup
        {
            Texture = texture,
            Bounds = bounds,
            First = -1,
            Last = -1
        };
        return index;
    }

    /// <summary>Submits pending geometry; also acts as an explicit ordering barrier for external drawing.</summary>
    public void Flush()
    {
        EnsureActive();
        if (_indexCount == 0)
            return;
        int index = 0;
        for (int i = 0; i < _groupCount; i++)
        {
            ref var group = ref _groups[i];
            group.StartIndex = index;
            for (int item = group.First; item >= 0; item = _items[item].Next)
            {
                var entry = _items[item];
                Array.Copy(_indices, entry.Start, _orderedIndices, index, entry.Count);
                index += entry.Count;
            }

            group.IndexCount = index - group.StartIndex;
        }

        _vertexBuffer.SetData(_vertices, 0, _vertexCount, SetDataOptions.Discard);
        _indexBuffer.SetData(_orderedIndices, 0, index, SetDataOptions.Discard);
        GraphicsDevice.SetVertexBuffer(_vertexBuffer);
        GraphicsDevice.Indices = _indexBuffer;
        GraphicsDevice.BlendState = BlendState.AlphaBlend;
        GraphicsDevice.DepthStencilState = DepthStencilState.None;
        GraphicsDevice.RasterizerState = _rasterizer;
        GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
        for (int i = 0; i < _groupCount; i++)
        {
            ref var g = ref _groups[i];
            _effect.Texture = g.Texture;
            _effect.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, g.StartIndex, g.IndexCount / 3);
            DrawCalls++;
        }

        FlushCount++;
        Array.Clear(_groups, 0, _groupCount);
        _vertexCount = _indexCount = _itemCount = _groupCount = 0;
    }

    internal void End()
    {
        if (!_active)
            return;
        try
        {
            Flush();
        }
        finally
        {
            Array.Clear(_groups, 0, _groupCount);
            _vertexCount = _indexCount = _itemCount = _groupCount = 0;
            _active = false;
        }
    }

    private void EnsureActive()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!_active)
            throw new InvalidOperationException("The frame batch is only valid during control drawing.");
    }

    /// <summary>Releases owned buffers and effect; caller-owned textures are retained.</summary>
    public void Dispose()
    {
        if (_disposed)
            return;
        if (_active)
            throw new InvalidOperationException("Cannot dispose an active frame batch.");
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
        _effect.Dispose();
        _rasterizer.Dispose();
        _disposed = true;
    }
}

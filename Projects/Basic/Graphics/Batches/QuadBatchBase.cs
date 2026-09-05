using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Graphics.Cameras;
using System;

namespace Sachssoft.Sasogine.Graphics.Rendering.Batches;

/// <summary>
/// Base class for tile based rendering batches.
/// </summary>
/// <remarks>
/// Provides shared GPU rendering functionality for different tile systems
/// such as orthogonal, isometric and hexagonal tile layouts.
/// 
/// The class manages dynamic vertex buffers, index buffers, textures,
/// shaders and quad generation. Derived classes are responsible for
/// converting tile coordinates into world transformations.
/// </remarks>
public abstract class QuadBatchBase : IDisposable
{
    private readonly GraphicsDevice _graphicsDevice;

    private VertexPositionColorNormalTexture[] _vertices;
    private int[] _indices;

    private DynamicVertexBuffer? _vertexBuffer;
    private IndexBuffer? _indexBuffer;

    private bool _isBegun;
    private bool _disposed;

    private int _capacity;
    private int _quadCount;
    private int _vertexCount;

    private IShader? _shader;
    private ICamera? _camera;
    private Texture2D? _texture;

    /// <summary>
    /// Creates a new tile batch.
    /// </summary>
    /// <param name="graphicsDevice">
    /// Graphics device used for rendering.
    /// </param>
    /// <param name="initialCapacity">
    /// Initial tile capacity.
    /// </param>
    protected QuadBatchBase(
        GraphicsDevice graphicsDevice,
        int initialCapacity = 1024)
    {
        _graphicsDevice = graphicsDevice;

        _capacity = initialCapacity;

        _vertices =
            new VertexPositionColorNormalTexture[
                _capacity * 4];

        _indices =
            new int[
                _capacity * 6];

        BuildIndices();

        CreateBuffers();
    }

    /// <summary>
    /// Starts rendering.
    /// </summary>
    public virtual void Begin(
        IShader shader,
        ICamera camera,
        Texture2D? texture = null)
    {
        ThrowIfDisposed();

        if (_isBegun)
            throw new InvalidOperationException(
                "Batch is already active. Call End() before Begin().");

        _shader = shader ?? throw new ArgumentNullException(nameof(shader));
        _camera = camera ?? throw new ArgumentNullException(nameof(camera));

        _texture = texture;

        _quadCount = 0;
        _vertexCount = 0;

        _isBegun = true;
    }

    /// <summary>
    /// Adds a textured quad using a transformation matrix.
    /// </summary>
    /// <param name="transform">
    /// Transformation applied to the quad.
    /// </param>
    /// <param name="sourceBounds">
    /// Pixel bounds inside the source texture.
    /// </param>
    /// <param name="color">
    /// Color tint applied to the quad.
    /// </param>
    protected void AddQuad(
        Matrix transform,
        PixelBounds2 sourceBounds,
        Color color)
    {
        ThrowIfDisposed();

        if (!_isBegun)
            throw new InvalidOperationException(
                "Batch must be started with Begin() before adding quads.");

        EnsureCapacity();

        int vertex =
            _quadCount * 4;

        Vector3 normal =
            new(0, 0, -1);

        Vector2 uv0 = GetUV(
            sourceBounds.Left,
            sourceBounds.Top);

        Vector2 uv1 = GetUV(
            sourceBounds.Right,
            sourceBounds.Top);

        Vector2 uv2 = GetUV(
            sourceBounds.Right,
            sourceBounds.Bottom);

        Vector2 uv3 = GetUV(
            sourceBounds.Left,
            sourceBounds.Bottom);

        _vertices[vertex + 0] =
            CreateVertex(
                new Vector3(0, 0, 0),
                color,
                normal,
                transform,
                uv0);

        _vertices[vertex + 1] =
            CreateVertex(
                new Vector3(1, 0, 0),
                color,
                normal,
                transform,
                uv1);

        _vertices[vertex + 2] =
            CreateVertex(
                new Vector3(1, 1, 0),
                color,
                normal,
                transform,
                uv2);

        _vertices[vertex + 3] =
            CreateVertex(
                new Vector3(0, 1, 0),
                color,
                normal,
                transform,
                uv3);

        _quadCount++;
        _vertexCount += 4;
    }

    private Vector2 GetUV(
        int x,
        int y)
    {
        if (_texture == null)
            return Vector2.Zero;

        return new Vector2(
            x / (float)_texture.Width,
            y / (float)_texture.Height);
    }

    private static VertexPositionColorNormalTexture CreateVertex(
        Vector3 position,
        Color color,
        Vector3 normal,
        Matrix transform,
        Vector2 uv)
    {
        return new VertexPositionColorNormalTexture(
            Vector3.Transform(
                position,
                transform),
            color,
            normal,
            uv);
    }

    private void EnsureCapacity()
    {
        if (_quadCount < _capacity)
            return;

        _capacity *= 2;

        Array.Resize(ref _vertices, _capacity * 4);
        Array.Resize(ref _indices, _capacity * 6);

        BuildIndices();
        CreateBuffers();
    }

    private void BuildIndices()
    {
        for (int i = 0; i < _capacity; i++)
        {
            int vertex = i * 4;
            int index = i * 6;

            _indices[index + 0] = vertex + 0;
            _indices[index + 1] = vertex + 1;
            _indices[index + 2] = vertex + 2;
            _indices[index + 3] = vertex + 2;
            _indices[index + 4] = vertex + 3;
            _indices[index + 5] = vertex + 0;
        }
    }

    private void CreateBuffers()
    {
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();

        _vertexBuffer =
            new DynamicVertexBuffer(
                _graphicsDevice,
                typeof(VertexPositionColorNormalTexture),
                _vertices.Length,
                BufferUsage.WriteOnly);

        _indexBuffer =
            new IndexBuffer(
                _graphicsDevice,
                IndexElementSize.ThirtyTwoBits,
                _indices.Length,
                BufferUsage.WriteOnly);

        _indexBuffer.SetData(
            _indices);
    }

    /// <summary>
    /// Draws the accumulated tiles.
    /// </summary>
    public virtual void End()
    {
        ThrowIfDisposed();

        if (!_isBegun)
            throw new InvalidOperationException(
                "Batch has not been started. Call Begin() first.");

        try
        {
            if (_shader == null ||
                _camera == null ||
                _quadCount == 0)
            {
                return;
            }

            _vertexBuffer!.SetData(
                _vertices,
                0,
                _vertexCount,
                SetDataOptions.Discard);

            _graphicsDevice.SetVertexBuffer(
                _vertexBuffer);

            _graphicsDevice.Indices = _indexBuffer;

            if (_shader is IShaderTransform transform)
            {
                transform.Camera = _camera;
                transform.Transform = Matrix.Identity;
            }

            _shader.Texture = _texture;
            _shader.Color = Color.White;

            _shader.Apply();

            foreach (var pass in _shader.CurrentTechnique.Passes)
            {
                pass.Apply();

                _graphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0,
                    0,
                    _quadCount * 2);
            }
        }
        finally
        {
            _quadCount = 0;
            _vertexCount = 0;

            _isBegun = false;
        }
    }

    /// <summary>
    /// Releases all graphics resources used by this tile batch.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the tile batch has already been disposed.
    /// </exception>
    public void Dispose()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(
                nameof(QuadBatchBase));
        }

        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();

        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(
                nameof(QuadBatchBase));
        }
    }
}